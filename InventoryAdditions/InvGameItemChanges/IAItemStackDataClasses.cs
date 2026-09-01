using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("InventoryAdditions.Tests")]

namespace InventoryAdditions.InvGameItemChanges;

internal partial class ItemStack
{
  // The last item of the list is what should be displayed 
  private List<ItemStackEntry> _runLengthEncodedStack;
  internal long MaxItemDurability { get { return _maxItemDurability; } }
  private readonly long _maxItemDurability;

  // Constructors
  internal ItemStack(InvGameItem item)
      : this(new ItemData(item))
  { }
  internal ItemStack(ItemStack stack)
    : this(stack._runLengthEncodedStack, stack.MaxItemDurability)
  { }

  internal ItemStack(ItemData data)
  {
    if (data.MaxItemDurability <= 0 || data.InvGameItemCount < 0)
    {
      throw new ArgumentException("Item cannot have <= 0 durability or < 0 count");
    }

    _maxItemDurability = data.MaxItemDurability;
    _runLengthEncodedStack = NewFromCount(data.InvGameItemCount);
  }

  internal ItemStack(List<ItemStackEntry> RunLengthEncodedStack, long MaxItemDurability)
  {
    _maxItemDurability = MaxItemDurability;
    _runLengthEncodedStack = [.. RunLengthEncodedStack];
  }

}


internal struct ItemData
{
  internal long InvGameItemCount;
  internal long MaxItemDurability;

  internal ItemData(InvGameItem item)
  : this(item.count, item.durability)
  { }

  internal ItemData(long itemCount, long itemDurability)
  {
    InvGameItemCount = itemCount;
    MaxItemDurability = itemDurability;
  }
}

internal struct ItemStackEntry
{

  internal ItemStackEntry(long itemInstances, long entryDurability)
  {
    ItemInstances = itemInstances;
    EntryDurability = entryDurability;
  }

  private long _itemInstances;
  internal long ItemInstances
  {
    readonly get => _itemInstances;
    set
    {
      if (value < 0) // can be expected to be 0 in some cases
      {
        throw new ArgumentOutOfRangeException(nameof(value), "item instances cannot be < 0 ");
      }
      _itemInstances = value;
    }
  }

  private long _entryDurability;
  internal long EntryDurability
  {
    readonly get => _entryDurability;
    set
    {
      if (value <= 0)
      {
        throw new ArgumentOutOfRangeException(nameof(value), "entry durability cannot be <= 0 ");
      }
      _entryDurability = value;
    }
  }

  internal readonly long AvailableDurability => ItemInstances * EntryDurability;
}
