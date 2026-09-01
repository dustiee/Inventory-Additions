using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static InventoryAdditions.LogTools;

namespace InventoryAdditions.InvGameItemChanges;

internal partial class ItemStack
{

  internal long StackListElementCount => _runLengthEncodedStack.Count;
  internal long GetStackSize()
  {
    return _runLengthEncodedStack.Sum(entry => entry.ItemInstances);
  }

  internal long GetTotalAvailableDurability()
  {
    return _runLengthEncodedStack.Sum(entry => entry.AvailableDurability);
  }


  internal void ModifyDurability(long durabilityDifference)
  {
    if (durabilityDifference == 0)
    {
      return;
    }

    if (durabilityDifference > 0)
    {
      AddRepairCount(durabilityDifference);
      return;
    }

    if (durabilityDifference < 0)
    {
      DamageCount(durabilityDifference * -1);
      return;
    }
  }

  internal void Merge(ItemStack appendStack)
  {
    if (MaxItemDurability != appendStack.MaxItemDurability)
    {
      throw new ArgumentException("Stacks dont have the same max durability");
    }
    _runLengthEncodedStack.AddRange(appendStack._runLengthEncodedStack);
    UpdateStackEncoding();
  }

  internal static ItemStack Merge(ItemStack baseStack, ItemStack appendStack)
  {
    if (baseStack.MaxItemDurability != appendStack.MaxItemDurability)
    {
      throw new ArgumentException("Stacks dont have the same max durability");
    }
    List<ItemStackEntry> stackResult = new(baseStack._runLengthEncodedStack);
    stackResult.AddRange(appendStack._runLengthEncodedStack);

    ItemStack result = new(stackResult, baseStack._maxItemDurability);
    result.UpdateStackEncoding();
    return result;
  }

  public override string ToString()
  {
    StringBuilder sb = new();
    sb.Append($"_maxItemDurability: {_maxItemDurability} |");

    foreach (ItemStackEntry entry in _runLengthEncodedStack)
    {
      sb.Append($" [ed:{entry.EntryDurability}, ins:{entry.ItemInstances}] ");
    }

    return sb.ToString();
  }


  // Creates new item instances
  private List<ItemStackEntry> NewFromCount(long count)
  {
    long fullItems = count / _maxItemDurability;
    long remainder = count % _maxItemDurability;

    List<ItemStackEntry> result = [];

    if (fullItems > 0)
    {
      result.Add(new(fullItems, _maxItemDurability));
    }

    if (remainder > 0)
    {
      result.Add(new(1, remainder));
    }

    return result;
  }

  // Can destroy item instances
  private void DamageCount(long countToDamage)
  {
    if (countToDamage < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(countToDamage), "Damage count cannot be negative");
    }

    if (countToDamage >= GetTotalAvailableDurability())
    {
      _runLengthEncodedStack = [];
      Debug("Damage count >= total available durability.");
      return;
    }

    for (; ; )
    {
      if (countToDamage <= 0)
      {
        break;
      }

      ItemStackEntry target = _runLengthEncodedStack.Pop(); // front to back 
      if (countToDamage >= target.AvailableDurability)
      {
        countToDamage -= target.AvailableDurability;
        continue;
      }

      long countAvailableAfterDamage = target.AvailableDurability - countToDamage; // != 0 per above
      long baselineItems = countAvailableAfterDamage / target.EntryDurability;
      long remainderItem = countAvailableAfterDamage % target.EntryDurability;


      if (baselineItems > 0)
      {
        _runLengthEncodedStack.Add(new(baselineItems, target.EntryDurability));
      }
      if (remainderItem > 0)
      {
        _runLengthEncodedStack.Add(new(1, remainderItem));
      }
      break;

    }

    UpdateStackEncoding(^1);

  }

  // Does NOT create extra item instances in case of too much repair
  private void AddRepairCount(long countToRepair)
  {
    if (countToRepair < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(countToRepair), "Repair count cannot be negative");
    }
    if (countToRepair == 0)
    {
      return;
    }

    int updatableFromIndex = _runLengthEncodedStack.Count;

    for (int i = _runLengthEncodedStack.Count - 1; i >= 0; i--)
    {
      if (countToRepair <= 0)
      {
        break;
      }

      updatableFromIndex = Math.Max(0, i - 1);

      ItemStackEntry target = _runLengthEncodedStack[i]; // front to back 
      long missingCount = GetMissingDurability(target);

      if (countToRepair >= missingCount)
      {
        countToRepair -= missingCount;
        target.EntryDurability = _maxItemDurability;
        _runLengthEncodedStack[i] = target;
        continue;
      }
      // Last step if we reach this point, do not iterate again after this

      long singleMissingCount = GetMissingSingletonDurability(target);
      long itemsFullyRepaired = countToRepair / singleMissingCount;
      long itemPartialRepair = countToRepair % singleMissingCount;

      long unaffectedItems = target.ItemInstances - itemsFullyRepaired;
      if (itemPartialRepair > 0)
      {
        unaffectedItems--;
      }

      // reconstructing the entry, indexing isn't impacted since we're iterating backwards 
      _runLengthEncodedStack.RemoveAt(i);
      // unaffected > partial -> full
      // each insert shifts previous to the right so i do this in reverse order 

      if (itemsFullyRepaired > 0)
      {
        _runLengthEncodedStack.Insert(i, new(itemsFullyRepaired, _maxItemDurability));
      }

      if (itemPartialRepair > 0)
      {
        _runLengthEncodedStack.Insert(i, new(1, target.EntryDurability + itemPartialRepair));
      }

      if (unaffectedItems > 0)
      {
        _runLengthEncodedStack.Insert(i, new(unaffectedItems, target.EntryDurability));
      }
      break;
    }

    if (countToRepair > 0)
    {
      Debug("Repair count exceeded the amount of points actually repairable. Discarding remaining repair count.");
    }

    UpdateStackEncoding(updatableFromIndex);
    return;

  }

  private void UpdateStackEncoding(Index? inputPosition = null)
  {
    if (_runLengthEncodedStack.Count == 0)
    {
      return;
    }

    int startingPosition = inputPosition?.GetOffset(_runLengthEncodedStack.Count) ?? 0;

    if (startingPosition < 0 || startingPosition >= _runLengthEncodedStack.Count)
    {
      throw new ArgumentOutOfRangeException(nameof(inputPosition));
    }


    int writeIndex = startingPosition;

    for (int readIndex = startingPosition + 1; readIndex < _runLengthEncodedStack.Count; readIndex++)
    {
      ItemStackEntry cursor = _runLengthEncodedStack[writeIndex];
      ItemStackEntry target = _runLengthEncodedStack[readIndex];

      if (target.EntryDurability == cursor.EntryDurability)
      {
        cursor.ItemInstances += target.ItemInstances;
        _runLengthEncodedStack[writeIndex] = cursor;
        continue;
      }

      writeIndex++;
      _runLengthEncodedStack[writeIndex] = target;
    }

    int newCount = writeIndex + 1;
    _runLengthEncodedStack.RemoveRange(newCount, _runLengthEncodedStack.Count - newCount);

  }

  private long GetMissingDurability(ItemStackEntry entry)
  {
    return (_maxItemDurability * entry.ItemInstances) - entry.AvailableDurability;
  }

  private long GetMissingSingletonDurability(ItemStackEntry entry)
  {
    return _maxItemDurability - entry.EntryDurability;
  }
}



