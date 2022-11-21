using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class WarpCommandBufferCreator : CommandBufferCreator
{

    #region shaders
    private Shader m_ShaderMotion;
    public Shader shaderMotion
    {
        get
        {
            if (m_ShaderMotion == null)
                m_ShaderMotion = Shader.Find("Hidden/WarpImageMeshMotionInpaint");

            return m_ShaderMotion;
        }
    }

    private Material m_MaterialMotion;
    public Material materialMotion
    {
        get
        {
            if (m_MaterialMotion == null)
            {
                if (shaderMotion == null || shaderMotion.isSupported == false)
                    return null;

                m_MaterialMotion = new Material(shaderMotion);
            }

            return m_MaterialMotion;
        }
    }

    private Shader m_ShaderCopyAlphas;
    public Shader shaderCopyAlphas
    {
        get
        {
            if (m_ShaderCopyAlphas == null)
                m_ShaderCopyAlphas = Shader.Find("Hidden/CopyAlphaShader");

            return m_ShaderCopyAlphas;
        }
    }

    private Material m_MaterialCopyAlphas;
    public Material materialCopyAlphas
    {
        get
        {
            if (m_MaterialCopyAlphas == null)
            {
                if (shaderCopyAlphas == null || shaderCopyAlphas.isSupported == false)
                    return null;

                m_MaterialCopyAlphas = new Material(shaderCopyAlphas);
            }

            return m_MaterialCopyAlphas;
        }
    }
    #endregion

    private CommandBuffer m_CommandBufferWarp;
    public RenderTexture m_WarpedImage, m_WarpedAlphas, m_InpaintedMotion;
    public RenderBuffer[] m_outputBuffer;

    private Mesh m_Mesh;
    public Color clearColor;
    public float triangleValidThreshold = 0.01f;
    public int targetFrameRate = 0;
    public WarpFrameSource warpFrameSource;
    public bool saveSingleWarpedFrame = false;
    public bool saveAllFrames = false;
    private int width, height;
    public bool linearWarp = true;
   
    public override CommandBuffer GetBuffer()
    {
        return m_CommandBufferWarp;
    }

    public override RenderTexture ResultTexture(int index)
    {
        if(index == 0) { 
        return m_WarpedImage;
        }
        else if (index == 1)
        {
            return m_WarpedAlphas;
        }
        else
        {
            return m_InpaintedMotion;
        }

    }

    private void OnEnable()
    {
        m_CommandBufferWarp = null;

    }

    void OnDisable()
    {
        if (m_WarpedImage != null)
        {
            m_WarpedImage.Release();
            m_WarpedImage = null;
        }

        m_CommandBufferWarp = null;
    }
    public override void Init(int size, int pyramidDepth)
    {
        //GetComponent<Camera>().depthTextureMode = DepthTextureMode.MotionVectors | DepthTextureMode.Depth;
        GetComponent<Camera>().allowMSAA = false;
        Application.targetFrameRate = targetFrameRate;

        width = Screen.width;
        height = Screen.height;

        m_Mesh = GenerateMesh(width, height);

        m_WarpedImage = new RenderTexture(width, height, 32, RenderTextureFormat.ARGBFloat);
        m_WarpedAlphas = new RenderTexture(width, height, 32, RenderTextureFormat.RFloat);
        m_InpaintedMotion = new RenderTexture(width, height, 32, RenderTextureFormat.ARGBFloat);
   

        m_WarpedAlphas.filterMode = FilterMode.Point;
        m_WarpedAlphas.autoGenerateMips = false;

        //m_outputBuffer = new RenderBuffer[3];
        //m_outputBuffer[0] = m_WarpedImage.colorBuffer;
        //m_outputBuffer[1] = m_WarpedAlphas.colorBuffer;
        //m_outputBuffer[2] = m_InpaintedMotion.colorBuffer;

        m_CommandBufferWarp = new CommandBuffer();
        m_CommandBufferWarp.name = "Warp";

        RenderTargetIdentifier[] rti =new RenderTargetIdentifier[]{ m_WarpedImage, m_WarpedAlphas, m_InpaintedMotion };
       
        m_CommandBufferWarp.SetRenderTarget(rti,m_WarpedImage.depthBuffer);

        m_CommandBufferWarp.ClearRenderTarget(true, true, clearColor);
        m_CommandBufferWarp.SetGlobalTexture("_MainTex", warpFrameSource.color);
        m_CommandBufferWarp.SetGlobalTexture("_MotionTex", warpFrameSource.motion);
        m_CommandBufferWarp.SetGlobalTexture("_DepthTex", warpFrameSource.depth);
        m_CommandBufferWarp.SetGlobalFloat("warpMultiple", 1.0f);
        m_CommandBufferWarp.SetGlobalFloat("sideLenThreshold", triangleValidThreshold);
        m_CommandBufferWarp.DrawMesh(m_Mesh, Matrix4x4.identity, materialMotion);
      
        //m_CommandBufferWarp.Blit(m_WarpedImage, m_WarpedAlphas, materialCopyAlphas);
    }


    private void Update()
    {
        if (linearWarp)
        {
            materialMotion.SetFloat("warpMultiple", (float)(Time.frameCount % warpFrameSource.NWarpFrames()));
        } 
        else
        {
            materialMotion.SetFloat("warpMultiple", 1.0f);
        }

        if (saveSingleWarpedFrame)
        {
            SaveRenderTextureColorDepth(m_WarpedImage, "warped_image.exr");
            saveSingleWarpedFrame = false;
        }

        // Handle resize
        if(width != Screen.width || height != Screen.height)
        {
            width = Screen.width;
            height = Screen.height;
            m_Mesh = GenerateMesh(width, height);
            m_WarpedImage = new RenderTexture(width, height, 32, RenderTextureFormat.ARGBFloat);
            m_WarpedAlphas = new RenderTexture(width, height, 32, RenderTextureFormat.RFloat);
            m_WarpedAlphas.filterMode = FilterMode.Point;
            m_WarpedAlphas.autoGenerateMips = false;
        }
    }



    Mesh GenerateMesh(int width, int height)
    { 
        Vector3[] vertices = new Vector3[(width+2)*(height+2)];
        Vector2[] texCoords = new Vector2[(width+2)*(height+2)];
        int[] indices = new int[(width+1)*(height+1)*6];

        for (int r = 0; r < height+2; ++r)
        {
            for (int c = 0; c < width + 2; ++c)
            {
                float x = ((float)(c - 1) / (float)(width - 1) * 2.0f) - 1.0f;
                float y = -(((float)(r - 1) / (float)(height - 1) * 2.0f) - 1.0f);
                float z = 0.0f;
                if (r == 0 || r == height + 1 || c == 0 || c == width + 1) z = 1.0f;
                vertices[r * (width+2) + c] = new Vector3(x, y, z);
                float u = (((float)(c - 1) +0.5f) / (float)(width));
                float v = (((float)(r - 1) +0.5f) / (float)(height));
                texCoords[r * (width+2) + c] = new Vector2(u, v);
            }
        }
        for (int r = 0; r < height+1; ++r)
        {
            for (int c = 0; c < width+1; ++c)
            {
                int tl = r * (width+2) + c;
                int tr = tl + 1;
                int bl = tl + (width+2);
                int br = tl + (width+2) + 1;
                int baseIdx = (r * (width+1) + c) * 6;
                indices[baseIdx + 0] = tl;
                indices[baseIdx + 1] = bl;
                indices[baseIdx + 2] = br;
                indices[baseIdx + 3] = tl;
                indices[baseIdx + 4] = br;
                indices[baseIdx + 5] = tr;
            }
        }
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);
        mesh.SetUVs(0, texCoords);
        return mesh;
    }
    void SaveRenderTextureColorDepth(RenderTexture texture, string filename)
    {
        RenderTexture.active = texture;
        Texture2D tex = new Texture2D(texture.width, texture.height, TextureFormat.RGBAHalf, false);
        tex.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
        RenderTexture.active = null;
        byte[] bytes;
        bytes = tex.EncodeToEXR();

        System.IO.File.WriteAllBytes(filename, bytes);
    }
}

