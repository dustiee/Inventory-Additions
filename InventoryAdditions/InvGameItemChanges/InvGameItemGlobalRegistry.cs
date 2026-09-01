using System.Runtime.CompilerServices;

using static InventoryAdditions.LogTools;

namespace InventoryAdditions.InvGameItemChanges;

internal static class GlobalInvGameItemRegistry
{
  private static readonly ConditionalWeakTable<InvGameItem, ItemStack> _registry = [];



  internal static ItemStack? GetExtraItemData(InvGameItem item)
  {
    if (!_registry.TryGetValue(item, out ItemStack? data))
    {
      return null;
    }
    return data;
  }


  internal static void RegisterItem(InvGameItem item)
  {
    RegisterItem(item, new(item));
  }

  internal static void RegisterItem(InvGameItem item, ItemStack data)
  {
    if (!_registry.TryGetValue(item, out _))
    {
      _registry.Add(item, data);
      Verbose("Item registered");

      return;
    }

    Warn("Tried to register an item that already existed : " + item.ToInfoString());
    return;
  }
}
