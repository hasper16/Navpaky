# Tasks

## 1. Product identities (data shape first)

- [x] 1.1 Change the identity tests first so they fail: data folder `%LOCALAPPDATA%\Navpaky Data` lies outside install root `%LOCALAPPDATA%\navpaky` (`DataFolderTests`), mutex `Local\Navpaky-<SID>` (`SingleInstanceGuard`), Run value `Navpaky` (`SignInLaunchTests`), setup `navpaky-win-Setup.exe` (`InstallSourceTests`), export file `navpaky-diagnostics-*.txt`, crash type `Navpaky.Freeze.*`, tray tooltip texts. Then set `Product.Name`, `DataFolder`, `SingleInstanceGuard`, `SignInLaunch`, `InstallSource.SetupName`, `ExportViewModel`, `CrashReports` to the new names. Verify: `dotnet test` on `Rekey.App.Tests` and `Rekey.Win32.Tests` passes.
- [x] 1.2 Release workflow and docs: `--packId navpaky`, `--packTitle Navpaky`, `--packAuthors Navpaky`, `--releaseName "Navpaky <version>"`, `--mainExe Navpaky.App.exe`; `VelopackUpdates.Repository` unchanged. Verify: `git grep -n "key-switcher"` lists only the repository URL and its references.

## 2. Internal names

- [x] 2.1 `git mv` the solution, every project folder and file, the icon file and the resource designer to `Navpaky.*`; replace `Rekey`/`rekey` with `Navpaky`/`navpaky` in code, XAML, resx, csproj, slnx, `NativeMethods.txt`, `.editorconfig`, `.gitattributes`, `.githooks/pre-commit`, `.github/workflows/*`; rename `RekeyPublisher` and `RekeyUpdateManager`; point `XamlTextTests` at `Product.Name`. Verify: `dotnet build Navpaky.slnx -warnaserror` is clean and `dotnet test Navpaky.slnx` passes.

## 3. Docs and agent context

- [x] 3.1 Rename the product in `README.md` (with the gloss "Vice-Versa"), `AGENTS.md`, `GLOSSARY.md`, `openspec/config.yaml`, `docs/**` and `data/**/SOURCES.md`; keep every reference to the original macOS app, the notes path `rekey-macos.md` and `hasper16/key-switcher`. Add a by-hand item to `docs/manual-tests/deferred-by-hand.md`: uninstall old test installs, then install and check the install folder, data folder and Installed apps entry. Verify: `git grep -n -i rekey -- . ':!openspec/changes/archive'` lists only references to the original app and the notes path.
