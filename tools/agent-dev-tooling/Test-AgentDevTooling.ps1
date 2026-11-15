# Checks that this machine has the agent dev tools of openspec change add-agent-dev-tooling
# at the versions pinned in versions.json. Run: pwsh -File tools/agent-dev-tooling/Test-AgentDevTooling.ps1
# Prints one PASS/FAIL line per check (names, versions, paths only) and exits 1 on any failure.
# Each check is a function of its inputs, so Test-AgentDevTooling.Tests.ps1 can feed fakes.

function New-Result([string]$name, [bool]$pass, [string]$reason) {
  [pscustomobject]@{ Name = $name; Pass = $pass; Reason = $reason }
}

# SHA-256 over every file in ordinal order of its '/'-separated relative path:
# UTF-8 path, a 0x00 byte, the file's bytes, a 0x00 byte.
function Get-FolderHash([string]$folder) {
  $root = (Resolve-Path $folder).Path.TrimEnd('\', '/')
  $files = Get-ChildItem -LiteralPath $root -Recurse -File -Force |
    ForEach-Object { [pscustomobject]@{ Rel = $_.FullName.Substring($root.Length + 1).Replace('\', '/'); Full = $_.FullName } }
  $sorted = [Collections.Generic.List[object]]::new()
  $sorted.AddRange([object[]]@($files))
  $sorted.Sort([Comparison[object]] { param($a, $b) [string]::CompareOrdinal($a.Rel, $b.Rel) })
  $sha = [Security.Cryptography.IncrementalHash]::CreateHash('SHA256')
  $zero = [byte[]]@(0)
  foreach ($f in $sorted) {
    $sha.AppendData([Text.Encoding]::UTF8.GetBytes($f.Rel))
    $sha.AppendData($zero)
    $sha.AppendData([IO.File]::ReadAllBytes($f.Full))
    $sha.AppendData($zero)
  }
  [Convert]::ToHexString($sha.GetHashAndReset()).ToLowerInvariant()
}

function Test-GraphifyPackage([string]$uvToolList, [string]$pin) {
  $name = 'graphify package'
  # Official package is graphifyy (two y's); a look-alike such as "graphify" does not match.
  $line = @($uvToolList -split "`r?`n" | Where-Object { $_ -match '^graphifyy v' })[0]
  if (-not $line) { return New-Result $name $false 'graphifyy not in uv tool list' }
  $version = ($line -replace '^graphifyy v', '').Trim()
  New-Result $name ($version -eq $pin) "graphifyy $version, pin $pin"
}

function Test-GraphifyCli([string]$versionOutput, [int]$exitCode, [string]$pin) {
  $name = 'graphify cli'
  if ($exitCode -ne 0) { return New-Result $name $false "graphify --version exit $exitCode" }
  $version = ($versionOutput -replace '^graphify\s+', '').Trim()
  New-Result $name ($version -eq $pin) "graphify $version, pin $pin"
}

function Test-EnvValue([string]$variable, [string]$actual, [string]$want) {
  $shown = if ([string]::IsNullOrEmpty($actual)) { 'unset' } else { $actual }
  New-Result "env $variable" ($actual -eq $want) "$shown, want $want"
}

function Test-SkillFolder([string]$storeRoot, [string]$linkRoot, [string]$skill) {
  $inStore = Test-Path (Join-Path $storeRoot "$skill/SKILL.md")
  $inLink = Test-Path (Join-Path $linkRoot "$skill/SKILL.md")
  $reason = "store=$inStore link=$inLink ($storeRoot, $linkRoot)"
  New-Result "skill $skill" ($inStore -and $inLink) $reason
}

function Test-PonytailPlugin([string]$pluginListJson, [string]$pin) {
  $name = 'ponytail plugin'
  $plugin = @($pluginListJson | ConvertFrom-Json | Where-Object { $_.id -eq 'ponytail@ponytail' })[0]
  if (-not $plugin) { return New-Result $name $false 'ponytail@ponytail not installed' }
  $ok = $plugin.version -eq $pin -and $plugin.enabled -and $plugin.scope -eq 'user'
  New-Result $name $ok "version $($plugin.version), pin $pin, scope $($plugin.scope), enabled $($plugin.enabled)"
}

function Test-PonytailMarketplace([string]$marketplaceListJson, [string]$repo, [string]$tag) {
  $name = 'ponytail marketplace'
  $market = @($marketplaceListJson | ConvertFrom-Json | Where-Object { $_.name -eq 'ponytail' })[0]
  if (-not $market) { return New-Result $name $false 'marketplace ponytail not added' }
  $ok = $market.repo -eq $repo -and $market.ref -eq $tag
  New-Result $name $ok "source $($market.repo)#$($market.ref), pin $repo#$tag"
}

function Test-CuratedSkill([string]$storeRoot, [string]$linkRoot, [string]$skill, [string]$commit) {
  $folder = Test-SkillFolder $storeRoot $linkRoot $skill
  $name = "agent-skills $skill"
  if (-not $folder.Pass) { return New-Result $name $false $folder.Reason }
  $source = Join-Path $storeRoot "$skill/SOURCE"
  if (-not (Test-Path $source)) { return New-Result $name $false "no SOURCE in $storeRoot" }
  $ok = (Get-Content -Raw $source) -match "(?m)^commit: $commit\s*$"
  New-Result $name $ok "SOURCE commit pin $commit"
}

# Lifecycle commands (/spec, /plan, /build, /ship) arrive either as command files or with the
# agent-skills Claude Code plugin; both routes compete with OpenSpec.
function Test-NoLifecycleCommand([string]$commandsDir, [string]$pluginListJson) {
  $lifecycle = 'spec.md', 'plan.md', 'build.md', 'ship.md'
  $found = @(Get-ChildItem -LiteralPath $commandsDir -Recurse -File -EA SilentlyContinue |
    Where-Object { $lifecycle -contains $_.Name } | ForEach-Object Name)
  $found += @($pluginListJson | ConvertFrom-Json | Where-Object { $_.id -like 'agent-skills@*' } |
    ForEach-Object id)
  $reason = if ($found) { "found $($found -join ', ')" } else { "none in $commandsDir or plugins" }
  New-Result 'no agent-skills lifecycle commands' (-not $found) $reason
}

function Test-SecurityAudit([string]$storeRoot, [string]$linkRoot, [string]$skill, [string]$pin) {
  $folder = Test-SkillFolder $storeRoot $linkRoot $skill
  $name = "security-audit $skill"
  if (-not $folder.Pass) { return New-Result $name $false $folder.Reason }
  $hash = Get-FolderHash (Join-Path $storeRoot $skill)
  New-Result $name ($hash -eq $pin) "sha256 $hash, pin $pin"
}

function Test-NoToolFilesInRepo([string[]]$porcelainLines) {
  $hits = @($porcelainLines | Where-Object { $_ -match '^.. "?(graphify-out/|\.claude/skills/|\.agents/skills/)' })
  $reason = if ($hits) { "git status lists $($hits.Count) tool path(s)" } else { 'git status clean for tool paths' }
  New-Result 'no tool files in repo' (-not $hits) $reason
}

function Get-ExitCode([object[]]$results) {
  if (-not $results -or @($results | Where-Object { -not $_.Pass }).Count -gt 0) { return 1 }
  0
}

function Get-UserEnv([string]$variable) {
  [Environment]::GetEnvironmentVariable($variable, 'User')
}

function Invoke-AgentDevToolingCheck {
  $pins = Get-Content -Raw (Join-Path $PSScriptRoot 'versions.json') | ConvertFrom-Json
  $store = Join-Path $HOME '.ai-global/skills'
  $link = Join-Path $HOME '.claude/skills'
  $repo = (git -C $PSScriptRoot rev-parse --show-toplevel)

  $versionOutput = (graphify --version 2>$null) -join "`n"
  $graphifyExit = $LASTEXITCODE
  $plugins = (claude plugin list --json 2>$null) -join "`n"
  $results = @(
    Test-GraphifyPackage ((uv tool list 2>$null) -join "`n") $pins.graphify.version
    Test-GraphifyCli $versionOutput $graphifyExit $pins.graphify.version
    Test-EnvValue 'GRAPHIFY_BACKEND' (Get-UserEnv 'GRAPHIFY_BACKEND') 'none'
    Test-EnvValue 'GRAPHIFY_QUERY_LOG_DISABLE' (Get-UserEnv 'GRAPHIFY_QUERY_LOG_DISABLE') '1'
    Test-EnvValue 'GRAPHIFY_NO_TIPS' (Get-UserEnv 'GRAPHIFY_NO_TIPS') '1'
    Test-SkillFolder $store $link 'graphify'
    Test-PonytailPlugin $plugins $pins.ponytail.version
    Test-PonytailMarketplace ((claude plugin marketplace list --json 2>$null) -join "`n") `
      $pins.ponytail.marketplace $pins.ponytail.tag
    Test-EnvValue 'PONYTAIL_DEFAULT_MODE' (Get-UserEnv 'PONYTAIL_DEFAULT_MODE') 'lite'
    foreach ($skill in $pins.agentSkills.skills) {
      Test-CuratedSkill $store $link $skill $pins.agentSkills.commit
    }
    Test-NoLifecycleCommand (Join-Path $HOME '.claude/commands') $plugins
    Test-SecurityAudit $store $link 'security-audit' $pins.securityAudit.sha256
    Test-NoToolFilesInRepo @(git -C $repo status --porcelain --untracked-files=all)
  )
  foreach ($r in $results) {
    Write-Output ('{0} {1}: {2}' -f ($(if ($r.Pass) { 'PASS' } else { 'FAIL' })), $r.Name, $r.Reason)
  }
  exit (Get-ExitCode $results)
}

if ($MyInvocation.InvocationName -ne '.') {
  Invoke-AgentDevToolingCheck
}
