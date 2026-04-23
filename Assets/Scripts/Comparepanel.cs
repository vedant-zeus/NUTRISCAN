using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Shows a side-by-side comparison of two food products.
/// Created and managed by UIBuilder.
/// </summary>
public class ComparePanel : MonoBehaviour
{
    private GameObject panel;
    private RectTransform contentParent;

    private static readonly Color greenColor = new Color(0.18f, 0.76f, 0.35f);
    private static readonly Color amberColor = new Color(1f, 0.76f, 0.03f);
    private static readonly Color redColor = new Color(0.91f, 0.22f, 0.22f);
    private static readonly Color betterColor = new Color(0.2f, 0.8f, 0.3f);
    private static readonly Color worseColor = new Color(0.9f, 0.3f, 0.3f);
    private static readonly Color neutralColor = new Color(0.7f, 0.7f, 0.7f);

    public void Initialize(GameObject panelObj, RectTransform content)
    {
        panel = panelObj;
        contentParent = content;
        panel.SetActive(false);
    }

    public void ShowComparison(FoodData a, FoodData b)
    {
        if (panel == null || contentParent == null) return;

        // Clear
        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);

        // Header row
        AddRow("", a.productName, b.productName, FontStyles.Bold, 20, Color.white, Color.white);
        AddRow("Brand", a.brand, b.brand, FontStyles.Normal, 14, neutralColor, neutralColor);

        // Verdict row
        string va = a.GetVerdict(), vb = b.GetVerdict();
        Color ca = GetVerdictColor(va), cb = GetVerdictColor(vb);
        AddRow("Verdict", va.ToUpper(), vb.ToUpper(), FontStyles.Bold, 18, ca, cb);

        // Nutri-Score
        string nsa = string.IsNullOrEmpty(a.nutriScore) ? "?" : a.nutriScore.ToUpper();
        string nsb = string.IsNullOrEmpty(b.nutriScore) ? "?" : b.nutriScore.ToUpper();
        AddRow("Nutri-Score", nsa, nsb, FontStyles.Bold, 16, neutralColor, neutralColor);

        // Nutrition comparisons (lower is better for these)
        AddCompareRow("Calories", a.calories, b.calories, "F0", " kcal", true);
        AddCompareRow("Fat", a.fat, b.fat, "F1", "g", true);
        AddCompareRow("Sat. Fat", a.saturatedFat, b.saturatedFat, "F1", "g", true);
        AddCompareRow("Sugars", a.sugars, b.sugars, "F1", "g", true);
        AddCompareRow("Salt", a.salt, b.salt, "F2", "g", true);

        // Higher is better for these
        AddCompareRow("Fiber", a.fiber, b.fiber, "F1", "g", false);
        AddCompareRow("Protein", a.proteins, b.proteins, "F1", "g", false);

        // Allergen comparison
        string allerA = a.allergens.Count > 0 ? string.Join(", ", a.allergens) : "None";
        string allerB = b.allergens.Count > 0 ? string.Join(", ", b.allergens) : "None";
        AddRow("Allergens", allerA, allerB, FontStyles.Normal, 13,
            a.allergens.Count > 0 ? worseColor : betterColor,
            b.allergens.Count > 0 ? worseColor : betterColor);

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent);

        panel.SetActive(true);
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
    }

    private void AddCompareRow(string label, float valA, float valB, string format, string unit, bool lowerIsBetter)
    {
        string sa = valA.ToString(format) + unit;
        string sb = valB.ToString(format) + unit;

        Color ca = neutralColor, cb = neutralColor;
        if (Mathf.Abs(valA - valB) > 0.01f)
        {
            if (lowerIsBetter)
            {
                ca = valA < valB ? betterColor : worseColor;
                cb = valB < valA ? betterColor : worseColor;
            }
            else
            {
                ca = valA > valB ? betterColor : worseColor;
                cb = valB > valA ? betterColor : worseColor;
            }
        }

        AddRow(label, sa, sb, FontStyles.Normal, 15, ca, cb);
    }

    private void AddRow(string label, string valA, string valB, FontStyles style, int fontSize,
        Color colorA, Color colorB)
    {
        GameObject row = new GameObject("Row", typeof(RectTransform));
        row.transform.SetParent(contentParent, false);
        row.layer = LayerMask.NameToLayer("UI");

        // Subtle separator
        Image rowBg = row.AddComponent<Image>();
        rowBg.color = new Color(0.15f, 0.15f, 0.2f, 0.5f);
        rowBg.raycastTarget = false;

        LayoutElement le = row.AddComponent<LayoutElement>();
        le.preferredHeight = fontSize < 16 ? 30 : 40;
        le.minHeight = le.preferredHeight;

        RectTransform rowRT = row.GetComponent<RectTransform>();

        // Label (center)
        if (!string.IsNullOrEmpty(label))
        {
            MakeText(rowRT, label, fontSize - 2, FontStyles.Normal, new Color(0.5f, 0.5f, 0.55f),
                TextAlignmentOptions.Center, 0.3f, 0, 0.7f, 1, 0, 2, 0, -2);
        }

        // Value A (left)
        MakeText(rowRT, valA, fontSize, style, colorA,
            TextAlignmentOptions.Center, 0, 0, 0.3f, 1, 5, 2, -2, -2);

        // Value B (right)
        MakeText(rowRT, valB, fontSize, style, colorB,
            TextAlignmentOptions.Center, 0.7f, 0, 1, 1, 2, 2, -5, -2);
    }

    private void MakeText(RectTransform parent, string text, int size, FontStyles style,
        Color color, TextAlignmentOptions align,
        float aMinX, float aMinY, float aMaxX, float aMaxY,
        float oMinX, float oMinY, float oMaxX, float oMaxY)
    {
        GameObject o = new GameObject("T", typeof(RectTransform));
        o.transform.SetParent(parent, false);
        o.layer = LayerMask.NameToLayer("UI");
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

    private Color GetVerdictColor(string verdict)
    {
        switch (verdict)
        {
            case "Healthy": return greenColor;
            case "Moderate": return amberColor;
            case "Unhealthy": return redColor;
            default: return neutralColor;
        }
    }
}