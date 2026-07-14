#if UNITY_EDITOR
using MCPForUnity.Editor.Services;
using MCPForUnity.Editor.Services.Transport;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class CodexUnityMcpAutoConnect
{
    private const string SessionKey = "CodexUnityMcpAutoConnect.Ran";

    static CodexUnityMcpAutoConnect()
    {
        if (UnityEditorInternal.InternalEditorUtility.inBatchMode)
            return;

        EditorApplication.delayCall += ConnectOnce;
    }

    private static async void ConnectOnce()
    {
        if (SessionState.GetBool(SessionKey, false))
            return;

        SessionState.SetBool(SessionKey, true);

        EditorPrefs.SetBool("MCPForUnity.UseHttpTransport", true);
        EditorPrefs.SetBool("MCPForUnity.AutoStartOnLoad", true);
        EditorPrefs.SetBool("MCPForUnity.HttpServerLaunchConfirmed", true);
        EditorPrefs.SetString("MCPForUnity.HttpTransportScope", "local");
        EditorPrefs.SetString("MCPForUnity.HttpUrl", "http://127.0.0.1:8080");

        bool connected = await MCPServiceLocator.TransportManager.StartAsync(TransportMode.Http);
        Debug.Log($"CodexUnityMcpAutoConnect: Unity MCP HTTP connection {(connected ? "connected" : "failed")}.");
    }
}
#endif
