using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Graphics.Sprites;
using osum.Graphics.Skins;
using osum.Helpers;
using OpenTK;
using OpenTK.Graphics;
using osum.Audio;

namespace osum.GameModes.Play.Components
{
    class StreamSwitchDisplay : GameComponent
    {
        pDrawable arrowLarge;
        pDrawable arrowSmall;
        pDrawable text;

        public override void Initialize()
        {
            arrowLarge = new pSprite(TextureManager.Load(OsuTexture.stream_changing_arrow), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, Vector2.Zero, 0.95f, true, Color4.White);
            arrowLarge.Additive = true;
            //arrowLarge.Offset = new Vector2(100, 0);

            arrowSmall = arrowLarge.Clone();
            arrowSmall.DrawDepth = 0.9f;
            arrowSmall.ScaleScalar = 0.5f;

            text = arrowLarge.Clone();
            text.DrawDepth = 1;
            text.Additive = false;

            spriteManager.Add(arrowLarge);
            spriteManager.Add(arrowSmall);
            spriteManager.Add(text);

            //start hidden
            spriteManager.Sprites.ForEach(s => s.Alpha = 0);

            base.Initialize();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override void Update()
        {
            base.Update();
        }

        bool isSwitching;
        bool increase;

        internal void BeginSwitch(bool increase)
        {
            if (isSwitching) return;
            isSwitching = true;

            this.increase = increase;

            arrowSmall.ScaleScalar = 0.5f;
            arrowLarge.ScaleScalar = 1;

            text.ScaleScalar = 1;
            ((pSprite)text).Texture = TextureManager.Load(increase ? OsuTexture.stream_changing_up : OsuTexture.stream_changing_down);

            Transformation fadeIn = new Transformation(TransformationType.Fade, 0, 1, Clock.ModeTime, Clock.ModeTime + 300);
            text.Transform(fadeIn);

            if (increase)
            {
                arrowSmall.Rotation = 0;
                arrowLarge.Rotation = 0;
            }
            else
            {
                arrowSmall.Rotation = (float)Math.PI;
                arrowLarge.Rotation = (float)Math.PI;
            }


            const int animation_length = 2000;

            Transformation smallMove = new Transformation(new Vector2(0, increase ? 10 : -10), new Vector2(0, increase ? -10 : 10), Clock.ModeTime, Clock.ModeTime + animation_length);
            smallMove.Looping = true;

            Transformation largeMove = new Transformation(new Vector2(0, increase ? 5 : -5), new Vector2(0, increase ? -5 : 5), Clock.ModeTime, Clock.ModeTime + animation_length);
            largeMove.Looping = true;

            Transformation fade1 = new Transformation(TransformationType.Fade, 0, 0.5f, Clock.ModeTime, Clock.ModeTime + animation_length / 2) { Looping = true, LoopDelay = animation_length / 2 };
            Transformation fade2 = new Transformation(TransformationType.Fade, 0.5f, 0, Clock.ModeTime + animation_length / 2, Clock.ModeTime + animation_length) { Looping = true, LoopDelay = animation_length / 2 };

            arrowLarge.Transform(largeMove, fade1, fade2);
            arrowSmall.Transform(smallMove, fade1, fade2);
        }

        internal void EndSwitch()
        {
            if (!isSwitching) return;
            isSwitching = false;

            spriteManager.Sprites.ForEach(s =>
                {
                    s.Transformations.Clear();
                    s.Transform(new Transformation(TransformationType.Fade, s.Alpha, 0, Clock.ModeTime, Clock.ModeTime + 400));
                    s.ScaleTo(3, 600, EasingTypes.In);
                });

            AudioEngine.PlaySample(increase ? OsuSamples.stream_up : OsuSamples.stream_down);

            if (!increase)
            {
                arrowLarge.RotateTo(arrowLarge.Rotation + 2, 600, EasingTypes.In);
                arrowSmall.RotateTo(arrowSmall.Rotation + 2, 600, EasingTypes.In);
            }
        }
    }
}
