using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TestDrawMeshNow : MonoBehaviour
{
    private Shader m_ShaderPass;
    public Shader shaderPass
    {
        get
        {
            if (m_ShaderPass == null)
                m_ShaderPass = Shader.Find("Hidden/PassthroughShader");


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

    public Mesh mesh;
    public Material material;
    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        Vector3[] vertices = new Vector3[3];
        int[] indices = new int[] {0,1,2,0,2,1};
        vertices[0] = new Vector3(-0.5f, -0.5f, 0.0f);
        vertices[1] = new Vector3(0.5f, -0.5f, 0.0f);
        vertices[2] = new Vector3(0.0f, 0.5f, 0.0f);
        mesh.vertices = vertices;
        mesh.triangles = indices;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /*
    private void OnPostRender()
    {
        material.SetPass(0);
        
        Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
    }
    */
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, materialPass);

        Graphics.SetRenderTarget(destination);
        material.SetPass(0);
        
        Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
    }
}
