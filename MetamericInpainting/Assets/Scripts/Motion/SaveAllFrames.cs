using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class SaveAllFrames : MonoBehaviour
{
    #region shaders

    private Shader m_ShaderPass;
    public Shader shaderPass
    {
        get
        {
            if (m_ShaderPass == null)
                m_ShaderPass = Shader.Find("Hidden/Passthrough");


            return m_ShaderPass;
        }
    }

    private Material m_MaterialPass;
    public Material materialPass
    {
        get
        {
            if (m_MaterialPass == null)
            {
                if (shaderPass == null || shaderPass.isSupported == false)
                    return null;

                m_MaterialPass = new Material(shaderPass);
            }

            return m_MaterialPass;
        }
    }

    #endregion

    private int frameNo = 0;
    public RenderTexture colorFrame;
    public int targetFrameRate = 10;


    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.MotionVectors | DepthTextureMode.Depth;
        colorFrame = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
        Application.targetFrameRate = targetFrameRate;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // Save both color and motion frames
        Graphics.Blit(source, colorFrame, materialPass);

        Graphics.Blit(colorFrame, destination, materialPass); //display color


        SaveRenderTextureColor(colorFrame, string.Format("frame{0}.png", frameNo));
        //SaveRenderTextureColorDepth(warpedColorFrame, string.Format("frame{0}.exr", frameNo));

        ++frameNo;
    }

    public static void FlipTextureVertically(Texture2D original)
    {
        var originalPixels = original.GetPixels();

        Color[] newPixels = new Color[originalPixels.Length];

        int width = original.width;
        int rows = original.height;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                newPixels[x + y * width] = originalPixels[x + (rows - y - 1) * width];
            }
        }

        original.SetPixels(newPixels);
        original.Apply();
    }

    void SaveRenderTextureColor(RenderTexture texture, string filename)
    {
        RenderTexture.active = texture;
        Texture2D tex = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
        RenderTexture.active = null;
        byte[] bytes;
        bytes = tex.EncodeToPNG();

        System.IO.File.WriteAllBytes(filename, bytes);
    }
    void SaveRenderTextureColorDepth(RenderTexture texture, string filename)
    {
        RenderTexture.active = texture;
        Texture2D tex = new Texture2D(texture.width, texture.height, TextureFormat.RGBAHalf, false);
        tex.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
        FlipTextureVertically(tex);
        RenderTexture.active = null;
        byte[] bytes;
        bytes = tex.EncodeToEXR();

        System.IO.File.WriteAllBytes(filename, bytes);
    }

}
