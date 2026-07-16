using System;
using UnityEngine;

/// <summary>
/// Local ownership storage. Server inventory can replace this later without changing UI flow.
/// </summary>
public static class PlayerInventory
{
    private const string OwnedPrefix = "RhythmGame.Inventory.Owned.";

    public static event Action Changed;

    public static bool IsOwned(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            return false;

        return PlayerPrefs.GetInt(OwnedPrefix + itemId, 0) == 1;
    }

    public static void SetOwned(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            return;

        PlayerPrefs.SetInt(OwnedPrefix + itemId, 1);
        PlayerPrefs.Save();
        Changed?.Invoke();
    }

    public static void RemoveForTesting(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            return;

        PlayerPrefs.DeleteKey(OwnedPrefix + itemId);
        PlayerPrefs.Save();
        Changed?.Invoke();
    }
}
