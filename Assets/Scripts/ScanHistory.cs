using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ScanHistory : MonoBehaviour
{
    private List<FoodData> history = new List<FoodData>();
    private GameObject historyPanel;
    private RectTransform contentParent;

    private static readonly Color greenColor = new Color(0.18f, 0.76f, 0.35f);
    private static readonly Color amberColor = new Color(1f, 0.76f, 0.03f);
    private static readonly Color redColor = new Color(0.91f, 0.22f, 0.22f);

    public void Initialize(GameObject panel, RectTransform content)
    {
        historyPanel = panel;
        contentParent = content;
        historyPanel.SetActive(false);
    }

    public void AddEntry(FoodData data)
    {
        if (data == null) return;
        if (history.Count > 0 && history[history.Count - 1].barcode == data.barcode)
            return;
        history.Add(data);
        Debug.Log($"[ScanHistory] Added: {data.productName} (total: {history.Count})");
    }

    public void ShowHistory()
    {
        if (historyPanel == null || contentParent == null) return;

        // Clear
        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);

        if (history.Count == 0)
        {
            AddSimpleText("No scans yet.\nTap SCAN to get started!");
        }
        else
        {
            for (int i = history.Count - 1; i >= 0; i--)
                CreateEntry(history[i]);
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent);

        historyPanel.SetActive(true);
    }

    public void HideHistory()
    {
        if (historyPanel != null) historyPanel.SetActive(false);
    }

    public void ToggleHistory()
    {
        if (historyPanel == null) return;
        if (historyPanel.activeSelf) HideHistory(); else ShowHistory();
    }

    private void AddSimpleText(string text)
    {
        GameObject obj = MakeObj("Msg", contentParent);
        TextMeshProUGUI t = obj.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = 18;
        t.color = new Color(0.6f, 0.6f, 0.6f);
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;
        LayoutElement le = obj.AddComponent<LayoutElement>();
        le.preferredHeight = 80;
        le.minHeight = 80;
    }

    private void CreateEntry(FoodData data)
    {
        string verdict = data.GetVerdict();

        GameObject entry = MakeObj("Entry", contentParent);
        Image bg = entry.AddComponent<Image>();
        bg.raycastTarget = false;
        switch (verdict)
        {
            case "Healthy":   bg.color = new Color(0.1f, 0.35f, 0.15f, 0.95f); break;
            case "Moderate":  bg.color = new Color(0.35f, 0.28f, 0.05f, 0.95f); break;
            case "Unhealthy": bg.color = new Color(0.4f, 0.1f, 0.1f, 0.95f); break;
            default:          bg.color = new Color(0.2f, 0.2f, 0.2f, 0.95f); break;
        }

        LayoutElement le = entry.AddComponent<LayoutElement>();
        le.preferredHeight = 90;
        le.minHeight = 90;

        RectTransform er = entry.GetComponent<RectTransform>();

        // Product name
        MakeText(er, data.productName, 17, FontStyles.Bold, Color.white,
            TextAlignmentOptions.TopLeft, 0, 0.45f, 0.65f, 1, 12, 0, 0, -6);

        // Info
        MakeText(er, $"{data.calories:F0} kcal | Sugar: {data.sugars:F1}g | Fat: {data.fat:F1}g",
            12, FontStyles.Normal, new Color(0.8f, 0.8f, 0.8f),
            TextAlignmentOptions.BottomLeft, 0, 0, 0.65f, 0.45f, 12, 6, 0, 0);

        // Badge
        GameObject badge = MakeObj("Badge", er);
        Image badgeImg = badge.AddComponent<Image>();
        badgeImg.raycastTarget = false;
        switch (verdict)
        {
            case "Healthy":   badgeImg.color = greenColor; break;
            case "Moderate":  badgeImg.color = amberColor; break;
            case "Unhealthy": badgeImg.color = redColor; break;
        }
        RectTransform br = badge.GetComponent<RectTransform>();
        br.anchorMin = new Vector2(0.67f, 0.15f);
        br.anchorMax = new Vector2(0.98f, 0.85f);
        br.offsetMin = br.offsetMax = Vector2.zero;

        MakeText(br, verdict.ToUpper(), 14, FontStyles.Bold, Color.white,
            TextAlignmentOptions.Center, 0, 0, 1, 1, 0, 0, 0, 0);
    }

    private GameObject MakeObj(string name, Transform parent)
    {
        GameObject o = new GameObject(name, typeof(RectTransform));
        o.transform.SetParent(parent, false);
        o.layer = LayerMask.NameToLayer("UI");
        return o;
    }

    private void MakeText(RectTransform parent, string text, int size, FontStyles style,
        Color color, TextAlignmentOptions align,
        float aMinX, float aMinY, float aMaxX, float aMaxY,
        float oMinX, float oMinY, float oMaxX, float oMaxY)
    {
        GameObject o = MakeObj("T", parent);
        TextMeshProUGUI t = o.AddComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = size; t.fontStyle = style;
        t.color = color; t.alignment = align;
        t.raycastTarget = false;
        t.overflowMode = TextOverflowModes.Ellipsis;
        RectTransform r = o.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(aMinX, aMinY);
        r.anchorMax = new Vector2(aMaxX, aMaxY);
        r.offsetMin = new Vector2(oMinX, oMinY);
        r.offsetMax = new Vector2(oMaxX, oMaxY);
    }
}