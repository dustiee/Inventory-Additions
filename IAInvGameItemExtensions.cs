namespace InventoryAdditions;

internal static class InvGameItemExtensions
{
  internal static int? ExactItemCount(this InvGameItem item)
  {
    if (item.count % item.durability != 0)
    {
      return null;
    }

    return item.count / item.durability;
  }

  internal static int IngameCount(this InvGameItem item, int ExactItemCount)
  {
    return item.durability * ExactItemCount;
  }
}
