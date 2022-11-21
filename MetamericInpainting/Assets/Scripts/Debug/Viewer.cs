using UnityEngine;
using System.IO;

[RequireComponent(typeof (CameraCommandBuffers))]
public class Viewer: MonoBehaviour
{
    [Range(0, 16)]
    public float lod = 0;

    [Range(0,10)]
    public int displayMode = 0;

    [Range(0, 5)]
    public int showStep = 0;
    private Shader m_Shader;

    [Range(0, 8)]
    public int showTexture = 0;

    public bool pointSample = false;

    #region shaders
    public Shader shader
    {
        get
        {
            if (m_Shader == null)
                m_Shader = Shader.Find("Hidden/Viewer");

            return m_Shader;
        }
    }

    private Material m_Material;
    public Material material
    {
        get
        {
            if (m_Material == null)
            {
                if (shader == null || shader.isSupported == false)
                    return null;

                m_Material = new Material(shader);
            }

            return m_Material;
        }
    }

    private Shader m_passShader;
    public Shader passShader
    {
        get
        {
            if (m_passShader == null)
                m_passShader = Shader.Find("Hidden/PassTransform");

            return m_passShader;
        }
    }

    private Material m_passMaterial;
    public Material passMaterial
    {
        get
        {
            if (m_passMaterial == null)
            {
                if (passShader == null || passShader.isSupported == false)
                    return null;

                m_passMaterial = new Material(passShader);
            }

            return m_passMaterial;
        }
    }

    #endregion

    private RenderTexture m_Pyramid
    {
        get
        {
            var ccb = GetComponent<CameraCommandBuffers>();

            if (ccb == null)
                return null;

            return ccb.GetTexture(showStep, showTexture);
        }
    }

    public void Update()
    {
        if (Input.GetKeyUp(KeyCode.Comma))
        {
            ScreenCapture.CaptureScreenshot("Print.png");            
        }
    }

    private int pyramidDepth; 
    private bool print = false;
    public void OnEnable()
    {
        if (saveAllBands)
        {
            displayMode = 0;
            if (squaredForSave)
                displayMode = 6;
            lod = 0;
            showStep = 0;
            showTexture = 0;
            var ccb = GetComponent<CameraCommandBuffers>();
            pyramidDepth = ccb.pyramidDepth;
            print = true;
        }
    }

    public bool transformRect;
    public bool saveAllBands;
    public bool squaredForSave;

    public WarpFrameSource frameSource;
    
    public void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (m_Pyramid == null)
        {
            Graphics.Blit(source, destination);
            return;
        }
        material.SetInt("_DisplayMode", displayMode);
        material.SetFloat("_LOD", (float)lod);
        material.SetInt("_pointSample", pointSample ? 1 : 0);
        if(frameSource != null) { 
            material.SetTexture("_motionTexture", frameSource.motion);
            material.SetTexture("_inputColorTexture", frameSource.color);
        }

        if (transformRect) {
            RenderTexture temp = RenderTexture.GetTemporary(m_Pyramid.width, m_Pyramid.height, 0, m_Pyramid.format, RenderTextureReadWrite.Linear);
            Graphics.Blit(m_Pyramid, temp, material);

            passMaterial.SetFloat("_screenWidth",GetComponent<Camera>().pixelWidth);
            passMaterial.SetFloat("_screenHeight", GetComponent<Camera>().pixelHeight);
            passMaterial.SetFloat("_texSize", m_Pyramid.width);
            passMaterial.SetInt("_direction",-1);

            Graphics.Blit(temp, destination, passMaterial);
            RenderTexture.ReleaseTemporary(temp);
        }
        else
        {
            Graphics.Blit(m_Pyramid, destination, material);
        }

        if (print)
        {
            print = false;
            Camera c = this.GetComponent<Camera>();
            int width = c.pixelWidth;
            int height = c.pixelHeight;
            width = width >> ((int)lod);
            height = height >> ((int)lod);

            RenderTexture r = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGBFloat);
            Graphics.Blit(m_Pyramid, r, material);
            RenderTexture.active = r;
            Texture2D resulTex = new Texture2D(width, height, TextureFormat.RGBAFloat, false);
            resulTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            resulTex.Apply();
            RenderTexture.ReleaseTemporary(r);
            byte[] bytes = resulTex.EncodeToEXR(Texture2D.EXRFlags.None);
            print("Saving at " + Application.dataPath + "/Maps/" + ((int)lod) + "_" + ((int)showTexture) + ".exr");
            File.WriteAllBytes(Application.dataPath + "/Maps/" + ((int)lod) + "_" + ((int)showTexture) + ".exr", bytes);
        }

        if (saveAllBands)
        {
            print = true;
            if (lod == 0)
            {
                displayMode = 0;
                showTexture++;
                if (showTexture == 3)
                {
                    showTexture = 1;
                    lod++;
                }
            }
            else
            {
                showTexture++;
                if (showTexture == 3)
                {
                    showTexture = 1;
                    lod++;
                    if (lod == pyramidDepth)
                        saveAllBands = false;
                }
            }
        }
    }
}
