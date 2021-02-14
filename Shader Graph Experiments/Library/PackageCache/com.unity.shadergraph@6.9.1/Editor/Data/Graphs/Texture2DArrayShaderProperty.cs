using System;
using System.Text;
using UnityEditor.Graphing;
using UnityEngine;

namespace UnityEditor.ShaderGraph
{
    [Serializable]
    class Texture2DArrayShaderProperty : AbstractShaderProperty<SerializableTextureArray>
    {
        [SerializeField]
        private bool m_Modifiable = true;

        public Texture2DArrayShaderProperty()
        {
            value = new SerializableTextureArray();
            displayName = "Texture2D Array";
        }

        public override PropertyType propertyType
        {
            get { return PropertyType.Texture2DArray; }
        }

        public bool modifiable
        {
            get { return m_Modifiable; }
            set { m_Modifiable = value; }
        }

        public override Vector4 defaultValue
        {
            get { return new Vector4(); }
        }

        public override bool isBatchable
        {
            get { return false; }
        }

        public override bool isExposable
        {
            get { return true; }
        }

        public override bool isRenamable
        {
            get { return true; }
        }

        public override string GetPropertyBlockString()
        {
            var result = new StringBuilder();
            if (!m_Modifiable)
            {
                result.Append("[NonModifiableTextureData] ");
            }
            result.Append("[NoScaleOffset] ");

            result.Append(referenceName);
            result.Append("(\"");
            result.Append(displayName);
            result.Append("\", 2DArray) = \"white\" {}");
            return result.ToString();
        }

        public override string GetPropertyDeclarationString(string delimiter = ";")
        {
            return string.Format("TEXTURE2D_ARRAY({0}){1} SAMPLER(sampler{0}){1}", referenceName, delimiter);
        }

        public override string GetPropertyAsArgumentString()
        {
            return string.Format("TEXTURE2D_ARRAY_PARAM({0}, sampler{0})", referenceName);
        }

        public override PreviewProperty GetPreviewMaterialProperty()
        {
            return new PreviewProperty(PropertyType.Texture2D)
            {
                name = referenceName,
                textureValue = value.textureArray
            };
        }

        public override AbstractMaterialNode ToConcreteNode()
        {
            return new Texture2DArrayAssetNode { texture = value.textureArray };
        }

        public override AbstractShaderProperty Copy()
        {
            var copied = new Texture2DArrayShaderProperty();
            copied.displayName = displayName;
            copied.value = value;
            return copied;
        }
    }
}
