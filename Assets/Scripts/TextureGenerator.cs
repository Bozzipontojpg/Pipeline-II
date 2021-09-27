using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator
{
    public static Texture2D TextureFromColorMap(Color[] colorMap, int widht, int height, bool textureFilter, int blur)
    {
        Texture2D texture = new Texture2D(widht, height);
        if (textureFilter)
        {
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
        }
        texture.SetPixels(colorMap);
        texture.Apply();
        if (blur > 0)
        {
            return Blur(texture, blur);
        }
        else
        {
            return texture;
        }
    }
    
    public static Texture2D TextureFromHeightMap(float[,] heightMap, bool textureFilter)
    {
        int widht = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        Texture2D texture = new Texture2D(widht, height);

        Color[] colorMap = new Color[widht * height];
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < widht; x++)
            {
                colorMap[y * widht + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
            }
        }

        return TextureFromColorMap(colorMap, widht, height, textureFilter, 0);
    }
    
    private static Texture2D Blur(Texture2D image, int blurSize)
    {
        Texture2D blurred = new Texture2D(image.width, image.height);
     
        // look at every pixel in the blur rectangle
        for (int xx = 0; xx < image.width; xx++)
        {
            for (int yy = 0; yy < image.height; yy++)
            {
                float avgR = 0, avgG = 0, avgB = 0, avgA = 0;
                int blurPixelCount = 0;
     
                // average the color of the red, green and blue for each pixel in the
                // blur size while making sure you don't go outside the image bounds
                for (int x = xx; (x < xx + blurSize && x < image.width); x++)
                {
                    for (int y = yy; (y < yy + blurSize && y < image.height); y++)
                    {
                        Color pixel = image.GetPixel(x, y);
     
                        avgR += pixel.r;
                        avgG += pixel.g;
                        avgB += pixel.b;
                        avgA += pixel.a;
     
                        blurPixelCount++;
                    }
                }
     
                avgR = avgR / blurPixelCount;
                avgG = avgG / blurPixelCount;
                avgB = avgB / blurPixelCount;
                avgA = avgA / blurPixelCount;
     
                // now that we know the average for the blur size, set each pixel to that color
                for (int x = xx; x < xx + blurSize && x < image.width; x++)
                    for (int y = yy; y < yy + blurSize && y < image.height; y++)
                        blurred.SetPixel(x, y, new Color(avgR, avgG, avgB, avgA));
            }
        }
        blurred.Apply();
        return blurred;
    }
}
