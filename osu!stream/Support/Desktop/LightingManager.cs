using System;
using osum.Input.Sources;
using osum.Audio;
using OpenTK.Graphics.OpenGL;
using osum.GameModes;
using osum.GameplayElements.Beatmaps;
using osum.GameModes.Play;
using System.IO.Ports;
using System.Drawing;
using System.Collections.Generic;
using OpenTK.Graphics;


namespace osum
{
    public class LightingColour
    {
        public byte R;
        public byte G;
        public byte B;

        internal void FromColor4(Color4 colour)
        {
            R = (byte)Math.Round(colour.R * 255);
            G = (byte)Math.Round(colour.G * 255);
            B = (byte)Math.Round(colour.B * 255);
        }

        internal void AddColor4(Color4 colour, bool cap = true)
        {
            if (cap)
            {
                byte newR = (byte)Math.Round(colour.R * 255);
                byte newG = (byte)Math.Round(colour.G * 255);
                byte newB = (byte)Math.Round(colour.B * 255);

                if (newR > R) R = (byte)Math.Min(255, R + (byte)Math.Round(colour.R * 255));
                if (newG > G) G = (byte)Math.Min(255, G + (byte)Math.Round(colour.G * 255));
                if (newB > B) B = (byte)Math.Min(255, B + (byte)Math.Round(colour.B * 255));
            }
            else
            {
                R = (byte)Math.Min(255, R + (byte)Math.Round(colour.R * 255));
                G = (byte)Math.Min(255, G + (byte)Math.Round(colour.G * 255));
                B = (byte)Math.Min(255, B + (byte)Math.Round(colour.B * 255));
            }
        }
    }

    public class LightingManager : GameComponent, IDisposable
    {
        private SerialPort port;

        int led_count = 160;
        const float intensity = 1;
        const float diminish = 0.96f;

        internal static LightingManager Instance;

        LightingColour[] colours;
        byte[] buffer;

        public override void Dispose()
        {
            /*for (int i = 0; i < led_count; i++)
                colours[i] = new LightingColour() { R = 1, G = 1, B = 1 };
            Update();*/

            base.Dispose();
        }

        public LightingManager()
        {
            Instance = this;

            try
            {
                string[] portNames = SerialPort.GetPortNames();

                foreach (string s in portNames)
                {
                    try
                    {
                        port = new SerialPort(s, 576000);
                        port.Open();
                        port.ReadTimeout = 100;
                        string l = port.ReadLine();

                        if (!string.IsNullOrEmpty(l))
                        {
                            led_count = Int32.Parse(l);
                            break;
                        }

                        port.Close();
                    }
                    catch { }
                }
            }
            catch
            {
                port = null;
            }

            if (port != null)
            {
                colours = new LightingColour[led_count];
                buffer = new byte[led_count * 3];
                for (int i = 0; i < led_count; i++)
                    colours[i] = new LightingColour() { R = 1, G = 1, B = 1 };
            }
        }

        int currentLight = 0;

        internal bool UseVolume = true;

        public override void Update()
        {
            if (port == null || !port.IsOpen)
                return;

            float power = UseVolume ? AudioEngine.Music.CurrentPower : 0;

            //byte colour = (byte)Math.Max(1,((power - 0.5) * 2) * (255*intensity));
            byte colour = (byte)Math.Max(1, (power - 0.3f) / 0.7f * (255 * intensity));

            MainMenu m = Director.CurrentMode as MainMenu;

            float r = 0, g = 0, b = 0;

            if (m != null)
            {
                switch (m.lastExplode)
                {
                    case 0:
                        r = 152 / 255f;
                        g = 110 / 255f;
                        b = 201 / 255f;
                        break;
                    case 1:
                        r = 247 / 255f;
                        g = 74 / 255f;
                        b = 189 / 255f;
                        break;
                    case 2:
                        r = 255 / 255f;
                        g = 175 / 255f;
                        b = 142 / 255f;
                        break;
                }
            }
            else
            {
                r = (float)GameBase.Random.NextDouble();
                g = (float)GameBase.Random.NextDouble();
                b = (float)GameBase.Random.NextDouble();
            }

            if (colour > 1)
            {
                if (colour > 128) currentLight = GameBase.Random.Next(led_count);

                colours[currentLight].R = (byte)(colour * r);
                colours[currentLight].G = (byte)(colour * g);
                colours[currentLight].B = (byte)(colour * b);
            }

            int i = 0;
            foreach (LightingColour c in colours)
            {
                if (c.R > 1) c.R = (byte)(c.R * diminish);
                if (c.G > 1) c.G = (byte)(c.G * diminish);
                if (c.B > 1) c.B = (byte)(c.B * diminish);

                //c.R = (byte)(c.R * diminish);
                //c.G = (byte)(c.G * diminish);
                //c.B = (byte)(c.B * diminish);


                buffer[i * 3] = c.B;
                buffer[i * 3 + 1] = c.G;
                buffer[i * 3 + 2] = c.R;

                i++;
            }

            port.Write(buffer, 0, buffer.Length);

            base.Update();
        }

        public void Blind(Color4 colour)
        {
            if (colours == null) return;
            foreach (LightingColour c in colours)
                c.FromColor4(colour);
        }

        int spacingCurrent;
        internal void Add(Color4 colour, int spacingInterval = 0)
        {
            if (colours == null) return;

            bool reverse = spacingInterval < 0;
            if (reverse) spacingInterval = Math.Abs(spacingInterval);

            int i = spacingCurrent;
            foreach (LightingColour c in colours)
            {
                if (i % spacingInterval == 0)
                    c.AddColor4(colour);
                i = (i + 1) % colours.Length;
            }

            spacingCurrent = (spacingCurrent + colours.Length - (reverse ? -1 : 1)) % colours.Length;
        }
    }
}

