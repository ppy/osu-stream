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


namespace osum
{
    public class LightingColour
    {
        public byte R;
        public byte G;
        public byte B;
    }

    public class LightingManager : GameComponent, IDisposable
    {
        private SerialPort port;

        const int LED_COUNT = 58;
        const float intensity = 1;
        const float diminish = 0.99f;
        
        LightingColour[] colours = new LightingColour[LED_COUNT];
        byte[] buffer = new byte[LED_COUNT * 3];

        public override void Dispose()
        {
            for (int i = 0; i < LED_COUNT; i++)
                colours[i] = new LightingColour() { R = 1, G = 1, B = 1 };
            Update();

            base.Dispose();
        }

        public LightingManager()
        {
            
            for (int i = 0; i < LED_COUNT; i++)
                colours[i] = new LightingColour() { R = 1, G = 1, B = 1 };

            try
            {
                port = new SerialPort("COM15", 576000);
                port.Open();
            }
            catch
            {
                port = null;
            }
        }

        int currentLight = 0;

        public override void Update()
        {
            if (port == null)
                return;

            float power = AudioEngine.Music.CurrentPower;

            byte colour = (byte)Math.Max(1,((power - 0.5) * 2) * (255*intensity));

            MainMenu m = Director.CurrentMode as MainMenu;

            float r = 0, g = 0, b = 0;

            if (m != null)
            {
                switch (m.lastExplode)
                {
                    case 0:
                        r = 152/255f;
                        g = 110/255f;
                        b = 201 / 255f;
                        break;
                    case 1:
                        r = 247/255f;
                        g = 74/255f;
                        b = 189 / 255f;
                        break;
                    case 2:
                        r = 255/255f;
                        g = 175/255f;
                        b = 142 / 255f;
                        break;
                }
            }

            if (colour > 1)
            {
                if (colour > 128) currentLight = GameBase.Random.Next(LED_COUNT);

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

                buffer[i * 3] = c.R;
                buffer[i * 3 + 1] = c.G;
                buffer[i * 3 + 2] = c.B;
                
                i++;
            }
            
            port.Write(buffer, 0, buffer.Length);

            base.Update();
        }
    }
}

