using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class PostRenderMotion : MonoBehaviour
{
    #region shaders
    private Shader m_ShaderMotion;
    public Shader shaderMotion
    {
        get
        {
            if (m_ShaderMotion == null)
                m_ShaderMotion = Shader.Find("Hidden/MotionVectorShader");


            return m_ShaderMotion;
        }
    }

    private Material m_MaterialMotion;
    public Material materialMotion
    {
        get
        {
            if (m_MaterialMotion == null)
            {
                if (shaderMotion == null || shaderMotion.isSupported == false)
                    return null;

                m_MaterialMotion = new Material(shaderMotion);
            }

            return m_MaterialMotion;
        }
    }
    private Shader m_ShaderSavedMotion;
    public Shader shaderSavedMotion
    {
        get
        {
            if (m_ShaderSavedMotion == null)
                m_ShaderSavedMotion = Shader.Find("Hidden/SavedMotionVectorShader");


            return m_ShaderSavedMotion;
        }
    }

    private Material m_MaterialSavedMotion;
    public Material materialSavedMotion
    {
        get
        {
            if (m_MaterialSavedMotion == null)
            {
                if (shaderSavedMotion == null || shaderSavedMotion.isSupported == false)
                    return null;

                m_MaterialSavedMotion = new Material(shaderSavedMotion);
            }

            return m_MaterialSavedMotion;
        }
    }

    private Shader m_ShaderPass;
    public Shader shaderPass
    {
        get
        {
            if (m_ShaderPass == null)
                m_ShaderPass = Shader.Find("Hidden/Passthrough");


            return m_ShaderPass;
        }
    }

    private Material m_MaterialPass;
    public Material materialPass
    {
        get
        {
            if (m_MaterialPass == null)
            {
                if (shaderPass == null || shaderPass.isSupported == false)
                    return null;

                m_MaterialPass = new Material(shaderPass);
            }

            return m_MaterialPass;
        }
    }

    private Shader m_ShaderMotionPass;
    public Shader shaderMotionPass
    {
        get
        {
            if (m_ShaderMotionPass == null)
                m_ShaderMotionPass = Shader.Find("Hidden/MotionVectorPassShader");


            return m_ShaderMotionPass;
        }
    }
    private Material m_MaterialMotionPass;
    public Material materialMotionPass
    {
        get
        {
            if (m_MaterialMotionPass == null)
            {
                if (shaderMotionPass == null || shaderMotionPass.isSupported == false)
                    return null;

                m_MaterialMotionPass = new Material(shaderMotionPass);
            }

            return m_MaterialMotionPass;
        }
    }

    private Shader m_ShaderMotionMesh;
    public Shader shaderMotionMesh
    {
        get
        {
            if (m_ShaderMotionMesh == null)
                m_ShaderMotionMesh = Shader.Find("Hidden/WarpShaderMesh");


            return m_ShaderMotionMesh;
        }
    }
    private Material m_MaterialMotionMesh;
    public Material materialMotionMesh
    {
        get
        {
            if (m_MaterialMotionMesh == null)
            {
                if (shaderMotionMesh == null || shaderMotionMesh.isSupported == false)
                    return null;

                m_MaterialMotionMesh = new Material(shaderMotionMesh);
            }

            return m_MaterialMotionMesh;
        }
    }

    private Shader m_ShaderMotionMeshDepth;
    public Shader shaderMotionMeshDepth
    {
        get
        {
            if (m_ShaderMotionMeshDepth == null)
                m_ShaderMotionMeshDepth = Shader.Find("Hidden/WarpShaderMeshDepths");


            return m_ShaderMotionMeshDepth;
        }
    }
    private Material m_MaterialMotionMeshDepth;
    public Material materialMotionMeshDepth
    {
        get
        {
            if (m_MaterialMotionMeshDepth == null)
            {
                if (shaderMotionMeshDepth == null || shaderMotionMeshDepth.isSupported == false)
                    return null;

                m_MaterialMotionMeshDepth = new Material(shaderMotionMeshDepth);
            }

            return m_MaterialMotionMeshDepth;
        }
    }

    private Shader m_ShaderMotionMeshVisualise;
    public Shader shaderMotionMeshVisualise
    {
        get
        {
            if (m_ShaderMotionMeshVisualise == null)
                m_ShaderMotionMeshVisualise = Shader.Find("Hidden/WarpShaderMeshVisualise");


            return m_ShaderMotionMeshVisualise;
        }
    }
    private Material m_MaterialMotionMeshVisualise;
    public Material materialMotionMeshVisualise
    {
        get
        {
            if (m_MaterialMotionMeshVisualise == null)
            {
                if (shaderMotionMeshVisualise == null || shaderMotionMeshVisualise.isSupported == false)
                    return null;

                m_MaterialMotionMeshVisualise = new Material(shaderMotionMeshVisualise);
            }

            return m_MaterialMotionMeshVisualise;
        }
    }

    private Shader m_ShaderMotionMeshVisualisePerspective;
    public Shader shaderMotionMeshVisualisePerspective
    {
        get
        {
            if (m_ShaderMotionMeshVisualisePerspective == null)
                m_ShaderMotionMeshVisualisePerspective = Shader.Find("Hidden/WarpShaderMeshVisualisePerspective");


            return m_ShaderMotionMeshVisualisePerspective;
        }
    }
    private Material m_MaterialMotionMeshVisualisePerspective;
    public Material materialMotionMeshVisualisePerspective
    {
        get
        {
            if (m_MaterialMotionMeshVisualisePerspective == null)
            {
                if (shaderMotionMeshVisualisePerspective == null || shaderMotionMeshVisualisePerspective.isSupported == false)
                    return null;

                m_MaterialMotionMeshVisualisePerspective = new Material(shaderMotionMeshVisualisePerspective);
            }

            return m_MaterialMotionMeshVisualisePerspective;
        }
    }

    public ComputeShader pushPullShader;

    #endregion

    private int frameNo = 0;
    public RenderTexture currColorFrame, colorFrame, motionFrame, warpedColorFrame, warpedDepthFrame;
    public int frameToInterpolate = 10;
    public enum Mode { COLOR, MOTION, WARPING, WARPING_VISUALISE, WARPING_VISUALISE_PERSPECTIVE }
    public Mode mode = Mode.WARPING;
    public Mesh motionMesh, motionMeshVis;
    public int targetFrameRate = 10;
    public int warpMultiple;
    private float sideLenThreshold = 0.01f;
    public bool saveFrames = false;


    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.MotionVectors | DepthTextureMode.Depth;
        GetComponent<Camera>().allowMSAA = false;
        colorFrame = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
        warpedColorFrame = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGBFloat);
        currColorFrame = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGBFloat);
        warpedDepthFrame = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32);
        //warpedColorFrame.depth = 16;
        warpedColorFrame.enableRandomWrite = true;
        warpedColorFrame.useMipMap = true;
        warpedColorFrame.autoGenerateMips = false;
        warpedColorFrame.Create();
        warpedDepthFrame.enableRandomWrite = true;
        warpedDepthFrame.useMipMap = true;
        warpedDepthFrame.autoGenerateMips = false;
        warpedDepthFrame.Create();
        currColorFrame.enableRandomWrite = true;
        currColorFrame.useMipMap = true;
        currColorFrame.autoGenerateMips = false;
        currColorFrame.Create();
        motionFrame = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGBFloat);
        motionFrame.Create();
        Application.targetFrameRate = targetFrameRate;

        motionMesh = GenerateMesh(Screen.width, Screen.height);
        motionMeshVis = GenerateMesh(64, 64);
    }

    Mesh GenerateSurfelMesh()
    { 
        int numSurfels = Screen.width * Screen.height;
        Vector3[] vertices = new Vector3[numSurfels*4];
        Vector2[] texCoords = new Vector2[numSurfels*4];
        int[] indices = new int[numSurfels*6];
        for(int r = 0; r < Screen.height; ++r) { 
            for(int c = 0; c < Screen.width; ++c) {
                int i = r * Screen.width + c;
                //TODO setup vertices at screen pixel coords
                float x = ((float)c / (float)(Screen.width-1) * 2.0f) - 1.0f;
                float y = 1.0f - ((float)r / (float)(Screen.height-1) * 2.0f);
                float w = 2.0f / ((float)Screen.width-1);
                float h = 2.0f / ((float)Screen.height-1);

			    vertices[i*4 + 0] = new Vector3(x, y, 0.0f);
			    vertices[i*4 + 1] = new Vector3(x+w, y, 0.0f);
			    vertices[i*4 + 2] = new Vector3(x, y+h, 0.0f);
			    vertices[i*4 + 3] = new Vector3(x+w, y+h, 0.0f);

                //float u = (float)c / (float)Screen.width;
                //float v = 1.0f - ((float)r / (float)Screen.height);
                //float u = (float)c / ((float)Screen.width-1);
                //float v = 1.0f - ((float)r / ((float)Screen.height-1));
                float u = ((float)c + 0.5f) / (float)Screen.width;
                float v = 1.0f - (((float)r + 0.5f) / (float)Screen.height);
                Vector2 tex = new Vector2(u, v);
			    texCoords[i * 4 + 0] = tex;
			    texCoords[i * 4 + 1] = tex;
			    texCoords[i * 4 + 2] = tex;
			    texCoords[i * 4 + 3] = tex;

			    indices[i*6 + 0] = i*4 + 0;
			    indices[i*6 + 1] = i*4 + 2;
			    indices[i*6 + 2] = i*4 + 1;
			    indices[i*6 + 3] = i*4 + 1;
			    indices[i*6 + 4] = i*4 + 2;
			    indices[i*6 + 5] = i*4 + 3;
	        }
	    }
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);
        mesh.SetUVs(0, texCoords);
        return mesh;
    }
    Mesh GenerateMesh(int width, int height)
    { 
        Vector3[] vertices = new Vector3[width*height];
        Vector2[] texCoords = new Vector2[width*height];
        int[] indices = new int[(width-1)*(height-1)*6];

        for (int r = 0; r < height; ++r)
        {
            for (int c = 0; c < width; ++c)
            {
                float x = ((float)c / (float)(width-1) * 2.0f) - 1.0f;
                float y = ((float)r / (float)(height-1) * 2.0f) - 1.0f;
                vertices[r*width + c] = new Vector3(x, y, 0);
                float u = (((float)c + 0.5f) / (float)width);
                float v = 1.0f - (((float)r + 0.5f) / (float)height);
                texCoords[r * width + c] = new Vector2(u, v);
            }
        }
        for (int r = 0; r < height-1; ++r)
        {
            for (int c = 0; c < width-1; ++c)
            {
                int tl = r * width + c;
                int tr = tl + 1;
                int bl = tl + width;
                int br = tl + width + 1;
                int baseIdx = (r * (width-1) + c) * 6;
                indices[baseIdx + 0] = tl;
                indices[baseIdx + 1] = bl;
                indices[baseIdx + 2] = br;
                indices[baseIdx + 3] = tl;
                indices[baseIdx + 4] = br;
                indices[baseIdx + 5] = tr;
            }
        }
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);
        mesh.SetUVs(0, texCoords);
        return mesh;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        var foundMeshRenderers = FindObjectsOfType<MeshRenderer>();
        
        // Special case: if frameToInterpolate is <=1, don't do any interpolation & just show frame as usual.
        if(frameToInterpolate <= 1)
        {
            Graphics.Blit(source, destination, materialPass);
            foreach (var r in foundMeshRenderers)
            {
                r.enabled = true;
            }
            ++frameNo;
            return;
        }

        // Interpolation code
        // Case 1: Rendering
        if (frameNo % frameToInterpolate == 0)
        {
            // Save both color and motion frames
            Graphics.Blit(source, colorFrame, materialPass);
            Graphics.Blit(source, motionFrame, materialMotionPass);

            // Write frame to output
            if (mode == Mode.COLOR || mode == Mode.WARPING)
            {
                Graphics.Blit(colorFrame, destination, materialPass); //display color
            }
            else if (mode == Mode.MOTION)
            {
                Graphics.Blit(motionFrame, destination, materialSavedMotion); //display motion
            }

            // Disable rendering all MeshRenderers
            foreach (var r in foundMeshRenderers)
            {
                //r.enabled = false;
            }
        }
        // Case 2: Interpolation
        else
        {
            if (mode == Mode.COLOR)
            {
                Graphics.Blit(colorFrame, destination, materialPass); //display color
            }
            else if (mode == Mode.MOTION)
            {
                Graphics.Blit(motionFrame, destination, materialSavedMotion); //display motion
            }
            else if (mode == Mode.WARPING)
            {
	            warpMultiple = frameNo % frameToInterpolate;
                //Warp(colorFrame, motionFrame, warpedColorFrame, warpMultiple);

                // ** Warping colour, writing validity mask **
                Graphics.SetRenderTarget(destination);

                materialMotionMesh.SetFloat("xmul", warpMultiple);
                materialMotionMesh.SetFloat("ymul", warpMultiple);
                materialMotionMesh.SetFloat("sideLenThreshold", sideLenThreshold);
                motionFrame.filterMode = FilterMode.Point;
                materialMotionMesh.SetTexture("_MotionTex", motionFrame);
                colorFrame.filterMode = FilterMode.Point;
                materialMotionMesh.SetTexture("_MainTex", colorFrame);
                if (! materialMotionMesh.SetPass(0) ) 
                {
                    Debug.LogError("Failed to set pass");
                }
                GL.Clear(true, true, new Color(1,0,1,0));
                Graphics.DrawMeshNow(motionMesh, Matrix4x4.identity);

                Graphics.Blit(destination, warpedColorFrame);

                SaveRenderTextureColor(warpedColorFrame, string.Format("frame{0}.png", frameNo));
                SaveRenderTextureColorDepth(warpedColorFrame, string.Format("frame{0}.exr", frameNo), true);
                //PushPullInpaint(warpedColorFrame);
                //Graphics.Blit(warpedColorFrame, destination);
            }
            else if (mode == Mode.WARPING_VISUALISE)
            {
	            warpMultiple = frameNo % frameToInterpolate;
                //Warp(colorFrame, motionFrame, warpedColorFrame, warpMultiple);

                // ** Warping colour, writing validity mask **
                //Graphics.SetRenderTarget(destination);
                Graphics.SetRenderTarget(warpedColorFrame);

                materialMotionMeshVisualise.SetFloat("xmul", warpMultiple);
                materialMotionMeshVisualise.SetFloat("ymul", warpMultiple);
                materialMotionMeshVisualise.SetFloat("sideLenThreshold", 0.08f);
                motionFrame.filterMode = FilterMode.Point;
                materialMotionMeshVisualise.SetTexture("_MotionTex", motionFrame);
                colorFrame.filterMode = FilterMode.Point;
                materialMotionMeshVisualise.SetTexture("_MainTex", colorFrame);
                if (! materialMotionMeshVisualise.SetPass(0) ) 
                {
                    Debug.LogError("Failed to set pass");
                }
                GL.Clear(true, true, new Color(1,0,1,0));
                Graphics.DrawMeshNow(motionMeshVis, Matrix4x4.identity);

                //Graphics.Blit(destination, warpedColorFrame);
                Graphics.Blit(warpedColorFrame, destination);

                SaveRenderTextureColor(warpedColorFrame, string.Format("vis_frame{0}.png", frameNo));
                //SaveRenderTextureDepth(warpedDepthFrame, string.Format("a{0}.exr", frameNo % frameToInterpolate));
                //PushPullInpaint(warpedColorFrame);
                //Graphics.Blit(warpedColorFrame, destination);
            }
            else if (mode == Mode.WARPING_VISUALISE_PERSPECTIVE)
            {
	            warpMultiple = frameNo % frameToInterpolate;
                //Warp(colorFrame, motionFrame, warpedColorFrame, warpMultiple);

                // ** Warping colour, writing validity mask **
                Graphics.SetRenderTarget(destination);

                materialMotionMeshVisualisePerspective.SetFloat("xmul", warpMultiple);
                materialMotionMeshVisualisePerspective.SetFloat("ymul", warpMultiple);
                materialMotionMeshVisualisePerspective.SetFloat("sideLenThreshold", 0.08f);
                motionFrame.filterMode = FilterMode.Point;
                materialMotionMeshVisualisePerspective.SetTexture("_MotionTex", motionFrame);
                colorFrame.filterMode = FilterMode.Point;
                materialMotionMeshVisualisePerspective.SetTexture("_MainTex", colorFrame);
                Quaternion rotation = new Quaternion();
                rotation.eulerAngles = new Vector3(0, 20f, 0);
                materialMotionMeshVisualisePerspective.SetMatrix("perspective", Matrix4x4.Perspective(45, Screen.width / Screen.height, 0, 10) * Matrix4x4.Translate(new Vector3(0,0,-3)) * Matrix4x4.Rotate(rotation));
                if (! materialMotionMeshVisualisePerspective.SetPass(0) ) 
                {
                    Debug.LogError("Failed to set pass");
                }
                GL.Clear(true, true, new Color(1,0,1,0));
                Graphics.DrawMeshNow(motionMeshVis, Matrix4x4.identity);

                Graphics.Blit(destination, warpedColorFrame);

                if (saveFrames)
                {
                    SaveRenderTextureColor(warpedColorFrame, string.Format("frame{0}.png", frameNo));
                    SaveRenderTextureColorDepth(warpedColorFrame, string.Format("frame{0}.exr", frameNo), true);
                }
                //PushPullInpaint(warpedColorFrame);
                //Graphics.Blit(warpedColorFrame, destination);
            }

            Graphics.Blit(source, currColorFrame, materialPass);
            if(saveFrames)
            {
                SaveRenderTextureColorDepth(currColorFrame, string.Format("ref_frame{0}.exr", frameNo), false);
            }

            // If at the end of the interpolation period, re-enable rendering ready for the next frame.
            // Motion doesn't seem to render properly unless you render the frame before (:() 
            // Need to work out why and try to avoid this.
            if (frameNo % frameToInterpolate == (frameToInterpolate-2)) 
            {
                foreach (var r in foundMeshRenderers)
                {
                    r.enabled = true;
                }
            }
        }

        ++frameNo;
    }

    public static void FlipTextureVertically(Texture2D original)
    {
        var originalPixels = original.GetPixels();

        Color[] newPixels = new Color[originalPixels.Length];

        int width = original.width;
        int rows = original.height;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                newPixels[x + y * width] = originalPixels[x + (rows - y - 1) * width];
            }
        }

        original.SetPixels(newPixels);
        original.Apply();
    }

    void SaveRenderTextureColor(RenderTexture texture, string filename)
    {
        RenderTexture.active = texture;
        Texture2D tex = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
        FlipTextureVertically(tex);
        RenderTexture.active = null;
        byte[] bytes;
        bytes = tex.EncodeToPNG();

        System.IO.File.WriteAllBytes(filename, bytes);
    }
    void SaveRenderTextureColorDepth(RenderTexture texture, string filename, bool flip)
    {
        RenderTexture.active = texture;
        Texture2D tex = new Texture2D(texture.width, texture.height, TextureFormat.RGBAHalf, false);
        tex.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
        if (flip)
        {
            FlipTextureVertically(tex);
        }
        RenderTexture.active = null;
        byte[] bytes;
        bytes = tex.EncodeToEXR();

        System.IO.File.WriteAllBytes(filename, bytes);
    }

    void SaveRenderTextureDepth(RenderTexture texture, string filename)
    {
        RenderTexture.active = texture;
        Texture2D tex = new Texture2D(texture.width, texture.height, TextureFormat.RGBAFloat, false);
        tex.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
        FlipTextureVertically(tex);
        RenderTexture.active = null;
        byte[] bytes;
        bytes = tex.EncodeToEXR();

        System.IO.File.WriteAllBytes(filename, bytes);
    }

    void PushPullInpaint(RenderTexture input)
    {
        RenderTexture tmp = RenderTexture.GetTemporary(Screen.width, Screen.height, 16);
        tmp.enableRandomWrite = true;
        tmp.useMipMap = true;
        tmp.autoGenerateMips = false;

        int nMips = input.mipmapCount;
        int pushIdx = pushPullShader.FindKernel("push");
        int pullIdx = pushPullShader.FindKernel("pull");
        int width = Screen.width;
        int height = Screen.height;
        for(int i = 0; i < nMips-1; ++i) 
    	{
            width = width / 2;
            height = height / 2;
            pushPullShader.SetTexture(pushIdx, "input", input, i);
            pushPullShader.SetTexture(pushIdx, "output", input, i+1);
            pushPullShader.Dispatch(pushIdx, width, height, 1);
            UnityEngine.Rendering.GraphicsFence fence = Graphics.CreateAsyncGraphicsFence();
            Graphics.WaitOnAsyncGraphicsFence(fence);
	    }
        for(int i = nMips-2; i > 0; --i) 
    	{
            width = width * 2;
            height = height * 2;
            pushPullShader.SetTexture(pullIdx, "input", input, i);
            pushPullShader.SetTexture(pullIdx, "output", input, i-1);
            pushPullShader.Dispatch(pullIdx, width, height, 1);
            UnityEngine.Rendering.GraphicsFence fence = Graphics.CreateAsyncGraphicsFence();
            Graphics.WaitOnAsyncGraphicsFence(fence);
	    }

        tmp.Release();
    }
}
