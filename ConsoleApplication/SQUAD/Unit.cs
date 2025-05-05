namespace ConsoleApplication.SQUAD
{
  public class Unit
  {
    public string Tag { get; set; }
    public string FullName { get; set; }

    public override String ToString()
    {
      return $"[{Tag}] {FullName}";
    }
  }
}
