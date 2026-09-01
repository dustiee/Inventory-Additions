using System;
using System.Collections.Generic;

namespace InventoryAdditions.InvGameItemChanges; // this should be moved  elsewhere

internal static class ListExtensions
{
  /// <summary>
  /// Returns the element at the end of the list, and also removes it from that list.
  /// The list must not be empty.
  /// </summary>
  internal static T Pop<T>(this List<T> list)
  {
    if (list.Count <= 0)
    {
      throw new InvalidOperationException("List is empty.");
    }

    T element = list[^1];
    list.RemoveAt(list.Count - 1);
    return element;
  }
}
