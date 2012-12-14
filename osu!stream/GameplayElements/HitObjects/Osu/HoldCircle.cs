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
using osum.GameplayElements.Beatmaps;

namespace osum.GameplayElements.HitObjects.Osu
{
    class HoldCircle : Slider
    {
        internal HoldCircle(HitObjectManager hit_object_manager, Vector2 pos, int startTime, bool newCombo, int comboOffset, HitObjectSoundType soundType, double pathLength, int repeatCount, List<HitObjectSoundType> soundTypes, double velocity, double tickDistance, List<SampleSetInfo> sampleSets)
            : base(hit_object_manager, pos, startTime, newCombo, comboOffset, soundType, CurveTypes.Linear, repeatCount, pathLength, new List<Vector2>() { pos, pos }, soundTypes, velocity, tickDistance, sampleSets)
        {
            snakingBegin = StartTime - DifficultyManager.PreEmpt;
            snakingEnd = StartTime - DifficultyManager.PreEmpt;
        }

        internal HoldCircle(HitObjectManager hit_object_manager, Vector2 pos, int startTime, bool newCombo, int comboOffset, HitObjectSoundType soundType, double pathLength, int repeatCount, List<HitObjectSoundType> soundTypes, double velocity, double tickDistance)
            : base(hit_object_manager, pos, startTime, newCombo, comboOffset, soundType, CurveTypes.Linear, repeatCount, pathLength, new List<Vector2>() { pos, pos }, soundTypes, velocity, tickDistance)
        {
            snakingBegin = StartTime - DifficultyManager.PreEmpt;
            snakingEnd = StartTime - DifficultyManager.PreEmpt;
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
            trackingPosition = position;
        }


        protected override Vector2 positionAtProgress(double progress, out Line line)
        {
            line = null;
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

            Transformation bounce = new TransformationF(TransformationType.Scale,
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

            border.Transformations.RemoveAll(b => b.Type == TransformationType.Scale);
            border.Transform(bounce);
        }

        internal CircularProgress circularProgress;

        pSprite border;

        internal pSprite inactiveOverlay;
        internal pSprite activeOverlay;

        protected override void initializeSprites()
        {
            Transformation fadeInTrack = new TransformationF(TransformationType.Fade, 0, 1, StartTime - DifficultyManager.PreEmpt, StartTime - DifficultyManager.PreEmpt + DifficultyManager.FadeIn);
            Transformation fadeOut = new TransformationF(TransformationType.Fade, 1, 0, EndTime, EndTime + DifficultyManager.HitWindow50);

            activeOverlay = new pSprite(TextureManager.Load(OsuTexture.holdactive), FieldTypes.GamefieldSprites, OriginTypes.Centre, ClockTypes.Audio, Position, SpriteManager.drawOrderBwd(EndTime - 18), false, Color.White);
            spriteCollectionStart.Add(activeOverlay);

            inactiveOverlay = new pSprite(TextureManager.Load(OsuTexture.holdinactive), FieldTypes.GamefieldSprites, OriginTypes.Centre, ClockTypes.Audio, Position, SpriteManager.drawOrderBwd(EndTime - 19), false, Color.White);
            inactiveOverlay.Transform(new NullTransform(StartTime, EndTime));
            spriteCollectionStart.Add(inactiveOverlay);

            border = new pSprite(TextureManager.Load(OsuTexture.holdoverlay), FieldTypes.GamefieldSprites, OriginTypes.Centre, ClockTypes.Audio, Position, SpriteManager.drawOrderBwd(EndTime - 20), false, Color.White);
            border.Transform(fadeInTrack);
            border.Transform(fadeOut);
            Sprites.Add(border);

            spriteCollectionStart.ForEach(s => s.Transform(fadeInTrack));
            spriteCollectionStart.ForEach(s => s.Transform(fadeOut));

            circularProgress = new CircularProgress(position, 4, false, 0, Color.White);
            circularProgress.Clocking = ClockTypes.Audio;
            circularProgress.Field = FieldTypes.GamefieldExact;
            circularProgress.Additive = true;
            circularProgress.Transform(new NullTransform(StartTime, EndTime));

            spriteCollectionStart.Add(circularProgress);

            Sprites.AddRange(spriteCollectionStart);

            activeOverlay.Transform(new TransformationC(hold_colour, Color4.White, StartTime, EndTime));
            circularProgress.Transform(new TransformationC(new Color4(hold_colour.R, hold_colour.G, hold_colour.B, 0.8f), ColourHelper.Lighten(new Color4(hold_colour.R, hold_colour.G, hold_colour.B, 0.8f), 0.5f),
                StartTime, EndTime));

            border.TagNumeric = HitObject.DIMMABLE_TAG;
            inactiveOverlay.TagNumeric = HitObject.DIMMABLE_TAG;
        }

        protected override void initializeStartCircle()
        {
            base.initializeStartCircle();

            HitCircleStart.SpriteHitCircle1.Bypass = true;
            HitCircleStart.SpriteHitCircleText.Bypass = true;
            HitCircleStart.Colour = hold_colour;
        }

        internal override void PlaySound(HitObjectSoundType type, SampleSetInfo ssi)
        {
            float volume = ssi.Volume * (0.5f + 0.5f * circularProgress.Progress);

            if ((type & HitObjectSoundType.Finish) > 0)
                AudioEngine.PlaySample(OsuSamples.HitFinish, ssi.AdditionSampleSet, volume);

            if ((type & HitObjectSoundType.Whistle) > 0)
                AudioEngine.PlaySample(OsuSamples.HitWhistle, ssi.AdditionSampleSet, volume);

            if ((type & HitObjectSoundType.Clap) > 0)
                AudioEngine.PlaySample(OsuSamples.HitClap, ssi.AdditionSampleSet, volume);

            AudioEngine.PlaySample(OsuSamples.HitNormal, ssi.SampleSet, volume);
        }

        protected override void playRebound(int lastJudgedEndpoint)
        {
            if (lastJudgedEndpoint == RepeatCount)
                base.playRebound(lastJudgedEndpoint);
            else
            {
                SampleSetInfo ss = SampleSets != null ? SampleSets[lastJudgedEndpoint] : SampleSet;
                PlaySound(SoundTypeList != null ? SoundTypeList[lastJudgedEndpoint] : SoundType,ss);
            }
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
            }
        }

        internal override int ColourIndex
        {
            get
            {
                return base.ColourIndex;
            }
            set
            {
                //don't pass this down.
            }
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
            inactiveOverlay.FadeOut(160);
            circularProgress.FadeIn(160);
            circularProgress.AlwaysDraw = true;
        }

        protected override void endTracking()
        {
            inactiveOverlay.FadeIn(80);
            circularProgress.FadeOut(80);
            circularProgress.AlwaysDraw = false;

            Transformation returnto = new TransformationF(TransformationType.Scale, spriteCollectionStart[0].ScaleScalar, 1, ClockingNow, ClockingNow + 150, EasingTypes.In);

            foreach (pDrawable p in spriteCollectionStart)
            {
                p.Transformations.RemoveAll(t => t.Type == TransformationType.Scale);
                p.Transform(returnto);
            }

            border.Transformations.RemoveAll(t => t.Type == TransformationType.Scale);
            border.Transform(returnto);

        }

        protected override void newEndpoint()
        {

        }

        protected override void lastEndpoint()
        {
            inactiveOverlay.FadeOut(100);

            circularProgress.Transformations.Clear();

            int now = ClockingNow;

            if (circularProgress.Alpha > 0.5f)
                circularProgress.Alpha = 0.8f;
            circularProgress.FadeOut(500);
            circularProgress.EvenShading = true;
            circularProgress.Transform(new TransformationF(TransformationType.Scale, circularProgress.ScaleScalar + 0.1f, circularProgress.ScaleScalar + 0.4f, now, now + 500, EasingTypes.In));
            circularProgress.Transform(new TransformationC(circularProgress.Colour, Color4.White, now, now + 100, EasingTypes.In));
            circularProgress.AlwaysDraw = false;
        }

        public override Vector2 EndPosition
        {
            get
            {
                return Position;
            }
        }

        public override Vector2 Position2
        {
            get
            {
                return Position;
            }
        }
    }
}