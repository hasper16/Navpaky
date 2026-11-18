namespace Navpaky.Win32 {
  /// <summary>
  /// Cuts the sound to play into the chunks an open output stream asks for, with silence when
  /// no sound is left.
  /// </summary>
  /// <remarks>
  /// Any thread may call <see cref="Play" />; only the stream's thread calls
  /// <see cref="Fill" />. The newest sound replaces one still playing.
  /// </remarks>
  internal sealed class WaveChunks {
    // Written by Play; Fill takes it and owns what it took.
    private short[]? _next;
    private short[] _playing = [];
    private int _position;

    /// <summary>Gets a value indicating whether a sound is waiting or playing.</summary>
    public bool Busy => Volatile.Read(ref _next) is not null || _position < _playing.Length;

    /// <summary>Plays a sound from the next chunk on.</summary>
    /// <param name="samples">The sound's 16-bit samples.</param>
    public void Play(short[] samples) {
      Volatile.Write(ref _next, samples);
    }

    /// <summary>Fills a chunk with what is left of the sound, then silence.</summary>
    /// <param name="chunk">The chunk to fill.</param>
    public void Fill(Span<short> chunk) {
      if (Interlocked.Exchange(ref _next, null) is { } next) {
        _playing = next;
        _position = 0;
      }

      int taken = Math.Min(chunk.Length, _playing.Length - _position);
      _playing.AsSpan(_position, taken).CopyTo(chunk);
      chunk[taken..].Clear();
      _position += taken;
    }
  }
}
