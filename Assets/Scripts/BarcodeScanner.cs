using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ZXing;
using System.Collections;

public class BarcodeScanner : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private RawImage cameraPreview;
    [SerializeField] private int cameraWidth = 1280;
    [SerializeField] private int cameraHeight = 720;

    [Header("UI References")]
    [SerializeField] private Button scanButton;
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("References")]
    [SerializeField] private FoodDataFetcher foodDataFetcher;
    [SerializeField] private ResultOverlay resultOverlay;
    [SerializeField] private ScanHistory scanHistory;

    private WebCamTexture webCamTexture;
    private BarcodeReaderGeneric barcodeReader;
    private bool isScanning = false;
    private int scanFramesRemaining = 0;

    private void Start()
    {
        barcodeReader = new BarcodeReaderGeneric();
        barcodeReader.AutoRotate = true;
        barcodeReader.Options.TryHarder = true;
        barcodeReader.Options.PossibleFormats = new[]
        {
            BarcodeFormat.EAN_13, BarcodeFormat.EAN_8,
            BarcodeFormat.UPC_A, BarcodeFormat.UPC_E,
            BarcodeFormat.CODE_128, BarcodeFormat.CODE_39
        };

        StartCoroutine(InitCamera());

        if (scanButton != null)
            scanButton.onClick.AddListener(OnScanTapped);

        SetStatus("Initializing camera...");
    }

    private IEnumerator InitCamera()
    {
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
            yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);

        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            SetStatus("Camera permission denied!");
            yield break;
        }

        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            SetStatus("No camera found!");
            yield break;
        }

        // Prefer back-facing camera
        string cameraName = devices[0].name;
        for (int i = 0; i < devices.Length; i++)
        {
            if (!devices[i].isFrontFacing)
            {
                cameraName = devices[i].name;
                break;
            }
        }

        webCamTexture = new WebCamTexture(cameraName, cameraWidth, cameraHeight);
        webCamTexture.Play();

        // Wait for camera to actually start
        float timer = 0f;
        while (!webCamTexture.isPlaying && timer < 5f)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (!webCamTexture.isPlaying)
        {
            SetStatus("Camera failed to start!");
            yield break;
        }

        // Wait for camera to stabilize (important on Android)
        yield return new WaitForSeconds(0.5f);

        if (cameraPreview != null)
        {
            cameraPreview.texture = webCamTexture;

            // Fix rotation for mobile
            int angle = webCamTexture.videoRotationAngle;
            cameraPreview.rectTransform.localEulerAngles = new Vector3(0, 0, -angle);

            if (webCamTexture.videoVerticallyMirrored)
                cameraPreview.uvRect = new Rect(0, 1, 1, -1);
        }

        SetStatus("Ready — tap SCAN!");
        Debug.Log($"[BarcodeScanner] Camera: {webCamTexture.deviceName} {webCamTexture.width}x{webCamTexture.height} rot:{webCamTexture.videoRotationAngle}");
    }

    /// <summary>
    /// Called every frame. Only processes when actively scanning
    /// and only when the camera has a genuinely new frame.
    /// This guarantees fresh pixel data on Android.
    /// </summary>
    private void Update()
    {
        // Only process if we're actively scanning
        if (scanFramesRemaining <= 0) return;
        if (webCamTexture == null || !webCamTexture.isPlaying) return;

        // CRITICAL: only read when camera has a NEW frame
        if (!webCamTexture.didUpdateThisFrame) return;

        scanFramesRemaining--;

        // Get fresh pixels directly (new array each time)
        Color32[] pixels = webCamTexture.GetPixels32();
        if (pixels == null || pixels.Length == 0) return;

        // Try to decode
        var source = new Color32LuminanceSource(pixels, webCamTexture.width, webCamTexture.height);
        var result = barcodeReader.Decode(source);

        if (result != null && !string.IsNullOrEmpty(result.Text))
        {
            // Found a barcode! Stop scanning immediately
            scanFramesRemaining = 0;
            Debug.Log($"[BarcodeScanner] Decoded: {result.Text}");
            StartCoroutine(ProcessBarcode(result.Text));
            return;
        }

        // If we've used all our attempts
        if (scanFramesRemaining <= 0)
        {
            SetStatus("No barcode found. Try again!");
            if (scanButton != null) scanButton.interactable = true;
            isScanning = false;
        }
    }

    private void OnScanTapped()
    {
        if (isScanning || webCamTexture == null || !webCamTexture.isPlaying)
            return;

        isScanning = true;
        SetStatus("Scanning...");
        if (scanButton != null) scanButton.interactable = false;

        // Try for 30 fresh camera frames (~1-2 seconds depending on FPS)
        scanFramesRemaining = 30;
    }

    private IEnumerator ProcessBarcode(string barcode)
    {
        SetStatus($"Found: {barcode}");
        yield return foodDataFetcher.FetchFoodData(barcode, OnFoodDataReceived);
        if (scanButton != null) scanButton.interactable = true;
        isScanning = false;
    }

    private void OnFoodDataReceived(FoodData data)
    {
        if (data == null)
        {
            SetStatus("Product not found in database.");
            return;
        }

        SetStatus($"Scanned: {data.productName}");
        if (resultOverlay != null) resultOverlay.ShowResult(data);
        if (scanHistory != null) scanHistory.AddEntry(data);
    }

    private void SetStatus(string message)
    {
        if (statusText != null) statusText.text = message;
    }

    private void OnDestroy()
    {
        if (webCamTexture != null && webCamTexture.isPlaying)
            webCamTexture.Stop();
    }
}