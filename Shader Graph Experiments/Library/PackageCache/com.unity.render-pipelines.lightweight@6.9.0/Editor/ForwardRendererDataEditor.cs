using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.LWRP;

namespace UnityEditor.Rendering.LWRP
{
    [CustomEditor(typeof(ForwardRendererData), true)]
    public class ForwardRendererDataEditor : ScriptableRendererDataEditor
    {
        private static class Styles
        {
            public static readonly GUIContent RendererTitle = new GUIContent("Forward Renderer", "Custom Forward Renderer for LWRP.");
            public static readonly GUIContent OpaqueMask = new GUIContent("Default Layer Mask", "Controls which layers to globally include in the Custom Forward Renderer.");
            public static readonly GUIContent defaultStencilStateLabel = EditorGUIUtility.TrTextContent("Default Stencil State", "Configure stencil state for the opaque and transparent render passes.");
        }
        
        SerializedProperty m_OpaqueLayerMask;
        SerializedProperty m_TransparentLayerMask;
        SerializedProperty m_DefaultStencilState;

        SerializedProperty m_Shaders;

        private void OnEnable()
        {
            m_OpaqueLayerMask = serializedObject.FindProperty("m_OpaqueLayerMask");
            m_TransparentLayerMask = serializedObject.FindProperty("m_TransparentLayerMask");
            m_DefaultStencilState = serializedObject.FindProperty("m_DefaultStencilState");

            m_Shaders = serializedObject.FindProperty("shaders");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(Styles.RendererTitle, EditorStyles.boldLabel); // Title
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_OpaqueLayerMask, Styles.OpaqueMask);
            if (EditorGUI.EndChangeCheck()) // We copy the opaque mask to the transparent mask, later we might expose both
                m_TransparentLayerMask.intValue = m_OpaqueLayerMask.intValue;
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Overrides", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_DefaultStencilState, Styles.defaultStencilStateLabel, true);
            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI(); // Draw the base UI, contains ScriptableRenderFeatures list

            // Add a "Reload All" button in inspector when we are in developer's mode
            if (EditorPrefs.GetBool("DeveloperMode"))
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(m_Shaders, true);

                if (GUILayout.Button("Reload All"))
                {
                    var resources = target as ForwardRendererData;
                    resources.shaders = null;
                    ResourceReloader.ReloadAllNullIn(target, LightweightRenderPipelineAsset.packagePath);
                }
            }
        }
    }
}
