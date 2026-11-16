using System.ComponentModel;
using Rekey.App.Localization;
using Rekey.App.Telemetry;
using Rekey.App.ViewModels;

namespace Rekey.App.Views {
  // Diagnostics and privacy: the watchdogs, statistics, the word-analysis log, the diagnostics
  // export, telemetry consent with what was sent, and the email sign-up.
  internal sealed partial class DiagnosticsPage {
    private readonly SignUpViewModel _signUp;

    public DiagnosticsPage(
        SettingsViewModel model,
        SignUpViewModel signUp,
        Func<IReadOnlyList<string>> readSent,
        Action openStatistics,
        Action openExport) {
      InitializeComponent();
      DataContext = model;
      _signUp = signUp;
      SignUp.DataContext = signUp;
      StatisticsButton.Click += (_, _) => openStatistics();
      ExportButton.Click += (_, _) => openExport();

      // Read on each expand, so the view shows the requests exactly as they were sent.
      SentExpander.Expanded += (_, _) => ShowSent(readSent());
      SignUpButton.Click += async (_, _) => await signUp.SubmitAsync();

      // The settings window unloads this page when another section shows and loads it again.
      Loaded += (_, _) => {
        signUp.PropertyChanged += OnSignUpChanged;
        ShowSignUpState();
      };
      Unloaded += (_, _) => signUp.PropertyChanged -= OnSignUpChanged;
      Localizer.Follow(this, () => {
        ShowSignUpState();
        if (SentExpander.IsExpanded) {
          ShowSent(readSent());
        }
      });
    }

    private void OnSignUpChanged(object? sender, PropertyChangedEventArgs e) {
      if (e.PropertyName == nameof(SignUpViewModel.State)) {
        ShowSignUpState();
      }
    }

    private void ShowSent(IReadOnlyList<string> sent) {
      SentText.Text = sent.Count == 0
          ? Strings.NothingSentYet
          : string.Join(Environment.NewLine + Environment.NewLine, sent);
    }

    private void ShowSignUpState() {
      SignUpButton.IsEnabled = _signUp.State != SignUpState.Sending;
      SignUpStatus.Text = _signUp.State switch {
        SignUpState.Invalid => Strings.SignUpInvalid,
        SignUpState.Sending => Strings.SignUpSending,
        SignUpState.Sent => Strings.SignUpSent,
        SignUpState.Failed => Strings.SignUpFailed,
        _ => string.Empty,
      };
    }
  }
}
