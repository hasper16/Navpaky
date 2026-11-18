# Tasks

## 1. Product identities (data shape first)

- [x] 1.1 Change the identity tests first so they fail: data folders lie outside the install root (`DataFolderTests`), mutex `Local\Navpaky-<SID>` (`SingleInstanceGuard`), Run value `Navpaky` (`SignInLaunchTests`), setup `navpaky-app-win-Setup.exe` (`InstallSourceTests`), export file `navpaky-diagnostics-*.txt`, crash type `Navpaky.Freeze.*`, tray tooltip texts. Then set `Product.Name`, `DataFolder`, `SingleInstanceGuard`, `SignInLaunch`, `InstallSource.SetupName`, `ExportViewModel`, `CrashReports` to the new names. Verify: `dotnet test` on `Rekey.App.Tests` and `Rekey.Win32.Tests` passes.
- [x] 1.2 Release workflow and docs: `--packId navpaky-app`, `--packTitle Navpaky`, `--packAuthors Navpaky`, `--releaseName "Navpaky <version>"`, `--mainExe Navpaky.App.exe`; `VelopackUpdates.Repository` unchanged. Verify: `git grep -n "key-switcher"` lists only the repository URL and its references.

## 2. Internal names

- [x] 2.1 `git mv` the solution, every project folder and file, the icon file and the resource designer to `Navpaky.*`; replace `Rekey`/`rekey` with `Navpaky`/`navpaky` in code, XAML, resx, csproj, slnx, `NativeMethods.txt`, `.editorconfig`, `.gitattributes`, `.githooks/pre-commit`, `.github/workflows/*`; rename `RekeyPublisher` and `RekeyUpdateManager`; point `XamlTextTests` at `Product.Name`. Verify: `dotnet build Navpaky.slnx -warnaserror` is clean and `dotnet test Navpaky.slnx` passes.

## 3. Docs and agent context

- [x] 3.1 Rename the product in `README.md` (with the gloss "Vice-Versa"), `AGENTS.md`, `GLOSSARY.md`, `openspec/config.yaml`, `docs/**` and `data/**/SOURCES.md`; keep every reference to the original macOS app, the notes path `rekey-macos.md` and `hasper16/key-switcher`. Add a by-hand item to `docs/manual-tests/deferred-by-hand.md`: uninstall old test installs, then install and check the install folder, data folder and Installed apps entry. Verify: `git grep -n -i rekey -- . ':!openspec/changes/archive'` lists only references to the original app and the notes path.

## 4. Two data folders (design D7)

- [x] 4.1 Tests first: `DataFoldersTests` pins settings in `%APPDATA%\Navpaky` and local state in `%LOCALAPPDATA%\Navpaky`, and refuses an install root holding either; `UninstallerTests` asks once when either folder exists and passes both; `DataQuestionTests` hands both folders to the question. Then replace `DataFolder` with `DataFolders` and route `SettingsStore` and `WordListsStore` to the settings folder, everything else to the local folder; the question script deletes both only on Yes. Verify: `dotnet test` on `Navpaky.App.Tests` passes.
- [x] 4.2 Desktop checks keep and restore each file in its own folder (`SystemState.SettingsFolder`, `SystemState.LocalFolder`); docs, by-hand checks and README name the right folder per file. Verify: `dotnet build Navpaky.slnx -warnaserror` clean, `dotnet test Navpaky.slnx` passes, `git grep` finds no `%LOCALAPPDATA%\Navpaky` path for `settings.json` or `words.json`.
