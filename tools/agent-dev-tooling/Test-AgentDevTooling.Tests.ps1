# Case table for Test-AgentDevTooling.ps1. Run: pwsh -File tools/agent-dev-tooling/Test-AgentDevTooling.Tests.ps1
# Each check gets fake inputs; the case states whether it must pass. No Pester dependency.
. (Join-Path $PSScriptRoot 'Test-AgentDevTooling.ps1')

$tmp = Join-Path ([IO.Path]::GetTempPath()) "adt-tests-$PID"
$store = Join-Path $tmp 'store'
$link = Join-Path $tmp 'link'
$commands = Join-Path $tmp 'commands'
$commit = '39b723ceebf81474d0557c1fecd01eddab585563'

function New-Skill([string]$root, [string]$name, [string]$source) {
  $dir = Join-Path $root $name
  New-Item -ItemType Directory -Force $dir | Out-Null
  Set-Content (Join-Path $dir 'SKILL.md') "# $name"
  if ($source) { Set-Content (Join-Path $dir 'SOURCE') $source }
}

New-Item -ItemType Directory -Force $store, $link, $commands | Out-Null
foreach ($root in $store, $link) {
  New-Skill $root 'graphify' ''
  New-Skill $root 'good-skill' "repo: x`ncommit: $commit"
  New-Skill $root 'old-skill' "repo: x`ncommit: 0000000"
  New-Skill $root 'no-source' ''
}
New-Skill $store 'store-only' "commit: $commit"
New-Skill $store 'audit' ''
Set-Content (Join-Path $store 'audit\extra.md') 'rule'
$auditHash = Get-FolderHash (Join-Path $store 'audit')
$auditCopy = Join-Path $tmp 'audit-copy'
Copy-Item -Recurse (Join-Path $store 'audit') $auditCopy
[IO.File]::AppendAllText((Join-Path $auditCopy 'extra.md'), 'x')
$cmdsWithShip = Join-Path $tmp 'commands-ship'
New-Item -ItemType Directory -Force (Join-Path $commands 'opsx'), $cmdsWithShip | Out-Null
Set-Content (Join-Path $commands 'opsx\apply.md') 'apply'
Set-Content (Join-Path $cmdsWithShip 'ship.md') 'ship'

$plugins = '[{"id":"ponytail@ponytail","version":"5.3.3","scope":"user","enabled":true}]'
$pluginsOld = '[{"id":"ponytail@ponytail","version":"5.3.2","scope":"user","enabled":true}]'
$pluginsOff = '[{"id":"ponytail@ponytail","version":"5.3.3","scope":"user","enabled":false}]'
$market = '[{"name":"ponytail","source":"github","repo":"DietrichGebert/ponytail","ref":"v5.3.3"}]'
$marketNoRef = '[{"name":"ponytail","source":"github","repo":"DietrichGebert/ponytail"}]'
$marketFork = '[{"name":"ponytail","source":"github","repo":"someone/ponytail","ref":"v5.3.3"}]'

$cases = @(
  @{ n = 'graphifyy at pin'; want = $true
     r = { Test-GraphifyPackage "graphifyy v0.9.109`n- graphify.exe" '0.9.109' } },
  @{ n = 'graphifyy other version'; want = $false
     r = { Test-GraphifyPackage "graphifyy v0.9.108`n- graphify.exe" '0.9.109' } },
  @{ n = 'look-alike package graphify'; want = $false
     r = { Test-GraphifyPackage "graphify v0.9.109`n- graphify.exe" '0.9.109' } },
  @{ n = 'graphify not installed'; want = $false; r = { Test-GraphifyPackage '' '0.9.109' } },
  @{ n = 'graphify --version at pin'; want = $true; r = { Test-GraphifyCli 'graphify 0.9.109' 0 '0.9.109' } },
  @{ n = 'graphify --version mismatch'; want = $false; r = { Test-GraphifyCli 'graphify 0.9.1' 0 '0.9.109' } },
  @{ n = 'graphify --version exit 1'; want = $false; r = { Test-GraphifyCli 'graphify 0.9.109' 1 '0.9.109' } },
  @{ n = 'backend none'; want = $true; r = { Test-EnvValue 'GRAPHIFY_BACKEND' 'none' 'none' } },
  @{ n = 'backend missing'; want = $false; r = { Test-EnvValue 'GRAPHIFY_BACKEND' $null 'none' } },
  @{ n = 'backend openai'; want = $false; r = { Test-EnvValue 'GRAPHIFY_BACKEND' 'openai' 'none' } },
  @{ n = 'query log off'; want = $true; r = { Test-EnvValue 'GRAPHIFY_QUERY_LOG_DISABLE' '1' '1' } },
  @{ n = 'query log missing'; want = $false; r = { Test-EnvValue 'GRAPHIFY_QUERY_LOG_DISABLE' '' '1' } },
  @{ n = 'query log 0'; want = $false; r = { Test-EnvValue 'GRAPHIFY_QUERY_LOG_DISABLE' '0' '1' } },
  @{ n = 'ponytail lite'; want = $true; r = { Test-EnvValue 'PONYTAIL_DEFAULT_MODE' 'lite' 'lite' } },
  @{ n = 'ponytail full'; want = $false; r = { Test-EnvValue 'PONYTAIL_DEFAULT_MODE' 'full' 'lite' } },
  @{ n = 'skill in store and link'; want = $true; r = { Test-SkillFolder $store $link 'graphify' } },
  @{ n = 'skill missing'; want = $false; r = { Test-SkillFolder $store $link 'absent' } },
  @{ n = 'skill not through link'; want = $false; r = { Test-SkillFolder $store $link 'store-only' } },
  @{ n = 'ponytail plugin at pin'; want = $true; r = { Test-PonytailPlugin $plugins '5.3.3' } },
  @{ n = 'ponytail plugin old'; want = $false; r = { Test-PonytailPlugin $pluginsOld '5.3.3' } },
  @{ n = 'ponytail plugin disabled'; want = $false; r = { Test-PonytailPlugin $pluginsOff '5.3.3' } },
  @{ n = 'ponytail plugin absent'; want = $false; r = { Test-PonytailPlugin '[]' '5.3.3' } },
  @{ n = 'marketplace at tag'; want = $true
     r = { Test-PonytailMarketplace $market 'DietrichGebert/ponytail' 'v5.3.3' } },
  @{ n = 'marketplace without tag'; want = $false
     r = { Test-PonytailMarketplace $marketNoRef 'DietrichGebert/ponytail' 'v5.3.3' } },
  @{ n = 'marketplace from a fork'; want = $false
     r = { Test-PonytailMarketplace $marketFork 'DietrichGebert/ponytail' 'v5.3.3' } },
  @{ n = 'curated skill at commit'; want = $true; r = { Test-CuratedSkill $store $link 'good-skill' $commit } },
  @{ n = 'curated skill missing'; want = $false; r = { Test-CuratedSkill $store $link 'absent' $commit } },
  @{ n = 'curated skill no SOURCE'; want = $false; r = { Test-CuratedSkill $store $link 'no-source' $commit } },
  @{ n = 'curated skill old commit'; want = $false; r = { Test-CuratedSkill $store $link 'old-skill' $commit } },
  @{ n = 'no lifecycle commands'; want = $true; r = { Test-NoLifecycleCommand $commands } },
  @{ n = 'lifecycle command present'; want = $false; r = { Test-NoLifecycleCommand $cmdsWithShip } },
  @{ n = 'audit hash match'; want = $true
     r = { Test-SecurityAudit $store $store 'audit' $auditHash } },
  @{ n = 'audit hash after one-byte change'; want = $false
     r = { Test-SecurityAudit $tmp $tmp 'audit-copy' $auditHash } },
  @{ n = 'audit folder missing'; want = $false; r = { Test-SecurityAudit $store $link 'absent' $auditHash } },
  @{ n = 'clean repo'; want = $true; r = { Test-NoToolFilesInRepo @(' M AGENTS.md') } },
  @{ n = 'graph output untracked'; want = $false; r = { Test-NoToolFilesInRepo @('?? graphify-out/graph.json') } },
  @{ n = 'project skill untracked'; want = $false; r = { Test-NoToolFilesInRepo @('?? .agents/skills/graphify/') } },
  @{ n = 'project claude skill'; want = $false; r = { Test-NoToolFilesInRepo @('?? .claude/skills/x/SKILL.md') } }
)

$failures = 0
foreach ($t in $cases) {
  try {
    $result = & $t.r
    $ok = ($result.Pass -eq $t.want) -and $result.Name -and $result.Reason
  } catch {
    $ok = $false
    $result = @{ Reason = $_.Exception.Message }
  }
  if (-not $ok) {
    $failures++
    Write-Host "FAIL $($t.n): want pass=$($t.want), got $($result.Pass) ($($result.Reason))"
  }
}

$aggregate = @(
  @{ want = 0; results = @([pscustomobject]@{ Pass = $true }, [pscustomobject]@{ Pass = $true }) },
  @{ want = 1; results = @([pscustomobject]@{ Pass = $true }, [pscustomobject]@{ Pass = $false }) },
  @{ want = 1; results = @() }
)
foreach ($a in $aggregate) {
  try { $code = Get-ExitCode $a.results } catch { $code = 'error' }
  if ($code -ne $a.want) {
    $failures++
    Write-Host "FAIL exit code: want $($a.want), got $code"
  }
}

Remove-Item -Recurse -Force $tmp
$total = $cases.Count + $aggregate.Count
Write-Host "$($total - $failures)/$total passed"
exit [int]($failures -gt 0)
