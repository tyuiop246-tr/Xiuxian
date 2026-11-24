using UnityEngine;

public static class PixelArtLoader
{
    public static Sprite Load(string path, float pixelsPerUnit)
    {
        Texture2D source = LoadTexture(path);
        if (source == null) return null;
        return BuildSprite(source, pixelsPerUnit);
    }

    // 生成左右无缝的宽图：原图 + 水平镜像，拼接处像素完全一致
    public static Sprite LoadSeamless(string path, float pixelsPerUnit)
    {
        Texture2D source = LoadTexture(path);
        if (source == null) return null;

        int w = source.width;
        int h = source.height;
        var seamless = new Texture2D(w * 2, h, TextureFormat.RGBA32, false);
        seamless.filterMode = FilterMode.Point;
        seamless.wrapMode = TextureWrapMode.Clamp;

        Color[] src = source.GetPixels();
        var merged = new Color[w * 2 * h];
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                merged[y * w * 2 + x] = src[y * w + x];
                merged[y * w * 2 + (w * 2 - 1 - x)] = src[y * w + x];
            }
        }
        seamless.SetPixels(merged);
        seamless.Apply();
        Debug.Log("[道途] 无缝背景已生成：" + seamless.width + "x" + seamless.height);
        return Sprite.Create(seamless, new Rect(0, 0, seamless.width, seamless.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
    }

    static Texture2D LoadTexture(string path)
    {
        Sprite imported = Resources.Load<Sprite>(path);
        if (imported != null) return imported.texture;
        Texture2D tex = Resources.Load<Texture2D>(path);
        if (tex == null) Debug.LogError("[道途] 图片加载失败：" + path);
        return tex;
    }

    static Sprite BuildSprite(Texture2D source, float pixelsPerUnit)
    {
        var copy = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        copy.filterMode = FilterMode.Point;
        copy.wrapMode = TextureWrapMode.Clamp;
        copy.SetPixels(source.GetPixels());
        copy.Apply();
        return Sprite.Create(copy, new Rect(0, 0, copy.width, copy.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
    }
}
