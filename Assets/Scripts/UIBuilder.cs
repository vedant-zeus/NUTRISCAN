using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIBuilder : MonoBehaviour
{
    [SerializeField] private Canvas canvas;

    private void Awake()
    {
        if (canvas == null) { Debug.LogError("[UIBuilder] Canvas not assigned!"); return; }
        BuildUI();
    }

    private void BuildUI()
    {
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform root = canvas.GetComponent<RectTransform>();

        // ─── CAMERA PREVIEW ───
        GameObject camObj = MakeUI("CameraPreview", root);
        RawImage camPreview = camObj.AddComponent<RawImage>();
        camPreview.color = new Color(0.15f, 0.15f, 0.15f);
        camPreview.raycastTarget = false;
        Stretch(camObj);

        // ─── STATUS TEXT ───
        GameObject statusObj = MakeUI("Status", root);
        TextMeshProUGUI statusText = statusObj.AddComponent<TextMeshProUGUI>();
        statusText.text = "Initializing...";
        statusText.fontSize = 36;
        statusText.color = Color.white;
        statusText.alignment = TextAlignmentOptions.Center;
        statusText.raycastTarget = false;
        RectTransform statusRT = statusObj.GetComponent<RectTransform>();
        Anchor(statusRT, 0, 0, 1, 0);
        statusRT.pivot = new Vector2(0.5f, 0);
        statusRT.offsetMin = new Vector2(20, 200);
        statusRT.offsetMax = new Vector2(-20, 260);
        Shadow shadow = statusObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.9f);
        shadow.effectDistance = new Vector2(2, -2);

        // ─── BOTTOM BUTTONS ───
        Button scanBtn = MakeButton(root, "SCAN", 36, new Color(0.15f, 0.55f, 0.95f, 1f),
            0.02f, 0f, 0.48f, 0f, 0, 40, 0, 160);
        Button logBtn = MakeButton(root, "LOG", 24, new Color(0.3f, 0.3f, 0.38f, 1f),
            0.5f, 0f, 0.74f, 0f, 5, 100, 0, 160);
        Button compareBtn = MakeButton(root, "VS", 24, new Color(0.5f, 0.3f, 0.6f, 1f),
            0.76f, 0f, 1f, 0f, 0, 100, -5, 160);
        Button allergenBtn = MakeButton(root, "ALLERGY", 20, new Color(0.7f, 0.5f, 0.1f, 1f),
            0.5f, 0f, 1f, 0f, 5, 40, -5, 95);

        // ─── RESULT PANEL (simple, no scroll) ───
        GameObject resultObj = MakeUI("ResultPanel", root);
        Image resultBg = resultObj.AddComponent<Image>();
        resultBg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        RectTransform resultRT = resultObj.GetComponent<RectTransform>();
        Anchor(resultRT, 0.03f, 0.08f, 0.97f, 0.95f);
        resultRT.offsetMin = resultRT.offsetMax = Vector2.zero;

        ResultOverlay overlay = resultObj.AddComponent<ResultOverlay>();

        // Product name (top)
        TextMeshProUGUI prodName = MakeAnchoredText(resultRT, "Product", 36, FontStyles.Bold,
            Color.white, TextAlignmentOptions.Center, 0, 1, 1, 1, 20, -60, -60, -10);

        // Brand
        TextMeshProUGUI brand = MakeAnchoredText(resultRT, "Brand", 24, FontStyles.Normal,
            new Color(0.7f, 0.7f, 0.7f), TextAlignmentOptions.Center, 0, 1, 1, 1, 20, -90, -60, -60);

        // Verdict badge
        GameObject badgeObj = MakeUI("Badge", resultRT);
        Image badgeImg = badgeObj.AddComponent<Image>();
        badgeImg.color = new Color(0.3f, 0.3f, 0.3f);
        badgeImg.raycastTarget = false;
        RectTransform badgeRT = badgeObj.GetComponent<RectTransform>();
        Anchor(badgeRT, 0.1f, 1f, 0.9f, 1f);
        badgeRT.offsetMin = new Vector2(0, -155);
        badgeRT.offsetMax = new Vector2(0, -95);

        TextMeshProUGUI verdictText = MakeAnchoredText(badgeRT,
            "VERDICT", 40, FontStyles.Bold, Color.white, TextAlignmentOptions.Center,
            0, 0, 1, 1, 0, 0, 0, 0);

        // Nutrition text (left half)
        TextMeshProUGUI nutriText = MakeAnchoredText(resultRT, "Nutrition info", 20, FontStyles.Normal,
            new Color(0.85f, 0.85f, 0.85f), TextAlignmentOptions.TopLeft,
            0, 0, 0.55f, 1, 20, 50, -5, -165);

        // Allergen section (right half)
        MakeAnchoredText(resultRT, "ALLERGENS", 18, FontStyles.Bold,
            new Color(0.9f, 0.7f, 0.2f), TextAlignmentOptions.Center,
            0.55f, 1, 1, 1, 5, -185, -20, -165);

        TextMeshProUGUI allergenText = MakeAnchoredText(resultRT, "No allergen data", 18, FontStyles.Normal,
            new Color(0.85f, 0.85f, 0.85f), TextAlignmentOptions.TopLeft,
            0.55f, 0, 1, 1, 10, 50, -15, -190);
        allergenText.richText = true;

        // Close button
        Button closeBtn = MakeButton(resultRT, "X", 36, new Color(0.85f, 0.2f, 0.2f), 1, 1, 1, 1, 0, 0, 0, 0);
        RectTransform clRT = closeBtn.GetComponent<RectTransform>();
        clRT.anchorMin = clRT.anchorMax = new Vector2(1, 1);
        clRT.sizeDelta = new Vector2(70, 70);
        clRT.anchoredPosition = new Vector2(-35, -35);

        closeBtn.onClick.AddListener(() => resultObj.SetActive(false));
        resultObj.SetActive(false);

        // ─── ALLERGEN SETTINGS PANEL ───
        GameObject allergenPanel = MakeUI("AllergenPanel", root);
        Image apBg = allergenPanel.AddComponent<Image>();
        apBg.color = new Color(0.08f, 0.08f, 0.1f, 0.97f);
        RectTransform apRT = allergenPanel.GetComponent<RectTransform>();
        Anchor(apRT, 0.05f, 0.15f, 0.95f, 0.85f);
        apRT.offsetMin = apRT.offsetMax = Vector2.zero;

        MakeAnchoredText(apRT, "MY ALLERGENS", 32, FontStyles.Bold, Color.white,
            TextAlignmentOptions.Center, 0, 1, 1, 1, 20, -50, -20, -10);
        MakeAnchoredText(apRT, "Tap to toggle (orange = active):", 18, FontStyles.Normal,
            new Color(0.6f, 0.6f, 0.6f), TextAlignmentOptions.Center, 0, 1, 1, 1, 20, -75, -20, -50);

        string[] commonAllergens = { "Gluten", "Milk", "Nuts", "Peanuts", "Soy", "Eggs", "Fish", "Shellfish", "Celery", "Mustard", "Sesame", "Sulphites" };
        List<string> activeAllergens = new List<string>();

        float startY = -90;
        int col = 0;
        float currentY = startY;

        for (int i = 0; i < commonAllergens.Length; i++)
        {
            string allergen = commonAllergens[i];
            float xMin = (col == 0) ? 0.03f : 0.52f;
            float xMax = xMin + 0.46f;

            GameObject tObj = MakeUI("Tog_" + allergen, apRT);
            Image tBg = tObj.AddComponent<Image>();
            tBg.color = new Color(0.25f, 0.25f, 0.3f, 1f);
            Button tBtn = tObj.AddComponent<Button>();
            tBtn.targetGraphic = tBg;
            RectTransform tRT = tObj.GetComponent<RectTransform>();
            Anchor(tRT, xMin, 1, xMax, 1);
            tRT.offsetMin = new Vector2(3, currentY - 55);
            tRT.offsetMax = new Vector2(-3, currentY);

            MakeAnchoredText(tRT, allergen, 20, FontStyles.Normal, Color.white,
                TextAlignmentOptions.Center, 0, 0, 1, 1, 5, 5, -5, -5);

            Image bgRef = tBg; string a = allergen;
            tBtn.onClick.AddListener(() =>
            {
                if (activeAllergens.Contains(a))
                {
                    activeAllergens.Remove(a);
                    bgRef.color = new Color(0.25f, 0.25f, 0.3f, 1f);
                }
                else
                {
                    activeAllergens.Add(a);
                    bgRef.color = new Color(0.85f, 0.3f, 0.1f, 1f);
                }
                overlay.userAllergens = new List<string>(activeAllergens);
            });

            col++;
            if (col >= 2) { col = 0; currentY -= 60; }
        }

        Button apDone = MakeButton(apRT, "DONE", 28, new Color(0.15f, 0.55f, 0.95f, 1f),
            0.2f, 0, 0.8f, 0, 0, 15, 0, 70);
        apDone.onClick.AddListener(() => allergenPanel.SetActive(false));
        allergenPanel.SetActive(false);

        allergenBtn.onClick.AddListener(() => allergenPanel.SetActive(!allergenPanel.activeSelf));

        // ─── COMPARE PANEL ───
        GameObject cmpPanel = MakeUI("ComparePanel", root);
        Image cmpBg = cmpPanel.AddComponent<Image>();
        cmpBg.color = new Color(0.06f, 0.06f, 0.08f, 0.97f);
        Stretch(cmpPanel);

        GameObject cmpTitle = MakeUI("CmpTitle", cmpPanel.GetComponent<RectTransform>());
        Image cmpTitleBg = cmpTitle.AddComponent<Image>();
        cmpTitleBg.color = new Color(0.12f, 0.12f, 0.18f);
        RectTransform cmpTitleRT = cmpTitle.GetComponent<RectTransform>();
        Anchor(cmpTitleRT, 0, 1, 1, 1);
        cmpTitleRT.offsetMin = new Vector2(0, -80);
        cmpTitleRT.offsetMax = Vector2.zero;

        MakeAnchoredText(cmpTitleRT, "PRODUCT COMPARISON", 32, FontStyles.Bold, Color.white,
            TextAlignmentOptions.Center, 0, 0, 1, 1, 0, 0, 0, 0);

        Button cmpClose = MakeButton(cmpTitleRT, "X", 28, new Color(0.85f, 0.2f, 0.2f), 1, 0.5f, 1, 0.5f, 0, 0, 0, 0);
        RectTransform ccRT = cmpClose.GetComponent<RectTransform>();
        ccRT.anchorMin = ccRT.anchorMax = new Vector2(1, 0.5f);
        ccRT.sizeDelta = new Vector2(60, 60);
        ccRT.anchoredPosition = new Vector2(-40, 0);

        GameObject cmpScroll = MakeUI("CmpScroll", cmpPanel.GetComponent<RectTransform>());
        RectTransform csRT = cmpScroll.GetComponent<RectTransform>();
        Anchor(csRT, 0, 0, 1, 1); csRT.offsetMin = new Vector2(0, 0); csRT.offsetMax = new Vector2(0, -80);

        GameObject cmpVP = MakeUI("CmpVP", csRT);
        Stretch(cmpVP);
        Image cvpImg = cmpVP.AddComponent<Image>(); cvpImg.color = Color.white;
        Mask cvpMask = cmpVP.AddComponent<Mask>(); cvpMask.showMaskGraphic = false;

        GameObject cmpContent = MakeUI("CmpContent", cmpVP.GetComponent<RectTransform>());
        RectTransform ccntRT = cmpContent.GetComponent<RectTransform>();
        ccntRT.anchorMin = new Vector2(0, 1); ccntRT.anchorMax = new Vector2(1, 1);
        ccntRT.pivot = new Vector2(0.5f, 1); ccntRT.sizeDelta = Vector2.zero;
        ccntRT.anchoredPosition = Vector2.zero;

        ContentSizeFitter ccntF = cmpContent.AddComponent<ContentSizeFitter>();
        ccntF.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        VerticalLayoutGroup ccntV = cmpContent.AddComponent<VerticalLayoutGroup>();
        ccntV.spacing = 4; ccntV.padding = new RectOffset(10, 10, 10, 10);
        ccntV.childForceExpandWidth = true; ccntV.childForceExpandHeight = false;
        ccntV.childControlWidth = true; ccntV.childControlHeight = false;

        ScrollRect cmpSR = cmpScroll.AddComponent<ScrollRect>();
        cmpSR.horizontal = false; cmpSR.vertical = true;
        cmpSR.content = ccntRT; cmpSR.viewport = cmpVP.GetComponent<RectTransform>();

        ComparePanel comparePanelComp = gameObject.AddComponent<ComparePanel>();
        comparePanelComp.Initialize(cmpPanel, ccntRT);
        cmpClose.onClick.AddListener(() => comparePanelComp.Hide());
        cmpPanel.SetActive(false);

        // ─── HISTORY PANEL ───
        GameObject histPanel = MakeUI("HistoryPanel", root);
        Image histBg = histPanel.AddComponent<Image>();
        histBg.color = new Color(0.06f, 0.06f, 0.08f, 0.97f);
        Stretch(histPanel);

        GameObject titleBar = MakeUI("TitleBar", histPanel.GetComponent<RectTransform>());
        Image titleBg = titleBar.AddComponent<Image>();
        titleBg.color = new Color(0.12f, 0.12f, 0.18f);
        RectTransform titleRT = titleBar.GetComponent<RectTransform>();
        Anchor(titleRT, 0, 1, 1, 1);
        titleRT.offsetMin = new Vector2(0, -80); titleRT.offsetMax = Vector2.zero;

        MakeAnchoredText(titleRT, "SCAN HISTORY", 32, FontStyles.Bold, Color.white,
            TextAlignmentOptions.Center, 0, 0, 1, 1, 0, 0, 0, 0);

        Button histClose = MakeButton(titleRT, "X", 28, new Color(0.85f, 0.2f, 0.2f), 1, 0.5f, 1, 0.5f, 0, 0, 0, 0);
        RectTransform hcRT = histClose.GetComponent<RectTransform>();
        hcRT.anchorMin = hcRT.anchorMax = new Vector2(1, 0.5f);
        hcRT.sizeDelta = new Vector2(60, 60); hcRT.anchoredPosition = new Vector2(-40, 0);

        GameObject scrollObj = MakeUI("Scroll", histPanel.GetComponent<RectTransform>());
        RectTransform scrollRT = scrollObj.GetComponent<RectTransform>();
        Anchor(scrollRT, 0, 0, 1, 1); scrollRT.offsetMin = new Vector2(0, 0); scrollRT.offsetMax = new Vector2(0, -80);

        GameObject viewportObj = MakeUI("Viewport", scrollRT);
        Stretch(viewportObj);
        Image vpImg = viewportObj.AddComponent<Image>(); vpImg.color = Color.white;
        Mask vpMask = viewportObj.AddComponent<Mask>(); vpMask.showMaskGraphic = false;

        GameObject contentObj = MakeUI("Content", viewportObj.GetComponent<RectTransform>());
        RectTransform contentRT = contentObj.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1); contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1); contentRT.sizeDelta = Vector2.zero;
        contentRT.anchoredPosition = Vector2.zero;

        ContentSizeFitter fitter = contentObj.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        VerticalLayoutGroup vlg = contentObj.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 12; vlg.padding = new RectOffset(15, 15, 15, 15);
        vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true; vlg.childControlHeight = false;

        ScrollRect scroll = scrollObj.AddComponent<ScrollRect>();
        scroll.horizontal = false; scroll.vertical = true;
        scroll.content = contentRT; scroll.viewport = viewportObj.GetComponent<RectTransform>();

        // ─── WIRE SCAN HISTORY ───
        ScanHistory scanHistory = gameObject.AddComponent<ScanHistory>();
        scanHistory.Initialize(histPanel, contentRT);
        scanHistory.SetCompareCallback((a, b) => comparePanelComp.ShowComparison(a, b));
        logBtn.onClick.AddListener(() => scanHistory.ToggleHistory());
        histClose.onClick.AddListener(() => scanHistory.HideHistory());
        compareBtn.onClick.AddListener(() => scanHistory.ToggleCompare());

        // ─── WIRE ALL COMPONENTS ───
        SetField(overlay, "productNameText", prodName);
        SetField(overlay, "brandText", brand);
        SetField(overlay, "verdictText", verdictText);
        SetField(overlay, "nutritionText", nutriText);
        SetField(overlay, "allergenText", allergenText);
        SetField(overlay, "verdictBackground", badgeImg);
        SetField(overlay, "closeButton", closeBtn);

        BarcodeScanner scanner = GetComponent<BarcodeScanner>();
        if (scanner != null)
        {
            SetField(scanner, "cameraPreview", camPreview);
            SetField(scanner, "scanButton", scanBtn);
            SetField(scanner, "statusText", statusText);
            SetField(scanner, "foodDataFetcher", GetComponent<FoodDataFetcher>());
            SetField(scanner, "resultOverlay", overlay);
            SetField(scanner, "scanHistory", scanHistory);
        }

        Debug.Log("[UIBuilder] UI built successfully!");
    }

    // ─── HELPERS ───
    private GameObject MakeUI(string name, RectTransform parent)
    {
        GameObject o = new GameObject(name, typeof(RectTransform));
        o.transform.SetParent(parent, false);
        o.layer = LayerMask.NameToLayer("UI");
        return o;
    }

    private void Stretch(GameObject obj)
    {
        RectTransform r = obj.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = r.offsetMax = Vector2.zero;
    }

    private void Anchor(RectTransform r, float minX, float minY, float maxX, float maxY)
    { r.anchorMin = new Vector2(minX, minY); r.anchorMax = new Vector2(maxX, maxY); }

    private Button MakeButton(RectTransform parent, string label, int fontSize, Color bgColor,
        float aMinX, float aMinY, float aMaxX, float aMaxY,
        float oMinX, float oMinY, float oMaxX, float oMaxY)
    {
        GameObject obj = MakeUI("Btn_" + label, parent);
        Image img = obj.AddComponent<Image>(); img.color = bgColor;
        Button btn = obj.AddComponent<Button>(); btn.targetGraphic = img;
        ColorBlock cb = btn.colors;
        cb.pressedColor = new Color(bgColor.r * 0.7f, bgColor.g * 0.7f, bgColor.b * 0.7f, bgColor.a);
        cb.highlightedColor = bgColor; btn.colors = cb;
        RectTransform rt = obj.GetComponent<RectTransform>();
        Anchor(rt, aMinX, aMinY, aMaxX, aMaxY);
        rt.offsetMin = new Vector2(oMinX, oMinY); rt.offsetMax = new Vector2(oMaxX, oMaxY);
        GameObject lbl = MakeUI("Lbl", rt);
        TextMeshProUGUI t = lbl.AddComponent<TextMeshProUGUI>();
        t.text = label; t.fontSize = fontSize; t.color = Color.white;
        t.alignment = TextAlignmentOptions.Center; t.raycastTarget = false;
        Stretch(lbl);
        return btn;
    }

    private TextMeshProUGUI MakeAnchoredText(RectTransform parent, string text, int size,
        FontStyles style, Color color, TextAlignmentOptions align,
        float aMinX, float aMinY, float aMaxX, float aMaxY,
        float oMinX, float oMinY, float oMaxX, float oMaxY)
    {
        GameObject obj = MakeUI("Txt", parent);
        TextMeshProUGUI t = obj.AddComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = size; t.fontStyle = style;
        t.color = color; t.alignment = align; t.raycastTarget = false;
        RectTransform r = obj.GetComponent<RectTransform>();
        Anchor(r, aMinX, aMinY, aMaxX, aMaxY);
        r.offsetMin = new Vector2(oMinX, oMinY); r.offsetMax = new Vector2(oMaxX, oMaxY);
        return t;
    }

    private void SetField(object target, string name, object value)
    {
        var f = target.GetType().GetField(name,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (f != null) f.SetValue(target, value);
        else Debug.LogWarning($"[UIBuilder] Field '{name}' not found on {target.GetType().Name}");
    }
}