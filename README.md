# Inventory Additions 
## Description

A BepInEx plugin for Block Story that adds the following changes:

0. Enables equal and manual distribution across selected slots 
0. Enables crafting many copies of an item at once by Shift Clicking an item output slot (Does not apply to cauldron/furnaces)
0. Enables double-clicking an item to stack all compatible copies within the same storage
0. Displays the count of items held in the cursor, alongside their max durability
0. Displays initials of a mob's name that an Antique Spawner produces 
0. Scales down the labels that display the item counts of items, such that they don't overlap
0. Provides a sort button for chests and the player's inventory. Items are sorted by category.
0. Provides a quick-stack button to chests, which automatically stacks an item from your inventory into the chest, if the 
   chest has a slot containing that item.

For documentation and controls, press the "(i)" button in the inventory screen.

### Tips

If you're trying to pick up stacks of items and find them automatically getting distributed across slots you hover over,
you're probably pressing RIGHT CLICK to pick up a stack, and then not releasing it. Press RIGHT CLICK once to pick up 
a stack, and press it again without dragging to drop it.

You can pick up and move a single item from a stack by pressing left click and dragging, as in the base game.

**NOTE:** Distribution and auto-stacking does not work on damaged items. This is intentional. They may be better 
supported in the future.

**WARNING:** This mod may have some minor quirks, as it is quite new. I have made a best effort to ensure there 
isn't anything game breaking, so while this is likely stable enough to be playable, there may be a risk of unexpected behaviour.


## Installation 

Download the latest release and move ```InventoryAdditions.dll``` into ```/path/to/BlockStory/BepInEx/plugins/```

### Requirements

0. BepInEx properly installed in the BlockStory directory. Installation guide for BepInEx is available <u>[here](https://docs.bepinex.dev/articles/user_guide/installation/index.html).</u>

## Building prerequisites

You'll need the game's assemblies, so you'll need to paste Assembly-CSharp.dll from the game's ```Managed``` folder into 
```./InventoryAdditions/lib/```

## Disclaimer

This software is publicly available in the hope that it will be useful. I do not take responsibility for maintaining or 
improving it in the future.
