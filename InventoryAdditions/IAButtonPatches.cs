using HarmonyLib;
using UnityEngine;
using static InventoryAdditions.SorterTools;
using static InventoryAdditions.LogTools;
using static InventoryAdditions.InventoryTools;
using System.Collections.Generic;


[HarmonyPatch(typeof(Inventory))]
public class InventoryButtonPatch
{
  private static GameObject? _organizeInventoryButton;
  private static GameObject? _chestOrganizeButton;
  private static GameObject? _chestQuickStackButton;

  private static GameObject? InventoryParent => GameObject.Find(
            "PlayerInventory/Panel/Inventory/InvAnchor"
        );


  private static void QuickStackIntoChest(GameObject button)
  {

    Transform chest = button.transform.parent.Find("Chest");
    if (chest == null)
    {
      Warn("Could not find Chest");
      return;
    }
    Transform chestSlots = chest.Find("ChestSlots");
    if (chestSlots == null)
    {
      Warn("Could not find chest slots");
      return;
    }

    GameObject? inventoryParent = InventoryParent;
    if (inventoryParent == null)
    {
      Warn("Could not find inventory parent");
      return;
    }

    Transform? invStorage = GameObject.Find("PlayerInventory/Panel/Inventory/InvAnchor/InvPanel/InvStorage")?.transform;
    if (invStorage == null)
    {
      Warn("Could not find invStorage");
      return;
    }

    List<UIItemSlot> validChestSlots = [];
    foreach (Transform child in chestSlots)
    {
      UIItemSlot slot = child.GetComponent<UIItemSlot>();
      if (slot == null)
        continue;

      if (IsBadSlot(slot))
        continue;

      validChestSlots.Add(slot);
    }

    if (validChestSlots.Count == 0)
    {
      Debug("No valid chest slots found");
      return;
    }

    foreach (Transform child in invStorage)
    {
      UIItemSlot invSlot = child.GetComponent<UIItemSlot>();
      if (invSlot == null)
        continue;

      InvGameItem? invItem = invSlot.observedItem;
      if (IsBadItem(invItem))
        continue;

      foreach (UIItemSlot chestSlot in validChestSlots)
      {
        InvGameItem? chestItem = chestSlot.observedItem;

        if (
            chestItem == null
            || chestItem.baseItemID != invItem.baseItemID
            || chestItem.data != invItem.data
            || chestItem.paintData != invItem.paintData
        )
        {
          continue;
        }

        if (chestSlot.TakeItem(invItem))
        {
          invSlot.Replace(null);
          break;
        }

        invItem = invSlot.observedItem;
        if (IsBadItem(invItem))
          break;
      }
    }

    Debug("Quick stack OK");
  }

  [HarmonyPostfix]
  [HarmonyPatch("ShowChest")]
  public static void AfterShowChest(Inventory __instance, GameObject chest)
  {
    EnsureInventorySortButtonExists();

    if (__instance.invChest == null)
    {
      return;
    }

    if (_chestOrganizeButton != null && _chestQuickStackButton != null)
    {
      return;
    }


    GameObject chestRoot = __instance.invChest;

    _chestOrganizeButton = CreateButtonWithHandler(
        "[InventoryAdditions] Chest Organize Button",
        new Vector3(-172f, 102f, 0f),
        "Sort",
        OrganizeThisChest,
        chestRoot
    );

    _chestQuickStackButton = CreateButtonWithHandler(
        "[InventoryAdditions] Chest Quick Stack Button",
        new Vector3(-172f, 75f, 0f),
        "QS",
        QuickStackIntoChest,
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
      GameObject? inventoryParent = InventoryParent;
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
    Transform invStorage = button.transform.parent.Find("InvPanel/InvStorage");

    if (invStorage == null)
    {
      Debug("Couldn't find InvStorage");
      return;
    }

    SortContainer(invStorage.gameObject);
  }
}
