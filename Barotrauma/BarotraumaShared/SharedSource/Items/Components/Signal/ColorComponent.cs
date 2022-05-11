using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
    class ColorComponent : ItemComponent
    {
        protected float[] receivedSignal;

        private System.Collections.Generic.Dictionary<string, float> dict = new System.Collections.Generic.Dictionary<string, float>();

        private string output = "0,0,0,0";

        [InGameEditable, Serialize(false, IsPropertySaveable.Yes, description: "When enabled makes the component translate the signal from HSV into RGB where red is the hue between 0 and 360, green is the saturation between 0 and 1 and blue is the value between 0 and 1.", alwaysUseInstanceValues: true)]
        public bool UseHSV { get; set; }
        [InGameEditable, Serialize(false, IsPropertySaveable.Yes, description: "When enabled use signal_dict to set values: R:255,G:120,B:123,A:127", alwaysUseInstanceValues: true)]
        public bool UseDictionary { get; set; }

        public ColorComponent(Item item, ContentXElement element)
            : base(item, element)
        {
            receivedSignal = new float[4];
            IsActive = true;
        }

        public override void Update(float deltaTime, Camera cam)
        {
            item.SendSignal(output, "signal_out");
        }

        private void UpdateOutput()
        {
            float signalR = receivedSignal[0],
                    signalG = receivedSignal[1],
                    signalB = receivedSignal[2],
                    signalA = receivedSignal[3];

            if (UseHSV)
            {
                Color hsvColor = ToolBox.HSVToRGB(signalR, signalG, signalB);
                signalR = hsvColor.R;
                signalG = hsvColor.G;
                signalB = hsvColor.B;
            }
            else if (UseDictionary)
            {
                dict.TryGetValue("R", out signalR);
                dict.TryGetValue("G", out signalG);
                dict.TryGetValue("B", out signalB);
                dict.TryGetValue("A", out signalA);
            }

            output = signalR.ToString("G", CultureInfo.InvariantCulture);
            output += "," + signalG.ToString("G", CultureInfo.InvariantCulture);
            output += "," + signalB.ToString("G", CultureInfo.InvariantCulture);
            output += "," + signalA.ToString("G", CultureInfo.InvariantCulture);
        }

        public override void ReceiveSignal(Signal signal, Connection connection)
        {
            switch (connection.Name)
            {
                case "signal_r":
                    float.TryParse(signal.value, NumberStyles.Float, CultureInfo.InvariantCulture, out receivedSignal[0]);
                    UpdateOutput();
                    break;
                case "signal_g":
                    float.TryParse(signal.value, NumberStyles.Float, CultureInfo.InvariantCulture, out receivedSignal[1]);
                    UpdateOutput();
                    break;
                case "signal_b":
                    float.TryParse(signal.value, NumberStyles.Float, CultureInfo.InvariantCulture, out receivedSignal[2]);
                    UpdateOutput();
                    break;
                case "signal_a":
                    float.TryParse(signal.value, NumberStyles.Float, CultureInfo.InvariantCulture, out receivedSignal[3]);
                    UpdateOutput();
                    break;
                case "signal_dict":
                    try
                    {
                        char[] separators = new char[] { ' ', ',' };
                        dict = signal.value.Split(separators, StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => x.Split(':'))
                            .ToDictionary(x => x[0], x => float.Parse(x[1]));
                    }
                    catch
                    {
                        break;
                    }


                    UpdateOutput();
                    break;
            }
        }
    }
}
