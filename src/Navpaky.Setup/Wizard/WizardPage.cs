namespace Navpaky.Setup.Wizard {
  // The wizard's pages in their order (spec install-wizard); WelcomeBack only when data is kept.
  internal enum WizardPage {
    Language,
    Licence,
    WelcomeBack,
    Folder,
    Options,
    Installing,
    Done,
  }
}
