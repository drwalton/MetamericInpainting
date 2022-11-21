using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

public class EquiDepthVideoLoader : WarpFrameSource
{
    //public string colorVideoPath, depthVideoPath;
    public VideoPlayer colorVideoPlayer, depthVideoPlayer;

    override public Matrix4x4 ProjectionMatrix() { return this.GetComponent<Camera>().projectionMatrix; }

    public override int RenderWidth()
    {
        return Screen.width;
    }
    public override int RenderHeight()
    {
        return Screen.height;
    }

    // Start is called before the first frame update
    void Start()
    {
        /*
        if(!File.Exists(colorVideoPath) || !File.Exists(depthVideoPath))
        {
            //throw new FileNotFoundException("Bad video path");
            Application.Quit();
        }

        colorVideoPlayer = this.gameObject.AddComponent<UnityEngine.Video.VideoPlayer>();
        colorVideoPlayer.source = VideoSource.Url;
        colorVideoPlayer.url = colorVideoPath;
        depthVideoPlayer = this.gameObject.AddComponent<UnityEngine.Video.VideoPlayer>();
        depthVideoPlayer.source = VideoSource.Url;
        depthVideoPlayer.url = depthVideoPath;

        colorVideoPlayer.renderMode = VideoRenderMode.RenderTexture;
        depthVideoPlayer.renderMode = VideoRenderMode.RenderTexture;

        colorVideoPlayer.isLooping = true;
        depthVideoPlayer.isLooping = true;

        colorVideoPlayer.Play();
        depthVideoPlayer.Play();
        */
        CreateTextures();
        colorVideoPlayer.targetTexture = color;
        depthVideoPlayer.targetTexture = depth;
    }

    void CreateTextures()
    { 
        color = new RenderTexture((int)colorVideoPlayer.width, (int)colorVideoPlayer.height, 32, RenderTextureFormat.ARGBFloat);
        depth = new RenderTexture((int)depthVideoPlayer.width, (int)depthVideoPlayer.height, 32, RenderTextureFormat.RFloat);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
