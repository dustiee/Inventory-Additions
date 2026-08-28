using HarmonyLib;
using UnityEngine;
using static InventoryAdditions.SorterTools;
using static InventoryAdditions.InventoryTools;
using static InventoryAdditions.LogTools;


[HarmonyPatch(typeof(Inventory))]
public class InventoryButtonPatch
{
  private static GameObject? _organizeInventoryButton;
  private static GameObject? _chestOrganizeButton;

  [HarmonyPostfix]
  [HarmonyPatch("ShowChest")]
  public static void AfterShowChest(Inventory __instance, GameObject chest)
  {
    EnsureInventorySortButtonExists();

    if (__instance.invChest == null)
      return;

    if (_chestOrganizeButton != null)
      return;

    GameObject chestRoot = __instance.invChest;

    _chestOrganizeButton = CreateButtonWithHandler(
        "[InventoryAdditions] Chest Organize Button",
        new Vector3(-172f, 102f, 0f),
        "Sort",
        OrganizeThisChest,
        chestRoot
    );
  }

  private static void OrganizeThisChest(GameObject button)
  {
    Transform chest = button.transform.parent.Find("Chest");
    if (chest == null)
    {
      return;
    }
    Transform chestSlots = chest.Find("ChestSlots");
    if (chestSlots == null)
    {
      return;
    }

    SortContainer(chestSlots.gameObject);

    Debug("Sorted chest");
  }

  [HarmonyPostfix]
  [HarmonyPatch("ShowInventory")]
  public static void AfterShowInventory()
  {
    EnsureInventorySortButtonExists();
  }

  private static void EnsureInventorySortButtonExists()
  {
    if (_organizeInventoryButton == null)
    {
      GameObject inventoryParent = GameObject.Find(
          "PlayerInventory/Panel/Inventory/InvAnchor"
      );

      if (inventoryParent == null)
      {
        Debug("Could not find inventory parent");
        return;
      }

      _organizeInventoryButton = CreateButtonWithHandler(
          "[InventoryAdditions] Organize Button",
          new Vector3(540f, 335f, 0f),
          "Sort",
          OrganizeClicked,
          inventoryParent
      );
    }

  }

  private static void OrganizeClicked(GameObject button)
  {
    Transform invStorage = button.transform.parent.Find("InvPanel").Find("InvStorage");

    if (invStorage == null)
    {
      Debug("Couldn't find InvStorage");
      return;
    }

    SortContainer(invStorage.gameObject);
  }
}
