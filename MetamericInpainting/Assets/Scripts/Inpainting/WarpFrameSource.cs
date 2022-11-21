using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class WarpFrameSource : MonoBehaviour
{
    public RenderTexture color, depth, motion;

    abstract public Matrix4x4 ProjectionMatrix();

    virtual public int NWarpFrames() { return 1; }

    public bool stereoPreviewMode;

    abstract public int RenderWidth();
    abstract public int RenderHeight();

}
