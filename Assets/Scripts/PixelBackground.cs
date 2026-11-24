using UnityEngine;

public class PixelBackground : MonoBehaviour
{
    public float scrollSpeed = 1.5f;

    private Transform[] tiles;
    private float tileWidth;

    void Awake()
    {
        Sprite bg = PixelArtLoader.LoadSeamless("Art/Bg_Xianxia", 100f);
        if (bg == null)
        {
            Debug.LogError("[道途] 背景加载失败！检查 Assets/Resources/Art/Bg_Xianxia.png");
            return;
        }

        tileWidth = bg.bounds.size.x;
        Debug.Log("[道途] 无缝背景宽度=" + tileWidth.ToString("F2"));
        tiles = new Transform[2];
        for (int i = 0; i < 2; i++)
        {
            var go = new GameObject("BackgroundTile_" + i);
            go.transform.SetParent(transform, false);
            go.transform.position = new Vector3(i * tileWidth, 0f, 5f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = bg;
            sr.sortingOrder = -100;
            tiles[i] = go.transform;
        }
    }

    void Update()
    {
        if (tiles == null) return;
        if (GameManager.instance != null && GameManager.instance.state != GameState.Playing) return;

        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].Translate(Vector3.left * scrollSpeed * Time.deltaTime);
            if (tiles[i].position.x < -tileWidth)
            {
                float rightMost = float.MinValue;
                foreach (var t in tiles) if (t.position.x > rightMost) rightMost = t.position.x;
                tiles[i].position = new Vector3(rightMost + tileWidth, 0f, 5f);
            }
        }
    }
}
