using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Media.Audio;

namespace Rekey.Win32 {
  /// <summary>
  /// One open output stream that plays silence and the sounds given to it, so the audio device
  /// never sees a new stream start for a sound.
  /// </summary>
  /// <remarks>
  /// Monitor audio over DisplayPort or HDMI drops the start of a stream; with one stream kept
  /// open, a sound is only new samples in it (re-check of fix 2.7, add-final-acceptance). A
  /// thread of its own keeps three 40 ms buffers queued with waveOut. Any thread may call the
  /// members.
  /// </remarks>
  [SuppressMessage(
      "Security",
      "S6640:Avoid using unsafe code blocks",
      Justification = "waveOut reads the buffers and headers in place, from native memory.")]
  public sealed unsafe class SoundOutput : IDisposable {
    private const int _buffers = 3;
    private const int _bufferMilliseconds = 40;
    private static readonly TimeSpan _stopWait = TimeSpan.FromSeconds(2);

    private readonly int _sampleRate;
    private readonly WaveChunks _chunks = new();
    private readonly AutoResetEvent _done = new(false);
    private readonly Thread _thread;
    private bool _wanted;
    private long _buffersQueued;
    private long _opens;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="SoundOutput" /> class for 16-bit mono
    /// sound, closed until <see cref="Open" />.
    /// </summary>
    /// <param name="sampleRate">The samples per second of every sound it plays.</param>
    public SoundOutput(int sampleRate) {
      _sampleRate = sampleRate;
      _thread = new Thread(Run) { IsBackground = true, Name = "Rekey sound stream" };
      _thread.Start();
    }

    /// <summary>Gets the number of 40 ms buffers handed to the device so far.</summary>
    public long BuffersQueued => Interlocked.Read(ref _buffersQueued);

    /// <summary>Gets the number of times the device was opened.</summary>
    public long Opens => Interlocked.Read(ref _opens);

    /// <summary>Keeps the stream open, playing silence between sounds.</summary>
    public void Open() {
      Volatile.Write(ref _wanted, true);
      _done.Set();
    }

    /// <summary>Lets the stream close once the sound playing is over.</summary>
    public void Close() {
      Volatile.Write(ref _wanted, false);
      _done.Set();
    }

    /// <summary>Plays a sound in the stream, opening it if it is closed.</summary>
    /// <param name="samples">The sound's 16-bit samples.</param>
    public void Play(short[] samples) {
      _chunks.Play(samples);
      _done.Set();
    }

    /// <inheritdoc />
    public void Dispose() {
      Volatile.Write(ref _disposed, true);
      _done.Set();
      if (_thread.Join(_stopWait)) {
        _done.Dispose();
      }
    }

    private void Run() {
      while (!Volatile.Read(ref _disposed)) {
        _done.WaitOne();
        if (!Volatile.Read(ref _disposed) && (Volatile.Read(ref _wanted) || _chunks.Busy)) {
          PlayWhileWanted();
        }
      }
    }

    // Opens the device, keeps the buffers queued until nothing is wanted or playing, then
    // closes it. Runs on the stream's thread only.
    [SuppressMessage(
        "Reliability",
        "S3869:SafeHandle.DangerousGetHandle should not be called",
        Justification = "Dispose joins this thread before it disposes the event.")]
    private void PlayWhileWanted() {
      int samples = _sampleRate * _bufferMilliseconds / 1000;
      var format = new WAVEFORMATEX {
        wFormatTag = (ushort)PInvoke.WAVE_FORMAT_PCM,
        nChannels = 1,
        nSamplesPerSec = (uint)_sampleRate,
        nAvgBytesPerSec = (uint)(_sampleRate * 2),
        nBlockAlign = 2,
        wBitsPerSample = 16,
      };
      HWAVEOUT device;
      uint opened = PInvoke.waveOutOpen(
          &device,
          PInvoke.WAVE_MAPPER,
          &format,
          (nuint)_done.SafeWaitHandle.DangerousGetHandle(),
          0,
          MIDI_WAVE_OPEN_TYPE.CALLBACK_EVENT);
      if (opened != 0) {
        return;
      }

      Interlocked.Increment(ref _opens);

      var headers = (WAVEHDR*)NativeMemory.AllocZeroed((nuint)(_buffers * sizeof(WAVEHDR)));
      short* audio = (short*)NativeMemory.AllocZeroed((nuint)(_buffers * samples * 2));
      try {
        for (int i = 0; i < _buffers; i++) {
          headers[i].lpData = new PSTR((byte*)(audio + (i * samples)));
          headers[i].dwBufferLength = (uint)(samples * 2);
          _ = PInvoke.waveOutPrepareHeader(device, &headers[i], (uint)sizeof(WAVEHDR));
          Queue(device, &headers[i], audio + (i * samples), samples);
        }

        bool queued = true;
        while (queued && !Volatile.Read(ref _disposed)) {
          _done.WaitOne(_bufferMilliseconds * 2);
          bool keep = Volatile.Read(ref _wanted) || _chunks.Busy;
          queued = false;
          for (int i = 0; i < _buffers; i++) {
            bool finished = (headers[i].dwFlags & PInvoke.WHDR_DONE) != 0;
            if (finished && keep) {
              Queue(device, &headers[i], audio + (i * samples), samples);
            }

            queued |= !finished || keep;
          }
        }
      } finally {
        _ = PInvoke.waveOutReset(device);
        for (int i = 0; i < _buffers; i++) {
          _ = PInvoke.waveOutUnprepareHeader(device, &headers[i], (uint)sizeof(WAVEHDR));
        }

        _ = PInvoke.waveOutClose(device);
        NativeMemory.Free(audio);
        NativeMemory.Free(headers);
      }
    }

    private void Queue(HWAVEOUT device, WAVEHDR* header, short* buffer, int samples) {
      _chunks.Fill(new Span<short>(buffer, samples));
      header->dwFlags &= ~PInvoke.WHDR_DONE;
      _ = PInvoke.waveOutWrite(device, header, (uint)sizeof(WAVEHDR));
      Interlocked.Increment(ref _buffersQueued);
    }
  }
}
