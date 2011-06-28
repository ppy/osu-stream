using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Graphics.Skins;
using osum.Graphics.Sprites;
using OpenTK;
using osum.GameplayElements;
using osum.Helpers;
using OpenTK.Graphics;
using osum.Graphics;

namespace osum.GameModes.Play.Components
{
    class GuideFinger : GameComponent
    {
        List<pDrawable> fingers = new List<pDrawable>();


        pSprite leftFinger;

        internal HitObjectManager HitObjectManager;
        internal TouchBurster TouchBurster;
        private pSprite rightFinger;
        private pSprite leftFinger2;
        private pSprite rightFinger2;
        public override void Initialize()
        {
            leftFinger = new pSprite(TextureManager.Load(OsuTexture.finger_inner), new Vector2(-100, 200))
            {
                Field = FieldTypes.GamefieldSprites,
                Origin = OriginTypes.Centre,
                Colour = ColourHelper.Lighten2(Color4.LimeGreen, 0.5f),
                Alpha = 0,
                Additive = true
            };

            leftFinger2 = new pSprite(TextureManager.Load(OsuTexture.finger_outer), Vector2.Zero)
            {
                Field = FieldTypes.GamefieldSprites,
                Origin = OriginTypes.Centre,
                Colour = Color4.LimeGreen,
                Alpha = 0,
                Additive = false
            };

            rightFinger = new pSprite(TextureManager.Load(OsuTexture.finger_inner), new Vector2(612, 200))
            {
                Field = FieldTypes.GamefieldSprites,
                Origin = OriginTypes.Centre,
                Colour = ColourHelper.Lighten2(Color4.Red, 0.5f),
                Alpha = 0,
                Additive = true
            };

            rightFinger2 = new pSprite(TextureManager.Load(OsuTexture.finger_outer), Vector2.Zero)
            {
                Field = FieldTypes.GamefieldSprites,
                Origin = OriginTypes.Centre,
                Colour = Color4.Red,
                Alpha = 0,
                Additive = false
            };

            spriteManager.Add(leftFinger);
            spriteManager.Add(rightFinger);
            spriteManager.Add(leftFinger2);
            spriteManager.Add(rightFinger2);

            fingers.Add(leftFinger);
            fingers.Add(rightFinger);

            base.Initialize();
        }

        public override bool Draw()
        {
            if (HitObjectManager == null) return false;

            return base.Draw();
        }

        public override void Update()
        {
            base.Update();

            if (HitObjectManager == null) return;

            HitObject nextObject = HitObjectManager.NextObject;
            HitObject nextObjectConnected = nextObject != null ? nextObject.connectedObject : null;

            bool objectHasFinger = false;
            bool connectedObjectHasFinger = false;

            foreach (pDrawable finger in fingers)
            {
                HitObject obj = finger.Tag as HitObject;

                if (obj != null)
                {
                    if (obj == nextObject) objectHasFinger = true;
                    if (obj == nextObjectConnected) connectedObjectHasFinger = true;

                    if (obj.IsHit)
                    {
                        lastHitTime = Clock.AudioTime;
                        lastFinger = finger;

                        finger.Tag = null;
                        finger.FadeOut(1000, 0.3f);
                        finger.MoveTo(new Vector2(finger == leftFinger ? 50 : 512 - 50, 200), 1000, EasingTypes.InOut);
                    }
                    else if (obj.IsActive)
                    {
                        finger.Position = obj.TrackingPosition;

                        if (TouchBurster != null)
                            TouchBurster.Burst(GameBase.GamefieldToStandard(finger.Position + finger.Offset), 40, 0.5f, 1);
                    }
                    else if (obj.IsVisible)
                    {
                        int timeUntilObject = obj.StartTime - Clock.AudioTime;

                        if (timeUntilObject < 350)
                        {
                            Vector2 src = finger.Position;
                            Vector2 dest = obj.TrackingPosition;

                            finger.Position = src + (dest - src) * 0.015f * (float)GameBase.ElapsedMilliseconds;

                            float vOffset = 0;
                            if (timeUntilObject > 100)
                                vOffset = (1 - pMathHelper.ClampToOne((timeUntilObject - 100) / 300f));
                            else
                                vOffset = pMathHelper.ClampToOne(timeUntilObject / 100f);

                            finger.Offset.Y = vOffset * -55;
                            finger.ScaleScalar = 1 + 0.6f * vOffset;

                            if (TouchBurster != null)
                                TouchBurster.Burst(GameBase.GamefieldToStandard(finger.Position + finger.Offset), 40, 0.5f, 1);
                        }
                    }
                }
            }

            {
                int timeUntilObject = nextObject == null ? Int32.MaxValue : nextObject.StartTime - Clock.AudioTime;

                if (timeUntilObject < 500)
                {
                    if (!objectHasFinger) checkObject(nextObject);
                    if (nextObjectConnected != null && !connectedObjectHasFinger) checkObject(nextObjectConnected);
                }
            }

            leftFinger2.Position = leftFinger.Position;
            leftFinger2.Offset = leftFinger.Offset;
            leftFinger2.ScaleScalar = leftFinger.ScaleScalar;
            leftFinger2.Alpha = leftFinger.Alpha;

            rightFinger2.Position = rightFinger.Position;
            rightFinger2.Offset = rightFinger.Offset;
            rightFinger2.ScaleScalar = rightFinger.ScaleScalar;
            rightFinger2.Alpha = rightFinger.Alpha;
        }

        int lastHitTime = 0;
        pDrawable lastFinger;

        private void checkObject(HitObject nextObject)
        {
            pDrawable preferred = null;

            float leftPart = GameBase.GamefieldBaseSize.Width / 3f * 1;
            float rightPart = GameBase.GamefieldBaseSize.Width / 3f * 2;

            float distFromLeft = pMathHelper.Distance(nextObject.Position, leftFinger.Tag == null ? leftFinger.Position : ((HitObject)leftFinger.Tag).EndPosition);
            float distFromRight = pMathHelper.Distance(nextObject.Position, rightFinger.Tag == null ? rightFinger.Position : ((HitObject)rightFinger.Tag).EndPosition);

            if (nextObject.connectedObject != null)
            {
                //if there is a connected object, always use the correct L-R arrangement.
                if (nextObject.Position.X < nextObject.connectedObject.Position.X)
                    preferred = leftFinger;
                else
                    preferred = rightFinger;
            }
            else if (distFromLeft < 20)
                //stacked objects (left finger)
                preferred = leftFinger;
            else if (distFromRight < 20)
                //stacked objects (right finger)
                preferred = rightFinger;
            else if (nextObject.Position.X < leftPart || nextObject.Position2.X < leftPart)
                //starts or ends in left 1/3 of screen.
                preferred = leftFinger;
            else if (nextObject.Position.X > rightPart || nextObject.Position2.X > rightPart)
                //starts or ends in right 1/3 of screen.
                preferred = rightFinger;
            else if (nextObject.StartTime - lastHitTime < 150)
                //fast hits; always alternate fingers
                preferred = lastFinger == leftFinger ? rightFinger : leftFinger;
            else
                //fall back to the closest finger.
                preferred = distFromLeft < distFromRight ? leftFinger : rightFinger;

            if (preferred == leftFinger && nextObject.Position.X > rightFinger.Position.X && rightFinger.Tag == null)
                //if we're about to use left finger but the object is wedged between the right finger and right side of screen, use right instead.
                preferred = rightFinger;
            else if (preferred == rightFinger && nextObject.Position.X < leftFinger.Position.X && leftFinger.Tag == null)
                //if we're about to use right finger but the object is wedged between the left finger and right side of screen, use left instead.
                preferred = leftFinger;

            pDrawable alternative = preferred == leftFinger ? rightFinger : leftFinger;

            if (preferred.Tag == null)
            {
                preferred.Tag = nextObject;
                preferred.Transformations.Clear();
                preferred.FadeIn(300);
            }
            else
            {
                //finger is bxusy...
                HitObject busyObject = preferred.Tag as HitObject;

                if (busyObject.EndTime > nextObject.StartTime - 80)
                {
                    alternative.Tag = nextObject;
                    alternative.Transformations.Clear();
                    alternative.FadeIn(300);
                }
            }
        }

    }
}
