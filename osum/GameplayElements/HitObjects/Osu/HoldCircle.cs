using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Graphics.Skins;
using osum.Helpers;
using osum.Graphics.Sprites;
using OpenTK.Graphics;
using OpenTK;
using osum.Graphics.Primitives;
using System.Drawing;
using osum.Graphics.Drawables;
using osum.Audio;

namespace osum.GameplayElements.HitObjects.Osu
{
    class HoldCircle : Slider
    {
        internal pSprite holdCircleOverlay;
        internal HoldCircle(HitObjectManager hit_object_manager, Vector2 pos, int startTime, bool newCombo, int comboOffset, HitObjectSoundType soundType, double pathLength, int repeatCount, List<HitObjectSoundType> soundTypes, double velocity, double tickDistance)
            : base(hit_object_manager, pos, startTime, newCombo, comboOffset, soundType, CurveTypes.Linear, repeatCount, pathLength, new List<Vector2>() { pos, pos }, soundTypes, velocity, tickDistance)
        {
        }

        public override bool IncrementCombo
        {
            get
            {
                return false;
            }
        }

        protected override void CalculateSplines()
        {
            EndTime = StartTime + (int)(1000 * PathLength / Velocity * RepeatCount);
            TrackingPosition = position;
        }


        protected override Vector2 positionAtProgress(double progress)
        {
            return position;
        }

        protected override ScoreChange HitActionInitial()
        {
            ScoreChange s = base.HitActionInitial();

            if (s != ScoreChange.Ignore)
                burstEndpoint();

            return s;

        }

        internal override void burstEndpoint()
        {
            int duration = (EndTime - StartTime) / RepeatCount;

            int now = spriteCollectionStart[0].ClockingNow;

            Transformation bounce = new Transformation(TransformationType.Scale,
                1.1f + 0.4f * progressCurrent / RepeatCount,
                1 + 0.3f * progressCurrent / RepeatCount,
                now, now + duration,
                EasingTypes.In
            );

            foreach (pDrawable p in spriteCollectionStart)
            {
                p.Transformations.RemoveAll(b => b.Type == TransformationType.Scale);
                p.Transform(bounce);
            }
        }

        internal CircularProgress circularProgress;

        protected override void initializeSprites()
        {
            Transformation fadeInTrack = new Transformation(TransformationType.Fade, 0, 1,
    StartTime - DifficultyManager.PreEmpt, StartTime - DifficultyManager.PreEmpt + DifficultyManager.FadeIn);
            Transformation fadeOut = new Transformation(TransformationType.Fade, 1, 0,
                EndTime, EndTime + DifficultyManager.HitWindow50);

            spriteCollectionStart.Add(new pSprite(TextureManager.Load(OsuTexture.hitcircle), FieldTypes.GamefieldSprites, OriginTypes.Centre, ClockTypes.Audio, Position, SpriteManager.drawOrderBwd(EndTime + 9), false, Color.White));
            spriteCollectionStart.Add(new pSprite(TextureManager.Load(OsuTexture.hitcircleoverlay), FieldTypes.GamefieldSprites, OriginTypes.Centre, ClockTypes.Audio, Position, SpriteManager.drawOrderBwd(EndTime + 8), false, Color.White));

            holdCircleOverlay = new pSprite(TextureManager.Load(OsuTexture.holdcircle), FieldTypes.GamefieldSprites, OriginTypes.Centre, ClockTypes.Audio, Position, SpriteManager.drawOrderBwd(EndTime + 7), false, Color.White);
            holdCircleOverlay.Transform(new NullTransform(StartTime, EndTime));

            spriteCollectionStart.Add(holdCircleOverlay);

            spriteCollectionStart.ForEach(s => s.Transform(fadeInTrack));
            spriteCollectionStart.ForEach(s => s.Transform(fadeOut));

            circularProgress = new CircularProgress(position, 4, false, 0, Color.White);
            circularProgress.Clocking = ClockTypes.Audio;
            circularProgress.Field = FieldTypes.GamefieldExact;
            circularProgress.Additive = true;
            circularProgress.Transform(new NullTransform(StartTime, EndTime));

            spriteCollectionStart.Add(circularProgress);

            Sprites.AddRange(spriteCollectionStart);
            SpriteCollectionDim.Add(holdCircleOverlay);
        }

        protected override void initializeStartCircle()
        {
            base.initializeStartCircle();

            HitCircleStart.SpriteHitCircle1.Texture = null;
            HitCircleStart.SpriteHitCircle2.Texture = null;
            HitCircleStart.SpriteHitCircleText.Colour = Color4.Transparent;
        }

        internal override void PlaySound(HitObjectSoundType type)
        {
            float volume = Volume * (0.5f + 0.5f * circularProgress.Progress);

            if ((type & HitObjectSoundType.Finish) > 0)
                AudioEngine.PlaySample(OsuSamples.HitFinish, SampleSet, volume);

            if ((type & HitObjectSoundType.Whistle) > 0)
                AudioEngine.PlaySample(OsuSamples.HitWhistle, SampleSet, volume);

            if ((type & HitObjectSoundType.Clap) > 0)
                AudioEngine.PlaySample(OsuSamples.HitClap, SampleSet, volume);

            AudioEngine.PlaySample(OsuSamples.HitNormal, SampleSet, volume);
        }

        static Color4 hold_colour = new Color4(0.648f, 0, 244 / 256f, 1);

        internal override Color4 Colour
        {
            get
            {
                return base.Colour;
            }
            set
            {
                base.Colour = hold_colour;
                spriteCollectionStart[0].Transformations.RemoveAll(t => t.Type == TransformationType.Colour);
                spriteCollectionStart[0].Transform(new Transformation(Colour, Color4.White, StartTime, EndTime));
                circularProgress.Transformations.RemoveAll(t => t.Type == TransformationType.Colour);
                circularProgress.Transform(new Transformation(
                    new Color4(Colour.R, Colour.G, Colour.B, 0.8f),
                    ColourHelper.Lighten(new Color4(Colour.R, Colour.G, Colour.B, 0.8f), 0.5f),
                    StartTime, EndTime));
            }
        }

        protected override Graphics.Primitives.Line lineAtProgress(double progress)
        {
            return new Line(position, position);
        }

        internal override void UpdatePathTexture()
        {
        }

        public override float HpMultiplier
        {
            get
            {
                return (float)(PathLength / TickDistance);
            }
        }

        public override void Update()
        {
            progressCurrent = pMathHelper.ClampToOne((float)(circularProgress.ClockingNow - StartTime) / (EndTime - StartTime)) * RepeatCount;
            circularProgress.Progress = progressCurrent / RepeatCount;

            //don't want to base.Update() due to Slider-specific stuff, but we need to call this as per HitObject's Update().
            UpdateDimming();
        }

        protected override void beginTracking()
        {
            holdCircleOverlay.FadeOut(160);
            circularProgress.FadeIn(160);
            circularProgress.AlwaysDraw = true;
        }

        protected override void endTracking()
        {
            holdCircleOverlay.FadeIn(80);
            circularProgress.FadeOut(80);
            circularProgress.AlwaysDraw = false;

            Transformation returnto = new Transformation(TransformationType.Scale, spriteCollectionStart[0].ScaleScalar, 1, ClockingNow, ClockingNow + 150, EasingTypes.In);

            foreach (pDrawable p in spriteCollectionStart)
            {
                p.Transformations.RemoveAll(t => t.Type == TransformationType.Scale);
                p.Transform(returnto);
            }

        }

        protected override void newEndpoint()
        {

        }

        protected override void lastEndpoint()
        {
            holdCircleOverlay.FadeOut(100);

            circularProgress.Transformations.Clear();

            int now = ClockingNow;

            circularProgress.Alpha = 0.8f;
            circularProgress.FadeOut(500);
            circularProgress.EvenShading = true;
            circularProgress.Transform(new Transformation(TransformationType.Scale, circularProgress.ScaleScalar + 0.1f, circularProgress.ScaleScalar + 0.4f, now, now + 500, EasingTypes.In));
            circularProgress.Transform(new Transformation(circularProgress.Colour, Color4.White, now, now + 100, EasingTypes.In));
            circularProgress.AlwaysDraw = false;
        }

        internal override Vector2 EndPosition
        {
            get
            {
                return Position;
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}