using System.Buffers.Binary;
using System.IO;

namespace Navpaky.Setup.Install {
  // The space Velopack's setup requires before it installs (design D5 of add-install-wizard):
  // its package's compressed plus extracted size plus 50 MB (Velopack 1.2.161,
  // commands/install.rs). The package is a zip inside Setup.exe; Velopack's bundle header gives
  // its offset and length, followed by a fixed signature.
  internal static class SetupPackage {
    public const long Overhead = 50_000_000;

    private const int _chunk = 1 << 20;
    private const int _endRecordLength = 22;
    private const int _entryLength = 46;
    private const uint _endRecordMark = 0x06054b50;
    private const uint _entryMark = 0x02014b50;

    public static ReadOnlySpan<byte> Signature => [
        0x94, 0xf0, 0xb1, 0x7b, 0x68, 0x93, 0xe0, 0x29, 0x37, 0xeb, 0x34, 0xef, 0x53, 0xaa, 0xe7,
        0xd4, 0x2b, 0x54, 0xf5, 0x70, 0x7e, 0xf5, 0xd6, 0xf5, 0x78, 0x54, 0x98, 0x3e, 0x5e, 0x94,
        0xed, 0x7d];

    // 0 when the setup holds no package this code can read.
    public static long RequiredBytes(Stream setup) {
      if (FindHeader(setup) is not { } at) {
        return 0;
      }

      Span<byte> header = stackalloc byte[16];
      setup.Position = at;
      setup.ReadExactly(header);
      long offset = BinaryPrimitives.ReadInt64LittleEndian(header);
      long length = BinaryPrimitives.ReadInt64LittleEndian(header[8..]);
      if (offset <= 0 || length < _endRecordLength || offset + length > setup.Length) {
        return 0;
      }

      return Sizes(setup, offset, length) is { } sizes ? sizes + Overhead : 0;
    }

    // The position of the offset and length that precede the signature.
    private static long? FindHeader(Stream setup) {
      int keep = Signature.Length - 1;
      byte[] buffer = new byte[_chunk + keep];
      int carried = 0;
      long start = 0;
      setup.Position = 0;
      int read;
      while ((read = setup.Read(buffer, carried, _chunk)) > 0) {
        int filled = carried + read;
        int found = buffer.AsSpan(0, filled).IndexOf(Signature);
        if (found >= 0) {
          long at = start + found - 16;
          return at >= 0 ? at : null;
        }

        carried = Math.Min(keep, filled);
        buffer.AsSpan(filled - carried, carried).CopyTo(buffer);
        start += filled - carried;
      }

      return null;
    }

    // The sum of the zip's compressed and extracted sizes, from its central directory.
    private static long? Sizes(Stream setup, long offset, long length) {
      int tail = (int)Math.Min(length, ushort.MaxValue + _endRecordLength);
      byte[] end = new byte[tail];
      setup.Position = offset + length - tail;
      setup.ReadExactly(end);
      int record = tail - _endRecordLength;
      while (record >= 0
          && BinaryPrimitives.ReadUInt32LittleEndian(end.AsSpan(record)) != _endRecordMark) {
        record--;
      }

      if (record < 0) {
        return null;
      }

      uint directoryLength = BinaryPrimitives.ReadUInt32LittleEndian(end.AsSpan(record + 12));
      uint directoryAt = BinaryPrimitives.ReadUInt32LittleEndian(end.AsSpan(record + 16));
      if (directoryAt + (long)directoryLength > length) {
        return null;
      }

      byte[] directory = new byte[directoryLength];
      setup.Position = offset + directoryAt;
      setup.ReadExactly(directory);
      long sum = 0;
      for (int entry = 0; entry + _entryLength <= directory.Length;) {
        ReadOnlySpan<byte> fields = directory.AsSpan(entry);
        if (BinaryPrimitives.ReadUInt32LittleEndian(fields) != _entryMark) {
          return null;
        }

        sum += BinaryPrimitives.ReadUInt32LittleEndian(fields[20..]);
        sum += BinaryPrimitives.ReadUInt32LittleEndian(fields[24..]);
        entry += _entryLength + BinaryPrimitives.ReadUInt16LittleEndian(fields[28..])
            + BinaryPrimitives.ReadUInt16LittleEndian(fields[30..])
            + BinaryPrimitives.ReadUInt16LittleEndian(fields[32..]);
      }

      return sum;
    }
  }
}
