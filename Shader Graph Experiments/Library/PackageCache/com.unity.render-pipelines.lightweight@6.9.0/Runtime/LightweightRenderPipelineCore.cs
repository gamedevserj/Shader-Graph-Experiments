using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.Rendering.PostProcessing;

namespace UnityEngine.Rendering.LWRP
{
    public enum MixedLightingSetup
    {
        None,
        ShadowMask,
        Subtractive,
    };

    public struct RenderingData
    {
        public CullingResults cullResults;
        public CameraData cameraData;
        public LightData lightData;
        public ShadowData shadowData;
        public bool supportsDynamicBatching;
        public PerObjectData perObjectData;
        public bool killAlphaInFinalBlit;
    }

    public struct LightData
    {
        public int mainLightIndex;
        public int additionalLightsCount;
        public int maxPerObjectAdditionalLightsCount;
        public NativeArray<VisibleLight> visibleLights;
        public bool shadeAdditionalLightsPerVertex;
        public bool supportsMixedLighting;
    }

    public struct CameraData
    {
        public Camera camera;
        public RenderTextureDescriptor cameraTargetDescriptor;
        public float renderScale;
        public bool isSceneViewCamera;
        public bool isDefaultViewport;
        public bool isHdrEnabled;
        public bool requiresDepthTexture;
        public bool requiresOpaqueTexture;

        public SortingCriteria defaultOpaqueSortFlags;

        public bool isStereoEnabled;

        public float maxShadowDistance;
        public bool postProcessEnabled;
        public PostProcessLayer postProcessLayer;
        public IEnumerator<Action<RenderTargetIdentifier, CommandBuffer> > captureActions;
    }

    public struct ShadowData
    {
        public bool supportsMainLightShadows;
        public bool requiresScreenSpaceShadowResolve;
        public int mainLightShadowmapWidth;
        public int mainLightShadowmapHeight;
        public int mainLightShadowCascadesCount;
        public Vector3 mainLightShadowCascadesSplit;
        public bool supportsAdditionalLightShadows;
        public int additionalLightsShadowmapWidth;
        public int additionalLightsShadowmapHeight;
        public bool supportsSoftShadows;
        public int shadowmapDepthBufferBits;
        public List<Vector4> bias;
    }

    public static class ShaderKeywordStrings
    {
        public static readonly string MainLightShadows = "_MAIN_LIGHT_SHADOWS";
        public static readonly string MainLightShadowCascades = "_MAIN_LIGHT_SHADOWS_CASCADE";
        public static readonly string AdditionalLightsVertex = "_ADDITIONAL_LIGHTS_VERTEX";
        public static readonly string AdditionalLightsPixel = "_ADDITIONAL_LIGHTS";
        public static readonly string AdditionalLightShadows = "_ADDITIONAL_LIGHT_SHADOWS";
        public static readonly string SoftShadows = "_SHADOWS_SOFT";
        public static readonly string MixedLightingSubtractive = "_MIXED_LIGHTING_SUBTRACTIVE";

        public static readonly string DepthNoMsaa = "_DEPTH_NO_MSAA";
        public static readonly string DepthMsaa2 = "_DEPTH_MSAA_2";
        public static readonly string DepthMsaa4 = "_DEPTH_MSAA_4";

        public static readonly string LinearToSRGBConversion = "_LINEAR_TO_SRGB_CONVERSION";
        public static readonly string KillAlpha = "_KILL_ALPHA";
    }

    public sealed partial class LightweightRenderPipeline
    {
        static List<Vector4> m_ShadowBiasData = new List<Vector4>();

        public static bool IsStereoEnabled(Camera camera)
        {
            if (camera == null)
                throw new ArgumentNullException("camera");

            bool isGameCamera = camera.cameraType == CameraType.Game || camera.cameraType == CameraType.VR;
            return XRGraphics.enabled && isGameCamera && (camera.stereoTargetEye == StereoTargetEyeMask.Both);
        }

        void SortCameras(Camera[] cameras)
        {
            Array.Sort(cameras, (lhs, rhs) => (int)(lhs.depth - rhs.depth));
        }

        static RenderTextureDescriptor CreateRenderTextureDescriptor(Camera camera, float renderScale,
            bool isStereoEnabled, bool isHdrEnabled, int msaaSamples)
        {
            RenderTextureDescriptor desc;
            RenderTextureFormat renderTextureFormatDefault = RenderTextureFormat.Default;

            if (isStereoEnabled)
            {
                desc = XRGraphics.eyeTextureDesc;
                renderTextureFormatDefault = desc.colorFormat;
            }
            else
            {
                desc = new RenderTextureDescriptor(camera.pixelWidth, camera.pixelHeight);
                desc.width = (int)((float)desc.width * renderScale);
                desc.height = (int)((float)desc.height * renderScale);
            }

            // TODO: when preserve framebuffer alpha is enabled we can't use RGB111110Float format.
            bool useRGB111110 = Application.isMobilePlatform && RenderingUtils.SupportsRenderTextureFormat(RenderTextureFormat.RGB111110Float);
            RenderTextureFormat hdrFormat = (useRGB111110) ? RenderTextureFormat.RGB111110Float : RenderTextureFormat.DefaultHDR;
            desc.colorFormat = isHdrEnabled ? hdrFormat : renderTextureFormatDefault;
            desc.depthBufferBits = 32;
            desc.enableRandomWrite = false;
            desc.sRGB = (QualitySettings.activeColorSpace == ColorSpace.Linear);
            desc.msaaSamples = msaaSamples;
            desc.bindMS = false;
            desc.useDynamicScale = camera.allowDynamicResolution;
            return desc;
        }
    }
}
