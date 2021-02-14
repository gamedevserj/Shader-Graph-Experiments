# 2D Lights Properties

## Creating a Light

![image alt text](images/image_7.png)

Create a __2D Light__ GameObject by going to __GameObject > Light > 2D__ and selecting one of the five available types:

- __Freeform__: You can edit the shape of this Light type with a spline editor.
- __Sprite__: You can select a Sprite to create this Light type.
- __Parametric__: You can use a n-sided polygon to create this his type of 2D Light.
- __Point__: You can control the inner and outer radius, direction and angle of this Light type.
- __Global__: This 2D Light affects all rendered Sprites on all targeted sorting layers.

The following are the common properties used by the different Light types. 

![](images/image_8.png)

| Property                                                     | Function                                                     |
| ------------------------------------------------------------ | ------------------------------------------------------------ |
| Light Type                                                   | Select the type of Light you want the selected Light to be. The available types are __Freeform__, __Sprite__, __Parametric__, __Point__, and __Global__. |
| [Alpha Blend On Overlap](#alpha-blend-on-overlap)            | Select this option to blend the selected Light with Lights below it based on their alpha values. |
| [Light Order](#light-order) *unavailable for __Global__ Lights | Enter a value here to specify the rendering order of this Light relative to other Lights on the same sorting layer(s). Lights with lower values are rendered first, and negative values are valid. |
| [Blend Style](LightBlendStyles.md)                           | Select the blend style used by this Light. Different blend styles can be customized in the [2D Renderer Asset](2DRendererConfig). |
| Color                                                        | Use the color picker to set the color of the emitted light.  |
| [Intensity](#intensity)                                      | Enter the desired brightness value of the Light. The default value is 1. |
| [Use Normal Map](#use-normal-map)                            | Select this to enable the Light to interact with a Sprite's [normal map Textures](SecondaryTextures). |
| [Distance](#distance) * available when __Use Normal Map__ is checked | Enter the desired distance (in Unity units) between the Light and the lit Sprite. This does not Transform the position of the Light in the Scene. |
| [Quality](#quality)                                          | Select either __Accurate__ or __Fast__ to adjust the accuracy of the lighting calculations used. |
| Volume Opacity                                               | Use the slider to select the opacity of the volumetric lighting. The value scales from 0 (transparent) to 1 (opaque). |
| Target Sorting Layers                                        | Select the sorting layers that this Light targets and affects. |



## Alpha Blend on Overlap 

This property controls the way in the selected Light interacts with other rendered Lights. You can toggle between the two modes by enabling or disabling this property. The effects of both modes are shown in the examples below:

| ![Alpha Blend on Overlap disabled (defaults to Additive blending) ](images\image_9.png) | ![Alpha Blend on Overlap enabled](images\image_10.png) |
| ------------------------------------------------------------ | ------------------------------------------------------ |
| __Alpha Blend on Overlap__ disabled (defaults to Additive blending) | __Alpha Blend on Overlap__ enabled                     |

When __Alpha Blend on Overlap__ disabled, the Light is blended with other Lights additively, where the pixel values of intersecting Lights are added together. This is the default Light blending behavior.

When __Alpha Blend on Overlap__ is enabled, Lights are blended together based on their alpha values. This can be used to completely overwrite one Light with another where they intersect, but the render order of the Lights is also dependent on the [Light Order](#light-order) of the different Lights.

## Light Order

The __Light Order__ value determines the position of the Light in the Render queue relative to other Lights that target the same sorting layer(s). Lower numbered Lights are rendered first, with higher numbered Lights rendered above those below. This especially affects the appearance of blended Lights when __Alpha Blend on Overlap__ is enabled. 

## Intensity

Light intensity are available to all types of Lights. Color adjusts the lights color, while intensity allows this color to go above 1. This allows lights which use multiply to brighten a sprite beyond its original color.

## Use Normal Map

All lights except for global lights can be toggled to use the normal maps in the sprites material. When enabled, Distance and Accuracy will be visible as new properties.

| ![Use Normal Map: Disabled](images\image_11.png) | ![Use Normal Map: Disabled](images\image_12.png) |
| ------------------------------------------------ | ------------------------------------------------ |
| __Use Normal Map: __Disabled                     | __Use Normal Map:__ Enabled                      |

## Distance

Distance controls the distance between the light and the surface of the Sprite, changing the resulting lighting effect. This distance does not affect intensity, or transform the position of the Light in the Scene. The following examples show the effects of changing the Distance values.

| ![Distance: 0.5](images\image_13.png) | ![Distance: 2](images\image_14.png) | ![Distance: 8](images\image_15.png) |
| ------------------------------------- | ----------------------------------- | ----------------------------------- |
| __Distance__: 0.5                     | __Distance__: 2                     | __Distance__: 8                     |

## Quality

Light quality allows the developer to choose between performance and accuracy. When choosing performance, artefacts may occur.  Smaller lights and larger distance values will reduce the difference between fast and accurate.

## Volume Opacity

Volumetric lighting is available to all Light types. Use the __Volume Opacity__ slider to control the visibility of the volumetric light. At a value of zero, no Light volume is shown while at a value of one, the Light volume appears at full opacity.

## Target Sorting Layers

Lights only light up the Sprites on their targeted sorting layers. Select the desired sorting layers from the drop-down menu for the selected Light. To add or remove sorting layers, refer to the [Tag Manager - Sorting Layers](https://docs.unity3d.com/Manual/class-TagManager.html#SortingLayers) for more information.