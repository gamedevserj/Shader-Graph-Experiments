using System;
using UnityEditor.Graphing;
using UnityEngine;

namespace UnityEditor.ShaderGraph
{
    [Serializable]
    class Vector2ShaderProperty : VectorShaderProperty
    {
        public Vector2ShaderProperty()
        {
            displayName = "Vector2";
        }

        public override PropertyType propertyType
        {
            get { return PropertyType.Vector2; }
        }

        public override Vector4 defaultValue
        {
            get { return new Vector4(value.x, value.y, 0, 0); }
        }

        public override bool isBatchable
        {
            get { return true; }
        }

        public override bool isExposable
        {
            get { return true; }
        }

        public override bool isRenamable
        {
            get { return true; }
        }

        public override string GetPropertyDeclarationString(string delimiter = ";")
        {
            return string.Format("{0}2 {1}{2}", concretePrecision.ToShaderString(), referenceName, delimiter);
        }

        public override PreviewProperty GetPreviewMaterialProperty()
        {
            return new PreviewProperty(PropertyType.Vector2)
            {
                name = referenceName,
                vector4Value = value
            };
        }

        public override AbstractMaterialNode ToConcreteNode()
        {
            var node = new Vector2Node();
            node.FindInputSlot<Vector1MaterialSlot>(Vector2Node.InputSlotXId).value = value.x;
            node.FindInputSlot<Vector1MaterialSlot>(Vector2Node.InputSlotYId).value = value.y;
            return node;
        }

        public override AbstractShaderProperty Copy()
        {
            var copied = new Vector2ShaderProperty();
            copied.displayName = displayName;
            copied.value = value;
            return copied;
        }
    }
}
