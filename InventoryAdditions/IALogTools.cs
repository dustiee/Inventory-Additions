using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.IO;

namespace InventoryAdditions;

// Reason why we have a null and bepinex logger is so that tests don't break for methods that use LogTools, as 
// using the bepinex logger in that case will try to retrieve assemblies only available at game runtime
internal interface ILogToolsLogger
{
  void LogInfo(string message);
  void LogDebug(string message);
  void LogWarning(string message);
  void LogError(string message);
  void LogFatal(string message);
}

internal class BepInExLogger : ILogToolsLogger
{
  private readonly BepInEx.Logging.ManualLogSource _log;

  internal BepInExLogger(BepInEx.Logging.ManualLogSource log)
  {
    _log = log;
  }

  public void LogInfo(string message) => _log.LogInfo(message);
  public void LogDebug(string message) => _log.LogDebug(message);
  public void LogWarning(string message) => _log.LogWarning(message);
  public void LogError(string message) => _log.LogError(message);
  public void LogFatal(string message) => _log.LogFatal(message);
}

internal class NullLogger : ILogToolsLogger
{
  public void LogInfo(string message) { return; }
  public void LogDebug(string message) { return; }
  public void LogWarning(string message) { return; }
  public void LogError(string message) { return; }
  public void LogFatal(string message) { return; }
}


internal static class LogTools
{
  internal static ILogToolsLogger Logger { get; set; } = new NullLogger();
  internal static bool VerboseLogging = false;

  // Timing


  internal static void EndStopwatchAndDebugPrint(
      Stopwatch stopwatch,
      string timePrefixMessage)
  {
    if (stopwatch.IsRunning)
      stopwatch.Stop();

    double milliseconds = stopwatch.Elapsed.TotalMilliseconds;
    string timeString = FormatMilliseconds(milliseconds);
    Debug($"{timePrefixMessage} : {timeString}");
  }

  internal static string StopAndGetFormattedTimeString(this Stopwatch stopwatch)
  {
    if (stopwatch.IsRunning)
      stopwatch.Stop();

    return FormatMilliseconds(stopwatch.Elapsed.TotalMilliseconds);
  }

  internal static string FormatMilliseconds(double milliseconds)
  {
    if (milliseconds < 10.0)
    {
      double microseconds = milliseconds * 1_000.0;
      return $"{microseconds:F0} µs";
    }
    else if (milliseconds < 500.0)
    {
      return $"{milliseconds:F3} ms";
    }
    else
    {
      double seconds = milliseconds / 1_000.0;
      return $"{seconds:F3} s";
    }
  }


  // Logging

  private static string FormatMessage(
      object? obj,
      string caller,
      string callerPath)
      => $"{obj} [ FROM: {caller} @ {Path.GetFileName(callerPath)}]";

  internal static void Print(
      object? obj,
      [CallerMemberName] string caller = "",
      [CallerFilePath] string callerPath = "")
      => Logger.LogInfo(FormatMessage(obj, caller, callerPath));

  internal static void Verbose(
      object? obj,
      [CallerMemberName] string caller = "",
      [CallerFilePath] string callerPath = "")
  {
    if (VerboseLogging)
      Logger.LogDebug(FormatMessage(obj, caller, callerPath));
  }

  internal static void Debug(
      object? obj,
      [CallerMemberName] string caller = "",
      [CallerFilePath] string callerPath = "")
      => Logger.LogDebug(FormatMessage(obj, caller, callerPath));

  internal static void Warn(
      object? obj,
      [CallerMemberName] string caller = "",
      [CallerFilePath] string callerPath = "")
      => Logger.LogWarning(FormatMessage(obj, caller, callerPath));

  internal static void Error(
      object? obj,
      [CallerMemberName] string caller = "",
      [CallerFilePath] string callerPath = "")
      => Logger.LogError(FormatMessage(obj, caller, callerPath));

  internal static void Fatal(
      object? obj,
      [CallerMemberName] string caller = "",
      [CallerFilePath] string callerPath = "")
      => Logger.LogFatal(FormatMessage(obj, caller, callerPath));
}
