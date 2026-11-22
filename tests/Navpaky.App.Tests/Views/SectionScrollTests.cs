using Navpaky.App.Views;
using Xunit;

namespace Navpaky.App.Tests.Views {
  public sealed class SectionScrollTests {
    private static readonly double[] _tops = [0, 300, 700, 1000];

    [Fact]
    public void AtTheTopTheFirstSectionIsInView() {
      Assert.Equal(0, SectionScroll.InView(_tops, offset: 0, viewport: 500, extent: 1600));
    }

    [Fact]
    public void TheSectionWhoseHeadingReachedTheTopIsInView() {
      Assert.Equal(2, SectionScroll.InView(_tops, offset: 690, viewport: 500, extent: 1600));
    }

    [Fact]
    public void AtTheEndTheLastSectionIsInView() {
      Assert.Equal(3, SectionScroll.InView(_tops, offset: 1100, viewport: 500, extent: 1600));
    }
  }
}
