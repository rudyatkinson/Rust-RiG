using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public enum ItemTypes
{
    All,
    Weapons,
    Construction,
    Items,
    Resources,
    Clothing,
    Tools,
    Medicals,
    Food,
    Ammunation,
    Traps,
    Misc,
    Components,
    Electrical
}

public enum Workbench
{
    Level0,
    Level1,
    Level2,
    Level3,
    MixingTable
}

[CreateAssetMenu]
public class Item : ScriptableObject, IComparable
{
    public Sprite image;
    public string inGameName;
    public ItemTypes type;
    public Item[] requiredItems;
    public int[] requiredItemCount;
    public bool isCraftable;
    public bool isBuildable;
    public bool isRaidable;
    public int craftSize;
    public Workbench workbench;
    public Item[] recycledItem;
    public int[] recycledItemCount;
    public int[] recyclePercentage;
    public Item[] raidItem;
    public int[] raidItemCount;
    public Item[] raidOptimalItem;
    public int[] raidOptimalItemCount;

    public int CompareTo(object obj)
    {
        Item otherItem = obj as Item;
        int result = inGameName.CompareTo(otherItem.inGameName);
        return result;
    }
}
