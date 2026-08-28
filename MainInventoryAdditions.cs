using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;
using HarmonyLib;

using static InventoryAdditions.LogTools;
using static InventoryAdditions.InventoryTools;

namespace InventoryAdditions;


[BepInPlugin("dev.dustie.inventoryadditions", "Inventory Additions", "1.0.0")]
public class InventoryAdditions : BaseUnityPlugin
{

  internal static ManualLogSource? Log;


  private static ConfigEntry<bool>? _configMuteVerbose;
  private const bool _muteVerboseDefault = true;

  private static ConfigEntry<float>? _configDoubleClickWindow; // s
  private const float _doubleClickWindowDefault = 0.15f;

  private static ConfigEntry<float>? _configPickupGracePeriod; //s 
  private const float _pickupGracePeriodDefault = 0.10f;

  internal static float PickupGracePeriod
  {
    get => _configPickupGracePeriod?.Value ?? _pickupGracePeriodDefault;
  }

  internal static float DoubleClickWindow
  {
    get => _configDoubleClickWindow?.Value ?? _doubleClickWindowDefault;
  }

  internal static bool Verbose
  {
    get => !_configMuteVerbose?.Value ?? _muteVerboseDefault;
  }


  private void Awake()
  {
    Log = Logger;

    Harmony harmony = new Harmony("dev.dustie.inventoryadditions");
    harmony.PatchAll();

    Configure();
  }

  private void Configure()
  {
    _configMuteVerbose = Config.Bind(
        "Options",
        "Mute Verbose",
        _muteVerboseDefault,
        "Should only enable this for development or debugging. Will likely spam your log file."
        );

    _configPickupGracePeriod = Config.Bind(
        "Options",
        "Pickup Grace Period",
        _pickupGracePeriodDefault,
        "The time, starting from the initial pickup of an item or stack, for which you cannot paint or" +
        " drop the item. A small value will prevent you from immediately dropping or painting on accident when picking " +
        "up items quickly."
        );


    _configDoubleClickWindow = Config.Bind(
        "Options",
        "Double Click Window",
        _doubleClickWindowDefault,
        "The time window, in seconds, for a double click to be registered in order to group all items" +
        "of the same type if possible."
        );

  }




  internal static bool IsEqualDistributing = false;
  internal static bool IsHoldingSomething => UIItemSlot.draggedItem != null;
  internal static double TimeSinceDragStart => Time.realtimeSinceStartupAsDouble - _dragStartTimestamp;
  internal static double TimeSinceHoldStart => Time.realtimeSinceStartupAsDouble - _holdItemTimestamp;

  private static bool _wasHoldingLastFrame = false;
  private static double _holdItemTimestamp = 0;
  private static double _dragStartTimestamp = 0;

  private static bool _dragging = false;

  private static bool DragStartedThisFrame =>
    (Input.GetMouseButtonDown(0) && !Input.GetMouseButton(1))
    || (Input.GetMouseButtonDown(1) && !Input.GetMouseButton(0));


  private void Update()
  {

    if (_wasHoldingLastFrame != IsHoldingSomething)
    {
      if (IsHoldingSomething)
      {
        Verbose("Started hold");
        _holdItemTimestamp = Time.realtimeSinceStartupAsDouble;
      }
      else
      {
        PaintDistributor.TryUpdate();
        Verbose("Stopped hold");

      }
    }
    _wasHoldingLastFrame = IsHoldingSomething;


    // Drag Started

    if (DragStartedThisFrame)
    {
      _dragStartTimestamp = Time.realtimeSinceStartupAsDouble;
      Verbose("Drag started");
      _dragging = true;
      if (Input.GetMouseButtonDown(0)) // Left click
      {
        IsEqualDistributing = true;
      }
      else // Right click
      {
        IsEqualDistributing = false;
      }
    }

    if (!IsM1OrM2Pressed() && _dragging)
    { // ended
      Verbose("Drag stopped");
      _dragging = false;
      IsEqualDistributing = false;
      PaintDistributor.Reset();
    }


    // Painting

    float scroll = Input.GetAxis("Mouse ScrollWheel");
    if (scroll > 0f)
    {
      IsEqualDistributing = false;
      PaintDistributor.DistributeViaScroll();
    }
    else if (scroll < 0f)
    {
      IsEqualDistributing = false;
      PaintDistributor.CollectViaScroll();
    }

    PaintDistributor.TryUpdate();
  }



}
