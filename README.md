# Shader-Graph-Experiments

Created with Unity 2019.2.2f1
Shader Graph v6.9.1

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

The difference is that this shader allows for some degree of vertical movement for the camera. Changing camera's orthographic size affects the shader and requires adjusting the multiplier property. 

One major issue with this shader is that water can't be above/below some level from the 0 on Y-axis, and it can't take up more than half of the screen.

