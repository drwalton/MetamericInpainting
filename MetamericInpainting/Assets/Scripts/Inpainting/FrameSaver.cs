using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameSaver : MonoBehaviour
{
    public WarpFrameSource warpFrameSource = null;
    public WarpCommandBufferCreator warper = null;
    public PullPushRGBNaive pushPull = null;
    public WarpedSynthesisBands synthesis = null;
    public bool saveFrames = false;

    #region shaders
    private Shader m_colorShader;
    public Shader colorShader
    {
        get
        {
            if (m_colorShader == null)
                m_colorShader = Shader.Find("Hidden/ConvertYCrCb");

            return m_colorShader;
        }
    }


    private Material m_colorMaterial;
    public Material colorMaterial
    {
        get
        {
            if (m_colorMaterial == null)
            {
                if (colorShader == null || colorShader.isSupported == false)
                    return null;

                m_colorMaterial = new Material(colorShader);
            }

            return m_colorMaterial;
        }
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if ((saveFrames && Time.frameCount >= 1) || Input.GetKeyUp(KeyCode.P) )
        {
            // Subtract 1 here because we're actually saving the previous frame.
            string prefix = "Saved_Frame_" + (Time.frameCount-1).ToString();

            if (warpFrameSource != null)
            {
                SaveRenderTextureRGBAPNG(warpFrameSource.color, prefix + "_input.png");
            }
            if (warper != null)
            {
                SaveRenderTextureRGBAPNG(warper.m_WarpedImage, prefix + "_warped.png");
                SaveRenderTextureRGBAEXR(warper.m_WarpedAlphas, prefix + "_warped_alphas.exr");
            }
            if(pushPull != null)
            {
                RenderTexture rgbTex = RenderTexture.GetTemporary(pushPull.ResultTexture(0).descriptor);
                Graphics.Blit(pushPull.ResultTexture(0), rgbTex);
                SaveRenderTextureRGBPNG(rgbTex, prefix + "_push_pull.png");
            }
            if(synthesis != null)
            {
                colorMaterial.SetFloat("_LOD", (float)0);
                colorMaterial.SetInt("_Direction", -1);
                RenderTexture rgbTex = RenderTexture.GetTemporary(synthesis.m_finalResult.descriptor);
                Graphics.Blit(synthesis.m_finalResult, rgbTex, colorMaterial);

                SaveRenderTextureRGBPNG(rgbTex, prefix + "_inpainted_ours.png");
            }

        }
    }

    void SaveRenderTextureRGBAEXR(RenderTexture texture, string filename)
    {
        RenderTexture.active = texture;
        Texture2D tex = new Texture2D(texture.width, texture.height, TextureFormat.RGBAHalf, false);
        tex.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
        RenderTexture.active = null;
        byte[] bytes;
        bytes = tex.EncodeToEXR();

        System.IO.File.WriteAllBytes(filename, bytes);
    }
    void SaveRenderTextureRGBAPNG(RenderTexture texture, string filename)
    {
        RenderTexture.active = texture;
        Texture2D tex = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0, false);
        RenderTexture.active = null;
        byte[] bytes;
        bytes = tex.EncodeToPNG();

        System.IO.File.WriteAllBytes(filename, bytes);
    }

    void SaveRenderTextureRGBPNG(RenderTexture texture, string filename)
    {
        RenderTexture.active = texture;
        Texture2D tex = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0, false);
        RenderTexture.active = null;
        byte[] bytes;
        bytes = tex.EncodeToPNG();

        System.IO.File.WriteAllBytes(filename, bytes);
    }

}
