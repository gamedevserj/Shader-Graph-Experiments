# Shader-Graph-Experiments

Created with Unity 2019.3.0f6  
Shader Graph v7.1.8

The project contains various Shader Graph effects

Some shaders use \_CameraOpaqueTexture. Sprite objects that use these shaders won't "interact" with shaders that have sprite lit/unlit master node. To avoid that you could either change master node to PBR and set rendering to opaque (example of that is a BackgroundMaterial in the project). Or create a second camera that renders to a texture and use that as a property instead of \_CameraOpaqueTexture.

## Texture dissolve 2D
<img src="https://github.com/gamedevserj/Shader-Graph-Experiments/blob/master/Images/Dissolve2D.png" height="256">

Dissolves texture using alpha clip

## Sprite outline
<img src="https://github.com/gamedevserj/Shader-Graph-Experiments/blob/master/Images/Outline2D.png" height="256">

Offsets sprite image in 4 directions to create outline

## Stealth cloak effect
### 2D and 3D
#### Uses \_CameraOpaqueTexture
<img src="https://github.com/gamedevserj/Shader-Graph-Experiments/blob/master/Images/StealthCloak2D-WithOutline.png" height="256"> <img src="https://github.com/gamedevserj/Shader-Graph-Experiments/blob/master/Images/StealthCloak3D.png" height="256">

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
<img src="https://github.com/gamedevserj/Shader-Graph-Experiments/blob/master/Images/Water2D.png" height="256">

Based on [this shader](https://www.reddit.com/r/Unity2D/comments/fcxjbu/i_always_wanted_to_create_water_reflection_shader/).

The difference is that this shader allows for vertical movement for the camera. The shader comes with the script example where materials adjust their properties to show the effect correctly. You may move the code from Update method to Start, if your water objects don't change their Y position and if Camera's orthographic size stays the same.

Here's how offset is calculated:

yOffset = cameraPositionY * (-1/cameraOrthographicSize) + objectPositionY * (1/cameraOrthographicSize)

## Grass sway
<img src="https://github.com/gamedevserj/Shader-Graph-Experiments/blob/master/Images/GrassSway2D.png" height="256">

A simple grass swaying shaders. The one that uses gradient causes some image distortion that can become very noticeable when amplitude it too high.

## Mirror effect
#### Uses \_CameraOpaqueTexture
<img src="https://github.com/gamedevserj/Shader-Graph-Experiments/blob/master/Images/Mirror2D-pivot-left.png" height="256"> <img src="https://github.com/gamedevserj/Shader-Graph-Experiments/blob/master/Images/Mirror2D-pivot-center.png" height="256"> <img src="https://github.com/gamedevserj/Shader-Graph-Experiments/blob/master/Images/Mirror2D-pivot-right.png" height="256">

Mirror reflection effect. Reflection is based on the object's pivot point. in the examples above pivot points are as follows - left, center, right.
Just like the water shader example scene uses script that adjusts material properties to reflect objects properly. If you have only one mirror in the scene you can replace _ObjectPositionX property with Object node and take X position from it.

Here's how offset is calculated:

xOffset = screenHeight/screenWidth/cameraOrthographicSize * cameraPositionX * (-1) + screenHeight/screenWidth/cameraOrthographicSize * objectPositionX


## Resizeable shape
<img src="https://github.com/gamedevserj/Shader-Graph-Experiments/blob/master/Images/ResizeableShape.png" height="256">
Shader that allows creation of a shape that preserves its width when changing size.

## Magnifying glass effect
#### Uses \_CameraOpaqueTexture
<img src="https://github.com/gamedevserj/Shader-Graph-Experiments/blob/master/Images/MagnifyingGlass.png" height="256">

The effect is implemented by modifying the tiling and offset values of the object. The script attached to the magnifying glass object adjusts values taking into account object position, camera position, camera orthographic size, and screen aspect ratio. 

Here's how offset is calculated:

xOffset = magnification * 0.5f + (halfZoom / cameraOrthographicSize) / screenAspect * objectPositionX - (halfZoom / cameraOrthographicSize) / screenAspect * cameraPositionX

yOffset = magnification * 0.5f + (halfZoom / cameraOrthographicSize) * objectPositionY - (halfZoom / cameraOrthographicSize) * cameraPositionY

The tiling is calculated by subtracting magnification amount from 1:

xTiling = yTiling = 1 - magnification;
