using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using Keyboard = UnityEngine.InputSystem.Keyboard;
#endif

/// <summary>Temporary local-wallet controls for Store testing before server integration.</summary>
public class StoreDebugHotkeys : MonoBehaviour
{
    private void Update()
    {
        if (WasMoneyPressed())
        {
            PlayerWallet.Add(CurrencyType.Money, 1000);
            Debug.Log("[Store Test] Added 1000 Money. Current Money: " + PlayerWallet.Money);
        }

        if (WasDiamondPressed())
        {
            PlayerWallet.Add(CurrencyType.Diamond, 50);
            Debug.Log("[Store Test] Added 50 Diamond. Current Diamond: " + PlayerWallet.Diamond);
        }

        if (WasResetPressed())
        {
            PlayerWallet.ResetForTesting();
            Debug.Log("[Store Test] Reset local wallet.");
        }
    }

    private static bool WasMoneyPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.f6Key.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.F6);
#endif
    }

    private static bool WasDiamondPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.f7Key.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.F7);
#endif
    }

    private static bool WasResetPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.f8Key.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.F8);
#endif
    }
}
