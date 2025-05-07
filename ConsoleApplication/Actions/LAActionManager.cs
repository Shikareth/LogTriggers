namespace ConsoleApplication.Actions;

public static class LAActionManager
{

  /// <summary>
  /// Contains definitions of Actions to look for
  /// </summary>
  public static List<LAAction> Actions { get; set; } = [];

  /// <summary>
  /// Contains history of executed Actions
  /// </summary>
  public static List<LAAction> ActionsArchived { get; set; } = [];

}

