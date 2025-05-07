using ConsoleApplication.Events;

namespace ConsoleApplication;

public sealed class LAFile
{
  public required string Label { get; set; }
  public required string Filename { get; set; }
  public required string Path { get; set; }
  public required bool Synchronize { get; set; }
  public string TimestampFormat { get; set; }
}
