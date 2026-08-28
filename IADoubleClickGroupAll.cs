using UnityEngine;

using static InventoryAdditions.InventoryTools;
using static InventoryAdditions.LogTools;

namespace InventoryAdditions;

internal static class DoubleClickGroupAll
{
  private static UIItemSlot? _lastClickedSlot;
  private static double _lastClickTimestamp = float.NegativeInfinity;


  internal static void HandleClick(GameObject go)
  {
    if (UIItemSlot.draggedItem != null)
      return;

    UIItemSlot slot = go.GetComponent<UIItemSlot>();
    if (slot == null)
      return;

    double now = Time.realtimeSinceStartupAsDouble;

    if (slot == _lastClickedSlot &&
        now - _lastClickTimestamp <= InventoryAdditions.DoubleClickWindow)
    {
      Verbose($"Grouping items after {now - _lastClickTimestamp}. (Configured to {InventoryAdditions.DoubleClickWindow})");
      GroupInto(slot);
      _lastClickedSlot = null;
      return;
    }

    _lastClickedSlot = slot;
    _lastClickTimestamp = now;
  }

  private static void GroupInto(UIItemSlot target)
  {
    InvGameItem targetItem = target.observedItem;
    if (
        IsBadItem(targetItem)
        || targetItem.count <= 0
       )

    {
      Verbose("Bad item");
      return;
    }

    Transform root =
        target.transform.parent ?? target.transform;
    UIItemSlot[] siblings = root.GetComponentsInChildren<UIItemSlot>(includeInactive: false);

    int statisticItemsGrouped = 0;
    foreach (UIItemSlot other in siblings)
    {
      if (other == target)
      {
        Verbose("Same item slot as target");
        continue;
      }

      InvGameItem otherItem = other.observedItem;

      if (IsBadItem(otherItem) || otherItem.count <= 0)
      {
        Verbose("Item slot has bad item");
        continue;
      }

      if (
          otherItem.baseItemID != targetItem.baseItemID
          || otherItem.data != targetItem.data
          || otherItem.paintData != targetItem.paintData
      )
      {
        Verbose("Item slot does not have same item as target");
        continue;
      }

      if (target.TakeItem(otherItem))
      {
        other.Replace(null);
        statisticItemsGrouped++;
      }
    }
    Debug($"{statisticItemsGrouped} items grouped.");

  }
}
