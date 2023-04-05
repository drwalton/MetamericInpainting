# MetamericInpainting
Code implementing the method from "Metameric Inpainting for Image Warping", TVCG 2022 [\[Paper\]](https://ieeexplore.ieee.org/document/9928218) | [\[Video\]](https://vimeo.com/772790447)

This contains the code implementing the algorithm, and some example scenes showing applications for our approach. The approach is implemented in Unity, and was tested in version 2020.3.19f1. For the smoothest experience we'd recommend installing this version of Unity, which is available [in the download archive](https://unity3d.com/get-unity/download/archive). It's probably compatible with other versions, but you may need to re-import. 
To note, this project should use the "Gamma" option for color correction - if this is changed to "Linear" after re-importing you may get weird inpainted results. For best results please also ensure AA is disabled.
Both these settings can be changed in "Project Settings". The Gamma option should be under Player -> Other Settings -> Rendering -> Color Space. The AA option is under Quality -> Anti Aliasing.

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

## Prefabs

We include a number of `InpaintingCameras` prefabs which can be used to perform the tasks in the example scenes above.

These all include an `AnalysisCamera` that applies warping, constructs the steerable pyramid and inpaints, and a `SynthesisCamera` that generates the final output based on the inpainted colours and stats maps.

The `Motion`, `Stereo` and `Transform` prefabs also include a `SceneCamera` responsible for rendering the initial input colour and depth images of the 3D scene. When setting this up in a new 3D scene, note that setting correct clipping planes is important to get the best results - try to move the near plane as far away as possible whilst avoiding clipping, and keep the far plane as close as possible. This will improve depth precision and give better results from the warping and inpainting.

The `Stereo`, `Transform` and `360 Video` prefabs include a `WarpTransform` object that sets the transformation used when warping. In the case of `Stereo` this is the offset between the two eye views. For `Transform` it is the warping transform applied to the input image. Finally, for `360 Video` it is the viewpoint the 360 video is rendered from.

The `SynthesisCamera` produces the final output image, so generally its `Depth` value should be set to be higher than the other cameras in the scene.

### Viewer

The `AnalysisCamera` includes a `Viewer` script that lets you see intermediate steps of the approach. To use it, set the `Depth` value for the `AnalysisCamera` to be higher than the other cameras in the scene. You can then use the sliders to control which step of the approach is displayed, change the display mode etc.

The `Show Step` slider selects which stage of the approach to display (these correspond to the steps in `CameraCommandBuffers`)

When showing the steerable pyramid bands we recommend changing the display mode to 1 or 2. The `Show Texture` slider can be used to select which orientation is displayed.

### Getting More Scenes

To limit the size of this repository, only one of the 3D scenes from the paper has been included. However the others (and many more scenes) are available on Sketchfab. For a complete list of the scenes used in the paper please see the Credits list on our [paper webpage](https://drwalton.github.io/metameric_inpainting_page/).

The sketchfab plugin is available [here](https://github.com/sketchfab/unity-plugin/releases). This is a standard `unitypackage` file that can be imported via Assets->Import Package->Custom Package in unity. To import the models I recommend downloading from Sketchfab in `.gltf` format and using SketchFab->Import GLTF in Unity to import them.

To download from SketchFab you will need to create an account. Please note that models on SketchFab have a variety of Creative Commons licenses and ensure that the license is appropriate for your use case. You can filter by license type in the search engine.

When importing make sure to change the prefab name to match the scene imported to avoid confusion later. Also I would recommend changing the "Import Into" location to a separate directory for each imported scene. By default all assets are imported into `Assets/Import` and if two different scenes have a texture with the same filename they *will* overwrite one another. Keeping them separate also makes deleting scenes later much easier.

Just to note, there is a [newer plugin developed by Zoe](https://sketchfab.com/blogs/community/new-sketchfab-plugin-for-unity-allows-model-download-at-runtime/) but I have not personally tested it yet.

### Attribution

The 3D Garden scene used in the examples included here was created by [Shahriar Shahrabi @ Sketchfab](https://sketchfab.com/shahriyarshahrabi).

[Link to original scene](https://sketchfab.com/3d-models/an-afternoon-in-a-persian-garden-b13afbaf1aae4f6aad03aaa081ce471e).
