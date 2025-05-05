using ConsoleApplication.Enums;

namespace ConsoleApplication
{

  public sealed class Settings
  {
    public required BrowserType Browser { get; set; }
    public required string SquadMapSite { get; set; }
    public required string LogFolder { get; set; }
    public required string ConfigsFolder { get; set; }
    public required string LogFile { get; set; }
    public required LogLevel LogLevel { get; set; }
    public required int UpdateDelay { get; set; }
    public required List<string> ConfigurationFiles { get; set; }
  }
}
