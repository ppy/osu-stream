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

namespace osum.GameplayElements.HitObjects.Osu
{
    class HoldCircle : Slider
    {
        internal HoldCircle(HitObjectManager hit_object_manager, Vector2 pos, int startTime, bool newCombo, HitObjectSoundType soundType, double pathLength, int repeatCount, List<HitObjectSoundType> soundTypes)
            : base(hit_object_manager, pos, startTime, newCombo, soundType, CurveTypes.Linear, repeatCount, pathLength, new List<Vector2>() { pos, pos }, soundTypes)
        {
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

        protected override void burstEndpoint()
        {
            Transformation bounce = new TransformationBounce(Clock.AudioTime, Clock.AudioTime + 200, 1.4f, 0.2f, 1);
            foreach (pSprite p in spriteCollectionStart)
            {
                p.Transform(bounce);
            }

        }

        protected override void initializeSprites()
        {
            Transformation fadeInTrack = new Transformation(TransformationType.Fade, 0, 1,
    StartTime - DifficultyManager.PreEmpt, StartTime - DifficultyManager.PreEmpt + DifficultyManager.FadeIn);
            Transformation fadeOut = new Transformation(TransformationType.Fade, 1, 0,
                EndTime, EndTime + DifficultyManager.HitWindow50);
            
            spriteCollectionStart.Add(new pSprite(TextureManager.Load(OsuTexture.hitcircle), FieldTypes.Gamefield512x384, OriginTypes.Centre, ClockTypes.Audio, Position, SpriteManager.drawOrderBwd(EndTime + 9), false, Color.White));
            spriteCollectionStart.Add(new pSprite(TextureManager.Load(OsuTexture.hitcircleoverlay), FieldTypes.Gamefield512x384, OriginTypes.Centre, ClockTypes.Audio, Position, SpriteManager.drawOrderBwd(EndTime + 8), false, Color.White));

            spriteCollectionStart.ForEach(s => s.Transform(fadeInTrack));
            spriteCollectionStart.ForEach(s => s.Transform(fadeOut));

            SpriteCollection.AddRange(spriteCollectionStart);
            DimCollection.AddRange(spriteCollectionStart);
        }

        internal override Color4 Colour
        {
            get
            {
                return base.Colour;
            }
            set
            {
                base.Colour = value;
                spriteCollectionStart[0].Transformations.RemoveAll(t => t.Type == TransformationType.Colour);
                spriteCollectionStart[0].Transform(new Transformation(Colour, Color4.White, StartTime, EndTime));
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
            progressLastUpdate = progressCurrent;
            progressCurrent = pMathHelper.ClampToOne((float)(Clock.AudioTime - StartTime) / (EndTime - StartTime)) * RepeatCount;
        }

        protected override void beginTracking()
        {
            
        }

        protected override void endTracking()
        {
            
        }

        protected override void newEndpoint()
        {
            
        }

        protected override void lastEndpoint()
        {
            
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