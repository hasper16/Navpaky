using System.Buffers.Binary;
using System.Text;

namespace Rekey.App.Feedback {
  // Reads and builds the plain RIFF PCM waves Rekey ships: a sound's length, and silence in a
  // sound's format to keep monitor audio awake (re-check of fix 2.7, add-final-acceptance).
  internal static class WaveFile {
    private const int _riffHeader = 12;
    private const int _chunkHeader = 8;

    public static TimeSpan Length(byte[] wave) {
      int format = Find(wave, "fmt ");
      int bytesPerSecond = BinaryPrimitives.ReadInt32LittleEndian(wave.AsSpan(format + 8));
      int data = Find(wave, "data");
      int length = BinaryPrimitives.ReadInt32LittleEndian(wave.AsSpan(data - 4));
      return TimeSpan.FromSeconds((double)length / bytesPerSecond);
    }

    // The template's header up to its data, then zero samples.
    public static byte[] Silence(byte[] template, TimeSpan length) {
      int format = Find(template, "fmt ");
      int bytesPerSecond = BinaryPrimitives.ReadInt32LittleEndian(template.AsSpan(format + 8));
      int blockAlign = BinaryPrimitives.ReadInt16LittleEndian(template.AsSpan(format + 12));
      int quiet = (int)(bytesPerSecond * length.TotalSeconds) / blockAlign * blockAlign;

      int data = Find(template, "data");
      byte[] silence = new byte[data + quiet];
      template.AsSpan(0, data).CopyTo(silence);
      BinaryPrimitives.WriteInt32LittleEndian(silence.AsSpan(4), silence.Length - 8);
      BinaryPrimitives.WriteInt32LittleEndian(silence.AsSpan(data - 4), quiet);
      return silence;
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
