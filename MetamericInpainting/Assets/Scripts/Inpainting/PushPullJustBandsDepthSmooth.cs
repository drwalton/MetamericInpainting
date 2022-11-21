using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(CommandBufferCreator))]
public class PushPullJustBandsDepthSmooth : PushPullJustBands
{
    //The following validity texture is used for all bands.
    //Lower resolution bands start using a higher LOD value.
    public RenderTexture[] m_validityTextures;
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


    private Shader m_pullSmoothShader;
    public Shader pullSmoothShader
    {
        get
        {
            if (m_pullSmoothShader == null)
                m_pullSmoothShader = Shader.Find("Hidden/PullSmoothDepth");

            return m_pullSmoothShader;
        }
    }

    private Material m_pullSmoothMaterial;
    public Material pullSmoothMaterial
    {
        get
        {
            if (m_pullSmoothMaterial == null)
            {
                if (pullSmoothShader == null || pullSmoothShader.isSupported == false)
                    return null;

                m_pullSmoothMaterial = new Material(pullSmoothShader);
            }

            return m_pullSmoothMaterial;
        }
    }

    private Shader m_pushSmoothShader;
    public Shader pushSmoothShader
    {
        get
        {
            if (m_pushSmoothShader == null)
                m_pushSmoothShader = Shader.Find("Hidden/PushSmoothDepth");

            return m_pushSmoothShader;
        }
    }

    private Material m_pushSmoothMaterial;
    public Material pushSmoothMaterial
    {
        get
        {
            if (m_pushSmoothMaterial == null)
            {
                if (pushSmoothShader == null || pushSmoothShader.isSupported == false)
                    return null;

                m_pushSmoothMaterial = new Material(pushSmoothShader);
            }

            return m_pushSmoothMaterial;
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
                m_pushShader = Shader.Find("Hidden/Push");

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

    private Shader m_alpha2ValidShader;
    public Shader alpha2ValidShader
    {
        get
        {
            if (m_alpha2ValidShader == null)
                m_alpha2ValidShader = Shader.Find("Hidden/AlphaToValidityTex");

            return m_alpha2ValidShader;
        }
    }

    private Material m_alpha2ValidMaterial;
    public Material alpha2ValidMaterial
    {
        get
        {
            if (m_alpha2ValidMaterial == null)
            {
                if (alpha2ValidShader == null || alpha2ValidShader.isSupported == false)
                    return null;

                m_alpha2ValidMaterial = new Material(alpha2ValidShader);
            }

            return m_alpha2ValidMaterial;
        }
    }

    private Shader m_sdevShader;
    public Shader sdevShader
    {
        get
        {
            if (m_sdevShader == null)
                m_sdevShader = Shader.Find("Hidden/Sdev");

            return m_sdevShader;
        }
    }

    private Material m_sdevMaterial;
    public Material sdevMaterial
    {
        get
        {
            if (m_sdevMaterial == null)
            {
                if (sdevShader == null || sdevShader.isSupported == false)
                    return null;

                m_sdevMaterial = new Material(sdevShader);
            }

            return m_sdevMaterial;
        }
    }

    private Shader m_squareShader;
    public Shader squareShader
    {
        get
        {
            if (m_squareShader == null)
                m_squareShader = Shader.Find("Hidden/SquarePixel");

            return m_squareShader;
        }
    }

    private Material m_squareMaterial;
    public Material squareMaterial
    {
        get
        {
            if (m_squareMaterial == null)
            {
                if (squareShader == null || squareShader.isSupported == false)
                    return null;

                m_squareMaterial = new Material(squareShader);
            }

            return m_squareMaterial;
        }
    }
    #endregion

    public bool smoothInpaint = true;

    public override RenderTexture ResultTexture(int index)
    {
        if(displayTexture == 0)
        {
            if (index > m_meanPyramids.Length - 1)
                index = m_meanPyramids.Length - 1;
            return m_meanPyramids[index];
        }
        else if(displayTexture == 1)
        {
            if (index > m_sdevPyramids.Length - 1)
                index = m_sdevPyramids.Length - 1;
            return m_sdevPyramids[index];
        }
        else
        {
            if (index > m_validityTextures.Length - 1)
                index = m_validityTextures.Length - 1;
            return m_validityTextures[index];
        }
    }

    [Range(1e-6f,10.0f)]
    public float validityPow = 1.0f;
    public bool do2x2 = true;

    public void Update()
    {
        pushMaterial.SetFloat("_threshold", threshold);
        pushSmoothMaterial.SetFloat("_threshold", threshold);
        pullSmoothMaterial.SetFloat("validityPow", validityPow);
        pushSmoothMaterial.SetInt("do2x2", do2x2 ? 1 : 0);
    }


    public override void Init(int size, int pyramidDepth)
    {
     
        base.Init(size, pyramidDepth);

        List<RenderTexture> inTexts = new List<RenderTexture>();
        int k = 0;
        RenderTexture t = pyramidGO.ResultTexture(k);
        while (t != null)
        {
            inTexts.Add(t);
            t = pyramidGO.ResultTexture(++k);

        }


   
        m_validityTextures = new RenderTexture[k];
        RenderTargetIdentifier[] validIDS = new RenderTargetIdentifier[k];

        m_inPyramids = inTexts.ToArray();
        RenderTargetIdentifier[] inputIDS = new RenderTargetIdentifier[k];

        m_meanPyramids = new RenderTexture[k];
        RenderTargetIdentifier[] meanIDS = new RenderTargetIdentifier[k];

        m_sdevPyramids = new RenderTexture[k];
        RenderTargetIdentifier[] sdevIDS = new RenderTargetIdentifier[k];



       


        for (int i = 0; i < k; i++)
        {
            m_meanPyramids[i] = new RenderTexture(size, size, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            m_meanPyramids[i].filterMode = FilterMode.Trilinear;
            m_meanPyramids[i].useMipMap = true;
            m_meanPyramids[i].autoGenerateMips = false;
            m_meanPyramids[i].Create();
            m_meanPyramids[i].hideFlags = HideFlags.HideAndDontSave;
            meanIDS[i] = new RenderTargetIdentifier(m_meanPyramids[i]);

            m_sdevPyramids[i] = new RenderTexture(size, size, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            m_sdevPyramids[i].filterMode = FilterMode.Trilinear;
            m_sdevPyramids[i].useMipMap = true;
            m_sdevPyramids[i].autoGenerateMips = false;
            m_sdevPyramids[i].Create();
            m_sdevPyramids[i].hideFlags = HideFlags.HideAndDontSave;
            sdevIDS[i] = new RenderTargetIdentifier(m_sdevPyramids[i]);

            if (smoothInpaint) { 
                m_validityTextures[i] = new RenderTexture(size, size, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
                m_validityTextures[i].filterMode = FilterMode.Trilinear;
                m_validityTextures[i].useMipMap = true;
                m_validityTextures[i].autoGenerateMips = false;
                m_validityTextures[i].Create();
                m_validityTextures[i].hideFlags = HideFlags.HideAndDontSave;
                validIDS[i] = new RenderTargetIdentifier(m_validityTextures[i]);
            }
            inputIDS[i] = new RenderTargetIdentifier(m_inPyramids[i]);

        }

        m_commandBufferPPJB = new CommandBuffer();
        m_commandBufferPPJB.name = "Smooth pull push using depth, just bands";

        //if we need, copy validity
        if (smoothInpaint) { 
            for(int i = 0; i< k; i++)
            {
                m_commandBufferPPJB.Blit(inputIDS[i], validIDS[i], alpha2ValidMaterial);
            }
        }
        int s = size;


        for (int i = 0; i < m_LODCount; i++)
        {
            int pushLevels = (int)Mathf.Floor(Mathf.Log(size, 2f));
            //pushing to calculate the mip levles of each one of InputTexs


            int nbands = i == m_LODCount? 1: inTexts.Count;
            int startIDX = (i == 0 || i==m_LODCount) ? 0 : 1;
            
            int validityLODOffset;
            if(s == size) {
                validityLODOffset = 0;
            } else {
                validityLODOffset = (int)Mathf.Log(size / s, 2f);
            }

          

            //per level, and per band, we need to push
            for (int j = startIDX; j < nbands; j++)
            {

                RenderTextureDescriptor useMips = new RenderTextureDescriptor(s, s, RenderTextureFormat.ARGBFloat, 0);
                useMips.autoGenerateMips = false;
                useMips.useMipMap = true;
                useMips.sRGB = false;

                int inputID = Shader.PropertyToID("_input" + i.ToString());
                m_commandBufferPPJB.GetTemporaryRT(inputID, useMips, FilterMode.Trilinear);
                m_commandBufferPPJB.SetGlobalFloat("_LOD", (float)i);
                m_commandBufferPPJB.Blit(inputIDS[j], inputID, passMaterial);


                int squareInputID = Shader.PropertyToID("_squareInput" + i.ToString());
                m_commandBufferPPJB.GetTemporaryRT(squareInputID, useMips, FilterMode.Trilinear);
                m_commandBufferPPJB.Blit(inputIDS[j], squareInputID, squareMaterial);

                int countToPixel = (int)Mathf.Floor(Mathf.Log(s, 2f));

                //Pushing to a single pixel using the shader for I and Isquared
                int sl = s;
                for (int l = 0; l < countToPixel; l++)
                {
                    RenderTextureDescriptor halfRes = new RenderTextureDescriptor(sl >> 1, sl >> 1, RenderTextureFormat.ARGBFloat, 0);
                    halfRes.autoGenerateMips = false;
                    halfRes.useMipMap = false;
                    halfRes.sRGB = false;
                    RenderTextureDescriptor halfResValid = new RenderTextureDescriptor(sl >> 1, sl >> 1, RenderTextureFormat.RFloat, 0);
                    halfRes.autoGenerateMips = false;
                    halfRes.useMipMap = false;
                    halfRes.sRGB = false;

                    m_commandBufferPPJB.SetGlobalVector("_TexelSize", Vector2.one / (float)((sl >> 1) - 1));


                    int pushOutput = Shader.PropertyToID("_pushOuput" + i.ToString() + j.ToString() + l.ToString());
                    int pushOutputValid = Shader.PropertyToID("_pushOuputValid"  + l.ToString());
                    m_commandBufferPPJB.GetTemporaryRT(pushOutput, halfRes, FilterMode.Trilinear);
                    m_commandBufferPPJB.GetTemporaryRT(pushOutputValid, halfResValid, FilterMode.Trilinear);
                    RenderTargetIdentifier[] pushOutputs = { 
                        new RenderTargetIdentifier(pushOutput), 
                        new RenderTargetIdentifier(pushOutputValid) };
                    m_commandBufferPPJB.SetGlobalTexture("_Validity", validIDS[j]);
                    m_commandBufferPPJB.SetGlobalFloat("_LOD", (float)l);
                    m_commandBufferPPJB.SetGlobalFloat("_ValidityLOD", (float)l+validityLODOffset);
                    m_commandBufferPPJB.SetGlobalInt("_size", sl);
                    m_commandBufferPPJB.SetGlobalFloat("_threshold", threshold);
                    m_commandBufferPPJB.SetGlobalFloat("_validityPow", validityPow);
                    if (smoothInpaint)
                    {
                        m_commandBufferPPJB.SetRenderTarget(pushOutputs, new RenderTargetIdentifier(pushOutput));
                        m_commandBufferPPJB.Blit(inputID, BuiltinRenderTextureType.CurrentActive, pushSmoothMaterial);
                        m_commandBufferPPJB.CopyTexture(pushOutputValid, 0, 0, validIDS[j], 0, validityLODOffset + l + 1);
                    }
                    else
                    {
                        m_commandBufferPPJB.Blit(inputID, pushOutput, pushMaterial);
                    }
                    m_commandBufferPPJB.CopyTexture(pushOutput, 0, 0, inputID, 0, l + 1);

                    if (smoothInpaint)
                    {
                        m_commandBufferPPJB.SetRenderTarget(pushOutputs, new RenderTargetIdentifier(pushOutput));
                        m_commandBufferPPJB.Blit(squareInputID, BuiltinRenderTextureType.CurrentActive, pushSmoothMaterial);
                    }
                    else
                    {
                        m_commandBufferPPJB.Blit(squareInputID, pushOutput, pushMaterial);

                    }
                    m_commandBufferPPJB.CopyTexture(pushOutput, 0, 0, squareInputID, 0, l + 1);
                    //Note we don't need to save the validity output this time, as it'll be the same as before.


                    m_commandBufferPPJB.ReleaseTemporaryRT(pushOutput);
                    m_commandBufferPPJB.ReleaseTemporaryRT(pushOutputValid);

                    sl >>= 1;

                }

                if (skipPull) return;

                for (int l = countToPixel; l > 0; l--)
                {
                    RenderTextureDescriptor doubleRes = new RenderTextureDescriptor(sl << 1, sl << 1, RenderTextureFormat.ARGBFloat, 0);
                    doubleRes.autoGenerateMips = false;
                    doubleRes.useMipMap = false;
                    doubleRes.sRGB = false;

                    m_commandBufferPPJB.SetGlobalVector("_TexelSize", Vector2.one / (float)((sl << 1) - 1));
                    m_commandBufferPPJB.SetGlobalTexture("_Validity", validIDS[j]);

                    int pullOutput = Shader.PropertyToID("_pullOuput" + l.ToString());
                    m_commandBufferPPJB.GetTemporaryRT(pullOutput, doubleRes, FilterMode.Trilinear);
                    m_commandBufferPPJB.SetGlobalFloat("_LOD", (float)l - 1);
                    m_commandBufferPPJB.SetGlobalFloat("_ValidityLOD", (float)((l - 1) + validityLODOffset));
                    m_commandBufferPPJB.SetGlobalFloat("_validityPow", validityPow);
                    if (smoothInpaint)
                    {
                        m_commandBufferPPJB.Blit(inputID, pullOutput, pullSmoothMaterial);
                    }
                    else
                    {
                        m_commandBufferPPJB.Blit(inputID, pullOutput, pullMaterial);
                    }
                    m_commandBufferPPJB.CopyTexture(pullOutput, 0, 0, inputID, 0, l - 1);

                    if (smoothInpaint)
                    { 
                        m_commandBufferPPJB.Blit(squareInputID, pullOutput, pullSmoothMaterial);
                    }
                    else
                    { 
                        m_commandBufferPPJB.Blit(squareInputID, pullOutput, pullMaterial);
                    }
                    m_commandBufferPPJB.CopyTexture(pullOutput, 0, 0, squareInputID, 0, l - 1);

                    m_commandBufferPPJB.ReleaseTemporaryRT(pullOutput);

                    sl <<= 1;

                }

                RenderTextureDescriptor noMips = new RenderTextureDescriptor(s, s, RenderTextureFormat.ARGBFloat, 0);
                noMips.autoGenerateMips = false;
                noMips.useMipMap = false;
                noMips.sRGB = false;

                int inpaintedInputID = Shader.PropertyToID("_inpaintedInput" + i.ToString());
                m_commandBufferPPJB.GetTemporaryRT(inpaintedInputID, noMips, FilterMode.Trilinear);
                m_commandBufferPPJB.SetGlobalInt("_nLODS", countToPixel);
                m_commandBufferPPJB.Blit(inputID, inpaintedInputID, pullMaterial);

                int inpaintedSquareInputID = Shader.PropertyToID("_inpaintedSquareInput" + i.ToString());
                m_commandBufferPPJB.GetTemporaryRT(inpaintedSquareInputID, noMips, FilterMode.Trilinear);
                m_commandBufferPPJB.SetGlobalInt("_nLODS", countToPixel);
                m_commandBufferPPJB.Blit(squareInputID, inpaintedSquareInputID, pullMaterial);


                //STANDARD DEVIATION////
                //Square of the input image, result already has mips because we have autoGenerateMips.true

                m_commandBufferPPJB.SetGlobalFloat("_MeanDepth", (float)0);
                m_commandBufferPPJB.SetGlobalFloat("_FoveaSize", (float)-1);

                int sdevID = Shader.PropertyToID("_stdev" + i.ToString());
                m_commandBufferPPJB.GetTemporaryRT(sdevID, noMips, FilterMode.Bilinear);
                m_commandBufferPPJB.SetGlobalTexture("_SecondaryTex", inpaintedSquareInputID);
                m_commandBufferPPJB.Blit(inpaintedInputID, sdevID, sdevMaterial);


                m_commandBufferPPJB.CopyTexture(inpaintedInputID, 0, 0, meanIDS[j], 0, i);
                m_commandBufferPPJB.CopyTexture(sdevID, 0, 0, sdevIDS[j], 0, i);

                m_commandBufferPPJB.ReleaseTemporaryRT(inputID);
                m_commandBufferPPJB.ReleaseTemporaryRT(squareInputID);
                m_commandBufferPPJB.ReleaseTemporaryRT(sdevID);
                m_commandBufferPPJB.ReleaseTemporaryRT(inpaintedSquareInputID);
                m_commandBufferPPJB.ReleaseTemporaryRT(inpaintedInputID);

            }

            s >>= 1;

        }

    }

    public bool skipPull;
}
