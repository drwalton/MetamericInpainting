using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PullPushRGBNaive : PullPushRGB
{
    public bool do2x2;

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
                m_pullShader = Shader.Find("Hidden/Pull");

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
                m_pushShader = Shader.Find("Hidden/PushNaive");

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

   


    #endregion
    public override void Init(int size, int pyramidDepth)
    {
        base.Init(size, pyramidDepth);
        
        RenderTargetIdentifier result = new RenderTargetIdentifier(m_inpainted);

        RenderTexture t = warpGO.ResultTexture(0);  
        RenderTargetIdentifier input = new RenderTargetIdentifier(t);

        m_CommandBufferPP = new CommandBuffer();
        m_CommandBufferPP.name = "Pull push rgb";

        RenderTextureDescriptor useMips = new RenderTextureDescriptor(size, size, RenderTextureFormat.ARGBFloat, 0);
        useMips.autoGenerateMips = false;
        useMips.useMipMap = true;
        useMips.sRGB = false;

        int countToPixel = (int)Mathf.Floor(Mathf.Log(size, 2f));

        //do the first level 
        m_CommandBufferPP.Blit(input, result);

        //Pushing to a single pixel using the shader for I and Isquared
        int sl = size;
        for (int l = 0; l < countToPixel - 1; l++)
        {
            RenderTextureDescriptor halfRes = new RenderTextureDescriptor(sl >> 1, sl >> 1, RenderTextureFormat.ARGBFloat, 0);
            halfRes.autoGenerateMips = false;
            halfRes.useMipMap = false;
            halfRes.sRGB = false;

            m_CommandBufferPP.SetGlobalVector("_TexelSize", Vector2.one / (float)((sl >> 1) - 1));


            int pushOutput = Shader.PropertyToID("_pushOuput"  + l.ToString());
            m_CommandBufferPP.GetTemporaryRT(pushOutput, halfRes, FilterMode.Trilinear);
            m_CommandBufferPP.SetGlobalFloat("_LOD", (float)l);
            m_CommandBufferPP.SetGlobalInt("_size", sl);
            m_CommandBufferPP.SetGlobalInt("do2x2", do2x2?1:0);
            m_CommandBufferPP.Blit(result, pushOutput, pushMaterial);
            m_CommandBufferPP.CopyTexture(pushOutput, 0, 0, result, 0, l + 1);

            m_CommandBufferPP.ReleaseTemporaryRT(pushOutput);

            sl >>= 1;

        }

        for (int l = countToPixel-1; l > 0 ; l--)
        {
            RenderTextureDescriptor doubleRes = new RenderTextureDescriptor(sl << 1, sl << 1, RenderTextureFormat.ARGBFloat, 0);
            doubleRes.autoGenerateMips = false;
            doubleRes.useMipMap = false;
            doubleRes.sRGB = false;

            m_CommandBufferPP.SetGlobalVector("_TexelSize", Vector2.one / (float)((sl << 1) - 1));


            int pullOutput = Shader.PropertyToID("_pullOuput" + l.ToString());
            m_CommandBufferPP.GetTemporaryRT(pullOutput, doubleRes, FilterMode.Trilinear);
            m_CommandBufferPP.SetGlobalFloat("_LOD", (float)l-1);
            m_CommandBufferPP.Blit(result, pullOutput, pullMaterial);
            m_CommandBufferPP.CopyTexture(pullOutput, 0, 0, result, 0, l - 1);

            m_CommandBufferPP.ReleaseTemporaryRT(pullOutput);

            sl <<= 1;

        }
       
    }
}
