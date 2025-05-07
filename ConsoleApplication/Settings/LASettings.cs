using ConsoleApplication.Enums;

namespace ConsoleApplication;

public sealed class LASettings
{
  // Selenium browser settings
  public BrowserType Browser { get; set; } = BrowserType.None;
  public string? BrowserEntrySite { get; set; }
  public LAWindowPosition BrowserPosition { get; set; } = new();

  // Source log files settings
  public required string LogFolder { get; set; }
  public required string TimestampFormat { get; set; }
  public required string TimestampFilter { get; set; }
  public required LogLevel LogLevel { get; set; }
  public required int UpdateDelay { get; set; }

  public required List<LAFile> LogFiles { get; set; }

  // Additional settings


}
