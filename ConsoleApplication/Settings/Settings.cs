using ConsoleApplication.Enums;

namespace ConsoleApplication;

public sealed class Settings
{
  // Selenium browser settings
  public BrowserType Browser { get; set; } = BrowserType.None;
  public string? BrowserEntrySite { get; set; }
  public WindowPosition BrowserPosition { get; set; } = new();

  // Source log files settings
  public required string LogFolder { get; set; }
  public required string ConfigsFolder { get; set; }
  public required string LogFile { get; set; }
  public required LogLevel LogLevel { get; set; }
  public required int UpdateDelay { get; set; }

  // Additional settings
  public required List<string> ConfigurationFiles { get; set; }
}

public sealed class WindowPosition
{
  public int Left { get; set; } = 0;
  public int Top { get; set; } = 0;
  public int Width { get; set; } = 800;
  public int Height { get; set; } = 600;
}

