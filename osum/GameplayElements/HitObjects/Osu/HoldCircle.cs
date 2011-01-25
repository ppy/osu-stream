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

namespace osum.GameplayElements.HitObjects.Osu
{
    class HoldCircle : Slider
    {
        private pSprite holdCircleOverlay;
        internal HoldCircle(HitObjectManager hit_object_manager, Vector2 pos, int startTime, bool newCombo, int comboOffset, HitObjectSoundType soundType, double pathLength, int repeatCount, List<HitObjectSoundType> soundTypes)
            : base(hit_object_manager, pos, startTime, newCombo, comboOffset, soundType, CurveTypes.Linear, repeatCount, pathLength, new List<Vector2>() { pos, pos }, soundTypes)
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
            EndTime = StartTime + (int)(1000 * PathLength / m_HitObjectManager.VelocityAt(StartTime) * RepeatCount);
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

        protected override void burstEndpoint()
        {
            int duration = (EndTime - StartTime) / RepeatCount;

            Transformation bounce = new Transformation(TransformationType.Scale,
                1.1f + 0.5f * progressCurrent/RepeatCount,
                1 + 0.4f * progressCurrent / RepeatCount,
                Clock.AudioTime,
                Clock.AudioTime + duration,
                EasingTypes.In
            );

            foreach (pDrawable p in spriteCollectionStart)
            {
                p.Transformations.RemoveAll(b => b.Type == TransformationType.Scale);
                p.Transform(bounce);
            }
        }

        CircularProgress circularProgress;

        protected override void initializeSprites()
        {
            Transformation fadeInTrack = new Transformation(TransformationType.Fade, 0, 1,
    StartTime - DifficultyManager.PreEmpt, StartTime - DifficultyManager.PreEmpt + DifficultyManager.FadeIn);
            Transformation fadeOut = new Transformation(TransformationType.Fade, 1, 0,
                EndTime, EndTime + DifficultyManager.HitWindow50);
            
            spriteCollectionStart.Add(new pSprite(TextureManager.Load(OsuTexture.hitcircle), FieldTypes.Gamefield512x384, OriginTypes.Centre, ClockTypes.Audio, Position, SpriteManager.drawOrderBwd(EndTime + 9), false, Color.White));
            spriteCollectionStart.Add(new pSprite(TextureManager.Load(OsuTexture.hitcircleoverlay), FieldTypes.Gamefield512x384, OriginTypes.Centre, ClockTypes.Audio, Position, SpriteManager.drawOrderBwd(EndTime + 8), false, Color.White));
            
            holdCircleOverlay = new pSprite(TextureManager.Load(OsuTexture.holdcircle), FieldTypes.Gamefield512x384, OriginTypes.Centre, ClockTypes.Audio, Position, SpriteManager.drawOrderBwd(EndTime + 8), false, Color.White);
            holdCircleOverlay.Transform(new NullTransform(StartTime, EndTime));
            spriteCollectionStart.Add(holdCircleOverlay);

            spriteCollectionStart.ForEach(s => s.Transform(fadeInTrack));
            spriteCollectionStart.ForEach(s => s.Transform(fadeOut));

            circularProgress = new CircularProgress(position, 180, false, 0, Color.White);
            circularProgress.Clocking = ClockTypes.Audio;
            circularProgress.Field = FieldTypes.Gamefield512x384;
            circularProgress.Additive = true;
            circularProgress.Transform(new NullTransform(StartTime, EndTime));
            
            spriteCollectionStart.Add(circularProgress);

            SpriteCollection.AddRange(spriteCollectionStart);
            DimCollection.AddRange(spriteCollectionStart);
        }

        protected override void initializeStartCircle()
        {
            base.initializeStartCircle();

            hitCircleStart.SpriteHitCircle1.Texture = null;
            hitCircleStart.SpriteHitCircle2.Texture = null;
            hitCircleStart.SpriteHitCircleText.Colour = Color4.Transparent;
        }

        internal override Color4 Colour
        {
            get
            {
                return base.Colour;
            }
            set
            {
                base.Colour = new Color4(0.648f,0,244/256f,1);
                spriteCollectionStart[0].Transformations.RemoveAll(t => t.Type == TransformationType.Colour);
                spriteCollectionStart[0].Transform(new Transformation(Colour, Color4.White, StartTime, EndTime));
                circularProgress.Transform(new Transformation(
                    new Color4(Colour.R, Colour.G, Colour.B, 0.8f),
                    ColourHelper.Lighten(new Color4(Colour.R, Colour.G, Colour.B, 0.8f),0.5f),
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

        public override void Update()
        {
            progressCurrent = pMathHelper.ClampToOne((float)(Clock.AudioTime - StartTime) / (EndTime - StartTime)) * RepeatCount;
            circularProgress.Progress = progressCurrent / RepeatCount;
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

            Transformation returnto = new Transformation(TransformationType.Scale,spriteCollectionStart[0].ScaleScalar, 1, Clock.AudioTime, Clock.AudioTime + 150, EasingTypes.In);

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
            
			circularProgress.Alpha = 0.8f;
			circularProgress.FadeOut(500);
			circularProgress.EvenShading = true;
			circularProgress.Transform(new Transformation(TransformationType.Scale, circularProgress.ScaleScalar + 0.2f, circularProgress.ScaleScalar + 0.4f, Clock.AudioTime, Clock.AudioTime + 300, EasingTypes.Out));
			circularProgress.Transform(new Transformation(circularProgress.Colour, Color4.White, Clock.AudioTime, Clock.AudioTime + 100, EasingTypes.Out));
            circularProgress.AlwaysDraw = true;
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