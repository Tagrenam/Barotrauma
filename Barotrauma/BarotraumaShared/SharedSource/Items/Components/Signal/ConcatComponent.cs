using System.Xml.Linq;
using System;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
    class ConcatComponent : StringComponent
    {
        private int maxOutputLength;

        [Editable, Serialize(256, IsPropertySaveable.No, description: "The maximum length of the output string. Warning: Large values can lead to large memory usage or networking load.")]
        public int MaxOutputLength
        {
            get { return maxOutputLength; }
            set
            {
                maxOutputLength = Math.Max(value, 0);
            }
        }

        [InGameEditable, Serialize("", IsPropertySaveable.No)]
        public string Separator_1_2
        {
            get;
            set;
        }

        [InGameEditable, Serialize("", IsPropertySaveable.No)]
        public string Separator_2_3
        {
            get;
            set;
        }

        public ConcatComponent(Item item, ContentXElement element)
            : base(item, element)
        {
        }

        protected override string Calculate(string signal1, string signal2, string signal3)
        {
            string output = "";

            if (string.IsNullOrEmpty(signal1) == false)
                output += signal1;

            if (string.IsNullOrEmpty(Separator_1_2) == false)
                output += Separator_1_2;

            if (string.IsNullOrEmpty(signal2) == false)
                output += signal2;

            if (string.IsNullOrEmpty(Separator_2_3) == false)
                output += Separator_2_3;

            if (string.IsNullOrEmpty(signal3) == false)
                output += signal3;

            return output.Length <= maxOutputLength ? output : output.Substring(0, MaxOutputLength);
        }
    }
}
