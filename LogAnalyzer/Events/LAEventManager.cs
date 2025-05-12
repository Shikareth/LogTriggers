namespace LogAnalyzer.Events;

public static class LAEventManager
{
  internal static int _maxEventLabelLength = 0;
  internal static int _maxLogfileLabelLength = 0;

  /// <summary>
  /// Contains definitions of Events to look for
  /// </summary>
  public static List<LAEvent> Events { get; set; } = [];
  /// <summary>
  /// Contains last detected Events. 
  /// New Event from definitions overwrites this instance.
  /// Old Event is moved to Archive
  /// </summary>
  public static List<LAEvent> EventsBuffered { get; set; } = [];
  /// <summary>
  /// Contains history of detected Events
  /// </summary>
  public static List<LAEvent> EventsArchived { get; set; } = [];

  public static void Init()
  {
    if (LogAnalyzer.Settings != null && LogAnalyzer.Settings.LogFiles.Count > 0)
      _maxLogfileLabelLength = LogAnalyzer.Settings.LogFiles.Select(x => x.Label).OrderByDescending(x => x.Length).First().Length;

    if (Events != null && Events.Count > 0)
      _maxEventLabelLength = Events.Select(x => x.Label).OrderByDescending(x => x.Length).First().Length;
  }

  public static void Parse(LAFileReader fileReader)
  {
    if (fileReader.CurrentLine == null)
    {
      LogAnalyzer.Warn($"Cannot parse null: {fileReader.FileInfo.Label} @ line: {fileReader.CurrentLineNumber}");
      return;
    }

    CheckEvents(fileReader);
  }
  public static LAEvent? GetLastEvent(string label)
  {
    return EventsBuffered.LastOrDefault(x => x.Label == label && !x.Consumed);
  }

  private static void CheckEvents(LAFileReader fileReader)
  {
    foreach (var e in Events)
    {
      if (!e.Enabled)
        continue;

      if (e.SourceLogs.Count > 0 && !e.SourceLogs.Contains(fileReader.FileInfo.Label))
        continue;

      if (e.CheckConditions(fileReader.CurrentLine, fileReader.CurrentLineNumber))
      {
        var logfileLabel = fileReader.FileInfo.Label + new string(' ', _maxLogfileLabelLength - fileReader.FileInfo.Label.Length);

        LogAnalyzer.Info($"[{logfileLabel}] " + e.ToString(), e.BackgroundColor, e.ForegroundColor);
        BufferEvent(e);
      }
    }
  }
  private static void BufferEvent(LAEvent e)
  {
    ArchiveEvent(e.Label);
    EventsBuffered.Add((LAEvent)e.Clone());
  }
  public static void ArchiveEvent(string label)
  {
    var bufferedEvent = EventsBuffered.FirstOrDefault(x => x.Label == label);
    if (bufferedEvent == null)
      return;

    EventsArchived.Add(bufferedEvent);
    EventsBuffered.Remove(bufferedEvent);
  }
}
