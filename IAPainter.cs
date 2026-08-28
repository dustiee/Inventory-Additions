using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

using static InventoryAdditions.LogTools;
using static InventoryAdditions.InventoryTools;
using static InventoryAdditions.InventoryAdditions;

namespace InventoryAdditions;

internal static class PaintDistributor
{
  internal static readonly List<UIItemSlot> TrackedSlots = [];
  internal static readonly List<TweenColor> TrackedSlotHighlights = [];

  internal static int? TrackedCount
  {
    get
    {
      List<UIItemSlot> slots = [.. TrackedSlots];
      int slotCount = slots.Count;

      if (slotCount == 0)
      {
        return null;
      }

      int trackedItemExactCount = 0;

      foreach (UIItemSlot slot in slots)
      {
        InvGameItem? slotItem = slot.observedItem;

        if (slotItem == null)
          continue;

        trackedItemExactCount += slotItem.ExactItemCount() ?? 0;
      }

      int? heldCount = UIItemSlot.draggedItem?.ExactItemCount();

      heldCount ??= 0;

      return heldCount.Value + trackedItemExactCount;
    }
  }

  internal static bool HighlightedAnything
  {
    get => TrackedSlots.Count > 1; // 1 will always be highlighted when clicking on a slot
  }


  // UIItemSlot methods
  private static MethodInfo? _clearDragMethod;
  private static MethodInfo? _updateCursorMethod;

  private static MethodInfo ClearDragMethod =>
      _clearDragMethod ??= AccessTools.Method(typeof(UIItemSlot), "ClearDrag");

  private static MethodInfo UpdateCursorMethod =>
      _updateCursorMethod ??= AccessTools.Method(typeof(UIItemSlot), "UpdateCursor");

  private static DummyItemSlot _dummyItemSlot = new();


  internal static void Reset()
  {
    TrackedSlots.Clear();
    if ((UIItemSlot.draggedItem?.count ?? 1) <= 0)
    {
      ClearCursorSlot();
    }

  }

  internal static void ClearCursorSlot()
  {
    ClearDragMethod.Invoke(_dummyItemSlot, null);
  }

  internal static void TryUpdate()
  {
    if (TrackedSlots.Count == 0 && TrackedSlotHighlights.Count == 0) { return; }
    if (TrackedSlots.Count == 0)
    {
      RemoveHighlights(TrackedSlotHighlights);
      TrackedSlotHighlights.Clear();
    }
    else
    {
      ApplyHighlights(TrackedSlotHighlights);
    }
    if (IsEqualDistributing && TrackedSlots.Count > 1) // Distribute only when we have at least 2 slots, this lets you add one item by 
                                                       // clicking as in the base game if you don't drag it
    {
      DistributeEqually();
    }

  }

  internal static void TryPaintSlot(UIItemSlot slot)
  {
    if (IsHoldingSomething && TimeSinceHoldStart < PickupGracePeriod)
    {
      Verbose("Did not paint because of grace period.");
      return;
    }

    if (IsBadSlot(slot))
    {
      Verbose("Did not paint because of a bad item slot.");
      return;
    }

    InvGameItem draggedItem = UIItemSlot.draggedItem;
    int? exactCount = draggedItem.ExactItemCount();
    if (exactCount == null)
    {
      Verbose("Did not point because of an invalid exactCount");
      return;
    }


    if (IsBadItem(draggedItem))
    {
      return;
    }

    if (TrackedSlots.Contains(slot))
    {
      Verbose("Did not paint because the slot is already painted");
      return;
    }

    // If we have a single item don't do anything, removing this will make trying to pick up
    // a single item extremely difficult (i,e, single click item collect immediately getting eaten when hovering another slot)
    if (TrackedSlots.Count == 0 && exactCount <= 1)
    {
      Verbose("Did not paint because we are only holding one item");
      return;
    }

    if (!GiveOneItem(slot, draggedItem))
    {
      Verbose("Did not paint because the slot does not take this item");
      return;
    }
    TrackedSlots.Add(slot);
    // if (IsEqualDistributing)
    // {
    //   DistributeEqually();
    // }

    TweenColor? thisHighlightTween = slot.transform.Find("Highlighted")?.GetComponent<TweenColor>();

    if (thisHighlightTween == null)
    {
      Warn("Failed to add a highlight, but we are still tracking this slot");
      return;
    }
    TrackedSlotHighlights.Add(thisHighlightTween);
    TryUpdate();
  }


  internal static void DistributeEqually()
  {
    if (TrackedSlots.Count == 0)
    {
      Verbose("No slots to distribute in");
      return;
    }

    InvGameItem? dragged = UIItemSlot.draggedItem;

    if (IsBadItem(dragged))
    {
      return;
    }

    // Get tracked slots and the total of the item we are trying to distribute
    var slots = new List<UIItemSlot>(TrackedSlots);
    int slotCount = slots.Count;

    // NOTE:
    // Tracked slots and their items shouldn't change and we've
    // already validated them, if this assumption ever turns out to be false for
    // some reason add some checks here (Likely to happen if we try to make this work
    // with tools, AHEM torches losing durability on their own)
    int trackedItemExactCount = 0;
    foreach (UIItemSlot slot in slots)
    {
      InvGameItem? slotItem = slot.observedItem;
      if (slotItem == null)
      {
        continue;
      }

      trackedItemExactCount += slotItem.ExactItemCount() ?? 0;
    }

    int? heldCount = dragged.ExactItemCount();
    if (heldCount == null)
    {
      return;
    }
    int totalCount = heldCount.Value + trackedItemExactCount;

    if (totalCount <= 0)
    {
      return;
    }

    // int distributedCount = totalCount / slotCount;
    // int remainderCount = totalCount % slotCount;

    // HACK:
    // I'm doing this so the user always has at least 1 item "in-hand", so it doesn't
    // cause dragging to end pre-maturely
    // Another reason why we want this is because it prevents a duplication bug,
    // and this is the best way I found of fixing it.
    // If you have a better way of doing this, do improve it!
    int distributedCount = (totalCount - 1) / slotCount;
    int remainderCount = ((totalCount - 1) % slotCount) + 1;


    foreach (UIItemSlot slot in slots)
    {
      InvGameItem slotItem = slot.observedItem;

      if (slotItem == null)
      {
        continue;
      }

      slotItem.count = dragged.IngameCount(distributedCount);
    }

    dragged.count = dragged.IngameCount(remainderCount);

    if (dragged.count <= 0)
    {
      // ClearDragMethod.Invoke(slots[0], null);
    }
    else
    {
      UpdateCursorMethod.Invoke(slots[0], null);
    }
  }

  // Add one to each tracked slot
  internal static void DistributeViaScroll()
  {
    if (TrackedSlots.Count == 0)
    {
      return;
    }

    InvGameItem? dragged = UIItemSlot.draggedItem;
    if (IsBadItem(dragged) || dragged.count < TrackedSlots.Count)
    {
      return;
    }

    List<UIItemSlot> slots = [.. TrackedSlots];
    foreach (UIItemSlot slot in slots)
    {
      if ((dragged.ExactItemCount() ?? 0) <= 0)
      {
        break;
      }

      GiveOneItem(slot, dragged);
      TryUpdate();
    }
  }


  // Take one from each tracked slot
  internal static void CollectViaScroll()
  {
    if (TrackedSlots.Count == 0)
    {
      return;
    }

    InvGameItem? dragged = UIItemSlot.draggedItem;
    if (IsBadItem(dragged))
    {
      return;
    }

    var slots = new List<UIItemSlot>(TrackedSlots);
    UIItemSlot? lastSuccessfulSlot = null;

    foreach (UIItemSlot slot in slots)
    {
      if (TakeOneItem(slot, dragged))
      {
        lastSuccessfulSlot = slot;
      }
    }

    if (lastSuccessfulSlot == null)
    {
      return;
    }

    UpdateCursorMethod.Invoke(lastSuccessfulSlot, null);
    TryUpdate();
  }

  // Add one item from dragged to slot
  private static bool GiveOneItem(UIItemSlot slot, InvGameItem? dragged)
  {
    if (!slot.CanAccept(dragged))
    {
      return false;
    }
    if (IsBadItem(dragged))
    {
      return false;
    }
    if (dragged!.count <= 0)
    {
      return false;
    }
    int? exactCount = dragged.ExactItemCount();
    if (exactCount == null)
    {
      return false;
    }

    InvGameItem single = InvDatabase.CreateItem(
        dragged!.baseItem,
        dragged.data,
        dragged.IngameCount(1),
        dragged.paintData
    );
    if (single == null)
    {
      return false;
    }

    if (!slot.TakeItem(single))
    {
      return false;
    }

    dragged.count -= dragged.IngameCount(1);

    if (dragged.count <= 0)
    {
      // ClearDragMethod.Invoke(slot, null);
    }
    else
    {
      UpdateCursorMethod.Invoke(slot, null);
    }

    return true;
  }

  // Take one item from slot to dragged
  private static bool TakeOneItem(UIItemSlot slot, InvGameItem dragged)
  {
    InvGameItem? slotItem = slot.observedItem;
    if (IsBadItem(slotItem))
    {
      return false;
    }

    if (
        slotItem.baseItemID != dragged.baseItemID
        || slotItem.data != dragged.data
        || slotItem.paintData != dragged.paintData
    )
    {
      return false;
    }
    int ingameCount = slotItem.IngameCount(1);

    slotItem.count -= ingameCount;
    dragged.count += ingameCount;

    return true;
  }

}

