using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * This script is used to compare warping conventionally, discarding triangles that are overly stretched
 * with the modified warping approach that remaps the depths of stretched triangles.
*/

[ExecuteInEditMode]
public class WarpingComparison : MonoBehaviour
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

    private Shader m_ShaderMotionMeshSimple;
    public Shader shaderMotionMeshSimple
    {
        get
        {
            if (m_ShaderMotionMeshSimple == null)
                m_ShaderMotionMeshSimple = Shader.Find("Hidden/WarpShaderMeshSimple");


            return m_ShaderMotionMeshSimple;
        }
    }
    private Material m_MaterialMotionMeshSimple;
    public Material materialMotionMeshSimple
    {
        get
        {
            if (m_MaterialMotionMeshSimple == null)
            {
                if (shaderMotionMeshSimple == null || shaderMotionMeshSimple.isSupported == false)
                    return null;

                m_MaterialMotionMeshSimple = new Material(shaderMotionMeshSimple);
            }

            return m_MaterialMotionMeshSimple;
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

    public ComputeShader warpShader;
    public ComputeShader pushPullShader;

    #endregion

    private int frameNo = 0;
    public RenderTexture currColorFrame, colorFrame, motionFrame, warpedColorFrame, warpedDepthFrame;
    public int frameToInterpolate = 10;
    public enum Mode { WARPING, WARPING_SIMPLE }
    public Mode mode = Mode.WARPING;
    public Mesh motionMesh;
    public int warpMultiple;
    private float sideLenThreshold = 0.01f;


    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.MotionVectors | DepthTextureMode.Depth;
        GetComponent<Camera>().allowMSAA = false;
        colorFrame = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
        warpedColorFrame = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGBFloat);
        currColorFrame = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGBFloat);
        warpedColorFrame.enableRandomWrite = true;
        warpedColorFrame.useMipMap = true;
        warpedColorFrame.autoGenerateMips = false;
        warpedColorFrame.Create();
        currColorFrame.enableRandomWrite = true;
        currColorFrame.useMipMap = true;
        currColorFrame.autoGenerateMips = false;
        currColorFrame.Create();
        motionFrame = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGBFloat);
        motionFrame.Create();
        Application.targetFrameRate = -1;

        motionMesh = GenerateMesh(Screen.width, Screen.height);
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
        // Special case: if frameToInterpolate is <=1, don't do any interpolation & just show frame as usual.
        if(frameToInterpolate <= 1)
        {
            Graphics.Blit(source, destination, materialPass);
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
            Graphics.Blit(colorFrame, destination, materialPass);
        }
        // Case 2: Interpolation
        else
        {
            warpMultiple = frameNo % frameToInterpolate;
            //Warp(colorFrame, motionFrame, warpedColorFrame, warpMultiple);

            // ** Warping colour, writing validity mask **
            Graphics.SetRenderTarget(destination);

            if (mode == Mode.WARPING)
            {
                materialMotionMesh.SetFloat("xmul", warpMultiple);
                materialMotionMesh.SetFloat("ymul", warpMultiple);
                materialMotionMesh.SetFloat("sideLenThreshold", sideLenThreshold);
                motionFrame.filterMode = FilterMode.Point;
                materialMotionMesh.SetTexture("_MotionTex", motionFrame);
                colorFrame.filterMode = FilterMode.Point;
                materialMotionMesh.SetTexture("_MainTex", colorFrame);
                if (!materialMotionMesh.SetPass(0))
                {
                    Debug.LogError("Failed to set pass");
                }
            }
            else if (mode == Mode.WARPING_SIMPLE)
            {
                materialMotionMeshSimple.SetFloat("xmul", warpMultiple);
                materialMotionMeshSimple.SetFloat("ymul", warpMultiple);
                materialMotionMeshSimple.SetFloat("sideLenThreshold", sideLenThreshold);
                motionFrame.filterMode = FilterMode.Point;
                materialMotionMeshSimple.SetTexture("_MotionTex", motionFrame);
                colorFrame.filterMode = FilterMode.Point;
                materialMotionMeshSimple.SetTexture("_MainTex", colorFrame);
                if (!materialMotionMeshSimple.SetPass(0))
                {
                    Debug.LogError("Failed to set pass");
                }
            }
            GL.Clear(true, true, new Color(1,0,1,0));
            Graphics.DrawMeshNow(motionMesh, Matrix4x4.identity);

            Graphics.Blit(source, currColorFrame, materialPass);
        }

        ++frameNo;
    }

}
