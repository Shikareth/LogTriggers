namespace ConsoleApplication.SQUAD
{
  public class Faction
  {
    public string Tag { get; set; }
    public string FullName { get; set; }
    public List<Unit> Units { get; set; }

    public override String ToString()
    {
      return $"[{Tag}] {FullName}";
    }
  }
}
