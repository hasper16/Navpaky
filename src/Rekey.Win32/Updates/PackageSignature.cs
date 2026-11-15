using System.IO.Compression;

namespace Rekey.Win32.Updates {
  /// <summary>
  /// Checks that every executable and library in a Velopack package carries a valid Authenticode
  /// signature from the pinned publisher (design D2 of add-auto-update-and-installer).
  /// </summary>
  public static class PackageSignature {
    private const string _appFolder = "lib/app/";

    /// <summary>
    /// Returns the verdict for a downloaded package.
    /// </summary>
    /// <param name="package">The path of the <c>.nupkg</c> file.</param>
    /// <param name="mainExe">The file name of the app's executable, which must be present.</param>
    /// <param name="pin">The publisher every signature must name.</param>
    /// <returns>
    /// <see cref="SignatureVerdict.Valid" /> if every <c>.exe</c> and <c>.dll</c> under
    /// <c>lib/app</c> passes; otherwise, the verdict of the first file that fails, or
    /// <see cref="SignatureVerdict.NotSigned" /> when the package cannot be read or lacks
    /// <paramref name="mainExe" />.
    /// </returns>
    public static SignatureVerdict Verify(string package, string mainExe, PublisherPin pin) {
      // Entries are copied under names of our own, so no entry path decides where a file lands.
      DirectoryInfo folder = Directory.CreateTempSubdirectory("rekey-verify");
      try {
        using ZipArchive zip = ZipFile.OpenRead(package);
        ZipArchiveEntry[] code = [.. zip.Entries.Where(IsCode)];
        if (!code.Any(entry => entry.FullName == _appFolder + mainExe)) {
          return SignatureVerdict.NotSigned;
        }

        for (int i = 0; i < code.Length; i++) {
          string copy = Path.Join(folder.FullName, $"{i}{Path.GetExtension(code[i].Name)}");
          code[i].ExtractToFile(copy);
          SignatureVerdict verdict = SignaturePolicy.Judge(Authenticode.Read(copy), pin);
          if (verdict != SignatureVerdict.Valid) {
            return verdict;
          }
        }

        return SignatureVerdict.Valid;
      } catch (InvalidDataException) {
        return SignatureVerdict.NotSigned;
      } finally {
        folder.Delete(recursive: true);
      }
    }

    private static bool IsCode(ZipArchiveEntry entry) {
      return entry.FullName.StartsWith(_appFolder, StringComparison.Ordinal)
          && (entry.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
              || entry.Name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase));
    }
  }
}
