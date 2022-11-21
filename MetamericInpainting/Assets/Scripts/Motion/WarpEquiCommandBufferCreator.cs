using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class WarpEquiCommandBufferCreator : CommandBufferCreator
{

    #region shaders
    private Shader m_ShaderMotion;
    public Shader shaderMotion
    {
        get
        {
            if (m_ShaderMotion == null)
                m_ShaderMotion = Shader.Find("Hidden/WarpImageMeshEqui");

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
    public RenderTexture m_WarpedImage, m_WarpedAlphas;
    public Mesh m_Mesh;
    public Color clearColor;
    public float triangleValidThreshold = 1000.0f;
    public int targetFrameRate = 0;
    public WarpFrameSource videoLoader;
    public bool saveSingleWarpedFrame = false;
    public bool saveAllFrames = false;
    private int width, height;
    public Transform warpTransform;
    private Matrix4x4 warpTransformMatrix;
    [Range(0.1f,10.0f)]
    public float depthScale;
    Vector3 initialPosition;
    public float translationMultiple = 5.0f;
    public bool vrMode = false;
   
    public override CommandBuffer GetBuffer()
    {
        return m_CommandBufferWarp;
    }

    public override RenderTexture ResultTexture(int index)
    {
        if (index == 0)
        {
            return m_WarpedImage;
        }
        else
        {
            return m_WarpedAlphas;
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

        width = videoLoader.color.width;
        height = videoLoader.color.height;

        m_Mesh = GenerateMesh(width, height);

        m_WarpedImage = new RenderTexture(Screen.width, Screen.height, 32, RenderTextureFormat.ARGBFloat);
        m_WarpedAlphas = new RenderTexture(Screen.width, Screen.height, 32, RenderTextureFormat.RFloat);
        m_WarpedAlphas.filterMode = FilterMode.Point;
        m_WarpedAlphas.autoGenerateMips = false;

        m_CommandBufferWarp = new CommandBuffer();
        m_CommandBufferWarp.name = "Warp With Transform";

        m_CommandBufferWarp.SetRenderTarget(m_WarpedImage);
        m_CommandBufferWarp.ClearRenderTarget(true, true, clearColor, 1.0f);
        m_CommandBufferWarp.SetGlobalTexture("_MainTex", videoLoader.color);
        m_CommandBufferWarp.SetGlobalTexture("_DepthTex", videoLoader.depth);
        m_CommandBufferWarp.SetGlobalFloat("depthDiffThreshold", triangleValidThreshold);
        m_CommandBufferWarp.DrawMesh(m_Mesh, Matrix4x4.identity, materialMotion);
        m_CommandBufferWarp.Blit(m_WarpedImage, m_WarpedAlphas, materialCopyAlphas);
    }

    private void Update()
    {
        Matrix4x4 camMatrix = GL.GetGPUProjectionMatrix(videoLoader.ProjectionMatrix(), false);
        //Matrix4x4 camMatrix = videoLoader.ProjectionMatrix();
        //Debug.Log(camMatrix);
        if (vrMode)
        {
            if (Time.frameCount == 10 || Input.GetKeyDown("r"))
            {
                initialPosition.x = warpTransform.worldToLocalMatrix[0, 3];
                initialPosition.y = warpTransform.worldToLocalMatrix[1, 3];
                initialPosition.z = warpTransform.worldToLocalMatrix[2, 3];
                //print(initialPosition);
            }

            //TODO check camera transforms work
            //warpTransformMatrix = warpTransform.worldToLocalMatrix;
            Matrix4x4 moveMatrix = warpTransform.worldToLocalMatrix;
            moveMatrix[0, 3] -= initialPosition.x;
            moveMatrix[1, 3] -= initialPosition.y;
            moveMatrix[2, 3] -= initialPosition.z;
            moveMatrix[0, 3] *= translationMultiple;
            moveMatrix[1, 3] *= -translationMultiple;
            moveMatrix[2, 3] *= translationMultiple;
            //print(moveMatrix);
            Matrix4x4 rotate = Matrix4x4.identity;
            rotate[0, 0] = -1;
            //rotate[0, 1] = 1;
            //rotate[1, 0] = 1;
            //rotate[1, 1] = 0;
            warpTransformMatrix = camMatrix * rotate * moveMatrix;
        } 
        else
        {
            warpTransformMatrix =
                camMatrix * warpTransform.parent.worldToLocalMatrix * warpTransform.localToWorldMatrix;
                //warpTransform.parent.worldToLocalMatrix * warpTransform.localToWorldMatrix * camMatrix;
        }
        materialMotion.SetMatrix("warpMatrix", warpTransformMatrix);
        materialMotion.SetFloat("depthScale", depthScale);
        materialMotion.SetFloat("triangleValidThreshold", triangleValidThreshold);

        if(saveSingleWarpedFrame)
        {
            SaveRenderTextureColorDepth(m_WarpedImage, "warped_image.exr");
            saveSingleWarpedFrame = false;
        }

        if(width != videoLoader.color.width || height != videoLoader.color.height)
        {
            width = videoLoader.color.width;
            height = videoLoader.color.height;
            m_Mesh = GenerateMesh(width, height);
            m_WarpedImage = new RenderTexture(Screen.width, Screen.height, 32, RenderTextureFormat.ARGBFloat);
            m_WarpedAlphas = new RenderTexture(Screen.width, Screen.height, 32, RenderTextureFormat.RFloat);
            m_WarpedAlphas.filterMode = FilterMode.Point;
            m_WarpedAlphas.autoGenerateMips = false;
        }
    }

    Mesh GenerateMesh(int width, int height)
    { 
        /*
        Vector3[] vertices = new Vector3[4];
        Vector2[] texCoords = new Vector2[4];
        int[] indices = new int[6];

        float d = 0.75f;

        vertices[0] = new Vector3(-1, -1, d);
        vertices[1] = new Vector3(-1, 1, d);
        vertices[2] = new Vector3(1, -1, d);
        vertices[3] = new Vector3(1, 1, d);

        texCoords[0] = new Vector2(0, 0);
        texCoords[1] = new Vector2(0, 1);
        texCoords[2] = new Vector2(1, 0);
        texCoords[3] = new Vector2(1, 1);

        indices[0] = 0;
        indices[1] = 1;
        indices[2] = 2;

        indices[3] = 1;
        indices[4] = 2;
        indices[5] = 3;
        */

        Vector3[] vertices = new Vector3[(width)*(height)];
        Vector2[] texCoords = new Vector2[(width)*(height)];
        int[] indices = new int[(width-1)*(height-1)*6];

        for (int r = 0; r < height; ++r)
        {
            for (int c = 0; c < width; ++c)
            {
                float theta = 2.0f * Mathf.PI * (float)(c) / (float)(width-1); //longitude
                float phi = -Mathf.PI * (((float)(r) / (float)(height-1)) - 0.5f); //latitude
                vertices[r * width + c] = new Vector3(
                    Mathf.Cos(theta)*Mathf.Cos(phi), 
                    Mathf.Sin(phi),
                    Mathf.Sin(theta)*Mathf.Cos(phi));
                float u = (((float)(c) + 0.5f) / (float)width);
                float v = (((float)(r) + 0.5f) / (float)height);
                texCoords[r * width + c] = new Vector2(u, v);
            }
        }
        for (int r = 0; r < height-1; ++r)
        {
            for (int c = 0; c < width-1; ++c)
            {
                int tl = r * width + c;
                int tr = tl + 1;
                int bl = tl + width;
                int br = tl + width + 1;
                int baseIdx = (r * (width-1) + c) * 6;
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

