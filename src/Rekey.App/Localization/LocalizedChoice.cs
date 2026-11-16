namespace Rekey.App.Localization {
  // A list entry whose text is the UI string Key; shown with the LocalizedChoice template, so it
  // follows the interface language, also in a combo box's selection box.
  internal sealed record LocalizedChoice(object Value, string Key);
}
