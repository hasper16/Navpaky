using Xunit;

namespace Navpaky.Win32.Tests {
  // Re-check of fix 2.7 (add-final-acceptance): one output stream carries silence and the
  // sounds, so monitor audio never sees a new stream start; each chunk takes what is left of
  // the sound and silence after it.
  public sealed class WaveChunksTests {
    [Fact]
    public void ChunkIsSilentWithoutASound() {
      var chunks = new WaveChunks();
      short[] chunk = [9, 9, 9];

      chunks.Fill(chunk);

      Assert.Equal([0, 0, 0], chunk);
    }

    [Fact]
    public void SoundSpreadsOverChunksThenSilenceFollows() {
      var chunks = new WaveChunks();
      chunks.Play([1, 2, 3, 4, 5]);
      short[] first = new short[3];
      short[] second = new short[3];
      short[] third = new short[3];

      chunks.Fill(first);
      chunks.Fill(second);
      chunks.Fill(third);

      Assert.Equal([1, 2, 3], first);
      Assert.Equal([4, 5, 0], second);
      Assert.Equal([0, 0, 0], third);
    }

    [Fact]
    public void NewSoundReplacesTheOnePlaying() {
      var chunks = new WaveChunks();
      chunks.Play([1, 2, 3, 4]);
      short[] first = new short[2];
      chunks.Fill(first);

      chunks.Play([7]);
      short[] second = new short[2];
      chunks.Fill(second);

      Assert.Equal([7, 0], second);
    }
  }
}
