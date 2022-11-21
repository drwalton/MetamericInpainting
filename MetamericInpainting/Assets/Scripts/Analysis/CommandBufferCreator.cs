using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public abstract class CommandBufferCreator : MonoBehaviour
{
    // Start is called before the first frame update
    public abstract void Init(int size,int pyramidDepth);

    public abstract CommandBuffer GetBuffer();

    public abstract RenderTexture ResultTexture(int index);
}
