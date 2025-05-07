using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ConsoleApplication;

public class LAFileReader
{
  private FileStream? FileStream { get; set; }
  private StreamReader? StreamReader { get; set; }
  private string? FullPath { get; set; }

  public LAFile FileInfo { get; private set; }
  public string? CurrentLine { get; set; }
  public DateTime? CurrentTimestamp { get; set; }
  public int CurrentLineNumber { get; set; }

  public LAFileReader(LAFile file)
  {
    FileInfo = file;
  }

  public void Open()
  {
    // Build path
    FullPath = Path.Combine([FileInfo.Path ?? Program.Settings.LogFolder, FileInfo.Filename]);

    if (!File.Exists(FullPath))
      throw new Exception($"File does not exist: {FileInfo.Label} @ {FullPath}");

    FileStream = File.Open(FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
    StreamReader = new StreamReader(FileStream, Encoding.Default);

    Program.Info($"Opened log file {FileInfo.Label} @ {FullPath}");
  }
  public void Open(LAFile file)
  {
    FileInfo = file;
    Open();
  }

  public string? ReadNext()
  {
    if (FileStream == null || StreamReader == null)
      return null;

    // Check if file has regenerated -> is shorter then last time
    if (FileStream.Position > FileStream.Length)
    {
      var sb = new StringBuilder();

      var msg = "File was regenerated! Reading from start...";
      sb.AppendLine(new string('*', msg.Length));
      sb.AppendLine(msg);
      sb.AppendLine(new string('*', msg.Length));

      Program.ConsoleWrite(sb.ToString(), ConsoleColor.Yellow, ConsoleColor.Black);

      FileStream.Seek(0, SeekOrigin.Begin);
    }

    // Read log file
    if (StreamReader.EndOfStream)
      return null;

    CurrentLine = StreamReader.ReadLine();
    ParseTimestamp(CurrentLine);

    CurrentLineNumber++;

    return CurrentLine;
  }
  public bool CanRead()
  {
    return !this.StreamReader.EndOfStream;
  }
  public void Close()
  {
    if (FileStream == null || StreamReader == null)
    {
      Program.Warn($"Log file {FileInfo.Label} is already closed @ {FullPath}");
      return;
    }

    StreamReader.Close();
    FileStream.Close();
  }

  private DateTime? ParseTimestamp(string line)
  {
    // 2025-04-13 00:00:54.0802 [  4]  INFO - ResourceMonitorService -          -              - Nothing done for 10.000 seconds run worker Task by timer - 
    var regex = new Regex(Program.Settings.TimestampFilter);

    var match = regex.Match(line);
    if (!match.Success)
    {
      Program.Debug($"Could not find Timestamp: [{FileInfo.Label}] {CurrentLine} @ {CurrentLineNumber}");
      return null;
    }

    if (DateTime.TryParseExact(match.Value, Program.Settings.TimestampFormat, CultureInfo.InvariantCulture, DateTimeStyles.AllowInnerWhite, out DateTime timestamp))
      CurrentTimestamp = timestamp;
    else
      Program.Debug($"Could not parse Timestamp: [{FileInfo.Label}] {CurrentLine} @ {CurrentLineNumber}");

    return CurrentTimestamp;
  }
}