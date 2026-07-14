using UnityEngine;

/// <summary>
/// Generates a rounded-rectangle Sprite at runtime via a Texture2D.
/// No external shader or asset required.
/// </summary>
public static class RoundedRectSprite
{
    /// <summary>
    /// Creates a white rounded-rectangle sprite.
    /// </summary>
    /// <param name="width">Texture width in pixels (higher = smoother).</param>
    /// <param name="height">Texture height in pixels.</param>
    /// <param name="radius">Corner radius in pixels.</param>
    public static Sprite Create(int width = 256, int height = 128, float radius = 24f)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode   = TextureWrapMode.Clamp
        };

        Color[] pixels = new Color[width * height];

        float cx = width  * 0.5f;
        float cy = height * 0.5f;

        // Clamp radius so it never exceeds half of either dimension
        float r = Mathf.Min(radius, cx, cy);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float px = x + 0.5f;
                float py = y + 0.5f;

                // Distance to the inner rounded-rect boundary (SDF)
                float dx = Mathf.Max(0f, Mathf.Abs(px - cx) - (cx - r));
                float dy = Mathf.Max(0f, Mathf.Abs(py - cy) - (cy - r));
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                // Anti-alias: 1 pixel feather at the edge
                float alpha = Mathf.Clamp01(r - dist + 0.5f);

                pixels[y * width + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        // Build sprite using the full texture as a 9-slice
        return Sprite.Create(
            tex,
            new Rect(0, 0, width, height),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            new Vector4(radius, radius, radius, radius)   // border for 9-slice
        );
    }

    /// <summary>Cached shared instance so we only build the texture once.</summary>
    private static Sprite _cached;
    private static float  _cachedRadius = -1f;

    public static Sprite GetCached(float radius = 20f)
    {
        if (_cached == null || !Mathf.Approximately(_cachedRadius, radius))
        {
            _cached       = Create(256, 128, radius);
            _cachedRadius = radius;
        }
        return _cached;
    }
}
