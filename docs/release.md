# Releasing Navpaky

Navpaky ships as a per-user Velopack installer on the GitHub Releases of `hasper16/key-switcher`. Installed copies check that feed, download a newer full package, and apply it only when every executable in it carries a valid Authenticode signature from the pinned Navpaky publisher (spec `auto-update`, design of `add-auto-update-and-installer`).

## One-time setup

1. **Code-signing certificate.** Choose a certificate whose key stays in hardware or a cloud signing service (for example an open-source code-signing certificate on a token, or Azure Trusted Signing). Never commit a certificate, key or password; `.githooks/pre-commit` blocks `.pfx`, `.pem` and `.key` files.
2. **Publisher pin.** Read the subject and the issuing CA of a file signed with that certificate:
   ```powershell
   $sig = Get-AuthenticodeSignature .\signed-file.exe
   $sig.SignerCertificate.Subject
   $chain = [Security.Cryptography.X509Certificates.X509Chain]::new()
   $null = $chain.Build($sig.SignerCertificate)
   $chain.ChainElements[1].Certificate.Thumbprint
   ```
   Put both into `NavpakyPublisher.Pin` in `src/Navpaky.App/Updates/NavpakyPublisher.cs` (an OpenSpec change, since it decides which updates users accept). List every CA thumbprint the provider may issue from; renewing with another CA needs a release that adds it first. Until the pin is set, Navpaky refuses every update.
3. **Release environment.** In the repository settings create the environment `release`, limit its deployment tags to `v*`, and optionally require a reviewer. Add the secret `VPK_SIGN_PARAMS`: the signtool arguments `vpk pack --signParams` passes, such as `/fd sha256 /tr http://timestamp.digicert.com /td sha256 /sha1 <thumbprint>` for a certificate on the runner, or the `/dlib` and `/dmdf` arguments of a cloud signing client. A provider that needs a client library or a sign-in step adds that step to `release.yml` before "Pack", reading its credentials from the same environment.

Pull-request workflows never see the secret: `release.yml` has no `pull_request` trigger, and only tag runs enter the `release` environment.

## Publishing a release

1. Merge everything into `main` and wait for CI.
2. Tag the commit and push the tag (the owner does this):
   ```bash
   git tag v0.2.0
   git push origin v0.2.0
   ```
3. `release.yml` builds, tests, publishes a self-contained single-file `win-x64` build, signs and packs it with `vpk pack`, and uploads it with `vpk upload github --publish`. A version with a suffix (`v0.2.0-beta.1`) becomes a pre-release, which installed copies ignore.
4. Check the release has `navpaky-app-win-Setup.exe`, `navpaky-app-<version>-full.nupkg` and `releases.win.json`.

The run fails instead of publishing when `VPK_SIGN_PARAMS` is missing.

## Dry run

From the Actions tab run **Release** with **Run workflow** and a version. It packs without signing, publishes nothing and keeps the packages as a run artifact. Use it to check the pipeline, and to see that a run outside the `release` environment gets no signing secret (its log says "No signing parameters provided").

## Local test build

For the by-hand checks in `docs/manual-tests/deferred-by-hand.md`. Install the packer once: `dotnet tool install vpk --version 1.2.161 --tool-path .tools`.

1. Create a test CA and a code-signing certificate in your user store, and trust the CA for your user only:
   ```powershell
   $ca = New-SelfSignedCertificate -Type Custom -Subject 'CN=Navpaky Test CA' -KeyUsage CertSign -TextExtension '2.5.29.19={text}CA=true' -CertStoreLocation Cert:\CurrentUser\My
   $leaf = New-SelfSignedCertificate -Subject 'CN=Navpaky Test Publisher' -Type CodeSigningCert -Signer $ca -CertStoreLocation Cert:\CurrentUser\My
   Export-Certificate -Cert $ca -FilePath $env:TEMP\navpaky-test-ca.cer
   Import-Certificate -FilePath $env:TEMP\navpaky-test-ca.cer -CertStoreLocation Cert:\CurrentUser\Root
   $leaf.Thumbprint; $ca.Thumbprint
   ```
   A test CA publishes no revocation list, so Debug builds skip the revocation check for a package from `--update-feed`; Release builds always check it.
2. Publish and pack a Debug build into one feed folder (here `%USERPROFILE%\NavpakyFeed`: any folder your account can write works, but not the `C:\` root, which a standard user cannot write), signing with the test certificate. Delete the `publish` folder before each version, so the second pack holds no files of the first:
   ```powershell
   dotnet publish src/Navpaky.App -c Debug -r win-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:Version=0.1.0 -o publish
   .tools\vpk pack --packId navpaky-app --packVersion 0.1.0 --packDir publish --mainExe Navpaky.App.exe --packTitle Navpaky --packAuthors Navpaky --icon src\Navpaky.App\Navpaky.ico --runtime win-x64 --delta None --outputDir $env:USERPROFILE\NavpakyFeed --signParams "/fd sha256 /sha1 <leaf thumbprint>"
   ```
   Repeat with `0.2.0` for an update.
3. Install with `%USERPROFILE%\NavpakyFeed\navpaky-app-win-Setup.exe` from the 0.1.0 pack: packing 0.2.0 replaces it, so first copy it out, for example to `%USERPROFILE%\NavpakySetup-0.1.0.exe`. To check updates, quit Navpaky and start `%LOCALAPPDATA%\navpaky-app\current\Navpaky.App.exe --update-feed %USERPROFILE%\NavpakyFeed --update-pin "CN=Navpaky Test Publisher" <CA thumbprint>`.
4. Afterwards uninstall Navpaky and remove both test certificates from `Cert:\CurrentUser\My` and the CA from `Cert:\CurrentUser\Root`.
