using System.Windows;
using Navpaky.App.Views;
using Xunit;

namespace Navpaky.App.Tests.Views {
  // Task 9.4 of add-install-wizard: the welcome window grows downward when step 3 adds rows,
  // which pushed Done below a 1024x768 work area (2026-11-25 VM run).
  public sealed class WorkAreaFitTests {
    private static readonly Rect _small = new(0, 0, 1024, 720);

    [Fact]
    public void AWindowThatFitsStaysWhereItIs() {
      Assert.Equal(40, WorkAreaFit.Top(40, 600, _small));
    }

    [Fact]
    public void AWindowRunningPastTheBottomMovesUp() {
      Assert.Equal(40, WorkAreaFit.Top(115, 680, _small));
    }

    [Fact]
    public void AWindowTallerThanTheWorkAreaStartsAtItsTop() {
      Assert.Equal(0, WorkAreaFit.Top(115, 760, _small));
    }

    [Fact]
    public void TheHeightIsCappedByTheWorkArea() {
      Assert.Equal(720, WorkAreaFit.MaxHeight(760, _small));
      Assert.Equal(760, WorkAreaFit.MaxHeight(760, new Rect(0, 0, 1920, 1040)));
    }
  }
}
