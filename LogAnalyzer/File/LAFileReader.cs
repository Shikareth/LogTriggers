using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace LogAnalyzer.File;

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
    FullPath = Path.Combine([FileInfo.Path ?? LogAnalyzer.Settings.LogFolder, FileInfo.Filename]);

    if (!File.Exists(FullPath))
      throw new Exception($"File does not exist: {FileInfo.Label} @ {FullPath}");

    FileStream = File.Open(FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
    StreamReader = new StreamReader(FileStream, Encoding.Default);

    LogAnalyzer.Info($"Opened log file {FileInfo.Label} @ {FullPath}");
  }
  public void Open(LAFile file)
  {
    FileInfo = file;
    Open();
  }

  public void ReadNext()
  {
    if (FileStream == null || StreamReader == null)
      return;

    CheckFileLength();

    // Read log file
    if (StreamReader.EndOfStream)
      return;

    CurrentLine = StreamReader.ReadLine();
    if (string.IsNullOrEmpty(CurrentLine))
      return;

    ParseTimestamp(CurrentLine);

    CurrentLineNumber++;
  }
  public bool CanRead()
  {
    return !StreamReader?.EndOfStream ?? false;
  }
  public void Close()
  {
    if (FileStream == null || StreamReader == null)
    {
      LogAnalyzer.Warn($"Log file {FileInfo.Label} is already closed @ {FullPath}");
      return;
    }

    StreamReader.Close();
    FileStream.Close();
  }


  private void CheckFileLength()
  {
    if (FileStream == null)
      throw new Exception($"Filestrem was null!! {Path.Combine(FileInfo.Path, FileInfo.Filename)}");

    // Check if file has regenerated -> is shorter then last time
    if (FileStream.Position > FileStream.Length)
    {
      var sb = new StringBuilder();

      var msg = "File was regenerated! Reading from start...";
      sb.AppendLine(new string('*', msg.Length));
      sb.AppendLine(msg);
      sb.AppendLine(new string('*', msg.Length));

      LogAnalyzer.ConsoleWrite(sb.ToString(), ConsoleColor.Yellow, ConsoleColor.Black);

      FileStream.Seek(0, SeekOrigin.Begin);
    }

  }
  private void ParseTimestamp(string line)
  {
    var regex = new Regex(FileInfo.TimestampFilter ?? LogAnalyzer.Settings.TimestampFilter);

    var match = regex.Match(line);
    if (!match.Success)
    {
      LogAnalyzer.Debug($"Could not find Timestamp: [{FileInfo.Label}] {CurrentLine} @ {CurrentLineNumber}");
      return;
    }

    if (DateTime.TryParseExact(match.Value, FileInfo.TimestampFormat ?? LogAnalyzer.Settings.TimestampFormat, CultureInfo.InvariantCulture, DateTimeStyles.AllowInnerWhite, out DateTime timestamp))
      CurrentTimestamp = timestamp;
    else
      LogAnalyzer.Debug($"Could not parse Timestamp: [{FileInfo.Label}] {CurrentLine} @ {CurrentLineNumber}");
  }

  public override string ToString()
  {
    return $"{CurrentTimestamp} [{FileInfo.Label}@{CurrentLineNumber}]";
  }
}