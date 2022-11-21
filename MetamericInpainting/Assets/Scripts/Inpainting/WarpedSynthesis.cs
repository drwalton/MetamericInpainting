using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[RequireComponent(typeof(Camera))]
public class WarpedSynthesis : MonoBehaviour
{
    public bool hiq;
    public bool pre_filter = false;
    public bool preLoadNoise;
    public WarpFrameSource warpFrameSource;

    public PushPullAdapted stats;
    public CommandBufferCreator warpGO;

    public int pyramidDepth;
    private int m_pyramidDepth;

    private int m_size = 0;
    public bool weightLevels = false;

    int m_nLods = 0;
    RenderTexture[] m_noiseTile;
    // Start is called before the first frame update
    void OnEnable()
    {
        m_pyramidDepth = pyramidDepth;
        m_noiseTile = null;
        m_hnoise = null;
        m_hfiltered = null;
        m_a = null;
        m_b = null;
        m_c = null;
        m_lnoise = null;
        m_combinedLevels = null;
        m_convolvedLevels = null;
    }

    #region shaders
    private Shader m_shaderMatchNoise;
    public Shader shaderMatchNoise
    {
        get
        {
            if (m_shaderMatchNoise == null)
                m_shaderMatchNoise = Shader.Find("Hidden/MatchNoise");


            return m_shaderMatchNoise;
        }
    }

    private Material m_MaterialMatchNoise;
    public Material materialMatchNoise
    {
        get
        {
            if (m_MaterialMatchNoise == null)
            {
                if (shaderMatchNoise == null || shaderMatchNoise.isSupported == false)
                    return null;

                m_MaterialMatchNoise = new Material(shaderMatchNoise);
            }

            return m_MaterialMatchNoise;
        }
    }

    private Shader m_shaderAdd;
    public Shader shaderAdd
    {
        get
        {
            if (m_shaderAdd == null)
                m_shaderAdd = Shader.Find("Hidden/AddN");


            return m_shaderAdd;
        }
    }

    private Material m_MaterialAdd;
    public Material materialAdd
    {
        get
        {
            if (m_MaterialAdd == null)
            {
                if (shaderAdd == null || shaderAdd.isSupported == false)
                    return null;

                m_MaterialAdd = new Material(shaderAdd);
            }

            return m_MaterialAdd;
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




    private Shader m_compositeShader;
    public Shader compositeShader
    {
        get
        {
            if (m_compositeShader == null)
                m_compositeShader = Shader.Find("Hidden/CompositeWarped");

            return m_compositeShader;
        }
    }


    private Material m_compositeMaterial;
    public Material compositeMaterial
    {
        get
        {
            if (m_compositeMaterial == null)
            {
                if (compositeShader == null || compositeShader.isSupported == false)
                    return null;

                m_compositeMaterial = new Material(compositeShader);
            }

            return m_compositeMaterial;
        }
    }

    //StreamWriter sw;
    //float LastTime;
    //private void Awake()
    //{
    //    sw = new StreamWriter("timingSyn.txt");
    //    LastTime = Time.realtimeSinceStartup;
    //}
    //public bool rec = false;
    //    private void OnPostRender()
    //{
    //    if (rec) { 
    //        float thistime = Time.realtimeSinceStartup;
    //        sw.WriteLine((thistime - LastTime));
    //        LastTime = thistime;
    //    }
    //}
    //private void OnDestroy()
    //{
    //    sw.Close();
    //}





    private Shader m_ShaderConvolution;
    public Shader shaderConvolution
    {
        get
        {
            if (m_ShaderConvolution == null)
                m_ShaderConvolution = Shader.Find("Hidden/Convolution2D");


            return m_ShaderConvolution;
        }
    }

    private Material m_materialConvolution;
    public Material materialConvolution
    {
        get
        {
            if (m_materialConvolution == null)
            {
                if (shaderConvolution == null || shaderConvolution.isSupported == false)
                    return null;

                m_materialConvolution = new Material(shaderConvolution);
            }

            return m_materialConvolution;
        }
    }

    private Shader m_DebugShader;
    public Shader debugShader
    {
        get
        {
            if (m_DebugShader == null)
                m_DebugShader = Shader.Find("Hidden/Viewer");


            return m_DebugShader;
        }
    }

    private Material m_DebugMaterial;
    public Material debugMaterial
    {
        get
        {
            if (m_DebugMaterial == null)
            {
                if (debugShader == null || debugShader.isSupported == false)
                    return null;

                m_DebugMaterial = new Material(debugShader);
            }

            return m_DebugMaterial;
        }
    }
    #endregion

    private Texture2D LoadNoise(int size)
    {
        Texture2D fullText = Resources.Load("NoiseTile") as Texture2D;
        Color[] c = fullText.GetPixels(0, 0, size, size);
        Texture2D res = new Texture2D(size, size, TextureFormat.RGBAFloat, true);
        res.SetPixels(c);
        return res;
    }


    void InitNoiseTile()
    {
        print("Initializing noise Tile....");
        int size = stats.MaxSize;
        if (size == 0) return;

        m_size = size;
        m_nLods = (int)Mathf.Floor(Mathf.Log(size, 2f));
        m_nLods = Mathf.Min(m_nLods, pyramidDepth);

        Texture2D noiseBase;
        if (preLoadNoise)
        {
            noiseBase = LoadNoise(size);
        }
        else
        {

            noiseBase = new Texture2D(size, size, TextureFormat.RGBAFloat, true);
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    noiseBase.SetPixel(i, j, new Color(Random.value, Random.value, Random.value, 1.0f));
                }
            }
        }
        noiseBase.Apply();
        //Get pyramid of white noise: 
        m_noiseTile = PyramidUtils.SteerablePyramid(noiseBase, pyramidDepth, 2,-1, 10, hiq, pre_filter,false);

        print("Noise tile initialized");


        if (m_hnoise != null) m_hnoise.Release();
        m_hnoise = new RenderTexture(size, size, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        m_hnoise.filterMode = FilterMode.Bilinear;
        m_hnoise.useMipMap = false;
        m_hnoise.Create();
        m_hnoise.hideFlags = HideFlags.HideAndDontSave;

        if (m_hfiltered != null) m_hfiltered.Release();
        m_hfiltered = new RenderTexture(size, size, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        m_hfiltered.filterMode = FilterMode.Bilinear;
        m_hfiltered.useMipMap = false;
        m_hfiltered.Create();
        m_hfiltered.hideFlags = HideFlags.HideAndDontSave;

        if (m_a != null)
        {
            foreach (RenderTexture r in m_a)
                r.Release();
        }
        m_a = new List<RenderTexture>();
        if (m_b != null)
        {
            foreach (RenderTexture r in m_b)
                r.Release();
        }
        m_b = new List<RenderTexture>();
        if (m_c != null)
        {
            foreach (RenderTexture r in m_c)
                r.Release();
        }
        m_c = new List<RenderTexture>();

        for (int i = 0; i < m_nLods; i++)
        {
            ////Combine with noise////
            RenderTexture a = new RenderTexture(size, size, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            a.filterMode = FilterMode.Bilinear;
            a.useMipMap = false;
            a.Create();
            a.hideFlags = HideFlags.HideAndDontSave;

            RenderTexture b = new RenderTexture(size, size, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            b.filterMode = FilterMode.Bilinear;
            b.useMipMap = false;
            b.Create();
            b.hideFlags = HideFlags.HideAndDontSave;

            RenderTexture c = new RenderTexture(size, size, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            c.filterMode = FilterMode.Bilinear;
            c.useMipMap = false;
            c.Create();
            c.hideFlags = HideFlags.HideAndDontSave;

            m_a.Add(a);
            m_b.Add(b);
            m_c.Add(c);

            size >>= 1;

        }
        m_lnoise = new RenderTexture(size, size, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        m_lnoise.filterMode = FilterMode.Bilinear;
        m_lnoise.useMipMap = false;
        m_lnoise.Create();
        m_lnoise.hideFlags = HideFlags.HideAndDontSave;

        m_combinedLevels = new RenderTexture(m_size, m_size, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        m_combinedLevels.filterMode = FilterMode.Bilinear;
        m_combinedLevels.useMipMap = false;
        m_combinedLevels.Create();
        m_combinedLevels.hideFlags = HideFlags.HideAndDontSave;

        m_convolvedLevels = new RenderTexture(m_size, m_size, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        m_convolvedLevels.filterMode = FilterMode.Bilinear;
        m_convolvedLevels.useMipMap = false;
        m_convolvedLevels.Create();
        m_convolvedLevels.hideFlags = HideFlags.HideAndDontSave;

        m_finalResult = new RenderTexture(m_size, m_size, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        m_finalResult.filterMode = FilterMode.Bilinear;
        m_finalResult.useMipMap = false;
        m_finalResult.Create();
        m_finalResult.hideFlags = HideFlags.HideAndDontSave;

        m_finalResultRGB = new RenderTexture(m_size, m_size, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        m_finalResultRGB.filterMode = FilterMode.Bilinear;
        m_finalResultRGB.useMipMap = false;
        m_finalResultRGB.Create();
        m_finalResultRGB.hideFlags = HideFlags.HideAndDontSave;

    }


    RenderTexture m_hnoise;
    RenderTexture m_hfiltered;
    List<RenderTexture> m_a;
    List<RenderTexture> m_b;
    List<RenderTexture> m_c;
    RenderTexture m_lnoise;
    RenderTexture m_combinedLevels;
    RenderTexture m_convolvedLevels;
    RenderTexture m_finalResult;
    RenderTexture m_finalResultRGB;

    public void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if(Time.frameCount % warpFrameSource.NWarpFrames() == 0 && warpFrameSource.NWarpFrames() != 1)
        {
            return;
        }
      

        if (m_noiseTile == null || m_pyramidDepth != pyramidDepth) // AJS - no need to redo on eye movement?
        {
            m_pyramidDepth = pyramidDepth;
            Graphics.Blit(source, destination);
            InitNoiseTile();
            return;
        }

        RenderTexture[] sdevs = stats.getSDev();
        RenderTexture[] means = stats.getMean();

        //Go through each level, combine with noise, and Filter
        List<RenderTexture> levelResults = new List<RenderTexture>();

        int size = m_size;

        ////Do it to H0 first////

        // to allow bigger size
        materialConvolution.SetFloat("_K", 1);
        materialConvolution.SetFloatArray("_Kernel", PyramidUtils.getFilter(1,hiq));
        materialConvolution.SetFloat("_MeanDepth", (float)0);
        materialConvolution.SetFloat("_FoveaSize", (float)-1);
        materialConvolution.SetFloat("_FoveaX", (float)0.5);
        materialConvolution.SetFloat("_FoveaY", (float)0.5);

        materialMatchNoise.SetFloat("_LOD", (float)0);
        materialMatchNoise.SetTexture("_StdevTex", sdevs[0]);
        materialMatchNoise.SetTexture("_MeanTex", means[0]);
        materialMatchNoise.SetTexture("_NoiseTex", m_noiseTile[0]);


        if (!pre_filter)
        {
            Graphics.Blit(null, m_hnoise, materialMatchNoise);


            materialConvolution.SetFloat("_LOD", (float)0);
            materialConvolution.SetVector("_TexelSize", Vector2.one / (float)(size - 1));
            materialConvolution.SetFloat("_K", 1.0f);
            materialConvolution.SetFloatArray("_Kernel", PyramidUtils.getFilter(0,hiq));
            materialConvolution.SetInt("_KernelWidth", PyramidUtils.getFilterWidth(0,hiq));
            Graphics.Blit(m_hnoise, m_hfiltered, materialConvolution);

          

        }
        else
        {
            Graphics.Blit(null, m_hfiltered, materialMatchNoise);
        }


        for (int i = 0; i < m_nLods; i++)
        {
            ////Combine with noise////

            materialMatchNoise.SetFloat("_LOD", (float)i);

            materialMatchNoise.SetTexture("_StdevTex", sdevs[1]);
            materialMatchNoise.SetTexture("_MeanTex", means[1]);
            materialMatchNoise.SetTexture("_NoiseTex", m_noiseTile[1]);
            Graphics.Blit(null, m_b[i], materialMatchNoise);


            materialMatchNoise.SetTexture("_StdevTex", sdevs[2]);
            materialMatchNoise.SetTexture("_MeanTex", means[2]);
            materialMatchNoise.SetTexture("_NoiseTex", m_noiseTile[2]);
            Graphics.Blit(null, m_c[i], materialMatchNoise);

            //Filter -B
            if (!pre_filter)
            {
                materialConvolution.SetFloat("_LOD", (float)0);
                materialConvolution.SetVector("_TexelSize", Vector2.one / (float)(size - 1));
                float weight = 1.0f;
                if(weightLevels)
                {
                    weight = 1.0f / (float)(m_nLods - i);
                }                
                materialConvolution.SetFloat("_K", -1.0f * weight);

                materialConvolution.SetFloatArray("_Kernel", PyramidUtils.getFilter(2,hiq));
                materialConvolution.SetInt("_KernelWidth", PyramidUtils.getFilterWidth(2,hiq));
                Graphics.Blit(m_b[i], m_a[i], materialConvolution);

                materialConvolution.SetFloatArray("_Kernel", PyramidUtils.getFilter(3,hiq));
                materialConvolution.SetInt("_KernelWidth", PyramidUtils.getFilterWidth(3,hiq));
                Graphics.Blit(m_c[i], m_b[i], materialConvolution);


                materialAdd.SetTexture("_Tex1", m_a[i]);
                materialAdd.SetTexture("_Tex2", m_b[i]);
                materialAdd.SetInt("_NTextures", 2);
                Graphics.Blit(null, m_c[i], materialAdd);
                levelResults.Add(m_c[i]);

            }
            else
            {
                materialAdd.SetTexture("_Tex1", m_b[i]);
                materialAdd.SetTexture("_Tex2", m_c[i]);
                materialAdd.SetInt("_NTextures", 2);
                Graphics.Blit(null, m_a[i], materialAdd);
                levelResults.Add(m_a[i]);

            }

            size >>= 1;
        }




        //Match stats on lowpass residual

        //materialMatchNoise.SetFloat("_LOD", (float)m_nLods);
        //materialMatchNoise.SetTexture("_StdevTex", sdevs[0]);
        //materialMatchNoise.SetTexture("_MeanTex", means[0]);
        //materialMatchNoise.SetTexture("_NoiseTex", m_noiseTile[0]);
        //Graphics.Blit(null, m_lnoise, materialMatchNoise);

        colorMaterial.SetFloat("_LOD", (float)m_nLods);
        colorMaterial.SetInt("_Direction", 0);
        Graphics.Blit(means[0], m_lnoise, colorMaterial);



        materialAdd.SetInt("_NTextures", levelResults.Count + 1);
        materialAdd.SetTexture("_Tex1", m_lnoise);

        int k = 2;
        foreach (RenderTexture t in levelResults)
        {
            string name = "_Tex" + k++;
            materialAdd.SetTexture(name, t);
        }
        Graphics.Blit(null, m_combinedLevels, materialAdd);

        materialConvolution.SetFloat("_LOD", (float)0);
        materialConvolution.SetVector("_TexelSize", Vector2.one / (float)(m_size - 1));
        materialConvolution.SetFloat("_K", 1);
        materialConvolution.SetFloatArray("_Kernel", PyramidUtils.getFilter(1,hiq));
        materialConvolution.SetInt("_KernelWidth", PyramidUtils.getFilterWidth(1,hiq));
        Graphics.Blit(m_combinedLevels, m_convolvedLevels, materialConvolution);


        materialAdd.SetTexture("_Tex1", m_convolvedLevels);
        materialAdd.SetTexture("_Tex2", m_hfiltered);
        materialAdd.SetInt("_NTextures", 2);

        Graphics.Blit(null, m_finalResult, materialAdd); ;

        
        colorMaterial.SetFloat("_LOD", (float)0);
        colorMaterial.SetInt("_Direction", -1);
        Graphics.Blit(m_finalResult, m_finalResultRGB, colorMaterial);



        compositeMaterial.SetTexture("_InpaintedTex", m_finalResultRGB);
        Graphics.Blit(warpGO.ResultTexture(0), destination, compositeMaterial);



        //debugMaterial.SetInt("_DisplayMode", displayMode);
        //debugMaterial.SetInt("_LOD", debugLOD);
        //Graphics.Blit(m_noiseTile[0], destination, debugMaterial);
    }

    private void Update()
    {
        //if (Input.GetKeyUp(KeyCode.P))
        //{
        //    Camera c = this.GetComponent<Camera>();
        //    int width = c.pixelWidth;
        //    int height = c.pixelHeight;

        //    RenderTexture r = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGBFloat);
        //    copyFoveaMaterial.SetInt("_Blend", 1);
        //    copyFoveaMaterial.SetFloat("_MeanDepth", (float)meanDepth);
        //    copyFoveaMaterial.SetFloat("_FoveaSize", (float)foveaSize);
        //    copyFoveaMaterial.SetTexture("_SecondTex", m_finalResultRGB);
        //    Graphics.Blit(stats.getMean()[3], r, copyFoveaMaterial);

        //    RenderTexture.active = r;
        //    Texture2D resulTex = new Texture2D(width, height, TextureFormat.RGBAFloat, false);
        //    resulTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        //    resulTex.Apply();
        //    RenderTexture.ReleaseTemporary(r);
        //    byte[] bytes = resulTex.EncodeToPNG();
        //    print("Saving at " + Application.dataPath + "/print" + Time.renderedFrameCount + ".png");
        //    File.WriteAllBytes(Application.dataPath + "/print" + Time.renderedFrameCount + ".png", bytes);

        //}
    }
}
