using System.Diagnostics.CodeAnalysis;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Media.Audio;

namespace Rekey.Win32 {
  /// <summary>
  /// A short wave sound held in memory that plays without blocking the caller.
  /// </summary>
  /// <remarks>
  /// Windows reads an asynchronous in-memory sound while it plays, after the call returns, so
  /// the bytes live in an array the garbage collector never moves. .NET's SoundPlayer pins its
  /// bytes only during the call: short sounds then played silence or nothing (fix 2.7 of
  /// add-final-acceptance).
  /// </remarks>
  [SuppressMessage(
      "Security",
      "S6640:Avoid using unsafe code blocks",
      Justification = "PlaySound takes the address of the pinned wave bytes.")]
  public sealed unsafe class WaveSound {
    private readonly byte[] _wave;

    /// <summary>
    /// Initializes a new instance of the <see cref="WaveSound" /> class.
    /// </summary>
    /// <param name="wave">The bytes of a RIFF wave file.</param>
    public WaveSound(ReadOnlySpan<byte> wave) {
      _wave = GC.AllocateUninitializedArray<byte>(wave.Length, pinned: true);
      wave.CopyTo(_wave);
    }

    /// <summary>Starts playing the sound, stopping any sound this process plays.</summary>
    public void Play() {
      fixed (byte* bytes = _wave) {
        PInvoke.PlaySound(
            new PCWSTR((char*)bytes),
            HMODULE.Null,
            SND_FLAGS.SND_MEMORY | SND_FLAGS.SND_ASYNC | SND_FLAGS.SND_NODEFAULT);
      }
    }
  }
}
