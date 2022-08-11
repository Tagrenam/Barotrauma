using System;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
    abstract class StringComponent : ItemComponent
    {
        //an array to keep track of how long ago a signal was received on both inputs
        protected float[] timeSinceReceived;

        protected string[] receivedSignal;

        protected Connection[] signalConnection;

        //the output is sent if both inputs have received a signal within the timeframe
        protected float timeFrame;


        [InGameEditable(DecimalCount = 2),
            Serialize(0.0f, IsPropertySaveable.Yes, description: "The item must have received signals to both inputs within this timeframe to output the result." +
            " If set to 0, the inputs must be received at the same time.", alwaysUseInstanceValues: true)]
        public float TimeFrame
        {
            get { return timeFrame; }
            set
            {
                if (value > timeFrame)
                {
                    timeSinceReceived[0] = timeSinceReceived[1] = timeSinceReceived[2] = Math.Max(value * 2.0f, 0.1f);
                }
                timeFrame = Math.Max(0.0f, value);
            }
        }

        private Connection GetConnection(string name)
        {
            if (item.Connections != null)
            {
                foreach (var each in item.Connections)
                {
                    if (each != null && each.Name == name) return each;
                }
            }
            return null;
        }

        public StringComponent(Item item, ContentXElement element)
            : base(item, element)
        {
            timeSinceReceived = new float[] { Math.Max(timeFrame * 2.0f, 0.1f), Math.Max(timeFrame * 2.0f, 0.1f), Math.Max(timeFrame * 2.0f, 0.1f) };
            receivedSignal = new string[3];
            signalConnection = new Connection[] { GetConnection("signal_in1"), GetConnection("signal_in2"), GetConnection("signal_in3") };
        }

        sealed public override void Update(float deltaTime, Camera cam)
        {
            bool deactivate = true;
            bool earlyReturn = false;
            for (int i = 0; i < timeSinceReceived.Length; i++)
            {
                if (signalConnection[i] == null)
                {
                    signalConnection = new Connection[] { GetConnection("signal_in1"), GetConnection("signal_in2"), GetConnection("signal_in3") };
                    DebugConsole.NewMessage($"Update ok signalConnection[{i}] is null", Color.Green);
                    continue;
                }
                if (signalConnection[i].IsConnected())
                {
                    deactivate &= timeSinceReceived[i] > timeFrame;
                    earlyReturn |= timeSinceReceived[i] > timeFrame;
                    timeSinceReceived[i] += deltaTime;
                }
            }
            // only stop Update() if both signals timed-out. if IsActive == false, then the component stops updating.
            IsActive = !deactivate;
            // early return if either of the signal timed-out
            if (earlyReturn) { return; }
            string output = Calculate(receivedSignal[0], receivedSignal[1], receivedSignal[2]);
            item.SendSignal(output, "signal_out");        
        }

        protected abstract string Calculate(string signal1, string signal2, string signal3);

        public override void ReceiveSignal(Signal signal, Connection connection)
        {
            switch (connection.Name)
            {
                case "signal_in1":
                    receivedSignal[0] = signal.value;
                    timeSinceReceived[0] = 0.0f;
                    IsActive = true;
                    break;
                case "signal_in2":
                    receivedSignal[1] = signal.value;
                    timeSinceReceived[1] = 0.0f;
                    IsActive = true;
                    break;
                case "signal_in3":
                    receivedSignal[2] = signal.value;
                    timeSinceReceived[2] = 0.0f;
                    IsActive = true;
                    break;
            }
        }
    }
}
