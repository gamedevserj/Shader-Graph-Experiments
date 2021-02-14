using System;
using UnityEditor.Graphing;
using UnityEngine;

namespace UnityEditor.ShaderGraph
{
    [Serializable]
    class Vector4ShaderProperty : VectorShaderProperty
    {
        public Vector4ShaderProperty()
        {
            displayName = "Vector4";
        }

        public override PropertyType propertyType
        {
            get { return PropertyType.Vector4; }
        }

        public override Vector4 defaultValue
        {
            get { return value; }
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
            return string.Format("{0}4 {1}{2}", concretePrecision.ToShaderString(), referenceName, delimiter);
        }

        public override PreviewProperty GetPreviewMaterialProperty()
        {
            return new PreviewProperty(PropertyType.Vector4)
            {
                name = referenceName,
                vector4Value = value
            };
        }

        public override AbstractMaterialNode ToConcreteNode()
        {
            var node = new Vector4Node();
            node.FindInputSlot<Vector1MaterialSlot>(Vector4Node.InputSlotXId).value = value.x;
            node.FindInputSlot<Vector1MaterialSlot>(Vector4Node.InputSlotYId).value = value.y;
            node.FindInputSlot<Vector1MaterialSlot>(Vector4Node.InputSlotZId).value = value.z;
            node.FindInputSlot<Vector1MaterialSlot>(Vector4Node.InputSlotWId).value = value.w;
            return node;
        }

        public override AbstractShaderProperty Copy()
        {
            var copied = new Vector4ShaderProperty();
            copied.displayName = displayName;
            copied.value = value;
            return copied;
        }
    }
}
