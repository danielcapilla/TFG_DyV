using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoChickenManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private FiltersBehaviour filters;
    [SerializeField] private GroupsBehaviour groups;

    [Header("UI Grid")]
    [SerializeField] private RectTransform gridRoot;
    [SerializeField] private RectTransform legendRoot;

    [Header("Colores")]
    [SerializeField] private Color freeColor = new Color(0.9f, 0.9f, 0.9f);
    [SerializeField] private Color obstacleColor = new Color(0.2f, 0.2f, 0.2f);
    [SerializeField] private Color startColor = new Color(0.2f, 0.7f, 0.2f);
    [SerializeField] private Color goalColor = new Color(0.8f, 0.6f, 0.1f);
    [SerializeField] private Color playerColor = new Color(0.1f, 0.5f, 1f);

    [Header("Jugador")]
    [SerializeField] private Sprite playerDotSprite;

    private GridLayoutGroup layout;
    private readonly List<GameObject> spawned = new List<GameObject>();

    private void Awake()
    {
        layout = gridRoot.GetComponent<GridLayoutGroup>();
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void OnDisable()
    {
        Clear();
    }

    public void Refresh()
    {
        Clear();
        var match = filters.chickenMatch;
        // Seguridad
        if (match == null || match.Grids == null || match.Grids.Count == 0) return;

        var lvl = match.Grids[0];
        int width = lvl.Width;
        int height = lvl.Height;
        var flat = lvl.Grid;

        layout.constraintCount = width;

        // Cell size
        if (gridRoot.rect.width > 1f && gridRoot.rect.height > 1f)
        {
            float cellW = (gridRoot.rect.width - (width - 1) * layout.spacing.x - layout.padding.left - layout.padding.right) / width;
            float cellH = (gridRoot.rect.height - (height - 1) * layout.spacing.y - layout.padding.top - layout.padding.bottom) / height;
            float c = Mathf.Floor(Mathf.Min(cellW, cellH));
            layout.cellSize = new Vector2(c, c);
        }


        // Celdas
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int idx = y * width + x;
                int code = flat[idx];

                GameObject cell = CreateCell();
                Image img = cell.GetComponent<Image>();
                img.color = CodeToColor(code);
                spawned.Add(cell);
            }
        }

        int groupId = int.Parse(groups.groupSelectedID) + 1;
        var player = match.Groups.FirstOrDefault(p => p.Group == groupId);
        var pos = player.FinalGridPos;
        int idxDot = pos.y * width + pos.x;
        var cellDot = gridRoot.GetChild(idxDot) as RectTransform;

        var dotGO = new GameObject("PlayerDot", typeof(RectTransform), typeof(Image), typeof(Outline));
        var dot = dotGO.GetComponent<RectTransform>();
        dot.SetParent(cellDot, false);
        dot.anchorMin = dot.anchorMax = new Vector2(0.5f, 0.5f);
        dot.sizeDelta = cellDot.sizeDelta * Mathf.Clamp01(0.5f);

        var dotImg = dotGO.GetComponent<Image>();
        dotImg.color = playerColor;
        dotImg.raycastTarget = false;
        if (playerDotSprite != null)
        {
            dotImg.sprite = playerDotSprite;
            dotImg.type = Image.Type.Simple;
            dotImg.preserveAspect = true;
        }

        var outline = dotGO.GetComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.5f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        // Recolorear start/goal
        ColorizeCell(lvl.Start, startColor, width, height);
        ColorizeCell(lvl.Goal, goalColor, width, height);
    }

    private GameObject CreateCell()
    {
        var go = new GameObject("Cell", typeof(RectTransform), typeof(Image));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(gridRoot, false);
        spawned.Add(go);
        return go;
    }

    private Color CodeToColor(int code)
    {
        // 0 libre, 1 obstacle, 2 start, 3 goal
        switch (code)
        {
            case 1: return obstacleColor;
            case 2: return startColor;
            case 3: return goalColor;
            default: return freeColor;
        }
    }

    private void ColorizeCell(Vector2Int gridPos, Color color, int width, int height)
    {
        if (gridPos.x < 0 || gridPos.y < 0 || gridPos.x >= width || gridPos.y >= height) return;
        int idx = gridPos.y * width + gridPos.x;
        if (idx < 0 || idx >= gridRoot.childCount) return;
        var img = gridRoot.GetChild(idx).GetComponent<Image>();
        if (img != null) img.color = color;
    }
    
    private void Clear()
    {
        for (int i = gridRoot.childCount - 1; i >= 0; i--)
            Destroy(gridRoot.GetChild(i).gameObject);

        spawned.Clear();
    }
}