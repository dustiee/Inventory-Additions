
namespace InventoryAdditions.InvGameItemChanges;

internal static class InvGameItemExtensions
{
  internal static ItemStack? GetExtraItemData(this InvGameItem item)
  {
    return GlobalInvGameItemRegistry.GetExtraItemData(item);
  }

  internal static void Register(this InvGameItem item)
  {
    GlobalInvGameItemRegistry.RegisterItem(item);
  }


  internal static string ToSimpleKey(this InvGameItem item)
  {
    return item.baseItemID + "|" + item.data + "|" + item.paintData;
  }

  internal static string ToInfoString(this InvGameItem item)
  {
    return
      $"{item.itemName}|{item.baseItemID}|{item.count}/{item.durability}|d{item.data}|pd{item.paintData}";
  }

}
