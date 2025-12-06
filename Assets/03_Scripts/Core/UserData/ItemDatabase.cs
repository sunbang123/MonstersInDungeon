using System.Collections.Generic;
using UnityEngine;

public static class ItemDatabase
{
    private static Dictionary<string, ItemData> items = new Dictionary<string, ItemData>();

    public static void Register(ItemData data)
    {
        if (!items.ContainsKey(data.itemName))
            items.Add(data.itemName, data);
    }

    public static ItemData GetItem(string name)
    {
        if (items.ContainsKey(name))
            return items[name];

        Debug.LogWarning($"Item not found: {name}");
        return null;
    }
}
