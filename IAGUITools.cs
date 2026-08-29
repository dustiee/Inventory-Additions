using System;
using System.Collections.Generic;
using UnityEngine;
using static InventoryAdditions.InventoryTools;
using static InventoryAdditions.LogTools;

namespace InventoryAdditions;

// Specific for IUSorters
internal static class SorterTools
{
  // AchievementsButton is loaded and stays active most of the time
  private const string _sourceButtonPath =
      "PlayerHUD/Panel/HUD/Buttons/Transform/Tween/Table/AchievementsButton";

  // Create a generic button with a handler cloned from achievementsbutton
  public static GameObject? CreateButtonWithHandler(
      string name,
      Vector3 position,
      string labelText,
      Action<GameObject> clickHandler,
      GameObject parent
  )
  {
    GameObject? sourceButton = GameObject.Find(_sourceButtonPath);

    if (sourceButton == null)
    {
      Debug("Could not find source button");
      return null;
    }

    GameObject buttonObject = UnityEngine.Object.Instantiate(sourceButton);
    buttonObject.name = name;
    SetLayerRecursive(buttonObject, 12); // inventory layer, works well for buttons here
                                         // should be changed if needed since some elements like the info panel
                                         // use layer 13-14, best to add a param to this function if that's needed

    buttonObject.transform.parent = parent.transform;

    // Ensure size is consistent with each new one we make, otherwise it can vary
    // depending on its parent
    buttonObject.transform.localPosition = position;
    Vector3 sourceWorldScale = sourceButton.transform.lossyScale;

    buttonObject.transform.localScale = new Vector3(
        sourceWorldScale.x / parent.transform.lossyScale.x,
        sourceWorldScale.y / parent.transform.lossyScale.y,
        sourceWorldScale.z / parent.transform.lossyScale.z
    );

    // some achievement component remenants, keeping these will make it behave like an achievement button
    RemoveComponentByName(buttonObject, "Attention");
    RemoveComponentByName(buttonObject, "IfNotCreative");
    RemoveComponentByName(buttonObject, "UIButtonMessage");

    RemoveChildObject(buttonObject, "Front"); // graphic sprite
    RemoveFirstChildObject(buttonObject, "Label"); // has 2 labels, only need one

    ReplaceClickHandler(buttonObject, clickHandler);

    // HAVE to do this otherwise it wont work
    WaitOneFrame(() =>
    {
      Transform labelTransform = buttonObject.transform.Find("Label");

      if (labelTransform == null)
        return;

      labelTransform.localPosition = new Vector3(0f, 0f, labelTransform.localPosition.z);

      UILabel label = labelTransform.GetComponent<UILabel>();

      if (label != null)
      {
        label.text = labelText;
        label.pivot = UIWidget.Pivot.Center;
        label.transform.localScale = new Vector3(20f, 20f, 1f);
        label.transform.localPosition = new Vector3(0f, 0f, 0f);
      }
    });

    return buttonObject;
  }

  // === Sorting ===
  private static readonly InvBaseItem.CreativeCategory[] _categoryOrder =
  [
        InvBaseItem.CreativeCategory.Animals,
        InvBaseItem.CreativeCategory.Vehicles,
        InvBaseItem.CreativeCategory.Armor,
        InvBaseItem.CreativeCategory.Weapons,
        InvBaseItem.CreativeCategory.Tools,
        InvBaseItem.CreativeCategory.Items,
        InvBaseItem.CreativeCategory.Foods,
        InvBaseItem.CreativeCategory.Blocks,
        InvBaseItem.CreativeCategory.Plants,
        InvBaseItem.CreativeCategory.Hide,
    ];

  public static void SortContainer(GameObject containerObject)
  {
    List<UIItemSlot> slots = [];

    foreach (Transform child in containerObject.transform)
    {
      UIItemSlot slot = child.GetComponent<UIItemSlot>();

      if (slot == null)
        continue;

      slots.Add(slot);
    }

    if (slots.Count == 0)
    {
      Debug("No UIItemSlots found");
      return;
    }

    List<InvGameItem> items = [];

    foreach (UIItemSlot slot in slots)
    {
      if (slot is UIStorageSlot storageSlot)
      {
        if (storageSlot.slot == null || storageSlot.slot.Locked)
          continue;
      }

      if (slot.observedItem != null)
        items.Add(slot.observedItem);
    }

    List<InvGameItem> finalItems = MergeItems(items);

    finalItems.Sort(CompareItems);

    int index = 0;

    foreach (UIItemSlot slot in slots)
    {
      if (slot is UIStorageSlot storageSlot)
      {
        if (storageSlot.slot == null || storageSlot.slot.Locked)
          continue;
      }

      if (slot is UIStorageSlot storage)
      {
        if (index < finalItems.Count)
          storage.Replace(finalItems[index++]);

        else
          storage.Replace(null);


      }


      // slot.UpdateItem();
    }
  }



  private static void ReplaceClickHandler(GameObject button, Action<GameObject> handler)
  {
    UIEventListener listener = UIEventListener.Get(button);
    listener.onClick = null;
    listener.onClick += delegate (GameObject go)
    {
      handler?.Invoke(go);
    };
  }

  private static void RemoveComponentByName(GameObject obj, string componentName)
  {
    foreach (Component component in obj.GetComponents<Component>())
    {
      if (component != null && component.GetType().Name == componentName)
      {
        UnityEngine.Object.Destroy(component);
      }
    }

    foreach (Transform child in obj.transform)
    {
      RemoveComponentByName(child.gameObject, componentName);
    }
  }

  private static void RemoveChildObject(GameObject parent, string childName)
  {
    Transform child = parent.transform.Find(childName);

    if (child != null)
    {
      UnityEngine.Object.Destroy(child.gameObject);
    }
  }

  private static void RemoveFirstChildObject(GameObject parent, string childName)
  {
    foreach (Transform child in parent.transform)
    {
      if (child.name == childName)
      {
        UnityEngine.Object.Destroy(child.gameObject);
        return;
      }
    }
  }

  private static void SetLayerRecursive(GameObject obj, int layer)
  {
    obj.layer = layer;

    foreach (Transform child in obj.transform)
    {
      SetLayerRecursive(child.gameObject, layer);
    }
  }

  private static void WaitOneFrame(Action action)
  {
    GameObject runner = new("[InventoryAdditions] CoroutineRunner");
    UnityEngine.Object.DontDestroyOnLoad(runner);
    runner.AddComponent<CoroutineRunner>().StartCoroutine(ExecuteNextFrame(action));
  }

  private static System.Collections.IEnumerator ExecuteNextFrame(Action action)
  {
    yield return null;
    action?.Invoke();
    UnityEngine.Object.Destroy(GameObject.Find("[InventoryAdditions] CoroutineRunner"));
  }

  private class CoroutineRunner : MonoBehaviour { }

  private static List<InvGameItem> MergeItems(List<InvGameItem> items)
  {
    Dictionary<string, InvGameItem> merged = [];
    List<InvGameItem> result = [];

    foreach (InvGameItem item in items)
    {
      if (IsBadItem(item))
      {
        result.Add(item);
        continue;
      }

      string key = item.baseItemID + "|" + item.data + "|" + item.paintData;

      if (merged.TryGetValue(key, out InvGameItem existing))
      {
        existing.count += item.count;
      }
      else
      {
        merged[key] = item;
        result.Add(item);
      }
    }

    return result;
  }

  private static int CompareItems(InvGameItem a, InvGameItem b)
  {
    int categoryA = GetCategoryIndex(a.baseItem.category);
    int categoryB = GetCategoryIndex(b.baseItem.category);

    int categoryCompare = categoryA.CompareTo(categoryB);

    if (categoryCompare != 0)
    {
      return categoryCompare;
    }

    return string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase);
  }

  private static int GetCategoryIndex(InvBaseItem.CreativeCategory category)
  {
    for (int i = 0; i < _categoryOrder.Length; i++)
    {
      if (_categoryOrder[i] == category)
        return i;
    }

    return int.MaxValue;
  }
}
