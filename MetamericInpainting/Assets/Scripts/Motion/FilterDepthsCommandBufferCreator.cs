using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class FilterDepthsCommandBufferCreator : CommandBufferCreator
{

    #region shaders
    private Shader m_ShaderFilter;
    public Shader shaderMotion
    {
        get
        {
            if (m_ShaderFilter == null)
                m_ShaderFilter = Shader.Find("Hidden/SmoothDepths");

            return m_ShaderFilter;
        }
    }

    private Material m_MaterialFilter;
    public Material materialFilter
    {
        get
        {
            if (m_MaterialFilter == null)
            {
                if (shaderMotion == null || shaderMotion.isSupported == false)
                    return null;

                m_MaterialFilter = new Material(shaderMotion);
            }

            return m_MaterialFilter;
        }
    }
    #endregion

    private CommandBuffer m_CommandBuffer;
    public WarpFrameSource videoLoader;
    public RenderTexture m_SmoothedDepth;
    public bool doFilter = true;
    public float colorSigma;
    public float spatialSigma;
    /*
    { set {
            spatialSigma = value;
            spatialWeighting = Make2DGaussian(2, spatialSigma);
            materialFilter.SetFloatArray("spatialWeighting", spatialWeighting);
        }
        get { return spatialSigma; } }
    */
    private float currSpatialSigma;
    private float[] spatialWeighting;

    public override CommandBuffer GetBuffer()
    {
        return m_CommandBuffer;
    }

    public override RenderTexture ResultTexture(int index)
    {
        return m_SmoothedDepth;
    }

    private void OnEnable()
    {
        m_CommandBuffer = null;

    }

    void OnDisable()
    {
        if (m_SmoothedDepth != null)
        {
            m_SmoothedDepth.Release();
            m_SmoothedDepth = null;
        }

        m_CommandBuffer = null;
    }
    public override void Init(int size, int pyramidDepth)
    {
        m_CommandBuffer = new CommandBuffer();
        m_CommandBuffer.name = "Filter depths";
        m_SmoothedDepth = new RenderTexture(2048, 1024, 32, RenderTextureFormat.RFloat);
        m_CommandBuffer.Blit(videoLoader.depth, m_SmoothedDepth, materialFilter);
        m_CommandBuffer.Blit(m_SmoothedDepth, videoLoader.depth);
        float[] spatialWeighting = Make2DGaussian(2, 1.0f);
        materialFilter.SetFloatArray("spatialWeighting", spatialWeighting);
    }

    private void Update()
    {
        materialFilter.SetTexture("colorTex", videoLoader.color);
        materialFilter.SetVector("texelSize", new Vector2(1.0f / (float)videoLoader.color.width, 1.0f / (float)videoLoader.color.height));
        materialFilter.SetInt("doFilter", doFilter ? 1 : 0);
        materialFilter.SetFloat("colorSigma", colorSigma);
        if (currSpatialSigma != spatialSigma)
        {
            currSpatialSigma = spatialSigma;
            float[] spatialWeighting = Make2DGaussian(2, spatialSigma);
            materialFilter.SetFloatArray("spatialWeighting", spatialWeighting);
        }
    }

    private float[] Make2DGaussian(int radius, float sigma)
    {

        int width = radius * 2 + 1;
        float[] gauss = new float[width * width];
        float c = 1.0f / (sigma * Mathf.Sqrt(2.0f * Mathf.PI));
        float oneOverSigmaSq = 1.0f / (sigma * sigma);
        int w = 0;
        for (int i = -radius; i <= radius; ++i)
        {
            for (int j = -radius; j <= radius; ++j)
            {
                float sqDist = (float)(i * i + j * j);
                gauss[w] = c * Mathf.Exp(-0.5f * sqDist * oneOverSigmaSq);
                ++w;

            }
        }
        return gauss;
    }
}
