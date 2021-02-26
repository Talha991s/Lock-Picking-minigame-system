using System;
using UnityEngine;

namespace Assignment2
{
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
}
