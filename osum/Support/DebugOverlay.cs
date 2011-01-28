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
using osum.Audio;

namespace osum.Support
{
    static class DebugOverlay
    {
        internal static pText fpsDisplay;
        private static double weightedAverageFrameTime;

        static double lastUpdateTime;
        static bool updateFrame;

        static internal void Update()
        {
            if (fpsDisplay == null)
            {
#if DEBUG
                fpsDisplay = new pText("", 16, new Vector2(0, 40), new Vector2(512,256), 0, true, Color4.White, false);
                GameBase.Instance.MainSpriteManager.Add(fpsDisplay);
#else
                return;
#endif
            }

            weightedAverageFrameTime = weightedAverageFrameTime * 0.98 + GameBase.ElapsedMilliseconds * 0.02;
            double fps = (1000 / weightedAverageFrameTime);

            lastUpdateTime += GameBase.ElapsedMilliseconds;

            if (lastUpdateTime > 500)
            {
                lastUpdateTime = 0;
                updateFrame = true;
            }
            else
            {
                updateFrame = false;
            }


            if (updateFrame)
            {

#if DEBUG
                int accurateAudio = (int)(AudioEngine.Music.CurrentTime*1000);
				fpsDisplay.Colour = fps < 50 ? Color.OrangeRed : Color.GreenYellow;
                fpsDisplay.Text = String.Format("{0:0}fps Game:{1:#,0}ms Mode:{4:#,0} AuFast:{2:#,0}ms AuDrv:{5:#,0}ms ({6}) {3}",
                                                Math.Round(fps),
                                                Clock.Time, Clock.AudioTime, Player.Autoplay ? "AP" : "", Clock.ModeTime,
				                                accurateAudio, Clock.AudioTime - accurateAudio);
#endif
            }
        }

        internal static void AddLine(string s)
        {
            if (!updateFrame) return;
			
#if DEBUG
            fpsDisplay.Text += "\n" + s;
#endif
        }
    }
}
