using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Graphics.Sprites;
using System.Drawing;
using osum.Helpers;
using osum.GameModes;
using OpenTK;
using OpenTK.Graphics;

namespace osum.Support
{
    static class DebugOverlay
    {
        internal static pText fpsDisplay;
        private static double weightedAverageFrameTime;

        static internal void Update()
        {
            if (fpsDisplay == null)
            {
#if DEBUG
                fpsDisplay = new pText("", 8, new Vector2(0, 40), Vector2.Zero, 0, true, Color4.White, false);
                GameBase.Instance.SpriteManager.Add(fpsDisplay);
#else
                return;
#endif
            }

            weightedAverageFrameTime = weightedAverageFrameTime * 0.98 + GameBase.ElapsedMilliseconds * 0.02;
            double fps = (1000 / weightedAverageFrameTime);

#if DEBUG
            fpsDisplay.Colour = fps < 50 ? Color.OrangeRed : Color.GreenYellow;
            fpsDisplay.Text = String.Format("{0:0}fps Game:{1:#,0}ms Mode:{4:#,0} Audio:{2:#,0}ms {3}", Math.Round(fps),
                                            Clock.Time, Clock.AudioTime, Player.Autoplay ? "AP" : "", Clock.ModeTime);
#endif
        }

        internal static void AddLine(string s)
        {
#if DEBUG
            fpsDisplay.Text += "\n" + s;
#endif
        }
    }
}
