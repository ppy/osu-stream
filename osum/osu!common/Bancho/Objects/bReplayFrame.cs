using System;
using System.IO;
using osu_common.Helpers;

namespace osu_common.Bancho.Objects
{
    [Flags]
    public enum pButtonState
    {
        None = 0,
        Left1 = 1,
        Right1 = 2,
        Left2 = 4,
        Right2 = 8
    }

    public class bReplayFrame :bSerializable
    {
        public float mouseX;
        public float mouseY;
        public bool mouseLeft;
        public bool mouseRight;
        public bool mouseLeft1;
        public bool mouseRight1;
        public bool mouseLeft2;
        public bool mouseRight2;
        public pButtonState buttonState;
        public int time;

        public bReplayFrame(int time, float posX, float posY, pButtonState buttonState)
        {
            mouseX = posX;
            mouseY = posY;
            this.buttonState = buttonState;
            SetButtonStates(buttonState);
            this.time = time;
        }

        public void SetButtonStates(pButtonState buttonState)
        {
            this.buttonState = buttonState;
            mouseLeft = (buttonState & (pButtonState.Left1 | pButtonState.Left2)) > 0;
            mouseLeft1 = (buttonState & pButtonState.Left1) > 0;
            mouseLeft2 = (buttonState & pButtonState.Left2) > 0;
            mouseRight = (buttonState & (pButtonState.Right1 | pButtonState.Right2)) > 0;
            mouseRight1 = (buttonState & pButtonState.Right1) > 0;
            mouseRight2 = (buttonState & pButtonState.Right2) > 0;
        }

        public bReplayFrame(Stream s) 
        {
            SerializationReader sr = new SerializationReader(s);

            buttonState = (pButtonState)sr.ReadByte();
            SetButtonStates(buttonState);
            
            byte bt = sr.ReadByte();
            if (bt > 0)//Handle Pre-Taiko compatible replays.
                SetButtonStates(pButtonState.Right1);

            mouseX = sr.ReadSingle();
            mouseY = sr.ReadSingle();
            time = sr.ReadInt32();
        }

        public void ReadFromStream(SerializationReader sr)
        {
            throw new System.NotImplementedException();
        }

        public void WriteToStream(SerializationWriter sw)
        {
            sw.Write((byte)buttonState);
            sw.Write((byte)0);
            sw.Write(mouseX);
            sw.Write(mouseY);
            sw.Write(time);
        }
    }
}