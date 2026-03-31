using UnityEngine;
using ZXing;

public class Color32LuminanceSource : BaseLuminanceSource
{
    public Color32LuminanceSource(Color32[] pixels, int width, int height)
        : base(width, height)
    {
        for (int y = 0; y < height; y++)
        {
            int srcRow = (height - 1 - y) * width;
            int dstRow = y * width;
            for (int x = 0; x < width; x++)
            {
                var c = pixels[srcRow + x];
                luminances[dstRow + x] = (byte)(0.299f * c.r + 0.587f * c.g + 0.114f * c.b);
            }
        }
    }

    protected override LuminanceSource CreateLuminanceSource(byte[] newLuminances, int width, int height)
    {
        var source = new Color32LuminanceSource(new Color32[width * height], width, height);
        System.Array.Copy(newLuminances, 0, source.luminances, 0, newLuminances.Length);
        return source;
    }
}