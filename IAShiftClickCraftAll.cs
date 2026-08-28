using HarmonyLib;
using UnityEngine;

namespace InventoryAdditions;

[HarmonyPatch(typeof(CraftingOutputSlot), nameof(CraftingOutputSlot.OnClick))]
public static class CraftingOutputSlot_OnClick_Patch
{
  static bool Prefix(CraftingOutputSlot __instance)
  {
    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
    {
      __instance.DoCraft(int.MaxValue);
      return false;
    }

    return true;
  }
}
