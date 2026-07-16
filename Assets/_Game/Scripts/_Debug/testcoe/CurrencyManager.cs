using System;

/// <summary>
/// Compatibility wrapper for older Store UI. Both old and new Store code use
/// PlayerWallet, so the balance stays identical across every scene.
/// </summary>
public static class CurrencyManager
{
    public const int StartingCoins = 0;
    public const int StartingDiamonds = 0;

    public static event Action OnChanged;

    static CurrencyManager()
    {
        PlayerWallet.Changed += NotifyChanged;
    }

    public static int Coins
    {
        get { return PlayerWallet.Money; }
    }

    public static int Diamonds
    {
        get { return PlayerWallet.Diamond; }
    }

    public static bool TrySpendCoins(int amount)
    {
        return PlayerWallet.TrySpend(CurrencyType.Money, amount);
    }

    public static bool TrySpendDiamonds(int amount)
    {
        return PlayerWallet.TrySpend(CurrencyType.Diamond, amount);
    }

    /// <summary>Cộng tiền - dùng khi nạp tiền (VNPay) hoặc nhận thưởng sau này.</summary>
    public static void AddCoins(int amount)
    {
        PlayerWallet.Add(CurrencyType.Money, amount);
    }

    public static void AddDiamonds(int amount)
    {
        PlayerWallet.Add(CurrencyType.Diamond, amount);
    }

    /// <summary>Reset số dư về ban đầu ngay lập tức (không cần chạy lại game).</summary>
    public static void ResetForTesting()
    {
        PlayerWallet.ResetForTesting();
    }

    private static void NotifyChanged()
    {
        OnChanged?.Invoke();
    }
}
