using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;

[System.Serializable]
public class FoodData
{
    public string barcode;
    public string productName;
    public string brand;
    public string imageUrl;
    public float calories;
    public float fat;
    public float saturatedFat;
    public float sugars;
    public float salt;
    public float fiber;
    public float proteins;
    public string nutriScore;

    public string GetVerdict()
    {
        if (!string.IsNullOrEmpty(nutriScore))
        {
            switch (nutriScore.ToLower())
            {
                case "a": return "Healthy";
                case "b": return "Healthy";
                case "c": return "Moderate";
                case "d": return "Unhealthy";
                case "e": return "Unhealthy";
            }
        }

        int badPoints = 0;
        if (sugars > 12.5f) badPoints += 2; else if (sugars > 6.3f) badPoints += 1;
        if (saturatedFat > 5f) badPoints += 2; else if (saturatedFat > 2f) badPoints += 1;
        if (salt > 1.5f) badPoints += 2; else if (salt > 0.6f) badPoints += 1;
        if (calories > 300f) badPoints += 2; else if (calories > 160f) badPoints += 1;

        int goodPoints = 0;
        if (fiber > 4.7f) goodPoints += 2; else if (fiber > 2.4f) goodPoints += 1;
        if (proteins > 8f) goodPoints += 2; else if (proteins > 4.7f) goodPoints += 1;

        int score = badPoints - goodPoints;
        if (score <= 1) return "Healthy";
        if (score <= 4) return "Moderate";
        return "Unhealthy";
    }
}

public class FoodDataFetcher : MonoBehaviour
{
    private const string API_URL = "https://world.openfoodfacts.org/api/v2/product/{0}.json";

    public IEnumerator FetchFoodData(string barcode, Action<FoodData> onComplete)
    {
        string url = string.Format(API_URL, barcode);
        Debug.Log($"[FoodDataFetcher] Requesting: {url}");

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("User-Agent", "FoodLabelAR-UnityApp/1.0");
            request.timeout = 10;
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[FoodDataFetcher] Request failed: {request.error}");
                onComplete?.Invoke(null);
                yield break;
            }

            FoodData data = ParseResponse(barcode, request.downloadHandler.text);
            onComplete?.Invoke(data);
        }
    }

    private FoodData ParseResponse(string barcode, string json)
    {
        try
        {
            JObject root = JObject.Parse(json);
            if (root.Value<int>("status") != 1) return null;

            JObject product = root.Value<JObject>("product");
            if (product == null) return null;

            JObject n = product.Value<JObject>("nutriments");

            return new FoodData
            {
                barcode = barcode,
                productName = GetStr(product, "product_name", "product_name_en") ?? "Unknown Product",
                brand = product.Value<string>("brands") ?? "Unknown Brand",
                imageUrl = product.Value<string>("image_url") ?? "",
                calories = GetNum(n, "energy-kcal_100g"),
                fat = GetNum(n, "fat_100g"),
                saturatedFat = GetNum(n, "saturated-fat_100g"),
                sugars = GetNum(n, "sugars_100g"),
                salt = GetNum(n, "salt_100g"),
                fiber = GetNum(n, "fiber_100g"),
                proteins = GetNum(n, "proteins_100g"),
                nutriScore = product.Value<string>("nutriscore_grade") ?? ""
            };
        }
        catch (Exception e)
        {
            Debug.LogError($"[FoodDataFetcher] Parse error: {e.Message}");
            return null;
        }
    }

    private float GetNum(JObject obj, string key)
    {
        if (obj == null) return 0f;
        JToken t = obj[key];
        if (t == null) return 0f;
        try { return t.Value<float>(); } catch { return 0f; }
    }

    private string GetStr(JObject obj, params string[] keys)
    {
        foreach (string k in keys)
        {
            string v = obj.Value<string>(k);
            if (!string.IsNullOrEmpty(v)) return v;
        }
        return null;
    }
}