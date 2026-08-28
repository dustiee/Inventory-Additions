
namespace InventoryAdditions;

internal sealed class DummyItemSlot : UIItemSlot
{
  public override InvGameItem? observedItem => null;

  public override InvGameItem? Replace(InvGameItem item) => null;
}
