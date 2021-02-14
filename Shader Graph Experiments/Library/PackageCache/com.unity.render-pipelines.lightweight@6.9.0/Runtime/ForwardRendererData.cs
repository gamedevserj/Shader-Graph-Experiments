#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
#endif
using System;

namespace UnityEngine.Rendering.LWRP
{    
    public class ForwardRendererData : ScriptableRendererData
    {
#if UNITY_EDITOR
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812")]
        internal class CreateForwardRendererAsset : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                var instance = CreateInstance<ForwardRendererData>();
                AssetDatabase.CreateAsset(instance, pathName);
                ResourceReloader.ReloadAllNullIn(instance, LightweightRenderPipelineAsset.packagePath);
                Selection.activeObject = instance;
            }
        }

        [MenuItem("Assets/Create/Rendering/Lightweight Render Pipeline/Forward Renderer", priority = CoreUtils.assetCreateMenuPriority1)]
        static void CreateForwardRendererData()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, CreateInstance<CreateForwardRendererAsset>(), "CustomForwardRendererData.asset", null, null);
        }
#endif

        [Serializable, ReloadGroup]
        public sealed class ShaderResources
        {
            [SerializeField, Reload("Shaders/Utils/Blit.shader")]
            public Shader blitPS;

            [SerializeField, Reload("Shaders/Utils/CopyDepth.shader")]
            public Shader copyDepthPS;

            [SerializeField, Reload("Shaders/Utils/ScreenSpaceShadows.shader")]
            public Shader screenSpaceShadowPS;
        
            [SerializeField, Reload("Shaders/Utils/Sampling.shader")]
            public Shader samplingPS;
        }

        public ShaderResources shaders = null;

        [SerializeField] LayerMask m_OpaqueLayerMask = -1;
        [SerializeField] LayerMask m_TransparentLayerMask = -1;

        [SerializeField] StencilStateData m_DefaultStencilState = null;

        protected override ScriptableRenderer Create()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                ResourceReloader.ReloadAllNullIn(this, LightweightRenderPipelineAsset.packagePath);
#endif
            return new ForwardRenderer(this);
        }

        internal LayerMask opaqueLayerMask => m_OpaqueLayerMask;

        public LayerMask transparentLayerMask => m_TransparentLayerMask;

        public StencilStateData defaultStencilState => m_DefaultStencilState;

        protected override void OnEnable()
        {
            base.OnEnable();

            // Upon asset creation, OnEnable is called and `shaders` reference is not yet initialized
            // We need to call the OnEnable for data migration when updating from old versions of LWRP that
            // serialized resources in a different format. Early returning here when OnEnable is called
            // upon asset creation is fine because we guarantee new assets get created with all resources initialized.
            if (shaders == null)
                return;

#if UNITY_EDITOR
            foreach (var shader in shaders.GetType().GetFields())
            {
                if (shader.GetValue(shaders) == null)
                {
                    ResourceReloader.ReloadAllNullIn(this, LightweightRenderPipelineAsset.packagePath);
                    break;
                }
            }
#endif
        }
    }
}
