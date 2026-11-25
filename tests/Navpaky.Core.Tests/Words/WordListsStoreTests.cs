using Navpaky.Core.Languages;
using Navpaky.Core.Words;
using Xunit;

namespace Navpaky.Core.Tests.Words {
  // Spec word-learning: lists persist across restarts; design D1: atomic write, and a file
  // Navpaky cannot read is kept aside.
  public sealed class WordListsStoreTests : IDisposable {
    private static readonly DateTimeOffset _monday = new(2026, 11, 16, 9, 0, 0, TimeSpan.Zero);
    private static readonly WordListsDocument _lists = new(
        [new ProtectedWord("ntcn", WordSource.Auto, _monday)],
        [new LearnedWord("кринж", Language.Russian, WordSource.Auto, _monday)]);

    private readonly DirectoryInfo _root = Directory.CreateTempSubdirectory("navpaky-words-");
    private readonly string _folder;

    public WordListsStoreTests() {
      _folder = Path.Join(_root.FullName, "Navpaky");
    }

    public void Dispose() {
      _root.Delete(recursive: true);
    }

    [Fact]
    public void ListsSurviveARestart() {
      new WordListsStore(_folder).Save(_lists);

      WordListsDocument reloaded = new WordListsStore(_folder).Load()!;

      Assert.Equal(_lists.Protected, reloaded.Protected);
      Assert.Equal(_lists.Learned, reloaded.Learned);
    }

    [Fact]
    public void FileIsNamedWordsJson() {
      var store = new WordListsStore(_folder);

      Assert.Equal(Path.Join(_folder, "words.json"), store.FilePath);
    }

    [Fact]
    public void MissingFileGivesEmptyLists() {
      WordListsDocument lists = new WordListsStore(_folder).Load()!;

      Assert.Empty(lists.Protected);
      Assert.Empty(lists.Learned);
    }

    [Fact]
    public void PeekReadsTheStoredLists() {
      new WordListsStore(_folder).Save(_lists);

      Assert.Equal(_lists.Learned, new WordListsStore(_folder).Peek()?.Learned);
    }

    [Fact]
    public void PeekLeavesACorruptFileAsItIs() {
      var store = new WordListsStore(_folder);
      Directory.CreateDirectory(_folder);
      File.WriteAllText(store.FilePath, "{ not json");

      Assert.Null(store.Peek());
      Assert.Equal(["words.json"], Directory.GetFiles(_folder).Select(Path.GetFileName));
    }

    [Fact]
    public void SaveReplacesTheFileAndLeavesNoTemporaryFile() {
      var store = new WordListsStore(_folder);
      store.Save(WordListsDocument.Empty);

      store.Save(_lists);

      Assert.Equal(["words.json"], Directory.GetFiles(_folder).Select(Path.GetFileName));
      Assert.Equal(_lists.Protected, store.Load()!.Protected);
    }

    [Fact]
    public void LockedFileGivesNoListsAndStaysIntact() {
      var store = new WordListsStore(_folder);
      store.Save(_lists);
      string saved = File.ReadAllText(store.FilePath);

      WordListsDocument? lists;
      using (File.Open(store.FilePath, FileMode.Open, FileAccess.Read, FileShare.None)) {
        lists = store.Load();
      }

      Assert.Null(lists);
      Assert.Equal(saved, File.ReadAllText(store.FilePath));
      Assert.False(File.Exists(store.FilePath + ".bak"));
    }

    [Theory]
    [InlineData("{ \"version\": 1, \"protected\": [")]
    [InlineData("null")]
    [InlineData("{ \"version\": 2, \"protected\": [], \"learned\": [] }")]
    [InlineData("{ \"version\": 1, \"protected\": [{ \"text\": \"a\", \"source\": \"x\" }] }")]
    public void CorruptFileGivesEmptyListsAndKeepsItAsBackup(string corrupt) {
      var store = new WordListsStore(_folder);
      Directory.CreateDirectory(_folder);
      File.WriteAllText(store.FilePath, corrupt);

      WordListsDocument lists = store.Load()!;

      Assert.Empty(lists.Protected);
      Assert.Empty(lists.Learned);
      Assert.Equal(corrupt, File.ReadAllText(store.FilePath + ".bak"));
      Assert.False(File.Exists(store.FilePath));
    }
  }
}
