/// <summary>
/// Purchase/unlock rules for songs. Kept separate from UI so account/server integration has one clear seam.
/// </summary>
public static class SongUnlockService
{
    public static string GetSongItemId(SongData song)
    {
        if (song == null)
            return string.Empty;

        string groupId = !string.IsNullOrWhiteSpace(song.songGroupId) ? song.songGroupId : song.name;
        return "song:" + SongData.SanitizeForFileName(groupId);
    }

    public static bool IsUnlocked(SongData song)
    {
        if (song == null)
            return false;

        return song.unlockType == SongUnlockType.Free || PlayerInventory.IsOwned(GetSongItemId(song));
    }

    public static int GetPrice(SongData song, CurrencyType currency)
    {
        if (song == null || song.unlockType == SongUnlockType.Free)
            return 0;

        return currency == CurrencyType.Diamond ? song.diamondPrice : song.moneyPrice;
    }

    public static bool TryPurchase(SongData song, CurrencyType currency, out string message)
    {
        message = string.Empty;

        if (song == null)
        {
            message = "No song selected.";
            return false;
        }

        if (song.unlockType == SongUnlockType.Free || IsUnlocked(song))
        {
            message = "Song is already unlocked.";
            return true;
        }

        int price = GetPrice(song, currency);
        if (price <= 0)
        {
            message = "This song has no valid price.";
            return false;
        }

        if (!PlayerWallet.TrySpend(currency, price))
        {
            message = $"Not enough {currency}. Need {price}, have {PlayerWallet.GetBalance(currency)}.";
            return false;
        }

        PlayerInventory.SetOwned(GetSongItemId(song));
        WalletTransactionJournal.Record("song_purchase", currency, price, GetSongItemId(song));
        message = $"Unlocked {song.SongTitle} for {price} {currency}.";
        return true;
    }
}
