using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneCamera : WarpFrameSource
{

    #region shaders
    private Shader m_ShaderMotion;
    public Shader shaderMotion
    {
        get
        {
            if (m_ShaderMotion == null)
                m_ShaderMotion = Shader.Find("Hidden/MotionShader");

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
    private Shader m_ShaderDepth;
    public Shader shaderDepth
    {
        get
        {
            if (m_ShaderDepth == null)
                m_ShaderDepth = Shader.Find("Hidden/DepthShader");

            return m_ShaderDepth;
        }
    }

    private Material m_MaterialDepth;
    public Material materialDepth
    {
        get
        {
            if (m_MaterialDepth == null)
            {
                if (shaderDepth == null || shaderDepth.isSupported == false)
                    return null;

                m_MaterialDepth = new Material(shaderDepth);
            }

            return m_MaterialDepth;
        }
    }
    #endregion

    [Range(1,200)]
    public int nWarpFrames = 10;
    private Camera thisCamera;
    private int initialCullingMask;
    public CameraCommandBuffers analysisCameraCommandBuffers;
    public int targetFramerate;
    int width, height;

    override public int NWarpFrames() { return nWarpFrames; }

    public override int RenderWidth()
    {
        return width;
    }
    public override int RenderHeight()
    {
        return height;
    }

    override public Matrix4x4 ProjectionMatrix() { return this.GetComponent<Camera>().projectionMatrix; }

    // Start is called before the first frame update
    void Start()
    {
        width = Screen.width;
        if (stereoPreviewMode) width /= 2;
        height = Screen.height;
        thisCamera = GetComponent<Camera>();
        thisCamera.pixelRect = new Rect(0, 0, width, height);
        thisCamera.depthTextureMode = DepthTextureMode.MotionVectors | DepthTextureMode.Depth;
        //if (stereoPreviewMode) thisCamera.aspect /= 2;
        thisCamera.allowMSAA = false;
        color = new RenderTexture(width, height, 32, RenderTextureFormat.ARGBFloat);
        motion = new RenderTexture(width, height, 32, RenderTextureFormat.ARGBFloat);
        depth = new RenderTexture(width, height, 32, RenderTextureFormat.RFloat);
        initialCullingMask = thisCamera.cullingMask;
        
    }

    private void Update()
    {
        if (Time.frameCount % nWarpFrames == 0 && nWarpFrames != 1)
        {
            thisCamera.cullingMask = initialCullingMask;
            analysisCameraCommandBuffers.DisableCommandBuffers();
            thisCamera.depth = 2;
        }
        Application.targetFrameRate = targetFramerate;

        int scrWidth = Screen.width;
        if (stereoPreviewMode) scrWidth /= 2;

        if(width != scrWidth || height != Screen.height)
        {
            width = scrWidth;
            height = Screen.height;
            thisCamera.pixelRect = new Rect(0, 0, width, height);
            color = new RenderTexture(width, height, 32, RenderTextureFormat.ARGBFloat);
            motion = new RenderTexture(width, height, 32, RenderTextureFormat.ARGBFloat);
            depth = new RenderTexture(width, height, 32, RenderTextureFormat.RFloat);
        }
        
    }

    // Update is called once per frame
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if(Time.frameCount % nWarpFrames == 0)
        {
            materialMotion.SetFloat("motionMultiple", 1.0f);
            Graphics.Blit(source, color);
            Graphics.Blit(source, motion, materialMotion);
            Graphics.Blit(source, depth, materialDepth);

            if (nWarpFrames != 1)
            {
                Graphics.Blit(source, destination);
                thisCamera.cullingMask = 0;
            }
        }

    }

    private void OnPostRender()
    {
        if (Time.frameCount % nWarpFrames == 0 && nWarpFrames != 1)
        {
            analysisCameraCommandBuffers.EnableCommandBuffers();
            thisCamera.depth = 0;
        }
        
    }
}
