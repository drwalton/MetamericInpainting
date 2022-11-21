using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(CommandBufferCreator))]
public class PushPullJustBands : CommandBufferCreator
{
    protected const int MAXIMUM_BUFFER_SIZE = 8192;

    public SteerablePyramidJustBands pyramidGO;
    private int m_maxSize;
    public int MaxSize
    {
        get
        {
            return m_maxSize;
        }
    }

    protected int m_LODCount = 0;
    public int lodCount
    {
        get
        {
            if (m_inPyramids == null)
                return 0;

            return 1 + m_LODCount;
        }
    }
    protected RenderTexture[] m_inPyramids;

    protected RenderTexture[] m_meanPyramids;
    protected RenderTexture[] m_sdevPyramids;

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

    protected CommandBuffer m_commandBufferPPJB;

    public override CommandBuffer GetBuffer()
    {
        return m_commandBufferPPJB;
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
        m_commandBufferPPJB = null;
    }

    private void OnEnable()
    {
        m_commandBufferPPJB = null;
        m_sdevPyramids = null;
        m_meanPyramids = null;
        m_maxSize = 0;

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
    }
}
