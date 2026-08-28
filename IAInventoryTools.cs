using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

using static InventoryAdditions.InventoryAdditions;
using static InventoryAdditions.PaintDistributor;
using static InventoryAdditions.LogTools;

namespace InventoryAdditions;

internal static class InventoryTools
{

  // Items and slots
  internal static bool IsBadSlot(UIItemSlot? slot)
  {
    if (slot == null)
    {
      return true;
    }
    if (slot.observedItem != null && IsBadItem(slot.observedItem))
    {
      return true;
    }
    return false;
  }

  internal static bool IsBadItem(InvGameItem? item)
  {
    if (item == null)
    {
      return true;
    }
    // Damaged tools are more complex to work with, so we dont for now
    if (item.count % item.durability != 0)
    {
      return true;
    }

    return false;
  }

  internal static bool IsM1OrM2Pressed()
  {
    return (Input.GetMouseButton(1) || Input.GetMouseButton(0));
  }

  internal static bool CanDrop()
  {
    Verbose($"Can drop? Hs: {HighlightedAnything}, ishold: {IsHoldingSomething}, time: {TimeSinceHoldStart < PickupGracePeriod}");
    if (
          HighlightedAnything ||
          (IsHoldingSomething && TimeSinceHoldStart < PickupGracePeriod) // Stop dropping after immediate pickup
        )
    {
      Verbose("Cannot drop");
      return false;
    }
    Verbose("Can drop");
    return true;
  }

  internal static void ApplyHighlights(List<TweenColor> ItemHighlights)
  {
    foreach (TweenColor highlight in ItemHighlights)
    {
      highlight.enabled = false;
      highlight.color = new Color32(255, 255, 255, 255);
    }
  }
  internal static void RemoveHighlights(List<TweenColor> ItemHighlights)
  {
    foreach (TweenColor highlight in ItemHighlights)
    {
      highlight.color = new Color32(255, 255, 255, 0);
    }
  }
}
