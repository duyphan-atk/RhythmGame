using UnityEngine;

/// <summary>Finds a scene UI canvas while ignoring the persistent transition overlay.</summary>
public static class RuntimeCanvasUtility
{
    public static Canvas FindSceneCanvas()
    {
        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Canvas fallback = null;
        for (int i = 0; i < canvases.Length; i++)
        {
            Canvas canvas = canvases[i];
            if (canvas == null || IsTransitionCanvas(canvas))
                continue;

            if (canvas.gameObject.activeInHierarchy && canvas.name == "Canvas")
                return canvas;

            if (fallback == null && canvas.gameObject.activeInHierarchy)
                fallback = canvas;
        }

        return fallback;
    }

    private static bool IsTransitionCanvas(Canvas canvas)
    {
        return canvas.name == "Fade Canvas" || canvas.transform.root.name == "RG Scene Transition";
    }
}
