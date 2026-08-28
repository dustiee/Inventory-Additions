using System;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using static InventoryAdditions.InventoryAdditions;
using static InventoryAdditions.InventoryTools;
using static InventoryAdditions.LogTools;
using static InventoryAdditions.DoubleClickGroupAll;
using static InventoryAdditions.PaintDistributor;

namespace InventoryAdditions;

// Chokepoint for NGUI click/hover/press/drop notifications, so we add our
// own logic to ignore input that would otherwise mess up the added
// inventory features.
internal static class IUPatches
{
  private static readonly string[] _targetFunctions = ["OnDrop", "OnClick",]; // "OnPress", "OnClick"

  [HarmonyPatch(typeof(UICamera), nameof(UICamera.Notify))]
  internal static class UICameraNotifyPatch
  {
    private static bool Prefix(GameObject? go, ref string funcName, ref object obj)
    {
      try
      {
        if (
      _targetFunctions.Contains(funcName) && !CanDrop()
                )
        {
          Verbose($"STOPPED function {funcName}");
          return false;
        }

        // Clicking the recipe/info button while holding an item should actually open the
        // entry for it
        if (go?.name == "InfoButton" && funcName == "OnClick")
        {
          var origin =
              AccessTools.Field(typeof(UIItemSlot), "dragOrigin").GetValue(null)
              as UIItemSlot;
          if (origin != null && UIItemSlot.draggedItem != null)
          {
            obj = origin.gameObject; // Object is needed here otherwise the info button does nothing
            funcName = "OnDrop"; // and we need to "drop" it onto the info button
          }
        }

      }
      catch (Exception ex)
      {
        Error(ex);
      }

      Verbose($"Passing {go} - {funcName} - {obj}");
      return true;
    }

    // Painting and double-click grouping
    private static void Postfix(GameObject go, string funcName, object obj)
    {
      if (go == null)
      {
        return;
      }
      if (funcName == "OnClick")
      {
        HandleClick(go);
      }
      if (funcName != "OnHover" && funcName != "OnPress")
      {
        return;
      }
      if (obj is not bool flag || !flag)
      {
        return;
      }
      if (UIItemSlot.draggedItem == null)
      {
        return;
      }

      if (!IsM1OrM2Pressed())
      {
        return;
      }
      UIItemSlot slot = go.GetComponent<UIItemSlot>();
      if (slot == null)
      {
        return;
      }
      TryPaintSlot(slot);
      TryUpdate();
    }
  }

  [HarmonyPatch(typeof(UIItemSlot), "OnDropOther", [typeof(GameObject)])]
  internal static class UIItemSlotOnDropOtherPatch
  {
    // This is from UICamera.ProcessTouch and bypasses the above patch, so
    // need to stop it here as well
    private static bool Prefix(GameObject other)
    {
      if (CanDrop())
      {
        return true;
      }
      return false;
    }
  }
}

