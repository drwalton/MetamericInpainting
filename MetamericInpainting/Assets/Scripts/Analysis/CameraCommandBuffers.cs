using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.IO;

[RequireComponent(typeof(Camera))]
public class CameraCommandBuffers : MonoBehaviour
{

    private const int MAXIMUM_BUFFER_SIZE = 8192;

    public int pyramidDepth = 5;
    private int m_pyramidDepth; 
    public CommandBufferCreator[] commands;
    public WarpFrameSource warpFrameSource;
    // Start is called before the first frame update
    void OnEnable()
    {
        m_size = 0;
        camera.depthTextureMode = DepthTextureMode.Depth;
        m_pyramidDepth = 0;
    }

    private int m_size;
    private CameraEvent m_CameraEvent = CameraEvent.AfterImageEffectsOpaque;

    private Camera m_Camera;
    public new Camera camera
    {
        get
        {
            if (m_Camera == null)
                m_Camera = GetComponent<Camera>();

            return m_Camera;
        }
    }

    public RenderTexture GetTexture(int step, int index)
    {
        int idx = Mathf.Min(step, commands.Length-1);
        return commands[idx].ResultTexture(index);
    }

    void OnDisable()
    {
        if (camera != null)
        {
              foreach(CommandBufferCreator c in commands)
                {
                    CommandBuffer cb = c.GetBuffer();
                    if (cb != null) camera.RemoveCommandBuffer(m_CameraEvent, cb);
                    c.enabled = false;
                }
        }
    }
        // Update is called once per frame
    void OnPreRender()
    {
        int size;
        if (warpFrameSource != null)
        {
            camera.pixelRect = new Rect(0, 0, warpFrameSource.RenderWidth(), warpFrameSource.RenderHeight());
            size = (int)Mathf.Max((float)warpFrameSource.RenderWidth(), (float)warpFrameSource.RenderHeight());
        }
        else
        {
            size = (int)Mathf.Max((float)camera.pixelWidth, (float)camera.pixelHeight);
        }
        size = (int)Mathf.Min((float)Mathf.NextPowerOfTwo(size), (float)MAXIMUM_BUFFER_SIZE);
        if(m_size != size || m_pyramidDepth != pyramidDepth)
        {
            m_size = size;
            m_pyramidDepth = pyramidDepth;
            foreach (CommandBufferCreator c in commands)
            {
                CommandBuffer cb = c.GetBuffer();
                if (cb != null) camera.RemoveCommandBuffer(m_CameraEvent, cb);
                c.Init(size,pyramidDepth);
                cb = c.GetBuffer();
                camera.AddCommandBuffer(m_CameraEvent, cb);

            }
        }
    }

    public void DisableCommandBuffers()
    {
        foreach(CommandBufferCreator c in commands)
        {
            CommandBuffer cb = c.GetBuffer();
            if (cb != null) camera.RemoveCommandBuffer(m_CameraEvent, cb);
        }
    }
    public void EnableCommandBuffers()
    {
        foreach(CommandBufferCreator c in commands)
        {
            CommandBuffer cb = c.GetBuffer();
            if (cb != null) camera.AddCommandBuffer(m_CameraEvent, cb);
        }
    }

    //StreamWriter sw;
    //float LastTime;
    //private void Awake()
    //{
    //    sw = new StreamWriter("timing.txt");
    //    LastTime = Time.realtimeSinceStartup;
    //}
    //private void OnPostRender()
    //{
    //    float thistime = Time.realtimeSinceStartup;
    //    sw.WriteLine((thistime - LastTime));
    //    LastTime = thistime;
    //}
    //private void OnDestroy()
    //{
    //    sw.Close();
    //}

}
