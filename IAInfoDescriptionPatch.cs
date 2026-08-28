using static InventoryAdditions.InventoryTools;
using HarmonyLib;
using UnityEngine;

namespace InventoryAdditions;


[HarmonyPatch(typeof(RecipeItemDescription))]
public static class RecipeDescriptionPatch
{
  private static string Green(string text) => $"[00FF00]{text}[-]";
  private static string Blue(string text) => $"[0000FF]{text}[-]";
  private static string Purple(string text) => $"[800080]{text}[-]";
  private static string Yellow(string text) => $"[222222]{text}[-]";

  [HarmonyPatch(
          "Initialize",
          [typeof(string), typeof(string)]
      )]
  [HarmonyPrefix]
  private static void PrintName(RecipeItemDescription __instance, string name, ref string description)
  {
    __instance.itemDescription.maxLineCount = 32;
    __instance.itemDescription.transform.localScale = new Vector3(12f, 13f, 1); // 15, 15, 1

    description =
$"""
Drag an item here to view its description.

{Purple("Item Controls:")}
  {Green("LEFT CLICK")} {Blue("HOLD")} -> Pick up one item.
  {Green("RIGHT CLICK")} -> Pick up stack.
  {Green("MIDDLE CLICK")} -> Pick up 1/2 stack.

While items are held:
  {Blue("HOLD")} {Green("LEFT CLICK")} -> {Yellow("SELECT")} hovered slots and equally distribute items across them.
  {Blue("HOLD")} {Green("RIGHT CLICK")} -> {Yellow("SELECT")} hovered slots and add one item to them.

While slots are {Yellow("SELECT")}ed:
  {Green("SCROLL WHEEL UP")} -> add one item to each slot.
  {Green("SCROLL WHEEL DOWN")} -> remove one item from each slot.

Let go of both {Green("LEFT CLICK")} and {Green("RIGHT CLICK")} to {Yellow("UNSELECT")} all slots.

{Purple("Other:")}
  {Blue("DOUBLE")} {Green("LEFT CLICK")} -> Collect all items of this type.
  {Blue("SHIFT")} + {Green("LEFT CLICK")} on crafting output to craft as many copies of output given current input.

""";
  }

  // return size to original for normal items
  [HarmonyPatch(
          "Initialize",
          [
            typeof(InvGameItem),
            typeof(Color),
            typeof(NumberExperiment),
            typeof(bool)
          ]
      )]
  [HarmonyPrefix]
  private static void FixSize(RecipeItemDescription __instance)
  {
    __instance.itemDescription.transform.localScale = new Vector3(15f, 15f, 1); // 15, 15, 1
  }
}

