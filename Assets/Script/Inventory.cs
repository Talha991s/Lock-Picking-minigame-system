using System;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public Item[] items;
    [Serializable]
    public class Item
    {
        public string tool = "lockpicks";
        public int count = 1;
    }
}
