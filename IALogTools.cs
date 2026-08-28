using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.IO;

namespace InventoryAdditions;

internal static class LogTools
{

  // Timing

  internal static void EndStopwatchAndDebugPrint(
      Stopwatch stopwatch,
      string timePrefixMessage)
  {
    if (stopwatch.IsRunning)
    {
      stopwatch.Stop();
    }

    double milliseconds = stopwatch.ElapsedMilliseconds;
    string timeString = FormatMilliseconds(milliseconds);
    Debug($"{timePrefixMessage} : {timeString}");
  }

  internal static string GetFormattedTimeString(this Stopwatch stopwatch)
  {
    if (stopwatch.IsRunning)
    {
      stopwatch.Stop();
    }
    return FormatMilliseconds(stopwatch.ElapsedMilliseconds);
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
      => $"""
            {obj} [ FROM: {caller} @ {Path.GetFileName(callerPath)}]
            """;

  internal static void Print(
      object? obj,
      [CallerMemberName] string caller = "",
      [CallerFilePath] string callerPath = "")
  {
    InventoryAdditions.Log!.LogInfo(FormatMessage(obj, caller, callerPath));
  }

  internal static void Verbose(
      object? obj,
      [CallerMemberName] string caller = "",
      [CallerFilePath] string callerPath = "")
  {
    if (InventoryAdditions.Verbose)
    {
      InventoryAdditions.Log!.LogDebug(FormatMessage(obj, caller, callerPath));
    }
  }

  internal static void Debug(
      object? obj,
      [CallerMemberName] string caller = "",
      [CallerFilePath] string callerPath = "")
  {
    InventoryAdditions.Log!.LogDebug(FormatMessage(obj, caller, callerPath));
  }

  internal static void Warn(
      object? obj,
      [CallerMemberName] string caller = "",
      [CallerFilePath] string callerPath = "")
  {
    InventoryAdditions.Log!.LogWarning(FormatMessage(obj, caller, callerPath));
  }

  internal static void Error(
      object? obj,
      [CallerMemberName] string caller = "",
      [CallerFilePath] string callerPath = "")
  {
    InventoryAdditions.Log!.LogError(FormatMessage(obj, caller, callerPath));
  }

  internal static void Fatal(
      object? obj,
      [CallerMemberName] string caller = "",
      [CallerFilePath] string callerPath = "")
  {
    InventoryAdditions.Log!.LogFatal(FormatMessage(obj, caller, callerPath));
  }

}
