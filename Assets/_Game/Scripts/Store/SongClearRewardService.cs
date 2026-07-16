/// <summary>Defines the temporary Money reward for a successfully cleared chart.</summary>
public static class SongClearRewardService
{
    public const int MoneyPerClear = 500;

    public static int GrantForClear(SongData song, GameplayResultData result)
    {
        if (song == null || !result.passed)
            return 0;

        PlayerWallet.Add(CurrencyType.Money, MoneyPerClear);
        WalletTransactionJournal.Record(
            "song_clear_reward",
            CurrencyType.Money,
            MoneyPerClear,
            SongUnlockService.GetSongItemId(song));
        return MoneyPerClear;
    }
}
