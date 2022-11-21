using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PullPushRGBDepthSmooth : PullPushRGB
{
    public float threshold = 0.04f;
    #region shaders
    private Shader m_passShader;
    public Shader passShader
    {
        get
        {
            if (m_passShader == null)
                m_passShader = Shader.Find("Hidden/Passthrough");

            return m_passShader;
        }
    }

    private Material m_passMaterial;
    public Material passMaterial
    {
        get
        {
            if (m_passMaterial == null)
            {
                if (passShader == null || passShader.isSupported == false)
                    return null;

                m_passMaterial = new Material(passShader);
            }

            return m_passMaterial;
        }
    }


    private Shader m_pullShader;
    public Shader pullShader
    {
        get
        {
            if (m_pullShader == null)
                m_pullShader = Shader.Find("Hidden/PullSmoothDepth");

            return m_pullShader;
        }
    }

    private Material m_pullMaterial;
    public Material pullMaterial
    {
        get
        {
            if (m_pullMaterial == null)
            {
                if (pullShader == null || pullShader.isSupported == false)
                    return null;

                m_pullMaterial = new Material(pullShader);
            }

            return m_pullMaterial;
        }
    }

    private Shader m_pushShader;
    public Shader pushShader
    {
        get
        {
            if (m_pushShader == null)
                m_pushShader = Shader.Find("Hidden/PushSmoothDepth");

            return m_pushShader;
        }
    }

    private Material m_pushMaterial;
    public Material pushMaterial
    {
        get
        {
            if (m_pushMaterial == null)
            {
                if (pushShader == null || pushShader.isSupported == false)
                    return null;

                m_pushMaterial = new Material(pushShader);
            }

            return m_pushMaterial;
        }
    }

    private Shader m_alphaToValidShader;
    public Shader alphaToValidShader
    {
        get
        {
            if (m_alphaToValidShader == null)
                m_alphaToValidShader = Shader.Find("Hidden/AlphaToValidityTex");

            return m_alphaToValidShader;
        }
    }

    private Material m_alphaToValidMaterial;
    public Material alphaToValidMaterial
    {
        get
        {
            if (m_alphaToValidMaterial == null)
            {
                if (alphaToValidShader == null || alphaToValidShader.isSupported == false)
                    return null;

                m_alphaToValidMaterial = new Material(alphaToValidShader);
            }

            return m_alphaToValidMaterial;
        }
    }
   


    #endregion

    public RenderTexture m_validity;

    public bool do2x2 = true;

    [Range(1e-6f, 10.0f)]
    public float validityPow = 1.0f;

    public bool skipPull = false;

    public void Update()
    {
        pushMaterial.SetFloat("_threshold", threshold);
        pullMaterial.SetFloat("validityPow", validityPow);
        pushMaterial.SetInt("do2x2", do2x2 ? 1 : 0);
    }


    public override void Init(int size, int pyramidDepth)
    {
        base.Init(size, pyramidDepth);

        m_validity = new RenderTexture(size, size, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
        m_validity.filterMode = FilterMode.Trilinear;
        m_validity.useMipMap = true;
        m_validity.autoGenerateMips = false;
        m_validity.Create();
        m_validity.hideFlags = HideFlags.HideAndDontSave;

        RenderTargetIdentifier result = new RenderTargetIdentifier(m_inpainted);
        RenderTargetIdentifier resultValidity = new RenderTargetIdentifier(m_validity);

        RenderTexture t = warpGO.ResultTexture(0);
        RenderTargetIdentifier input = new RenderTargetIdentifier(t);

        m_CommandBufferPP = new CommandBuffer();
        m_CommandBufferPP.name = "Pull push rgb using depth, with smooth continuous validity";

        RenderTextureDescriptor useMips = new RenderTextureDescriptor(size, size, RenderTextureFormat.ARGBFloat, 0);
        useMips.autoGenerateMips = false;
        useMips.useMipMap = true;
        useMips.sRGB = false;

        int countToPixel = (int)Mathf.Floor(Mathf.Log(size, 2f));

        //do the first level 
        m_CommandBufferPP.Blit(input, result);

        m_CommandBufferPP.Blit(input, resultValidity, alphaToValidMaterial);

        //Pushing to a single pixel using the shader for I and Isquared
        int sl = size;
        for (int l = 0; l < countToPixel; l++)
        {
            RenderTextureDescriptor halfRes = new RenderTextureDescriptor(sl >> 1, sl >> 1, RenderTextureFormat.ARGBFloat, 0);
            RenderTextureDescriptor halfResValid = new RenderTextureDescriptor(sl >> 1, sl >> 1, RenderTextureFormat.RFloat, 0);
            halfRes.autoGenerateMips = false;
            halfRes.useMipMap = false;
            halfRes.sRGB = false;

            m_CommandBufferPP.SetGlobalVector("_TexelSize", Vector2.one / (float)((sl >> 1) - 1));


            int pushOutput = Shader.PropertyToID("_pushOuput"  + l.ToString());
            int pushOutputValid = Shader.PropertyToID("_pushOuputValid"  + l.ToString());
            m_CommandBufferPP.GetTemporaryRT(pushOutput, halfRes, FilterMode.Trilinear);
            m_CommandBufferPP.GetTemporaryRT(pushOutputValid, halfResValid, FilterMode.Trilinear);
            RenderTargetIdentifier[] pushOutputs = { 
                new RenderTargetIdentifier(pushOutput), 
                new RenderTargetIdentifier(pushOutputValid) };
            m_CommandBufferPP.SetGlobalTexture("_Validity", m_validity);

            m_CommandBufferPP.SetGlobalFloat("_LOD", (float)l);
            m_CommandBufferPP.SetGlobalFloat("_ValidityLOD", (float)l);
            m_CommandBufferPP.SetGlobalInt("_size", sl);
            m_CommandBufferPP.SetGlobalFloat("_threshold", threshold);

            m_CommandBufferPP.SetRenderTarget(pushOutputs, new RenderTargetIdentifier(pushOutput));
            m_CommandBufferPP.Blit(result, BuiltinRenderTextureType.CurrentActive, pushMaterial);
            m_CommandBufferPP.CopyTexture(pushOutput, 0, 0, result, 0, l + 1);
            m_CommandBufferPP.CopyTexture(pushOutputValid, 0, 0, resultValidity, 0, l + 1);

            m_CommandBufferPP.ReleaseTemporaryRT(pushOutput);

            sl >>= 1;

        }

        if(skipPull)
        {
            return;
        }

        for (int l = countToPixel; l > 0 ; l--)
        {
            RenderTextureDescriptor doubleRes = new RenderTextureDescriptor(sl << 1, sl << 1, RenderTextureFormat.ARGBFloat, 0);
            doubleRes.autoGenerateMips = false;
            doubleRes.useMipMap = false;
            doubleRes.sRGB = false;

            m_CommandBufferPP.SetGlobalVector("_TexelSize", Vector2.one / (float)((sl << 1) - 1));
            m_CommandBufferPP.SetGlobalTexture("_Validity", m_validity);

            int pullOutput = Shader.PropertyToID("_pullOuput" + l.ToString());
            m_CommandBufferPP.GetTemporaryRT(pullOutput, doubleRes, FilterMode.Trilinear);
            m_CommandBufferPP.SetGlobalFloat("_LOD", (float)l-1);
            m_CommandBufferPP.SetGlobalFloat("_ValidityLOD", (float)l-1);
            m_CommandBufferPP.Blit(result, pullOutput, pullMaterial);
            m_CommandBufferPP.CopyTexture(pullOutput, 0, 0, result, 0, l - 1);

            m_CommandBufferPP.ReleaseTemporaryRT(pullOutput);

            sl <<= 1;

        }



    }
    public override RenderTexture ResultTexture(int index)
    {
        if(index == 0)
        {
            return m_inpainted;
        }
        else
        {
            return m_validity;
        }
    }
}
