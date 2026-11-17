using System.Buffers.Binary;
using System.Text;

namespace Rekey.App.Feedback {
  // Puts silence before a PCM wave sound. Monitor audio over HDMI or DisplayPort wakes up on
  // the first samples after a pause and drops them, so short sounds were lost entirely there
  // (re-check of fix 2.7, add-final-acceptance). Shipped sounds only: they are plain RIFF PCM.
  internal static class WaveLeadIn {
    private const int _riffHeader = 12;
    private const int _chunkHeader = 8;

    public static byte[] Add(byte[] wave, TimeSpan silence) {
      int format = Find(wave, "fmt ");
      int sampleRate = BinaryPrimitives.ReadInt32LittleEndian(wave.AsSpan(format + 4));
      int blockAlign = BinaryPrimitives.ReadInt16LittleEndian(wave.AsSpan(format + 12));
      int quiet = (int)(sampleRate * silence.TotalSeconds) * blockAlign;

      int data = Find(wave, "data");
      byte[] led = new byte[wave.Length + quiet];
      wave.AsSpan(0, data).CopyTo(led);
      wave.AsSpan(data).CopyTo(led.AsSpan(data + quiet));
      BinaryPrimitives.WriteInt32LittleEndian(led.AsSpan(4), led.Length - 8);
      int dataLength = BinaryPrimitives.ReadInt32LittleEndian(wave.AsSpan(data - 4));
      BinaryPrimitives.WriteInt32LittleEndian(led.AsSpan(data - 4), dataLength + quiet);
      return led;
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
