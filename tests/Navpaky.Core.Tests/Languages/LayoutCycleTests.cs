using Navpaky.Core.Languages;
using Xunit;

namespace Navpaky.Core.Tests.Languages {
  public sealed class LayoutCycleTests {
    private static readonly LayoutId _english = new(0x00000409);
    private static readonly LayoutId _ukrainian = new(0x00000422);
    private static readonly LayoutId _russian = new(0x00000419);
    private static readonly LayoutId[] _installed = [_english, _ukrainian, _russian];

    [Fact]
    public void NextFollowsTheInstalledOrder() {
      Assert.Equal(_ukrainian, LayoutCycle.Next(_installed, _english));
    }

    [Fact]
    public void NextWrapsAfterTheLast() {
      Assert.Equal(_english, LayoutCycle.Next(_installed, _russian));
    }

    [Fact]
    public void UnknownCurrentStartsAtTheFirst() {
      Assert.Equal(_english, LayoutCycle.Next(_installed, null));
    }

    [Fact]
    public void NoLayoutsGiveNone() {
      Assert.Null(LayoutCycle.Next([], _english));
    }
  }
}
