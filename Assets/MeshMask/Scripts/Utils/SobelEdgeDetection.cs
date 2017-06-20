/* ==============================================================================
 * 功能描述：Sobel算子边缘检测
 * 创 建 者：shuchangliu
 * ==============================================================================*/

using UnityEngine;
using System.Collections;

public class SobelEdgeDetection {

    /// <summary>
    /// 只对超过alpha阈值的像素点进行边缘检测
    /// </summary>
    private float _alphaThreshold;
    private float _grayscaleThreshold;

    /// <summary>
    /// sobel算子
    /// </summary>
    public static int[,] sobelRectGx = new int[3,3]
    {
        { -1, 0, 1 },
        { -2, 0, 2 },
        { -1, 0, 1 }
    };
    public static int[,] sobelRectGy = new int[3,3]
    {
        { -1, -2, -1 },
        { 0, 0, 0 },
        { 1, 2, 1 }
    };

    private Texture2D _sourceTexture;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sourceTexture"></param>
    /// <param name="alphaThreshold"></param>
    /// <param name="grayscaleThreshold"></param>
    /// <returns></returns>
    public Texture2D Detect(Texture2D sourceTexture, float alphaThreshold = 0.9f, float grayscaleThreshold = 0.01f)
    {
        _sourceTexture = sourceTexture;
        _alphaThreshold = alphaThreshold;
        _grayscaleThreshold = grayscaleThreshold;

        Texture2D targetTexture = new Texture2D(sourceTexture.width, sourceTexture.height, TextureFormat.ARGB32, false);
#if UNITY_EDITOR
        targetTexture.alphaIsTransparency = true;
#endif

        Color gx, gy;
        for (int i = 0; i < sourceTexture.width; i++)
        {
            for (int j = 0; j < sourceTexture.height; j++)
            {
                gx = sobelRectGx[0,0] * GetBlackOrWhitePixel(i - 1, j - 1) + sobelRectGx[0,1] * GetBlackOrWhitePixel(i, j - 1) + sobelRectGx[0,2] * GetBlackOrWhitePixel(i + 1, j - 1) +
                    sobelRectGx[1,0] * GetBlackOrWhitePixel(i - 1, j) + sobelRectGx[1,1] * GetBlackOrWhitePixel(i, j) + sobelRectGx[1,2] * GetBlackOrWhitePixel(i + 1, j) +
                    sobelRectGx[2,0] * GetBlackOrWhitePixel(i - 1, j + 1) + sobelRectGx[2,1] * GetBlackOrWhitePixel(i, j + 1) + sobelRectGx[2,2] * GetBlackOrWhitePixel(i + 1, j + 1);

                gy = sobelRectGy[0,0] * GetBlackOrWhitePixel(i - 1, j - 1) + sobelRectGy[0,1] * GetBlackOrWhitePixel(i, j - 1) + sobelRectGy[0,2] * GetBlackOrWhitePixel(i + 1, j - 1) +
                    sobelRectGy[1,0] * GetBlackOrWhitePixel(i - 1, j) + sobelRectGy[1,1] * GetBlackOrWhitePixel(i, j) + sobelRectGy[1,2] * GetBlackOrWhitePixel(i + 1, j) +
                    sobelRectGy[2,0] * GetBlackOrWhitePixel(i - 1, j + 1) + sobelRectGy[2,1] * GetBlackOrWhitePixel(i, j + 1) + sobelRectGy[2,2] * GetBlackOrWhitePixel(i + 1, j + 1);

                if (GetPixel(i, j).a >= _alphaThreshold)
                {
                    if (gx.grayscale * gx.grayscale + gy.grayscale * gy.grayscale > this._grayscaleThreshold)
                    {
                        targetTexture.SetPixel(i, j, Color.red);
                    }
                    else
                    {
                        targetTexture.SetPixel(i, j, Color.clear);
                    }

                    //边边alpha值不为0的点设置为边缘点
                    if (i == 0 || i == sourceTexture.width - 1 || j == 0 || j == sourceTexture.height - 1)
                    {
                        if (GetPixel(i, j).a >= _alphaThreshold)
                        {
                            targetTexture.SetPixel(i, j, Color.red);
                        }
                    }
                }
                else
                {
                    targetTexture.SetPixel(i, j, Color.clear);
                }

            }
        }

        targetTexture.Apply();

        return targetTexture;
    }

    public Color GetBlackOrWhitePixel(int x, int y)
    {
        if (GetPixel(x, y).a >= _alphaThreshold)
            return Color.white;
        else
            return Color.black;
    }

    public Color GetPixel(int x, int y)
    {
        return _sourceTexture.GetPixel(x, y);
    }

    public double GetGray(Color c)
    {
        return 0.299*c.r + 0.587*c.g + 0.114*c.b;
    }
	
}
