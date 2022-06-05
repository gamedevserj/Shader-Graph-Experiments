# Shader-Graph-Experiments

Created with Unity 2019.3.13.f1  
Shader Graph v7.3.1

The project contains various Shader Graph effects

Some shaders use __\_CameraOpaqueTexture__. Sprite objects that use these shaders won't "interact" with shaders that have sprite lit/unlit master node. To avoid that you could either change master node to PBR and set rendering to opaque (example of that is a BackgroundMaterial in the project). Or create a second camera that renders to a texture and use that as a property instead of __\_CameraOpaqueTexture__.

If you're using the 2D renderer pipeline, capturing the screen with __\_CameraOpaqueTexture__ does not work anymore in 2D.
Instead, you need to use the __\_CameraSortingLayerTexture__ tag.

The shaders can then be made to work as follows:

1. Use _CameraSortingLayerTexture tag, remove the "Exposed" checkmark.
2. In the 2D renderer data, set the "Foremost Sorting Layer" to the last layer you want to be renderer with distortion.
3. Set the "Sorting Layer" of the sprite renderer which contains your distortion material to be above the last layer of the "foremost sorting layer".


## Texture dissolve 2D
<img src="https://raw.githubusercontent.com/gamedevserj/Images-For-Repo/main/ShaderGrapExperiments/Dissolve2D.png" height="256">

Dissolves texture using alpha clip

## Sprite outline
<img src="https://raw.githubusercontent.com/gamedevserj/Images-For-Repo/main/ShaderGrapExperiments//Outline2D.png" height="256">

Offsets sprite image in 4 directions to create outline

## Stealth cloak effect
### 2D and 3D
#### Uses \_CameraOpaqueTexture
<img src="https://raw.githubusercontent.com/gamedevserj/Images-For-Repo/main/ShaderGrapExperiments/StealthCloak2D-WithOutline.png" height="256"> <img src="https://raw.githubusercontent.com/gamedevserj/Images-For-Repo/main/ShaderGrapExperiments/StealthCloak3D.png" height="256">

Uses \_CameraOpaqueTexture to imitate invisibility cloak effect.

It has a couple of caveats attached to it. 

When using it with 3D models:
1. Setup your object that should become invisible to cast no shadows in the Renderer settings:
Without it there will be a shadow even if object is "invisible", because technically it's not.

2. Setup a copy of it to only cast shadows:
Shadow needs to have another material that cuts out the model allowing for light to pass through and create holes in the shadow.

3. When turning object invisible change both values in model and shadow materials 

## Water reflection
#### Uses \_CameraOpaqueTexture
<img src="https://raw.githubusercontent.com/gamedevserj/Images-For-Repo/main/ShaderGrapExperiments/Water2D.png" height="256">

Based on [this shader](https://www.reddit.com/r/Unity2D/comments/fcxjbu/i_always_wanted_to_create_water_reflection_shader/).

The difference is that this shader allows for vertical movement for the camera. The shader comes with the script example where materials adjust their properties to show the effect correctly. You may move the code from Update method to Start, if your water objects don't change their Y position and if Camera's orthographic size stays the same.

Here's how offset is calculated:

Offset.Y = CameraPosition.Y * (-1/cameraOrthographicSize) + ObjectPosition.Y * (1/cameraOrthographicSize)

## Water reflection with objects off the screen
<img src="https://raw.githubusercontent.com/gamedevserj/Images-For-Repo/main/ShaderGrapExperiments/Water2D-full-reflection.png" height="256"> <img src="https://raw.githubusercontent.com/gamedevserj/Images-For-Repo/main/ShaderGrapExperiments/Water2D-full-reflection2.png" height="256">

Shader that uses \_CameraOpaqueTexture doesn't allow for the water to take up more than half of the screen starting from the bottom or if its height of the portion on the screen is greater than its distance to the top of the screen. This variation allows for it to render objects that are outside of the screen. The render camera has to have orthographic size twice time greater than the camera that renders the scene. The camera that renders to texture covers twice as much space, so in order for reflections to be crispy as the original the render texture must be 2x the resolutions. This can get quite expensive on higher resolutions, so if you're using this make sure to allow player choose the quality of reflections.

Since second camera covers larger portion of the screen it requires a different calculation of tiling and offset.

Tiling.Y = MainCameraOrthographicSize/SecondaryCameraOrthographicSize

Tiling.X = Tiling.Y * ScreenWidth/ScreenHeight

Offset.Y = (ObjectPosition.Y - MainCameraPosition.Y)/SecondaryCameraOrthographicSize

Offset.X = (1 - Tiling.X)/2

IMPORTANT:
When setting up your camera that renders to RT make it a child of the MainCamera and align it in a way where both MainCamera and Secondary camera have their bottom bound at the same position.

## Grass sway
<img src="https://raw.githubusercontent.com/gamedevserj/Images-For-Repo/main/ShaderGrapExperiments/GrassSway2D.png" height="256">

A simple grass swaying shaders. The one that uses gradient causes some image distortion that can become very noticeable when amplitude it too high.

## Mirror effect
#### Uses \_CameraOpaqueTexture
<img src="https://raw.githubusercontent.com/gamedevserj/Images-For-Repo/main/ShaderGrapExperiments/Mirror2D-pivot-left.png" height="256"> <img src="https://raw.githubusercontent.com/gamedevserj/Images-For-Repo/main/ShaderGrapExperiments/Mirror2D-pivot-center.png" height="256"> <img src="https://raw.githubusercontent.com/gamedevserj/Images-For-Repo/main/ShaderGrapExperiments/Mirror2D-pivot-right.png" height="256">

Mirror reflection effect. Reflection is based on the object's pivot point. in the examples above pivot points are as follows - left, center, right.
Just like the water shader example scene uses script that adjusts material properties to reflect objects properly. If you have only one mirror in the scene you can replace _ObjectPositionX property with Object node and take X position from it.

Here's how offset is calculated:

Offset.X = ScreenHeight/ScreenWidth/CameraOrthographicSize * CameraPosition.X * (-1) + ScreenHeight/ScreenWidth/CameraOrthographicSize * ObjectPosition.X


## Resizeable shape
<img src="https://raw.githubusercontent.com/gamedevserj/Images-For-Repo/main/ShaderGrapExperiments/ResizeableShape.png" height="256">
Shader that allows creation of a shape that preserves its width when changing size.

## Magnifying glass effect
#### Uses \_CameraOpaqueTexture
<img src="https://raw.githubusercontent.com/gamedevserj/Images-For-Repo/main/ShaderGrapExperiments/MagnifyingGlass.png" height="256">

The effect is implemented by modifying the tiling and offset values of the object. The script attached to the magnifying glass object adjusts values taking into account object position, camera position, camera orthographic size, and screen aspect ratio. 

Here's how offset is calculated:

Offset.X = magnification * 0.5f + (halfZoom / cameraOrthographicSize) / screenAspect * ObjectPosition.X - (halfZoom / cameraOrthographicSize) / screenAspect * CameraPosition.X

Offset.Y = magnification * 0.5f + (halfZoom / cameraOrthographicSize) * ObjectPosition.Y - (halfZoom / cameraOrthographicSize) * CameraPosition.Y

The tiling is calculated by subtracting magnification amount from 1:

Tiling.X = Tiling.Y = 1 - magnification;

Distortion strength around edges can be changed.

## Impact effect
### 2D and 3D
#### Uses \_CameraOpaqueTexture
<img src="https://raw.githubusercontent.com/gamedevserj/Images-For-Repo/main/ShaderGrapExperiments/ImpactEffect2D.png" height="256">

Effect simulates shock wave that can be seen during explosions. The effect is basically radial distortion from the center of the object. Changing the alpha can be used to create a double vision that looks similar to the flashbang effect.

## Circular fade out
<img src="https://raw.githubusercontent.com/gamedevserj/Images-For-Repo/main/Site/GodotFadeOutShaderTutorial/fade_out_final.gif" height="256">

Fades out to any point of the screen.
