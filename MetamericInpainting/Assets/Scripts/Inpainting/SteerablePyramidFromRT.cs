using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SteerablePyramidFromRT : CommandBufferCreator
{
    public bool hiq;
    public bool preFilter;
    public bool combinedMatrix;
    //Laplacian material to get the first level
    #region shaders
    private Shader m_convShader;
    public Shader convolutionShader
    {
        get
        {
            if (m_convShader == null)
                m_convShader = Shader.Find("Hidden/ConvolutionMasked");

            return m_convShader;
        }
    }


    private Material m_convolutionMaterial;
    public Material convolutionMaterial
    {
        get
        {
            if (m_convolutionMaterial == null)
            {
                if (convolutionShader == null || convolutionShader.isSupported == false)
                    return null;

                m_convolutionMaterial = new Material(convolutionShader);
            }

            return m_convolutionMaterial;
        }
    }

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
    #endregion

    //Steerable pyramid has on index 0 just 2 mips: high pass and low pass.
    //The rest of the indices contain the inbetween mips 
    private RenderTexture[] m_SteerablePyramid;

    public CommandBufferCreator warpGO;
    private float foveaSize = -1f;
    private float foveaX = 0.5f;
    private float foveaY = 0.5f;
    private float meanDepth = 0;

    private int m_LODCount = 0;
    public int lodCount
    {
        get
        {
            if (m_SteerablePyramid == null)
                return 0;

            return 1 + m_LODCount;
        }
    }

    private CommandBuffer m_CommandBufferSteerable;

    public override CommandBuffer GetBuffer()
    {
        return m_CommandBufferSteerable;
    }

    void OnDisable()
    {

        if (m_SteerablePyramid != null)
        {
            foreach (RenderTexture r in m_SteerablePyramid)
            {
                r.Release();
            }
            m_SteerablePyramid = null;
        }
        m_CommandBufferSteerable = null;

    }
    private void OnEnable()
    {
        m_CommandBufferSteerable = null;
        m_SteerablePyramid = null;
    }

    public override void Init(int size, int pyramidDepth)
    {

        m_LODCount = (int)Mathf.Floor(Mathf.Log(size, 2f));
        m_LODCount = Mathf.Min(m_LODCount, pyramidDepth);

        if (m_LODCount == 0)
            return;

        if (m_SteerablePyramid != null)
        {
            foreach (RenderTexture r in m_SteerablePyramid)
            {
                r.Release();
            }
        }

        m_SteerablePyramid = new RenderTexture[3];
        RenderTargetIdentifier[] steerIDS = new RenderTargetIdentifier[3];

 
        for (int i = 0; i < 3; i++)
        {
            m_SteerablePyramid[i] = new RenderTexture(size, size, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            m_SteerablePyramid[i].filterMode = FilterMode.Point;
            m_SteerablePyramid[i].useMipMap = true;
            m_SteerablePyramid[i].autoGenerateMips = false;
            m_SteerablePyramid[i].Create();
            m_SteerablePyramid[i].hideFlags = HideFlags.HideAndDontSave;
            steerIDS[i] = new RenderTargetIdentifier(m_SteerablePyramid[i]);
        }

        m_CommandBufferSteerable = new CommandBuffer();
        m_CommandBufferSteerable.name = "Steerable Pyramid";


        //Get input texture
        int inputRGB = Shader.PropertyToID("inputTextureRGB");
        m_CommandBufferSteerable.GetTemporaryRT(inputRGB, size, size, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        m_CommandBufferSteerable.Blit(warpGO.ResultTexture(0), inputRGB);


        //Color conversion to start it all
        int input = Shader.PropertyToID("inputTexture");
        m_CommandBufferSteerable.GetTemporaryRT(input, size, size, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        m_CommandBufferSteerable.SetGlobalInt("_Direction", 1);
        m_CommandBufferSteerable.Blit(inputRGB, input, colorMaterial);

       

        //Copy Fovea to a separate texture

        //We need a L0 pass to work with the pyramid.
        RenderTextureDescriptor withMips = new RenderTextureDescriptor(size, size, RenderTextureFormat.ARGBFloat, 0);
        withMips.autoGenerateMips = false;
        withMips.useMipMap = true;
        withMips.sRGB = false;

       
        m_CommandBufferSteerable.SetGlobalFloat("_MeanDepth", -1);
        m_CommandBufferSteerable.SetGlobalFloat("_FoveaSize", 0);
        m_CommandBufferSteerable.SetGlobalFloat("_FoveaX", 0.5f);
        m_CommandBufferSteerable.SetGlobalFloat("_FoveaY", 0.5f);


        int loOutput = Shader.PropertyToID("_loOutput");
        m_CommandBufferSteerable.GetTemporaryRT(loOutput, withMips, FilterMode.Point);
        m_CommandBufferSteerable.SetGlobalFloat("_LOD", 0);
        m_CommandBufferSteerable.SetGlobalVector("_TexelSize", Vector2.one / (float)(size - 1));
        m_CommandBufferSteerable.SetGlobalFloat("_K", 1);
        m_CommandBufferSteerable.SetGlobalFloatArray("_Kernel", PyramidUtils.getFilter(1,hiq));
        m_CommandBufferSteerable.SetGlobalInt("_KernelWidth", PyramidUtils.getFilterWidth(1,hiq));
        m_CommandBufferSteerable.Blit(input, loOutput, convolutionMaterial);

        //First level is a Hipass
        int hiOutput = Shader.PropertyToID("_hiOutput");
        m_CommandBufferSteerable.GetTemporaryRT(hiOutput, size, size, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);

        m_CommandBufferSteerable.SetGlobalFloat("_K", 1);


        m_CommandBufferSteerable.SetGlobalFloatArray("_Kernel", PyramidUtils.getFilter(0, hiq,combinedMatrix));
        m_CommandBufferSteerable.SetGlobalInt("_KernelWidth", PyramidUtils.getFilterWidth(0, hiq,combinedMatrix));

        if (preFilter && !combinedMatrix)
        {
            m_CommandBufferSteerable.Blit(input, hiOutput, convolutionMaterial);

            int hiOutput2 = Shader.PropertyToID("_hiOutput2");
            m_CommandBufferSteerable.GetTemporaryRT(hiOutput2, size, size, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            m_CommandBufferSteerable.Blit(hiOutput, hiOutput2, convolutionMaterial);
            m_CommandBufferSteerable.CopyTexture(hiOutput2, 0, 0, steerIDS[0], 0, 0);
            m_CommandBufferSteerable.ReleaseTemporaryRT(hiOutput2);
        }
        else
        {
            m_CommandBufferSteerable.Blit(input, hiOutput, convolutionMaterial);
            m_CommandBufferSteerable.CopyTexture(hiOutput, 0, 0, steerIDS[0], 0, 0);

        }

      

        m_CommandBufferSteerable.ReleaseTemporaryRT(hiOutput);



        int s = size;

        for (int i = 0; i < m_LODCount; i++)
        {

            int tempOutput = Shader.PropertyToID("_tempOutput" + (i + 1).ToString());
            m_CommandBufferSteerable.GetTemporaryRT(tempOutput, s, s, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);


            m_CommandBufferSteerable.SetGlobalVector("_TexelSize", Vector2.one / (float)(s - 1));
            m_CommandBufferSteerable.SetGlobalFloat("_K", 1.0f);
            m_CommandBufferSteerable.SetGlobalFloat("_LOD", (float)i);


            m_CommandBufferSteerable.SetGlobalFloatArray("_Kernel", PyramidUtils.getFilter(2, hiq,combinedMatrix));
            m_CommandBufferSteerable.SetGlobalInt("_KernelWidth", PyramidUtils.getFilterWidth(2, hiq, combinedMatrix));

            if (preFilter && !combinedMatrix)
            {
                m_CommandBufferSteerable.Blit(loOutput, tempOutput, convolutionMaterial);

                int tempOutput2 = Shader.PropertyToID("_tempOutput2" + (i + 1).ToString());
                m_CommandBufferSteerable.GetTemporaryRT(tempOutput2, s, s, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
                m_CommandBufferSteerable.SetGlobalFloat("_K", -1.0f);
                m_CommandBufferSteerable.Blit(tempOutput, tempOutput2, convolutionMaterial);
                m_CommandBufferSteerable.CopyTexture(tempOutput2, 0, 0, steerIDS[1], 0, i);
            }
            else
            {
                m_CommandBufferSteerable.Blit(loOutput, tempOutput, convolutionMaterial);
                m_CommandBufferSteerable.CopyTexture(tempOutput, 0, 0, steerIDS[1], 0, i);

            }
           

            m_CommandBufferSteerable.SetGlobalFloat("_K", 1.0f);


            m_CommandBufferSteerable.SetGlobalFloatArray("_Kernel", PyramidUtils.getFilter(3,hiq,combinedMatrix));
            m_CommandBufferSteerable.SetGlobalInt("_KernelWidth", PyramidUtils.getFilterWidth(3,hiq,combinedMatrix));

            if (preFilter && !combinedMatrix)
            {
                m_CommandBufferSteerable.Blit(loOutput, tempOutput, convolutionMaterial);

                int tempOutput2 = Shader.PropertyToID("_tempOutput2" + (i + 1).ToString());
                m_CommandBufferSteerable.GetTemporaryRT(tempOutput2, s, s, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
                m_CommandBufferSteerable.SetGlobalFloat("_K", -1.0f);
                m_CommandBufferSteerable.Blit(tempOutput, tempOutput2, convolutionMaterial);
                m_CommandBufferSteerable.CopyTexture(tempOutput2, 0, 0, steerIDS[2], 0, i);
                m_CommandBufferSteerable.ReleaseTemporaryRT(tempOutput2);
            }
            else
            {
                m_CommandBufferSteerable.Blit(loOutput, tempOutput, convolutionMaterial);
                m_CommandBufferSteerable.CopyTexture(tempOutput, 0, 0, steerIDS[2], 0, i);

                }
          

            m_CommandBufferSteerable.ReleaseTemporaryRT(tempOutput);

            //use push shader to build next mip level of loOutput without messing up alphas.

            RenderTextureDescriptor halfRes = new RenderTextureDescriptor(s >> 1, s >> 1, RenderTextureFormat.ARGBFloat, 0);
            halfRes.autoGenerateMips = false;
            halfRes.useMipMap = false;
            halfRes.sRGB = false;

            m_CommandBufferSteerable.SetGlobalVector("_TexelSize", Vector2.one / (float)((s >> 1) - 1));

            int pushOutput = Shader.PropertyToID("_pushOuput" + i.ToString());
            m_CommandBufferSteerable.GetTemporaryRT(pushOutput, halfRes, FilterMode.Point);
            m_CommandBufferSteerable.SetGlobalFloat("_LOD", (float)i);
            m_CommandBufferSteerable.SetGlobalInt("_size", s);
            m_CommandBufferSteerable.Blit(loOutput, pushOutput, pushMaterial);
            m_CommandBufferSteerable.CopyTexture(pushOutput, 0, 0, loOutput, 0, i + 1);

            s >>= 1;

        }
        float[] nothing = { 1 };

        m_CommandBufferSteerable.SetGlobalFloatArray("_Kernel",nothing);
        m_CommandBufferSteerable.SetGlobalInt("_KernelWidth", 1);
        m_CommandBufferSteerable.SetGlobalFloat("_K", 1.0f);
        m_CommandBufferSteerable.SetGlobalFloat("_LOD", (float)m_LODCount);

        int lopassExpanded = Shader.PropertyToID("_lopassExpanded");
        s = size >> m_LODCount;
        m_CommandBufferSteerable.GetTemporaryRT(lopassExpanded, s, s, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        m_CommandBufferSteerable.Blit(loOutput, lopassExpanded, convolutionMaterial);

        m_CommandBufferSteerable.CopyTexture(loOutput, 0, m_LODCount, steerIDS[0], 0, m_LODCount);


    }

    public override RenderTexture ResultTexture(int index)
    {
        if (m_SteerablePyramid != null)
        {
            if (index < m_SteerablePyramid.Length)
                return m_SteerablePyramid[index];
            else
                return null;
        }
        else
            return null;

    }

    public void Update()
    {
       

    }
}
