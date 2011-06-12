using System;
using System.Collections.Generic;
using osum.Graphics;
using osum.Graphics.Skins;
using osum.Graphics.Sprites;
using osum.Helpers;
using OpenTK;
using OpenTK.Graphics;
using osum.GameplayElements.HitObjects;
using osum.Graphics.Drawables;
using osum.GameModes;
using osum.Audio;

namespace osum.GameplayElements
{
    internal class Spinner : HitObjectSpannable
    {
        /// <summary>
        /// Used for the flicker effects on the score metre.
        /// </summary>
        private static readonly Random randomizer = new Random();

        internal readonly ApproachCircle ApproachCircle;
        internal readonly pSprite SpriteBackground;
        internal readonly pSprite SpriteClear;
        private readonly pRectangle spriteScoreMetreBackground;
        private readonly pRectangle spriteScoreMetreForeground;
        internal readonly pSprite SpriteSpin;
        internal pSpriteText spriteBonus;
        protected pSprite spriteCircle;

        /// <summary>
        /// The fastest acceleration that is allowed (depends on length of spinner).
        /// </summary>
        private double AccelerationCap;

        /// <summary>
        /// Have we cleared the spinner?
        /// </summary>
        internal bool Cleared;

        /// <summary>
        /// Number of rotations currently spun.
        /// </summary>
        internal float currentRotationCount;

        /// <summary>
        /// Number of scored rotations (last scoring update).
        /// </summary>
        private float lastRotationCount;

        /// <summary>
        /// Number of rotations are required for a "clear".
        /// </summary>
        internal int rotationRequirement;

        /// <summary>
        /// Weighted RPM value (used for display).
        /// </summary>
        private double Rpm;

        /// <summary>
        /// Has the spinner started spinning? (used for hiding SPIN! graphic).
        /// </summary>
        private bool StartedSpinning;

        /// <summary>
        /// Velocity the spinner is visually spinning at.
        /// </summary>
        internal double velocityCurrent;

        /// <summary>
        /// Velocity the cursor is "spinning" at.
        /// </summary>
        internal double velocityFromInputPerMillisecond;

        /// <summary>
        /// Usually scoring is done at every 180 degrees. This will make it happen n times more often.
        /// </summary>
        public const int sensitivity_modifier = 16;

        Vector2 spinnerCentre = new Vector2(0, 210);

        internal Spinner(HitObjectManager hitObjectManager, int startTime, int endTime, HitObjectSoundType soundType)
            : base(hitObjectManager, Vector2.Zero, startTime, soundType, true, 0)
        {
            Position = (new Vector2(GameBase.BaseSize.Width / 2, GameBase.BaseSize.Height) - spinnerCentre) - GameBase.GamefieldOffsetVector1;
            EndTime = endTime;
            Type = HitObjectType.Spinner;
            Colour = Color4.Gray;

            Color4 white = Color4.White;

            //Check for a jpg background for beatmap-based skins (used to reduce filesize), then fallback to png.
            SpriteBackground =
                new pSprite(TextureManager.Load(OsuTexture.spinner_background),
                            FieldTypes.StandardSnapBottomCentre, OriginTypes.BottomCentre, ClockTypes.Audio,
                            new Vector2(0, 0), SpriteManager.drawOrderFwdLowPrio(StartTime + 2), false, white);
            Sprites.Add(SpriteBackground);

            spriteCircle =
                new pSprite(TextureManager.Load(OsuTexture.spinner_circle),
                            FieldTypes.StandardSnapBottomCentre, OriginTypes.Centre, ClockTypes.Audio,
                            spinnerCentre, SpriteManager.drawOrderFwdLowPrio(StartTime + 3), false, white);
            Sprites.Add(spriteCircle);

            spriteScoreMetreBackground =
                new pRectangle(Vector2.Zero, new Vector2(GameBase.BaseSizeFixedWidth.Width, GameBase.BaseSize.Height), false, SpriteManager.drawOrderFwdLowPrio(StartTime), new Color4(20, 20, 20, 255))
                {
                    Clocking = ClockTypes.Audio,
                    Field = FieldTypes.StandardSnapBottomCentre,
                    Origin = OriginTypes.BottomCentre
                };

            Sprites.Add(spriteScoreMetreBackground);

            spriteScoreMetreForeground =
                new pRectangle(Vector2.Zero, new Vector2(GameBase.BaseSizeFixedWidth.Width, 0), false, SpriteManager.drawOrderFwdLowPrio(StartTime + 1), Color4.OrangeRed)
                {
                    Clocking = ClockTypes.Audio,
                    Field = FieldTypes.StandardSnapBottomCentre,
                    Origin = OriginTypes.BottomCentre
                };

            Sprites.Add(spriteScoreMetreForeground);

            ApproachCircle = new ApproachCircle(spinnerCentre, 1, false, SpriteManager.drawOrderFwdLowPrio(StartTime + 5), new Color4(77 / 255f, 139 / 255f, 217 / 255f, 1));
            ApproachCircle.Width = 8;
            ApproachCircle.Clocking = ClockTypes.Audio;
            ApproachCircle.Field = FieldTypes.StandardSnapBottomCentre;
            Sprites.Add(ApproachCircle);

            spriteBonus = new pSpriteText("", "score", 3, // SkinManager.Current.FontScore, SkinManager.Current.FontScoreOverlap,
                                          FieldTypes.StandardSnapBottomCentre, OriginTypes.Centre, ClockTypes.Audio,
                                          spinnerCentre - new Vector2(0, 80), SpriteManager.drawOrderFwdLowPrio(StartTime + 6), false, white);
            Sprites.Add(spriteBonus);

            foreach (pDrawable p in Sprites)
            {
                p.Transformations.Clear();
                p.Transform(new Transformation(TransformationType.Fade, 0, 1, StartTime - DifficultyManager.FadeIn, StartTime));
                p.Transform(new Transformation(TransformationType.Fade, 1, 0, EndTime, EndTime + DifficultyManager.FadeOut));
                p.AlignToSprites = true;
            }

            SpriteSpin =
                new pSprite(TextureManager.Load(OsuTexture.spinner_spin),
                            FieldTypes.StandardSnapBottomCentre, OriginTypes.Centre, ClockTypes.Audio,
                            spinnerCentre, SpriteManager.drawOrderFwdLowPrio(StartTime + 5), false, white);
            SpriteSpin.Transform(new Transformation(TransformationType.Fade, 0, 1, StartTime - DifficultyManager.FadeIn / 2, StartTime));
            SpriteSpin.Transform(new Transformation(TransformationType.Fade, 1, 0, EndTime - Math.Min(400, endTime - startTime), EndTime));
            SpriteSpin.AlignToSprites = true;
            Sprites.Add(SpriteSpin);

            ApproachCircle.Transform(new Transformation(TransformationType.Scale, GameBase.BaseSizeFixedWidth.Height * 0.47f, 0.1f, StartTime, EndTime));

            SpriteClear =
                new pSprite(TextureManager.Load(OsuTexture.spinner_clear),
                            FieldTypes.StandardSnapBottomCentre, OriginTypes.Centre, ClockTypes.Audio,
                            spinnerCentre + new Vector2(0, 80), SpriteManager.drawOrderFwdLowPrio(StartTime + 6), false, white);
            SpriteClear.AlignToSprites = true;
            SpriteClear.Transform(new Transformation(TransformationType.Fade, 0, 0, startTime, endTime));
            Sprites.Add(SpriteClear);

            currentRotationCount = 0;
            rotationRequirement = (int)((float)(EndTime - StartTime) / 1000 * DifficultyManager.SpinnerRotationRatio) * sensitivity_modifier;
            AccelerationCap = 0.00008 + Math.Max(0, (5000 - (double)(EndTime - StartTime)) / 1000 / 2000);
        }

        internal override int ComboNumber
        {
            get { return 1; }
            set { }
        }

        internal override bool IsVisible
        {
            get
            {
                return Clock.AudioTime >= StartTime - DifficultyManager.FadeIn && Clock.AudioTime <= EndTime;
            }
        }

        internal override void Shake()
        {
            return;
        }

        internal override int HittableEndTime
        {
            get
            {
                return EndTime;
            }
        }

        TrackingPoint cursorTrackingPoint;
        Vector2 cursorTrackingPosition;
        internal override ScoreChange CheckScoring()
        {
            //Update the angles
            velocityFromInputPerMillisecond = 0;

            ScoreChange change = base.CheckScoring();
            if (change != ScoreChange.Ignore)
            {
                hpMultiplier = 1;
                return change;
            }

            if (!Player.Autoplay)
            {
                if (InputManager.PrimaryTrackingPoint != cursorTrackingPoint)
                {
                    cursorTrackingPoint = InputManager.PrimaryTrackingPoint;
                    return ScoreChange.Ignore;
                }

                if (cursorTrackingPoint == null || !InputManager.IsPressed)
                {
                    velocityFromInputPerMillisecond = 0;
                }
                else
                {
                    Vector2 centre = spriteCircle.FieldPosition / GameBase.BaseToNativeRatio;

                    Vector2 oldPos = cursorTrackingPosition - centre;

                    //Update to the new mouse position.
                    cursorTrackingPosition = cursorTrackingPoint.BasePosition;

                    Vector2 newPos = cursorTrackingPosition - centre;

                    double oldAngle = Math.Atan2(oldPos.Y, oldPos.X);
                    double newAngle = Math.Atan2(newPos.Y, newPos.X);

                    double angleDiff = newAngle - oldAngle;

                    if (angleDiff < -Math.PI)
                        angleDiff = (2 * Math.PI) + angleDiff;
                    else if (oldAngle - newAngle < -Math.PI)
                        angleDiff = (-2 * Math.PI) - angleDiff;

                    velocityFromInputPerMillisecond = angleDiff / GameBase.ElapsedMilliseconds;
                }
            }

            if (IsActive)
            {
                double maxAccelPerSec = AccelerationCap * GameBase.ElapsedMilliseconds;

                if (Player.Autoplay)
                    velocityCurrent = 0.05f;
                else
                    velocityCurrent = velocityFromInputPerMillisecond * 0.5f + velocityCurrent * 0.5f;

                if (velocityCurrent > 0.0001f)
                    StartSound();
                else
                    StopSound();

                //hard rate limit
                velocityCurrent = Math.Max(-0.05, Math.Min(velocityCurrent, 0.05));

                float delta = (float)(velocityCurrent * GameBase.ElapsedMilliseconds);

                spriteCircle.Rotation += delta;

                currentRotationCount += (float)(Math.Abs(delta) * sensitivity_modifier / (Math.PI * 2));
            }

            if (currentRotationCount >= rotationRequirement && !Cleared)
            {
                Cleared = true;
                if (SpriteSpin != null)
                {
                    SpriteSpin.FadeOut(100);

                    int now = Clock.GetTime(SpriteClear.Clocking);

                    SpriteClear.Transformations.Clear();
                    SpriteClear.Transform(new Transformation(TransformationType.Fade, 0, 1, now, Math.Min(EndTime, now + 400), EasingTypes.In));
                    SpriteClear.Transform(new Transformation(TransformationType.Scale, 2, 0.8f, now, Math.Min(EndTime, now + 240), EasingTypes.In));
                    SpriteClear.Transform(new Transformation(TransformationType.Scale, 0.8f, 1, Math.Min(EndTime, now + 240), Math.Min(EndTime, now + 400), EasingTypes.None));
                    SpriteClear.Transform(new Transformation(TransformationType.Fade, 1, 0, EndTime - 50, EndTime));
                }
            }

            ScoreChange score = ScoreChange.Ignore;

            //Update the rotation count
            if (currentRotationCount != lastRotationCount)
            {
                hpMultiplier = Math.Max(1, (int)(currentRotationCount - lastRotationCount));

                if (currentRotationCount > rotationRequirement + 3 * sensitivity_modifier)
                {
                    score = ScoreChange.SpinnerBonus;

                    int now = spriteBonus.ClockingNow;

                    spriteBonus.Transformations.Clear();

                    if (currentRotationCount - lastSamplePlayedRotationCount > sensitivity_modifier)
                    {
                        hpMultiplier = 50;
                        AudioEngine.PlaySample(OsuSamples.SpinnerBonus, SampleSet, Volume);
                        spriteBonus.Transform(new Transformation(TransformationType.Scale, 2F, 1.28f, now, now + 800, EasingTypes.In));
                        lastSamplePlayedRotationCount = currentRotationCount;
                    }

                    BonusScore += hpMultiplier;

                    spriteBonus.ShowInt((int)BonusScore);

                    spriteBonus.Transform(new Transformation(TransformationType.Fade, 1, 0, now, now + 800, EasingTypes.In));

                    //Ensure we don't recycle this too early.
                    spriteBonus.Transform(new Transformation(TransformationType.Fade, 0, 0, EndTime + 800, EndTime + 800));
                }
                else if (currentRotationCount - lastScoredRotationCount > sensitivity_modifier / 4)
                {
                    score = ScoreChange.SpinnerSpinPoints;
                    lastScoredRotationCount = currentRotationCount;
                }
            }

            lastRotationCount = currentRotationCount;

            return score;
        }

        internal float BonusScore;
        float hpMultiplier = 1;
        public override float HpMultiplier
        {
            get
            {
                return hpMultiplier;
            }
        }

        float lastSamplePlayedRotationCount;
        float lastScoredRotationCount;

        public override void Update()
        {
            base.Update();

            if (IsHit || Clock.AudioTime < StartTime)
                return;

            if (sourceSpinning != null)
                sourceSpinning.Pitch = 0.5f + 0.5f * currentRotationCount / rotationRequirement;

            Rpm = Rpm * 0.9 + 0.1 * (Math.Abs(velocityCurrent) * 60000) / (Math.PI * 2);

            SetScoreMeter((int)(currentRotationCount / rotationRequirement * 100));
            
            //Hide the "SPIN!" sprite once we have started spinning.
            if (currentRotationCount > 0 && !StartedSpinning)
            {
                if (SpriteSpin != null)
                {
                    if (Clock.AudioTime > StartTime + 500)
                    {
                        SpriteSpin.FadeOut(300);
                        StartedSpinning = true;
                    }
                }
            }
        }

        Source sourceSpinning;

        private void StartSound()
        {
            if (sourceSpinning == null)
                sourceSpinning = AudioEngine.Effect.PlayBuffer(AudioEngine.LoadSample(OsuSamples.SpinnerSpin), 1, true, true);
            else
                sourceSpinning.Play();
        }

        internal override void StopSound()
        {
            if (sourceSpinning != null)
                sourceSpinning.Stop();
        }

        private void SetScoreMeter(int percent)
        {
            percent = Math.Min(99, percent);

            int randomAmount = percent % 10;
            int barCount = percent / 10;


            if (randomizer.NextDouble() < (float)randomAmount / 10) // || SkinManager.Current.SpinnerNoBlink)
                barCount++;

            spriteScoreMetreForeground.Scale.Y = 42.6f * barCount;
        }

        internal override bool HitTestInitial(TrackingPoint tracking)
        {
            return false;
        }

        protected override ScoreChange HitActionInitial()
        {
            if (Clock.AudioTime < EndTime)
                return ScoreChange.Ignore;

            StopSound();
            if (sourceSpinning != null)
                sourceSpinning.Reserved = false;

            ScoreChange val = ScoreChange.Miss;
            if (currentRotationCount > rotationRequirement + 1)
                val = ScoreChange.Hit300;
            else if (currentRotationCount > rotationRequirement)
                val = ScoreChange.Hit100;
            else if (currentRotationCount > rotationRequirement - 1)
                val = ScoreChange.Hit50;
            if (val > 0)
                PlaySound();
            return val;
        }
    }
}
