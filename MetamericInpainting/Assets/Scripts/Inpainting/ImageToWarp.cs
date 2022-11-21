using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageToWarp : WarpFrameSource
{

    public Texture colorImage;
    public Texture depthImage;

    public float verticalFov = 45.0f;
    public float zNear = 0.1f;
    public float zFar = 50.0f;

    public Matrix4x4 projection;

    // Start is called before the first frame update
    void Start()
    {
        color = new RenderTexture(colorImage.width, colorImage.height, 32, RenderTextureFormat.ARGBFloat);
        //motion = new RenderTexture(colorImage.width, colorImage.height, 32, RenderTextureFormat.ARGBFloat);
        depth = new RenderTexture(colorImage.width, colorImage.height, 32, RenderTextureFormat.RFloat);
        Graphics.Blit(colorImage, color);
        Graphics.Blit(depthImage, depth);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public override int RenderWidth()
    {
        return colorImage.width;
    }

    public override int RenderHeight()
    {
        return colorImage.height;
    }

    public override Matrix4x4 ProjectionMatrix()
    {
        projection = Matrix4x4.Perspective(verticalFov, (float)RenderWidth() / (float)RenderHeight(), zNear, zFar);
        return projection;
    }
}
