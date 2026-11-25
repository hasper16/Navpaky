# Tasks

## 1. Folder data (design D1, D2)

- [x] 1.1 Add `IReadOnlyList<string> Extras` to `FolderCheck` and `Entries(folder)` to `IFolderProbe`, plus the in-memory probe in the tests. Verify: `dotnet build` passes with warnings as errors.
- [x] 1.2 Test first: for a folder holding a Navpaky install, `InstallTarget.Check` lists the top-level entries other than `current`, `packages`, `Update.exe` and `Navpaky.exe`. The comparison ignores case and the list is sorted. A folder with only the install lists nothing, and a folder that can't be listed is NotWritable. Verify: `dotnet test tests/Navpaky.Setup.Tests`.
- [x] 1.3 Test first: `DiskFolders.Entries` returns the top-level names of a real temporary folder, and `null` for a folder it can't list. Verify: `dotnet test tests/Navpaky.Setup.Tests`.

## 2. Confirmation in the wizard (design D3)

- [ ] 2.1 Test first: on the Folder page with extras, Next is disabled until `ExtrasConfirmed` is set. The confirmation resets when another folder is picked. Without extras, Next follows the verdict alone. Verify: `dotnet test tests/Navpaky.Setup.Tests`.
- [ ] 2.2 Add the warning, at most 8 names, the "and N more" line and the "Delete them and reinstall" check box to the Folder page, with English and Ukrainian texts. The window test checks the names shown, the more line and the check box for 12 extras, and that none of it shows without extras. Verify: `dotnet test tests/Navpaky.Setup.Tests`.

## 3. Test cleanup (design D5)

- [ ] 3.1 `SetupRunnerTests.Dispose` retries the temporary-folder delete on `IOException`, up to 10 times, 200 ms apart. Verify: `dotnet test tests/Navpaky.Setup.Tests --filter SetupRunnerTests` passes 10 runs in a row.

## 4. Checks and docs

- [ ] 4.1 Add a "Reinstall with extra files" item to `docs/manual-tests/deferred-by-hand.md` and run it in the NavpakyLab VM. With a file and a folder added to the existing install's folder, the Folder page lists them and Next stays off until the box is checked. Closing then keeps both; confirming and installing deletes them and installs. Verify: the item is ticked with the run's date.
- [ ] 4.2 Run the ponytail review of the branch and the `security-audit` review in guidance mode (area: installer). Record the review in the PR body and fix or defer each finding. Verify: the PR body has the `## Security review` section with the reviewed commit.

## Workflow follow-up

- Archive the change on `chore/archive-warn-reinstall-extra-files` after the PR is merged.
