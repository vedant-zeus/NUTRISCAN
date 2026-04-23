using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ResultOverlay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI productNameText;
    [SerializeField] private TextMeshProUGUI brandText;
    [SerializeField] private TextMeshProUGUI verdictText;
    [SerializeField] private TextMeshProUGUI nutritionText;
    [SerializeField] private TextMeshProUGUI allergenText;
    [SerializeField] private Image verdictBackground;
    [SerializeField] private Button closeButton;

    private static readonly Color greenColor = new Color(0.18f, 0.76f, 0.35f);
    private static readonly Color amberColor = new Color(1f, 0.76f, 0.03f);
    private static readonly Color redColor = new Color(0.91f, 0.22f, 0.22f);

    public List<string> userAllergens = new List<string>();

    public void ShowResult(FoodData data)
    {
        if (data == null) return;
        gameObject.SetActive(true);

        if (productNameText != null) productNameText.text = data.productName;
        if (brandText != null) brandText.text = data.brand;

        string verdict = data.GetVerdict();
        if (verdictText != null)
        {
            verdictText.text = verdict.ToUpper();
            verdictText.color = Color.white;
        }

        if (verdictBackground != null)
        {
            switch (verdict)
            {
                case "Healthy":   verdictBackground.color = greenColor; break;
                case "Moderate":  verdictBackground.color = amberColor; break;
                case "Unhealthy": verdictBackground.color = redColor; break;
            }
        }

        if (nutritionText != null)
        {
            string ns = string.IsNullOrEmpty(data.nutriScore) ? "N/A" : data.nutriScore.ToUpper();
            nutritionText.text =
                $"Per 100g:\n" +
                $"Calories:  {data.calories:F0} kcal\n" +
                $"Fat:       {data.fat:F1}g  (sat: {data.saturatedFat:F1}g)\n" +
                $"Sugars:    {data.sugars:F1}g\n" +
                $"Salt:      {data.salt:F2}g\n" +
                $"Fiber:     {data.fiber:F1}g\n" +
                $"Protein:   {data.proteins:F1}g\n" +
                $"Nutri-Score: {ns}";
        }

        if (allergenText != null)
            allergenText.text = BuildAllergenDisplay(data);
    }

    private string BuildAllergenDisplay(FoodData data)
    {
        string result = "";

        if (data.allergens.Count > 0)
            result += "<b>Contains:</b> " + string.Join(", ", data.allergens);

        if (data.traces.Count > 0)
        {
            if (result.Length > 0) result += "\n";
            result += "<b>May contain:</b> " + string.Join(", ", data.traces);
        }

        if (userAllergens.Count > 0)
        {
            List<string> matched = data.GetMatchedAllergens(userAllergens);
            if (matched.Count > 0)
            {
                if (result.Length > 0) result += "\n\n";
                result += "<color=#FF3333><b>WARNING:</b> " + string.Join(", ", matched) + "</color>";
            }
            else
            {
                if (result.Length > 0) result += "\n\n";
                result += "<color=#33FF33>Safe for your allergen profile</color>";
            }
        }

        if (string.IsNullOrEmpty(result))
            result = "No allergen data available";

        return result;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}