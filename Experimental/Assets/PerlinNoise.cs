using UnityEngine;

public class PerlinNoise : MonoBehaviour
{
    public int width = 256;
    public int height = 256;

    public float scale = 13.0f;

    float offsetX;// = 100.0f;
    float offsetY;// = 100.0f;

    void Start()
    {
        offsetX = Random.Range(0f, 100f);
        offsetY = Random.Range(0f, 100f);
    }

    void Update()
    {
        Renderer renderer = GetComponent<Renderer>();
        renderer.material.mainTexture = GenerateTexture();
    }

    Texture2D GenerateTexture()
    {
        Texture2D texture = new Texture2D(width, height);

        // generate perlin noise map for texture
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Color colour = CalculateColour(x, y);
                texture.SetPixel(x, y, colour);
            }
        }
        texture.Apply();
        return texture;
    }

    Color CalculateColour(int x, int y)
    {
        float _x = (float)x / width * scale + offsetX;
        float _y = (float)y / height * scale + offsetY;
        float sample = Mathf.PerlinNoise(_x, _y);
        return new Color(sample, sample, sample);
    }   
}
