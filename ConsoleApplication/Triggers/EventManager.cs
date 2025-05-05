using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;

namespace ConsoleApplication.Triggers
{
  public static class EventManager
  {
    public static List<Action> Actions { get; set; } = [];

    /// <summary>
    /// Contains definitions of Events to look for
    /// </summary>
    public static List<Event> Events { get; set; } = [];
    /// <summary>
    /// Contains last detected Events. 
    /// New Event from definitions overwrites this instance.
    /// Old Event is moved to Archive
    /// </summary>
    public static List<Event> EventsBuffered { get; set; } = [];
    /// <summary>
    /// Contains history of detected Events
    /// </summary>
    public static List<Event> EventsArchived { get; set; } = [];

    public static void Parse(string line, int lineIndex){
      CheckEvents(line, lineIndex);
    }
    public static Event? GetLastEvent(string label){
      return EventsBuffered.LastOrDefault(x => x.Label == label && !x.Consumed);
    }
    public static void RegisterAction(string label, string[] args)
    {
      Actions.Add(new Action()
      {
        Label = label,
        Arguments = new List<string>(args)
      });
    }
    public static void UnregisterAction(string label)
    {

    }

    private static void CheckEvents(string line, int lineIndex)
    {
      foreach(var e in Events)
      {
        if(!e.Enabled)
          continue;

        if(e.CheckConditions(line, lineIndex)){
          Program.Info(e.ToString());
          BufferEvent(e);
        }
      }
    }
    private static void BufferEvent(Event e)
    {
      ArchiveEvent(e.Label);
      EventsBuffered.Add((Event)e.Clone());
    }
    public static void ArchiveEvent(string label)
    {
      var bufferedEvent = EventsBuffered.FirstOrDefault(x => x.Label == label);
      if(bufferedEvent == null)
        return;

      EventsArchived.Add(bufferedEvent);
      EventsBuffered.Remove(bufferedEvent);
    }
  }
}
