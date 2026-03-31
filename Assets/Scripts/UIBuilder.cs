using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
        // ─── CANVAS SCALER for mobile ───
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

        // ─── SCAN BUTTON ───
        Button scanBtn = MakeButton(root, "SCAN", 40, new Color(0.15f, 0.55f, 0.95f, 1f),
            0.15f, 0f, 0.85f, 0f, 0, 40, 0, 160);

        // ─── LOG BUTTON ───
        Button logBtn = MakeButton(root, "LOG", 28, new Color(0.3f, 0.3f, 0.38f, 1f),
            0.75f, 0f, 1f, 0f, -20, 40, -20, 160);

        // ─── RESULT PANEL ───
        GameObject resultObj = MakeUI("ResultPanel", root);
        Image resultBg = resultObj.AddComponent<Image>();
        resultBg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        RectTransform resultRT = resultObj.GetComponent<RectTransform>();
        Anchor(resultRT, 0.05f, 0.25f, 0.95f, 0.85f);
        resultRT.offsetMin = resultRT.offsetMax = Vector2.zero;

        ResultOverlay overlay = resultObj.AddComponent<ResultOverlay>();

        // Product name
        TextMeshProUGUI prodName = MakeAnchoredText(resultRT, "Product", 40, FontStyles.Bold,
            Color.white, TextAlignmentOptions.Center, 0, 1, 1, 1, 20, -100, -20, -15);

        // Brand
        TextMeshProUGUI brand = MakeAnchoredText(resultRT, "Brand", 28, FontStyles.Normal,
            new Color(0.7f, 0.7f, 0.7f), TextAlignmentOptions.Center, 0, 1, 1, 1, 20, -140, -20, -100);

        // Verdict badge
        GameObject badgeObj = MakeUI("Badge", resultRT);
        Image badgeImg = badgeObj.AddComponent<Image>();
        badgeImg.color = new Color(0.3f, 0.3f, 0.3f);
        badgeImg.raycastTarget = false;
        RectTransform badgeRT = badgeObj.GetComponent<RectTransform>();
        Anchor(badgeRT, 0.1f, 1f, 0.9f, 1f);
        badgeRT.offsetMin = new Vector2(0, -230);
        badgeRT.offsetMax = new Vector2(0, -150);

        TextMeshProUGUI verdictText = MakeAnchoredText(badgeRT, "VERDICT", 48, FontStyles.Bold,
            Color.white, TextAlignmentOptions.Center, 0, 0, 1, 1, 0, 0, 0, 0);

        // Nutrition text
        TextMeshProUGUI nutriText = MakeAnchoredText(resultRT, "Nutrition info", 26, FontStyles.Normal,
            new Color(0.85f, 0.85f, 0.85f), TextAlignmentOptions.TopLeft, 0, 0, 1, 1, 30, 100, -30, -240);

        // Close button (big enough for touch - 80x80)
        Button closeBtn = MakeButton(resultRT, "X", 36, new Color(0.85f, 0.2f, 0.2f),
            1f, 1f, 1f, 1f, 0, 0, 0, 0);
        RectTransform closeRT = closeBtn.GetComponent<RectTransform>();
        closeRT.anchorMin = closeRT.anchorMax = new Vector2(1, 1);
        closeRT.sizeDelta = new Vector2(80, 80);
        closeRT.anchoredPosition = new Vector2(-40, -40);

        // Wire close → hide result panel
        closeBtn.onClick.AddListener(() => resultObj.SetActive(false));

        // Start hidden
        resultObj.SetActive(false);

        // ─── HISTORY PANEL ───
        GameObject histPanel = MakeUI("HistoryPanel", root);
        Image histBg = histPanel.AddComponent<Image>();
        histBg.color = new Color(0.06f, 0.06f, 0.08f, 0.97f);
        Stretch(histPanel);

        // Title bar
        GameObject titleBar = MakeUI("TitleBar", histPanel.GetComponent<RectTransform>());
        Image titleBg = titleBar.AddComponent<Image>();
        titleBg.color = new Color(0.12f, 0.12f, 0.18f);
        RectTransform titleRT = titleBar.GetComponent<RectTransform>();
        Anchor(titleRT, 0, 1, 1, 1);
        titleRT.offsetMin = new Vector2(0, -100);
        titleRT.offsetMax = Vector2.zero;

        MakeAnchoredText(titleRT, "SCAN HISTORY", 40, FontStyles.Bold,
            Color.white, TextAlignmentOptions.Center, 0, 0, 1, 1, 0, 0, 0, 0);

        // History close button
        Button histClose = MakeButton(titleRT, "X", 32, new Color(0.85f, 0.2f, 0.2f),
            1, 0.5f, 1, 0.5f, 0, 0, 0, 0);
        RectTransform hcRT = histClose.GetComponent<RectTransform>();
        hcRT.anchorMin = hcRT.anchorMax = new Vector2(1, 0.5f);
        hcRT.sizeDelta = new Vector2(70, 70);
        hcRT.anchoredPosition = new Vector2(-50, 0);

        // Scroll view
        GameObject scrollObj = MakeUI("Scroll", histPanel.GetComponent<RectTransform>());
        RectTransform scrollRT = scrollObj.GetComponent<RectTransform>();
        Anchor(scrollRT, 0, 0, 1, 1);
        scrollRT.offsetMin = new Vector2(0, 0);
        scrollRT.offsetMax = new Vector2(0, -100);

        // Viewport
        GameObject viewportObj = MakeUI("Viewport", scrollRT);
        Stretch(viewportObj);
        Image vpImg = viewportObj.AddComponent<Image>();
        vpImg.color = Color.white;
        Mask vpMask = viewportObj.AddComponent<Mask>();
        vpMask.showMaskGraphic = false;

        // Content
        GameObject contentObj = MakeUI("Content", viewportObj.GetComponent<RectTransform>());
        RectTransform contentRT = contentObj.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.sizeDelta = new Vector2(0, 0);
        contentRT.anchoredPosition = Vector2.zero;

        ContentSizeFitter fitter = contentObj.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        VerticalLayoutGroup vlg = contentObj.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 12;
        vlg.padding = new RectOffset(15, 15, 15, 15);
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;

        ScrollRect scroll = scrollObj.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.content = contentRT;
        scroll.viewport = viewportObj.GetComponent<RectTransform>();
        scroll.movementType = ScrollRect.MovementType.Elastic;

        // ─── WIRE SCAN HISTORY ───
        ScanHistory scanHistory = gameObject.AddComponent<ScanHistory>();
        scanHistory.Initialize(histPanel, contentRT);

        logBtn.onClick.AddListener(() => scanHistory.ToggleHistory());
        histClose.onClick.AddListener(() => scanHistory.HideHistory());

        // ─── WIRE ALL COMPONENTS ───
        SetField(overlay, "productNameText", prodName);
        SetField(overlay, "brandText", brand);
        SetField(overlay, "verdictText", verdictText);
        SetField(overlay, "nutritionText", nutriText);
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
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = r.offsetMax = Vector2.zero;
    }

    private void Anchor(RectTransform r, float minX, float minY, float maxX, float maxY)
    {
        r.anchorMin = new Vector2(minX, minY);
        r.anchorMax = new Vector2(maxX, maxY);
    }

    private Button MakeButton(RectTransform parent, string label, int fontSize, Color bgColor,
        float aMinX, float aMinY, float aMaxX, float aMaxY,
        float oMinX, float oMinY, float oMaxX, float oMaxY)
    {
        GameObject obj = MakeUI("Btn_" + label, parent);
        Image img = obj.AddComponent<Image>();
        img.color = bgColor;
        Button btn = obj.AddComponent<Button>();
        btn.targetGraphic = img;

        // Make button colors respond to touch
        ColorBlock cb = btn.colors;
        cb.pressedColor = new Color(bgColor.r * 0.7f, bgColor.g * 0.7f, bgColor.b * 0.7f, bgColor.a);
        cb.highlightedColor = bgColor;
        btn.colors = cb;

        RectTransform rt = obj.GetComponent<RectTransform>();
        Anchor(rt, aMinX, aMinY, aMaxX, aMaxY);
        rt.offsetMin = new Vector2(oMinX, oMinY);
        rt.offsetMax = new Vector2(oMaxX, oMaxY);

        // Label
        GameObject lbl = MakeUI("Lbl", rt);
        TextMeshProUGUI t = lbl.AddComponent<TextMeshProUGUI>();
        t.text = label;
        t.fontSize = fontSize;
        t.color = Color.white;
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;
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
        t.text = text;
        t.fontSize = size;
        t.fontStyle = style;
        t.color = color;
        t.alignment = align;
        t.raycastTarget = false;
        RectTransform r = obj.GetComponent<RectTransform>();
        Anchor(r, aMinX, aMinY, aMaxX, aMaxY);
        r.offsetMin = new Vector2(oMinX, oMinY);
        r.offsetMax = new Vector2(oMaxX, oMaxY);
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