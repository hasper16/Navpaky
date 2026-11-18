using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

namespace Rekey.App.Feedback {
  // Reads the plain RIFF PCM waves Rekey ships (16-bit mono): their sample rate, samples and
  // length, so the sounds can be written into one open output stream (re-check of fix 2.7,
  // add-final-acceptance).
  internal static class WaveFile {
    private const int _riffHeader = 12;
    private const int _chunkHeader = 8;

    public static int SampleRate(byte[] wave) {
      return BinaryPrimitives.ReadInt32LittleEndian(wave.AsSpan(Find(wave, "fmt ") + 4));
    }

    public static short[] Samples(byte[] wave) {
      int data = Find(wave, "data");
      int length = BinaryPrimitives.ReadInt32LittleEndian(wave.AsSpan(data - 4));
      return MemoryMarshal.Cast<byte, short>(wave.AsSpan(data, length)).ToArray();
    }

    public static TimeSpan Length(byte[] wave) {
      return TimeSpan.FromSeconds((double)Samples(wave).Length / SampleRate(wave));
    }

    // Returns where a chunk's content starts.
    private static int Find(byte[] wave, string id) {
      int at = _riffHeader;
      while (Encoding.ASCII.GetString(wave, at, 4) != id) {
        int size = BinaryPrimitives.ReadInt32LittleEndian(wave.AsSpan(at + 4));
        at += _chunkHeader + size + (size & 1);
      }

      return at + _chunkHeader;
    }
  }
}
