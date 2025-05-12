using LogAnalyzer.Enums;
using LogAnalyzer.File;

namespace LogAnalyzer.Settings;

public sealed class LASettings
{
  // Selenium browser settings
  public BrowserType Browser { get; set; } = BrowserType.None;
  public string BrowserEntrySite { get; set; } = string.Empty;
  public LAWindowPosition BrowserPosition { get; set; } = new();

  // Source log files settings
  public required string LogFolder { get; set; } = Environment.CurrentDirectory;
  public required string TimestampFormat { get; set; } = string.Empty;
  public required string TimestampFilter { get; set; } = string.Empty;
  public required LogLevel LogLevel { get; set; } = LogLevel.INFO;
  public required int UpdateDelay { get; set; } = 1000;

  public required List<LAFile> LogFiles { get; set; } = [];

  // Additional settings
  [System.Diagnostics.CodeAnalysis.SetsRequiredMembersAttribute]
  public LASettings()
  {

  }
}
