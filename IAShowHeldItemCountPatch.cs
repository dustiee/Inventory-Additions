using System;
using System.Text;
using HarmonyLib;
using UnityEngine;

using static InventoryAdditions.LogTools;
using Object = UnityEngine.Object;

namespace InventoryAdditions;

[HarmonyPatch(typeof(UICursor), "Update")]
internal static class UICursorShowHeldItemCountPatch
{
  private static UILabel? _dragLabel;
  private static InvGameItem? Dragged => UIItemSlot.draggedItem;

  private static void Postfix(UICursor __instance)
  {
    try
    {
      if (!UICursor.itemDrag)
      {
        if (_dragLabel != null)
        {
          Object.Destroy(_dragLabel.gameObject);
          _dragLabel = null;
        }

        return;
      }

      UILabel source =
          (UILabel)AccessTools.Field(typeof(UICursor), "lable")
              .GetValue(__instance);

      if (source == null)
        return;

      if (_dragLabel == null)
      {
        _dragLabel = Object.Instantiate(source, source.transform.parent);
        _dragLabel.name = source.name + "_DragClone";
        _dragLabel.depth = source.depth + 1;
      }

      if (Dragged != null)
      {
        _dragLabel.text = GetCountString(Dragged);
        _dragLabel.color = source.color;
        _dragLabel.font = source.font;
      }

      Camera cam = UICursor.cursorCamera;

      if (cam != null)
      {
        Vector3 p = source.transform.position;

        Vector3 screenA = cam.WorldToScreenPoint(p);
        Vector3 screenB = screenA + new Vector3(0f, -100f, 0f);

        Vector3 worldA = cam.ScreenToWorldPoint(
            new Vector3(screenA.x, screenA.y, screenA.z));

        Vector3 worldB = cam.ScreenToWorldPoint(
            new Vector3(screenB.x, screenB.y, screenA.z));

        Vector3 offset = worldB - worldA;

        _dragLabel.transform.position = p + offset;
      }
      else
      {
        _dragLabel.transform.position =
            source.transform.position + Vector3.down * 100f;
      }

      _dragLabel.transform.rotation = source.transform.rotation;
      _dragLabel.transform.localScale = source.transform.localScale;

    }
    catch (Exception ex)
    {
      Error(ex);
    }
  }

  private static string GetCountString(InvGameItem item)
  {
    StringBuilder sb = new();
    int? trackedExact = PaintDistributor.TrackedCount;
    if (trackedExact != null)
    {
      sb.Append($"[44ffff]{trackedExact}[-] ");
    }
    if (item.count == 0)
    {
      sb.Append("[AD052A]0[-]");
      return sb.ToString();
    }

    if (item.durability == 1)
    {
      sb.Append(item.count.ToString());
      return sb.ToString();
    }


    int fullCount = item.count / item.durability;
    int remainderCount = item.count % item.durability;

    if (fullCount == 0)
    {
      sb.Append($"{item.count}/{item.durability}");
      return sb.ToString();
    }
    if (remainderCount == 0)
    {
      sb.Append($"{fullCount} (x{item.durability})");
      return sb.ToString();
    }
    sb.Append($"{fullCount} + {remainderCount}/{item.durability}");
    return sb.ToString();
  }
}
