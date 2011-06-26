using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Graphics.Sprites;
using osum.Graphics.Skins;
using osum.Helpers;
using OpenTK.Graphics;
using OpenTK;
using osum.Audio;

namespace osum.GameModes.Play.Components
{
    internal class CountdownDisplay : GameComponent
    {
        pSprite background;

        pSprite text;

        const float distance_from_bottom = 0;

        public override void Initialize()
        {
            background = new pSprite(TextureManager.Load(OsuTexture.countdown_background), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Audio, new Vector2(0, distance_from_bottom), 0.99f, true, Color4.White);
            spriteManager.Add(background);

            text = new pSprite(null, FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Audio, new Vector2(0, distance_from_bottom), 1, true, Color4.White);
            spriteManager.Add(text);

            spriteManager.Sprites.ForEach(s => s.Alpha = 0);

            base.Initialize();
        }

        internal int StartTime = -1;
        double BeatLength;

        internal void SetStartTime(int start, double beatLength)
        {
            StartTime = start;
            BeatLength = beatLength;
            spriteManager.Sprites.ForEach(s => s.ScaleScalar = 1);
            HasFinished = false;
        }

        internal void Hide()
        {
            HasFinished = true;
            StartTime = -1;
            spriteManager.Sprites.ForEach(s => s.Alpha = 0);
        }

        internal void SetDisplay(int countdown)
        {
            bool didChangeTexture = true;

            switch (countdown)
            {
                case 0:
                    text.Texture = TextureManager.Load(OsuTexture.countdown_go);
                    spriteManager.Sprites.ForEach(s => { s.FadeOut(150); s.ScaleTo(1.3f, 200); });
                    AudioEngine.PlaySample(OsuSamples.countgo);
                    HasFinished = true;
                    break;
                case 1:
                    text.Texture = TextureManager.Load(OsuTexture.countdown_1);
                    AudioEngine.PlaySample(OsuSamples.count1);
                    break;
                case 2:
                    text.Texture = TextureManager.Load(OsuTexture.countdown_2);
                    AudioEngine.PlaySample(OsuSamples.count2);
                    break;
                case 3:
                    text.Texture = TextureManager.Load(OsuTexture.countdown_3);
                    AudioEngine.PlaySample(OsuSamples.count3);
                    break;
                case 4:
                    spriteManager.Sprites.ForEach(s => { s.FadeIn(200); });
                    didChangeTexture = false; //don't flash on 4
                    break;
                case 7:
                    text.Texture = TextureManager.Load(OsuTexture.countdown_ready);
                    spriteManager.Sprites.ForEach(s => { s.FadeIn(200); });
                    break;
                default:
                    didChangeTexture = false;
                    break;
            }


            if (countdown < 4)
            {
                text.Transform(new TransformationBounce(Clock.AudioTime, Clock.AudioTime + 150, 1, 0.2f, 2));
                background.Transform(new TransformationBounce(Clock.AudioTime, Clock.AudioTime + 150, 1, 0.1f, 2));
            }

            if (didChangeTexture)
            {
                pDrawable flash = text.AdditiveFlash(250, 0.5f);
                flash.Transform(new Transformation(TransformationType.Scale, 1, 1.4f, Clock.Time, Clock.Time + 250));
            }
        }

        int lastCountdownUpdate = -1;
        private bool HasFinished;
        public override void Update()
        {
            base.Update();

            if (HasFinished) return;

            int countdown = (int)Math.Max(0, (StartTime - Clock.AudioTime) / BeatLength);

            if (countdown != lastCountdownUpdate)
            {
                lastCountdownUpdate = countdown;
                SetDisplay(countdown);
            }


        }
    }
}
