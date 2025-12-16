using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class editTexture : MonoBehaviour
{
    [SerializeField] private Texture2D[] textures;
    [SerializeField] private string path;
    [SerializeField] private float light_coef = 1.25f;

    private void Start()
    {
        foreach (var texture in textures)
        {


            Texture2D copyTexture = new Texture2D(texture.width, texture.height);

            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    var pixel = texture.GetPixel(x, y);
                    copyTexture.SetPixel(x, y, new Color(pixel.r * light_coef, pixel.g * light_coef, pixel.b * light_coef, pixel.a));
                }
            }
            copyTexture.Apply();

            byte[] _bytes = copyTexture.EncodeToPNG();
            File.WriteAllBytes(Application.dataPath + path + $"/{texture.name}.png", _bytes);

            DirectoryInfo dir = new DirectoryInfo(Application.dataPath + path);
            FileInfo[] info = dir.GetFiles("*.*");
            print(info[0].DirectoryName);
        }
    }
}
