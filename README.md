# MetamericInpainting
Code implementing the method from "Metameric Inpainting for Image Warping", TVCG 2022 [\[Paper\]](https://drwalton.github.io/papers/Metameric_Inpainting_for_Image_Warping.pdf) | [\[Video\]](https://vimeo.com/772790447)

This contains the code implementing the algorithm, and some example scenes showing applications for our approach. The approach is implemented in Unity, and was tested in version 2020.3.19f1. For the smoothest experience we'd recommend installing this version of Unity, which is available [in the download archive](https://unity3d.com/get-unity/download/archive). It's probably compatible with other versions, but you may need to re-import. To note, this project should use the "Gamma" option for color correction - if this is changed to "Linear" after re-importing you may get weird inpainted results.

## Example Scenes

These are contained in `MetamericInpainting/Assets/Scenes`.

### `ExampleScene_Transform`

This example renders an image from one perspective, applies a warp based on a 6DoF transform and then inpaints any holes in the resulting image using our approach. You can manipulate the transform by moving the `WarpTransform` `GameObject` inside the `InpaintingCameras` prefab.

### `ExampleScene_Motion`

This example renders only one in every N frames, generating the rest by inpainting. You can control the number of frames to warp by changing the `Warp Frames` inside the `SceneCamera` `GameObject`.

### `ExampleScene_Stereo`

This example operates in a similar way to ExampleScene_Transform, but shows both the rendered and warped images as a stereo pair. I'd recommend changing your resolution in the `Game` window to something wide like `2048x1024` for best results. You can change the stereo camera separation using the `WarpTransform` `GameObject`.

### `ExampleScene_360`

This scene shows a 360 RGBD video and allows the perspective to be changed, inpainting any holes using our approach.

For this demo you will need to download suitable RGBD equirectangular 360 videos. The 360 RGBD videos used in our video example were originally included with the demo of Motion Parallax for 360 RGBD Video (Serrano et al. TVCG 2019)

Their demo is available on Github here: https://github.com/ana-serrano/VR-6dof, and a series of suitable videos are included here: https://github.com/ana-serrano/VR-6dof/tree/master/6dof_demo_exe/vids

For this demo only the colour and depth videos are required (e.g. `cafeteria.mp4` and `cafeteria_depth.mp4`). These files can be added to the project (e.g. in `Assets/Videos` and then linked to the `VideoPlayer`s in the `Color` and `Depth` GameObjects in the `InpaintingCamerasEquiVideoSmooth` prefab.

Once everything is set up, the warp can again be controlled by moving the `WarpTransform` `GameObject`.

### Attribution

The 3D Garden scene used in the examples included here was created by [Shahriar Shahrabi @ Sketchfab](https://sketchfab.com/shahriyarshahrabi).

[Link to original scene](https://sketchfab.com/3d-models/an-afternoon-in-a-persian-garden-b13afbaf1aae4f6aad03aaa081ce471e).