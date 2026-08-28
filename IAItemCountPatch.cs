using HarmonyLib;
using static InventoryAdditions.LogTools;
using UnityEngine;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace InventoryAdditions;


internal class RefVector3(Vector3 v)
{
  internal Vector3 Value = v;
}

// Scaling so large quantities of items dont look ridiculous
[HarmonyPatch(typeof(UIItemSlot))]
internal class ItemCountPatch
{
  private static readonly ConditionalWeakTable<UILabel, RefVector3> _originalScales = [];

  [HarmonyPostfix]
  [HarmonyPatch("UpdateItem")]
  public static void UpdateItemPostfix(UIItemSlot __instance)
  {

    InvGameItem item = __instance.observedItem;
    UILabel label = __instance.countLabel;
    if (label == null || item == null)
    {
      Verbose("No label or item");
      RemoveExtraLabel(__instance);
      return;
    }


    Verbose($"Observed item is {__instance.observedItem?.name}");

    if (!_originalScales.TryGetValue(label, out _))
    {
      _originalScales.Add(label, new RefVector3(label.transform.localScale));
    }

    string baseitemName = item.name;
    if (baseitemName == "Antique Spawner" && item.itemName != "Antique Spawner") // Don't add a label if it's exactly just 
                                                                                 // "Antique Spawner", there is no mob to label
    {
      string thisItemName = item.itemName;


      string prefixName = thisItemName.Replace("Antique Spawner", "").Trim();


      string initials = string.Join(". ", prefixName
          .Split(' ', StringSplitOptions.RemoveEmptyEntries)
          .Select(word => word[0])) + ".";

      EnsureExtraLabel(__instance, initials);


      Verbose($"I see a spawner with data {item.data} and {thisItemName}");
    }
    else
    {
      RemoveExtraLabel(__instance);
    }



    int digits = item.stackCount.ToString().Trim().Length;

    float multiplier = 1f;

    if (digits >= 3)
    {
      multiplier = 1f - ((digits - 1) * 0.1f);
      multiplier = Mathf.Max(multiplier, 0.5f);
    }
    if (!_originalScales.TryGetValue(label, out RefVector3 storedScale))
    {
      Warn("UILabel disappeared during the update");
      return;
    }

    Vector3 original = storedScale.Value;
    label.transform.localScale = new Vector3(
        original.x * multiplier,
        original.y * multiplier,
        original.z
    );

    label.MarkAsChanged();
  }

  private static void EnsureExtraLabel(UIItemSlot Slot, string LabelText)
  {
    Transform countTransform = Slot.transform.Find("Count");

    if (countTransform == null)
    {
      Verbose("No count transform");
      return;
    }

    Transform extraTransform = Slot.transform.Find("IUExtra");

    if (extraTransform == null)
    {
      GameObject extraObject = UnityEngine.Object.Instantiate(
          countTransform.gameObject,
          countTransform.parent,
          false
      );

      extraObject.name = "IUExtra";
      extraTransform = extraObject.transform;

      extraTransform.localPosition = countTransform.localPosition + new Vector3(+15f, -70f, 0f);
      extraTransform.localRotation = countTransform.localRotation;
    }

    extraTransform.gameObject.SetActive(true);

    extraTransform.localScale = new Vector3(30f, 30f, 1f);

    UILabel extraLabel = extraTransform.GetComponent<UILabel>();

    if (extraLabel != null)
    {
      extraLabel.text = LabelText;
    }
    else
    {
      Warn("No extraLabel");
    }
  }

  private static void RemoveExtraLabel(UIItemSlot Slot)
  {
    Transform extraTransform = Slot.transform.Find("IUExtra");

    if (extraTransform != null)
    {
      UnityEngine.Object.Destroy(extraTransform.gameObject);
      Verbose("Extra label removed");
    }
  }
}
