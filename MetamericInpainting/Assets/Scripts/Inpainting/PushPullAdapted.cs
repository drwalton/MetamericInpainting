using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(CommandBufferCreator))]
public class PushPullAdapted : CommandBufferCreator
{
    private const int MAXIMUM_BUFFER_SIZE = 8192;

    public SteerablePyramidFromRT pyramidGO;
    public float threshold = 0.04f;
    int m_maxSize;
    #region shaders
    public int MaxSize
    {
        get
        {
            return m_maxSize;
        }
    }


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



    private int m_LODCount = 0;
    public int lodCount
    {
        get
        {
            if (m_inPyramids == null)
                return 0;

            return 1 + m_LODCount;
        }
    }
    #endregion

    private RenderTexture[] m_inPyramids;

    private RenderTexture[] m_meanPyramids;
    private RenderTexture[] m_sdevPyramids;

    public int displayTexture = 0;

    public override RenderTexture ResultTexture(int index)
    {
        return displayTexture == 0 ? m_meanPyramids[index] : m_sdevPyramids[index];
    }

    public RenderTexture[] getMean()
    {
        return m_meanPyramids;
    }

    public RenderTexture[] getSDev()
    {
        return m_sdevPyramids;
    }

    private CommandBuffer m_CommandBufferStats;

    public override CommandBuffer GetBuffer()
    {
        return m_CommandBufferStats;
    }

    void OnDisable()
    {
        if (m_meanPyramids != null)
        {
            foreach (RenderTexture r in m_meanPyramids)
            {
                r.Release();
            }
            m_meanPyramids = null;
        }

        if (m_sdevPyramids != null)
        {
            foreach (RenderTexture r in m_sdevPyramids)
            {
                r.Release();
            }
            m_sdevPyramids = null;
        }
        m_CommandBufferStats = null;
    }

    private void OnEnable()
    {
        m_CommandBufferStats = null;
        m_sdevPyramids = null;
        m_meanPyramids = null;
        m_maxSize = 0;

    }

    public void Update()
    {
        pushMaterial.SetFloat("_threshold", threshold);
    }


    public override void Init(int size, int pyramidDepth)
    {

        m_maxSize = size;
        m_LODCount = (int)Mathf.Floor(Mathf.Log(size, 2f));
        m_LODCount = Mathf.Min(m_LODCount, pyramidDepth);

        if (m_LODCount == 0)
            return;


        if (m_meanPyramids != null)
        {
            foreach (RenderTexture r in m_meanPyramids)
            {
                r.Release();
            }
            m_meanPyramids = null;
        }

        if (m_sdevPyramids != null)
        {
            foreach (RenderTexture r in m_sdevPyramids)
            {
                r.Release();
            }
            m_sdevPyramids = null;
        }

        List<RenderTexture> inTexts = new List<RenderTexture>();
        int k = 0;
        RenderTexture t = pyramidGO.ResultTexture(k);
        while (t != null)
        {
            inTexts.Add(t);
            t = pyramidGO.ResultTexture(++k);

        }

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

            inputIDS[i] = new RenderTargetIdentifier(m_inPyramids[i]);

        }

        /* -------------- Stats step -----------------*/

        m_CommandBufferStats = new CommandBuffer();
        m_CommandBufferStats.name = "Stats of a Pyramid on several bands";

        int s = size;


        for (int i = 0; i <= m_LODCount; i++)
        {
            int pushLevels = (int)Mathf.Floor(Mathf.Log(size, 2f));
            //pushing to calculate the mip levles of each one of InputTexs


            int nbands = i == m_LODCount? 1: inTexts.Count;
            int startIDX = (i == 0 || i==m_LODCount) ? 0 : 1;

            //per level, and per band, we need to push
            for (int j = startIDX; j < nbands; j++)
            {

                RenderTextureDescriptor useMips = new RenderTextureDescriptor(s, s, RenderTextureFormat.ARGBFloat, 0);
                useMips.autoGenerateMips = false;
                useMips.useMipMap = true;
                useMips.sRGB = false;

                int inputID = Shader.PropertyToID("_input" + i.ToString());
                m_CommandBufferStats.GetTemporaryRT(inputID, useMips, FilterMode.Trilinear);
                m_CommandBufferStats.SetGlobalFloat("_LOD", (float)i);
                m_CommandBufferStats.Blit(inputIDS[j], inputID, passMaterial);


                int squareInputID = Shader.PropertyToID("_squareInput" + i.ToString());
                m_CommandBufferStats.GetTemporaryRT(squareInputID, useMips, FilterMode.Trilinear);
                m_CommandBufferStats.Blit(inputIDS[j], squareInputID, squareMaterial);

                int countToPixel = (int)Mathf.Floor(Mathf.Log(s, 2f));




                //Pushing to a single pixel using the shader for I and Isquared
                int sl = s;
                for (int l = 0; l < countToPixel - 1; l++)
                {
                    RenderTextureDescriptor halfRes = new RenderTextureDescriptor(sl >> 1, sl >> 1, RenderTextureFormat.ARGBFloat, 0);
                    halfRes.autoGenerateMips = false;
                    halfRes.useMipMap = false;
                    halfRes.sRGB = false;

                    m_CommandBufferStats.SetGlobalVector("_TexelSize", Vector2.one / (float)((sl >> 1) - 1));


                    int pushOutput = Shader.PropertyToID("_pushOuput" + i.ToString() + j.ToString() + l.ToString());
                    m_CommandBufferStats.GetTemporaryRT(pushOutput, halfRes, FilterMode.Trilinear);
                    m_CommandBufferStats.SetGlobalFloat("_LOD", (float)l);
                    m_CommandBufferStats.SetGlobalInt("_size", sl);
                    m_CommandBufferStats.SetGlobalFloat("_threshold", threshold);
                    m_CommandBufferStats.Blit(inputID, pushOutput, pushMaterial);
                    m_CommandBufferStats.CopyTexture(pushOutput, 0, 0, inputID, 0, l + 1);

                    m_CommandBufferStats.Blit(squareInputID, pushOutput, pushMaterial);
                    m_CommandBufferStats.CopyTexture(pushOutput, 0, 0, squareInputID, 0, l + 1);


                    m_CommandBufferStats.ReleaseTemporaryRT(pushOutput);

                    sl >>= 1;

                }

                for (int l = countToPixel - 1; l > 0; l--)
                {
                    RenderTextureDescriptor doubleRes = new RenderTextureDescriptor(sl << 1, sl << 1, RenderTextureFormat.ARGBFloat, 0);
                    doubleRes.autoGenerateMips = false;
                    doubleRes.useMipMap = false;
                    doubleRes.sRGB = false;

                    m_CommandBufferStats.SetGlobalVector("_TexelSize", Vector2.one / (float)((sl << 1) - 1));

                    int pullOutput = Shader.PropertyToID("_pullOuput" + l.ToString());
                    m_CommandBufferStats.GetTemporaryRT(pullOutput, doubleRes, FilterMode.Trilinear);
                    m_CommandBufferStats.SetGlobalFloat("_LOD", (float)l - 1);
                    m_CommandBufferStats.Blit(inputID, pullOutput, pullMaterial);
                    m_CommandBufferStats.CopyTexture(pullOutput, 0, 0, inputID, 0, l - 1);

                    m_CommandBufferStats.Blit(squareInputID, pullOutput, pullMaterial);
                    m_CommandBufferStats.CopyTexture(pullOutput, 0, 0, squareInputID, 0, l - 1);

                    m_CommandBufferStats.ReleaseTemporaryRT(pullOutput);

                    sl <<= 1;

                }

                RenderTextureDescriptor noMips = new RenderTextureDescriptor(s, s, RenderTextureFormat.ARGBFloat, 0);
                noMips.autoGenerateMips = false;
                noMips.useMipMap = false;
                noMips.sRGB = false;

                int inpaintedInputID = Shader.PropertyToID("_inpaintedInput" + i.ToString());
                m_CommandBufferStats.GetTemporaryRT(inpaintedInputID, noMips, FilterMode.Trilinear);
                m_CommandBufferStats.SetGlobalInt("_nLODS", countToPixel);
                m_CommandBufferStats.Blit(inputID, inpaintedInputID, pullMaterial);

                int inpaintedSquareInputID = Shader.PropertyToID("_inpaintedSquareInput" + i.ToString());
                m_CommandBufferStats.GetTemporaryRT(inpaintedSquareInputID, noMips, FilterMode.Trilinear);
                m_CommandBufferStats.SetGlobalInt("_nLODS", countToPixel);
                m_CommandBufferStats.Blit(squareInputID, inpaintedSquareInputID, pullMaterial);


                //STANDARD DEVIATION////
                //Square of the input image, result already has mips because we have autoGenerateMips.true

                m_CommandBufferStats.SetGlobalFloat("_MeanDepth", (float)0);
                m_CommandBufferStats.SetGlobalFloat("_FoveaSize", (float)-1);

                int sdevID = Shader.PropertyToID("_stdev" + i.ToString());
                m_CommandBufferStats.GetTemporaryRT(sdevID, noMips, FilterMode.Bilinear);
                m_CommandBufferStats.SetGlobalTexture("_SecondaryTex", inpaintedSquareInputID);
                m_CommandBufferStats.Blit(inpaintedInputID, sdevID, sdevMaterial);


                m_CommandBufferStats.CopyTexture(inpaintedInputID, 0, 0, meanIDS[j], 0, i);
                m_CommandBufferStats.CopyTexture(sdevID, 0, 0, sdevIDS[j], 0, i);

                m_CommandBufferStats.ReleaseTemporaryRT(inputID);
                m_CommandBufferStats.ReleaseTemporaryRT(squareInputID);
                m_CommandBufferStats.ReleaseTemporaryRT(sdevID);
                m_CommandBufferStats.ReleaseTemporaryRT(inpaintedSquareInputID);
                m_CommandBufferStats.ReleaseTemporaryRT(inpaintedInputID);

            }

            s >>= 1;

        }

    }
}
