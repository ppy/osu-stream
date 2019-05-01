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
#if FULL_DEBUG
        internal static pText fpsDisplay;
        const int vertical_offset = 40;
#else
        internal static pSpriteText fpsDisplay;
        const int vertical_offset = 5;
#endif

        const int horizontal_offset = 5;

        private static double weightedAverageFrameTime;

        static double lastUpdateTime;
        static bool updateFrame;

        static internal void Update()
        {
            if (fpsDisplay == null)
            {
#if FULL_DEBUG
                fpsDisplay = new pText("", 10, new Vector2(horizontal_offset, 40), new Vector2(512,256), 0, true, Color4.White, true);
                GameBase.MainSpriteManager.Add(fpsDisplay);
#else
                fpsDisplay = new pSpriteText("", "default", 0, FieldTypes.StandardSnapBottomRight, OriginTypes.BottomRight, ClockTypes.Game, new Vector2(horizontal_offset, vertical_offset), 1, true, Color4.White);
                #if iOS
                fpsDisplay.ScaleScalar = 0.6f;
                #elif !FULL_DEBUG
                fpsDisplay.ScaleScalar = 0.3f;
                #endif
                GameBase.MainSpriteManager.Add(fpsDisplay);
#endif
            }

            weightedAverageFrameTime = weightedAverageFrameTime * 0.5 + Clock.ElapsedMilliseconds * 0.5;

#if iOS
            /*if (Clock.ElapsedMilliseconds > 25)
            {
                fpsDisplay.Position = new Vector2(fpsDisplay.Position.X, fpsDisplay.Position.Y + 5);
                fpsDisplay.MoveTo(new Vector2(horizontal_offset, vertical_offset), 600, EasingTypes.In);
            }

            int newGcCount = GC.CollectionCount(0) + GC.CollectionCount(1);


            if (gcCount < newGcCount)
            {
                gcCount = newGcCount;
                fpsDisplay.Position = new Vector2(fpsDisplay.Position.X + 40, fpsDisplay.Position.Y);
                fpsDisplay.MoveTo(new Vector2(horizontal_offset, vertical_offset), 600, EasingTypes.In);
            }*/
#endif

            double fps = (1000 / weightedAverageFrameTime);

            lastUpdateTime += Clock.ElapsedMilliseconds;

#if FULL_DEBUG
            if (lastUpdateTime > 50)
#else
            if (lastUpdateTime > 16)
#endif
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

                fpsDisplay.Colour = fps < 50 ? Color.OrangeRed : Color.GreenYellow;
#if FULL_DEBUG
                int accurateAudio = (int)(AudioEngine.Music.CurrentTime * 1000) + Clock.UNIVERSAL_OFFSET_MP3;
                fpsDisplay.Text = String.Format("{0:0}fps Game:{1:#,0}ms Mode:{4:#,0} Audio:{2:#,0}ms ({6}) {3}",
                                                Math.Round(fps),
                                                Clock.Time, Clock.AudioTime, Player.Autoplay ? "-AUTOPLAY-" : "", Clock.ModeTime,
                                                accurateAudio, Clock.AudioTime - accurateAudio);
                fpsDisplay.Position.Y = Director.CurrentOsuMode == OsuMode.Play ? 40 : 0;
#else
                fpsDisplay.ShowInt((int)Math.Round(Math.Min(60, fps), 0));
                fpsDisplay.Alpha = fps < 58f ? 1 : 0;
#endif
            }
        }

        internal static void AddLine(string s)
        {
            if (!updateFrame) return;

#if FULL_DEBUG
            fpsDisplay.Text += "\n" + s;
#endif
        }
    }
}
