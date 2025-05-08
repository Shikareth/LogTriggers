using ConsoleApplication.Events;

namespace ConsoleApplication;

public sealed class LAFile
{
  public required string Label { get; set; }
  public required string Filename { get; set; }
  public required string Path { get; set; }
  public bool Synchronize { get; set; }
  public string? TimestampFormat { get; set; }
  public string? TimestampFilter { get; set; }
}
