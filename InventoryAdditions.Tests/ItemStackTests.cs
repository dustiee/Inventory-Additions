
using InventoryAdditions.InvGameItemChanges;
using Xunit.Abstractions;

namespace InventoryAdditions.Tests;

public class ItemStackTests(ITestOutputHelper output)
{

  private readonly ITestOutputHelper _output = output;


  [Fact]
  public void CreationTest()
  {
    //Given
    ItemData data = new(100, 15);

    //When
    ItemStack stack = new(data);

    //Then
    Assert.Equal(7, stack.GetStackSize());
    Assert.Equal(2, stack.StackListElementCount);
  }

  [Fact]
  public void RepairTest()
  {
    //Given
    ItemStack stackA = new(new ItemData(61, 50));


    //When
    stackA.ModifyDurability(100);

    //Then
    Assert.Equal(2, stackA.GetStackSize());
    Assert.Equal(1, stackA.StackListElementCount);
    Assert.Equal(100, stackA.GetTotalAvailableDurability());
  }


  [Fact]
  public void MergeTest()
  {
    //Given
    ItemStack stackA = new(new ItemData(15, 50));
    ItemStack stackB = new(new ItemData(35, 50));
    ItemStack stackC = new(new ItemData(75, 100));
    ItemStack stackD = new(new ItemData(100, 50)); // 2 items
    ItemStack stackE = new(new ItemData(50, 50));

    ItemStack stackAMutate = new(stackA);
    ItemStack stackBMutate = new(stackB);

    //When

    ItemStack stackAB = ItemStack.Merge(stackA, stackB);
    _output.WriteLine($"AB: {stackAB}");
    ItemStack stackBA = ItemStack.Merge(stackB, stackA);
    _output.WriteLine($"BA: {stackBA}");

    ItemStack stackDE = ItemStack.Merge(stackD, stackE);
    _output.WriteLine($"DE: {stackDE}");
    ItemStack stackED = ItemStack.Merge(stackE, stackD);
    _output.WriteLine($"ED: {stackED}");

    stackAMutate.Merge(stackB);
    _output.WriteLine($"Amutate: {stackAMutate}");

    stackBMutate.Merge(stackBMutate); // 2
    stackBMutate.Merge(stackBMutate); // 4

    _output.WriteLine($"Bmutate: {stackBMutate}");

    //Then

    Assert.Equal(2, stackAB.StackListElementCount);
    Assert.Equal(2, stackAB.GetStackSize());

    Assert.Equal(2, stackBA.StackListElementCount);
    Assert.Equal(2, stackBA.GetStackSize());

    Assert.Equal(stackAB.GetTotalAvailableDurability(), stackBA.GetTotalAvailableDurability());
    Assert.Equal(stackAB.GetStackSize(), stackBA.GetStackSize());


    Assert.Equal(3, stackDE.GetStackSize());
    Assert.Equal(3, stackED.GetStackSize());
    Assert.Equal(1, stackDE.StackListElementCount);
    Assert.Equal(1, stackED.StackListElementCount);

    Assert.Equal(100 + 50, stackED.GetTotalAvailableDurability());
    Assert.Equal(100 + 50, stackDE.GetTotalAvailableDurability());

    Assert.Equal(2, stackAMutate.GetStackSize());
    Assert.Equal(2, stackAMutate.StackListElementCount);

    Assert.Equal(1, stackBMutate.StackListElementCount);
    Assert.Equal(4, stackBMutate.GetStackSize());
    Assert.Equal(35 * 4, stackBMutate.GetTotalAvailableDurability());

    Assert.Throws<ArgumentException>(() =>
    {
      ItemStack badStack = ItemStack.Merge(stackA, stackC);
    });

  }
}
