using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class ScanHistory : MonoBehaviour
{
    private List<FoodData> history = new List<FoodData>();
    private GameObject historyPanel;
    private RectTransform contentParent;

    // Compare mode
    private bool compareMode = false;
    private FoodData compareItemA = null;
    private FoodData compareItemB = null;
    private Action<FoodData, FoodData> onCompareReady;
    private TextMeshProUGUI compareModeLabel;

    private static readonly Color greenColor = new Color(0.18f, 0.76f, 0.35f);
    private static readonly Color amberColor = new Color(1f, 0.76f, 0.03f);
    private static readonly Color redColor = new Color(0.91f, 0.22f, 0.22f);

    public void Initialize(GameObject panel, RectTransform content)
    {
        historyPanel = panel;
        contentParent = content;
        historyPanel.SetActive(false);
    }

    public void SetCompareCallback(Action<FoodData, FoodData> callback)
    {
        onCompareReady = callback;
    }

    public void AddEntry(FoodData data)
    {
        if (data == null) return;
        if (history.Count > 0 && history[history.Count - 1].barcode == data.barcode)
            return;
        history.Add(data);
        Debug.Log($"[ScanHistory] Added: {data.productName} (total: {history.Count})");
    }

    public List<FoodData> GetHistory() { return history; }

    public void ShowHistory(bool asCompare = false)
    {
        if (historyPanel == null || contentParent == null) return;

        compareMode = asCompare;
        compareItemA = null;
        compareItemB = null;

        // Clear
        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);

        if (compareMode)
        {
            // Compare mode header
            GameObject header = MakeObj("Header", contentParent);
            TextMeshProUGUI headerText = header.AddComponent<TextMeshProUGUI>();
            headerText.text = "Tap TWO products to compare:";
            headerText.fontSize = 18;
            headerText.color = new Color(0.9f, 0.7f, 0.2f);
            headerText.alignment = TextAlignmentOptions.Center;
            headerText.raycastTarget = false;
            LayoutElement hle = header.AddComponent<LayoutElement>();
            hle.preferredHeight = 35;
            hle.minHeight = 35;

            // Status label
            GameObject statusObj = MakeObj("CompareStatus", contentParent);
            compareModeLabel = statusObj.AddComponent<TextMeshProUGUI>();
            compareModeLabel.text = "Selected: 0 / 2";
            compareModeLabel.fontSize = 15;
            compareModeLabel.color = new Color(0.6f, 0.6f, 0.6f);
            compareModeLabel.alignment = TextAlignmentOptions.Center;
            compareModeLabel.raycastTarget = false;
            LayoutElement sle = statusObj.AddComponent<LayoutElement>();
            sle.preferredHeight = 25;
            sle.minHeight = 25;
        }

        if (history.Count == 0)
        {
            AddSimpleText("No scans yet.\nTap SCAN to get started!");
        }
        else if (compareMode && history.Count < 2)
        {
            AddSimpleText("Need at least 2 scanned products to compare.\nScan more items first!");
        }
        else
        {
            for (int i = history.Count - 1; i >= 0; i--)
                CreateEntry(history[i], i);
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent);

        historyPanel.SetActive(true);
    }

    public void HideHistory()
    {
        if (historyPanel != null) historyPanel.SetActive(false);
        compareMode = false;
    }

    public void ToggleHistory()
    {
        if (historyPanel == null) return;
        if (historyPanel.activeSelf) HideHistory(); else ShowHistory(false);
    }

    public void ToggleCompare()
    {
        if (historyPanel == null) return;
        if (historyPanel.activeSelf && compareMode) HideHistory();
        else ShowHistory(true);
    }

    private void OnEntryTappedForCompare(FoodData data, Image entryBg)
    {
        if (!compareMode) return;

        if (compareItemA == null)
        {
            compareItemA = data;
            entryBg.color = new Color(0.2f, 0.4f, 0.7f, 0.95f); // blue highlight
            if (compareModeLabel != null) compareModeLabel.text = $"Selected: {data.productName} | Tap another...";
        }
        else if (compareItemB == null && data.barcode != compareItemA.barcode)
        {
            compareItemB = data;
            entryBg.color = new Color(0.2f, 0.4f, 0.7f, 0.95f);

            // Both selected — trigger compare
            if (compareModeLabel != null) compareModeLabel.text = "Comparing...";

            HideHistory();
            onCompareReady?.Invoke(compareItemA, compareItemB);
        }
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

    private void CreateEntry(FoodData data, int index)
    {
        string verdict = data.GetVerdict();

        GameObject entry = MakeObj("Entry", contentParent);
        Image bg = entry.AddComponent<Image>();
        switch (verdict)
        {
            case "Healthy":   bg.color = new Color(0.1f, 0.35f, 0.15f, 0.95f); break;
            case "Moderate":  bg.color = new Color(0.35f, 0.28f, 0.05f, 0.95f); break;
            case "Unhealthy": bg.color = new Color(0.4f, 0.1f, 0.1f, 0.95f); break;
            default:          bg.color = new Color(0.2f, 0.2f, 0.2f, 0.95f); break;
        }

        // Make tappable in compare mode
        if (compareMode)
        {
            Button entryBtn = entry.AddComponent<Button>();
            entryBtn.targetGraphic = bg;
            Image bgRef = bg;
            FoodData dataRef = data;
            entryBtn.onClick.AddListener(() => OnEntryTappedForCompare(dataRef, bgRef));
        }
        else
        {
            bg.raycastTarget = false;
        }

        LayoutElement le = entry.AddComponent<LayoutElement>();
        le.preferredHeight = 90;
        le.minHeight = 90;

        RectTransform er = entry.GetComponent<RectTransform>();

        MakeText(er, data.productName, 17, FontStyles.Bold, Color.white,
            TextAlignmentOptions.TopLeft, 0, 0.45f, 0.65f, 1, 12, 0, 0, -6);

        MakeText(er, $"{data.calories:F0} kcal | Sugar: {data.sugars:F1}g | Fat: {data.fat:F1}g",
            12, FontStyles.Normal, new Color(0.8f, 0.8f, 0.8f),
            TextAlignmentOptions.BottomLeft, 0, 0, 0.65f, 0.45f, 12, 6, 0, 0);

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