﻿using System.Text;

using LogAnalyzer.Events;
using LogAnalyzer.File;
using LogAnalyzer.Settings;

using Microsoft.Extensions.Configuration;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;

namespace LogAnalyzer;

public partial class LogAnalyzer
{
  private const string _title = "LogAnalyzer";
  private static Throbber _throbber = new(["-", "\\", "|", "/"]);
  public static bool Enabled { get; set; } = true;
  public static WebDriver? Driver { get; set; }
  public static LASettings Settings { get; set; } = new();
  private static List<LAFileReader> Files { get; set; } = [];

  public static void Main(string[] args)
  {
    Console.Title = _title;

    // Setup clean application stop
    Console.CancelKeyPress += (sender, args) =>
    {
      ConsoleWrite($"{args.SpecialKey} pressed. Cleaning up ...");

      // Set the Cancel property to true to prevent the process from terminating prematurely.
      args.Cancel = true;
      LogAnalyzer.Enabled = false;
    };

    try
    {
      var appsettings = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddEnvironmentVariables()
        .AddJsonFile("appsettings.json")
        .AddCommandLine(args)
        .Build();

      // Enable throwing error on invalid configuration
      static void getOptions(BinderOptions o) => o.ErrorOnUnknownConfiguration = true;

      Settings = appsettings.GetRequiredSection("Settings").Get<LASettings>(getOptions) ?? throw new Exception($"Settings object was NULL!!");
      LAEventManager.Events = appsettings.GetRequiredSection("Events").Get<List<LAEvent>>(getOptions) ?? throw new Exception($"Events definition object was NULL!!");
      LAEventManager.Init();

      // Setup Selenium browser
      Driver = Settings.Browser switch
      {
        Enums.BrowserType.Firefox => new FirefoxDriver(),
        Enums.BrowserType.Chrome => new ChromeDriver(),
        Enums.BrowserType.IE => new InternetExplorerDriver(),
        Enums.BrowserType.Edge => new EdgeDriver(),
        Enums.BrowserType.None => null,
        _ => throw new Exception($"Not supported browser: {Settings.Browser}"),
      };

      // Open files
      foreach (var file in Settings.LogFiles)
      {
        var filereader = new LAFileReader(file);
        filereader.Open();
        filereader.ReadNext();

        Files.Add(filereader);
      }

      while (Enabled)
      {
        while (true)
        {
          // Check if there is anything to read
          var currentFile = Files.Where(x => x.CanRead())?.MinBy(x => x.CurrentTimestamp);
          if (currentFile == null)
            break;

          currentFile.ReadNext();
          LAEventManager.Parse(currentFile);
        }

        // Deleay read to save resources
        Thread.Sleep(Settings.UpdateDelay);
        Console.Title = $"{_title}[{_throbber.Next()}]";
      }
    }
    catch (Exception ex)
    {
      Error(ParseException(ex));
    }
    finally
    {
      // Close files
      foreach (var file in Files)
        file.Close();

      Info("Press any key to exit ...");
    }
  }

  public static void Error(string text, ConsoleColor bgcolor = ConsoleColor.Black, ConsoleColor fgcolor = ConsoleColor.Red)
  {
    if (Settings.LogLevel >= Enums.LogLevel.ERROR)
      ConsoleWrite("[ERROR] " + text, bgcolor, fgcolor);
  }
  public static void Warn(string text, ConsoleColor bgcolor = ConsoleColor.Black, ConsoleColor fgcolor = ConsoleColor.Yellow)
  {
    if (Settings.LogLevel >= Enums.LogLevel.WARN)
      ConsoleWrite("[WARN] " + text, bgcolor, fgcolor);
  }
  public static void Info(string text, ConsoleColor bgcolor = ConsoleColor.Black, ConsoleColor fgcolor = ConsoleColor.White)
  {
    if (Settings.LogLevel >= Enums.LogLevel.INFO)
      ConsoleWrite("[INFO] " + text, bgcolor, fgcolor);
  }
  public static void Debug(string text, ConsoleColor bgcolor = ConsoleColor.Black, ConsoleColor fgcolor = ConsoleColor.Green)
  {
    if (Settings.LogLevel >= Enums.LogLevel.DEBUG)
      ConsoleWrite("[DEBUG] " + text, bgcolor, fgcolor);
  }
  public static void ConsoleWrite(string text, ConsoleColor bgcolor = ConsoleColor.Black, ConsoleColor fgcolor = ConsoleColor.White)
  {
    var lastBG = Console.BackgroundColor;
    var lastFG = Console.ForegroundColor;

    Console.BackgroundColor = bgcolor;
    Console.ForegroundColor = fgcolor;

    Console.WriteLine(text);

    Console.BackgroundColor = lastBG;
    Console.ForegroundColor = lastFG;
  }
  internal static string ParseException(Exception? ex)
  {
    var report = new StringBuilder();

    if (ex == null)
      return "******************** END ********************";

    report.AppendLine(ex.Source);
    report.AppendLine(ex.Message);
    report.AppendLine(ex.StackTrace);
    report.AppendLine(ParseException(ex.InnerException));

    return report.ToString();
  }
}


