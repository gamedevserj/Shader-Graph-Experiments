using System;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Drawing.Controls;
using UnityEngine;
using UnityEditor.Graphing;

namespace UnityEditor.ShaderGraph
{
    enum ColorMode
    {
        Default,
        HDR
    }

    [Title("Input", "Basic", "Color")]
    class ColorNode : AbstractMaterialNode, IGeneratesBodyCode, IPropertyFromNode
    {
        public const int OutputSlotId = 0;
        private const string kOutputSlotName = "Out";

        public ColorNode()
        {
            name = "Color";
            UpdateNodeAfterDeserialization();
        }


        [SerializeField]
        Color m_Color = new Color(UnityEngine.Color.clear, ColorMode.Default);

        [Serializable]
        public struct Color
        {
            public UnityEngine.Color color;
            public ColorMode mode;

            public Color(UnityEngine.Color color, ColorMode mode)
            {
                this.color = color;
                this.mode = mode;
            }
        }

        [ColorControl("")]
        public Color color
        {
            get { return m_Color; }
            set
            {
                if ((value.color == m_Color.color) && (value.mode == m_Color.mode))
                    return;
                m_Color = value;
                Dirty(ModificationScope.Node);
            }
        }

        public sealed override void UpdateNodeAfterDeserialization()
        {
            AddSlot(new Vector4MaterialSlot(OutputSlotId, kOutputSlotName, kOutputSlotName, SlotType.Output, Vector4.zero));
            RemoveSlotsNameNotMatching(new[] { OutputSlotId });
        }

        public override void CollectShaderProperties(PropertyCollector properties, GenerationMode generationMode)
        {
            if (!generationMode.IsPreview())
                return;

            properties.AddShaderProperty(new ColorShaderProperty()
            {
                overrideReferenceName = GetVariableNameForNode(),
                generatePropertyBlock = false,
                value = color.color,
                colorMode = color.mode
            });
        }

        public void GenerateNodeCode(ShaderStringBuilder sb, GraphContext graphContext, GenerationMode generationMode)
        {
            if (generationMode.IsPreview())
                return;

            sb.AppendLine(@"$precision4 {0} = IsGammaSpace() ? $precision4({1}, {2}, {3}, {4}) : $precision4(SRGBToLinear($precision3({1}, {2}, {3})), {4});"
                , GetVariableNameForNode()
                , NodeUtils.FloatToShaderValue(color.color.r)
                , NodeUtils.FloatToShaderValue(color.color.g)
                , NodeUtils.FloatToShaderValue(color.color.b)
                , NodeUtils.FloatToShaderValue(color.color.a));
        }

        public override string GetVariableNameForSlot(int slotId)
        {
            return GetVariableNameForNode();
        }

        public override void CollectPreviewMaterialProperties(List<PreviewProperty> properties)
        {
            properties.Add(new PreviewProperty(PropertyType.Color)
            {
                name = GetVariableNameForNode(),
                colorValue = color.color
            });
        }

        public AbstractShaderProperty AsShaderProperty()
        {
            return new ColorShaderProperty { value = color.color, colorMode = color.mode };
        }

        public int outputSlotId { get { return OutputSlotId; } }
    }
}
