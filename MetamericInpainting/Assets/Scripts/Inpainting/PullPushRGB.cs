using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PullPushRGB : CommandBufferCreator
{
    protected const int MAXIMUM_BUFFER_SIZE = 8192;

    public CommandBufferCreator warpGO;
    protected int m_maxSize;
    public int MaxSize
    {
        get
        {
            return m_maxSize;
        }
    }

    protected RenderTexture m_inpainted;

    public int showTexture = 0;
    public override RenderTexture ResultTexture(int index)
    {
        return m_inpainted;
    }
  
    protected CommandBuffer m_CommandBufferPP;

    public override CommandBuffer GetBuffer()
    {
        return m_CommandBufferPP;
    }

    void OnDisable()
    {
        if (m_inpainted != null)
        {
            
                m_inpainted.Release();
         
        }

        m_CommandBufferPP = null;
    }

    private void OnEnable()
    {
        m_CommandBufferPP = null;
        m_inpainted = null;
        m_maxSize = 0;

    }
    public override void Init(int size, int pyramidDepth)
    {

        m_maxSize = size;

        m_inpainted = new RenderTexture(size, size, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        m_inpainted.filterMode = FilterMode.Trilinear;
        m_inpainted.useMipMap = true;
        m_inpainted.autoGenerateMips = false;
        m_inpainted.Create();
        m_inpainted.hideFlags = HideFlags.HideAndDontSave;
    }
}
