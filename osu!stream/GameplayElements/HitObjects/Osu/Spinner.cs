using System;
using OpenTK;
using OpenTK.Graphics;
using osum.Audio;
using osum.GameModes.Play;
using osum.Graphics;
using osum.Graphics.Drawables;
using osum.Graphics.Sprites;
using osum.Helpers;
using osum.Input;

namespace osum.GameplayElements.HitObjects.Osu
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
        internal double currentRotationCount;

        /// <summary>
        /// Number of scored rotations (last scoring update).
        /// </summary>
        private double lastRotationCount;

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

        public static Vector2 SpinnerCentreFromBottom = new Vector2(0, 210);
        public static Vector2 SpinnerCentre;

        internal Spinner(HitObjectManager hitObjectManager, int startTime, int endTime, HitObjectSoundType soundType)
            : base(hitObjectManager, Vector2.Zero, startTime, soundType, true, 0)
        {
            if (SpinnerCentre == Vector2.Zero)
                SpinnerCentre = GameBase.StandardToGamefield(new Vector2(GameBase.BaseSizeFixedWidth.X / 2, GameBase.BaseSizeFixedWidth.Y - (GameBase.BaseSizeFixedWidth.Y / GameBase.BaseSize.Y) * SpinnerCentreFromBottom.Y));

            Position = SpinnerCentre;
            EndTime = endTime;
            Type = HitObjectType.Spinner;
            Colour = Color4.Gray;

            Color4 white = Color4.White;

            //Check for a jpg background for beatmap-based skins (used to reduce filesize), then fallback to png.
            SpriteBackground =
                new pSprite(TextureManager.Load(OsuTexture.spinner_background),
                            FieldTypes.StandardSnapBottomCentre, OriginTypes.BottomCentre, ClockTypes.Audio,
                            new Vector2(0, 0), SpriteManager.drawOrderFwdLowPrio(2), false, white);
            Sprites.Add(SpriteBackground);

            spriteCircle =
                new pSprite(TextureManager.Load(OsuTexture.spinner_circle),
                            FieldTypes.StandardSnapBottomCentre, OriginTypes.Centre, ClockTypes.Audio,
                            SpinnerCentreFromBottom, SpriteManager.drawOrderFwdLowPrio(3), false, white) { ExactCoordinates = false };
            Sprites.Add(spriteCircle);

            //todo: possible optimisation by changing the draw method for filling of spinner metres.
            spriteScoreMetreBackground =
                new pRectangle(Vector2.Zero, new Vector2(GameBase.BaseSizeFixedWidth.X, GameBase.BaseSize.Y), false, SpriteManager.drawOrderFwdLowPrio(0), new Color4(20, 20, 20, 255))
                {
                    Clocking = ClockTypes.Audio,
                    Field = FieldTypes.StandardSnapBottomCentre,
                    Origin = OriginTypes.BottomCentre
                };

            Sprites.Add(spriteScoreMetreBackground);

            spriteScoreMetreForeground =
                new pRectangle(Vector2.Zero, new Vector2(GameBase.BaseSizeFixedWidth.X, 0), false, SpriteManager.drawOrderFwdLowPrio(1), Color4.OrangeRed)
                {
                    Clocking = ClockTypes.Audio,
                    Field = FieldTypes.StandardSnapBottomCentre,
                    Origin = OriginTypes.BottomCentre
                };

            Sprites.Add(spriteScoreMetreForeground);

            ApproachCircle = new ApproachCircle(SpinnerCentreFromBottom, 1, false, SpriteManager.drawOrderFwdLowPrio(5), new Color4(77 / 255f, 139 / 255f, 217 / 255f, 1));
            ApproachCircle.Width = 8;
            ApproachCircle.Clocking = ClockTypes.Audio;
            ApproachCircle.Field = FieldTypes.StandardSnapBottomCentre;
            Sprites.Add(ApproachCircle);

            spriteBonus = new pSpriteText("", "score", -5, // SkinManager.Current.FontScore, SkinManager.Current.FontScoreOverlap,
                                          FieldTypes.StandardSnapBottomCentre, OriginTypes.Centre, ClockTypes.Audio,
                                          SpinnerCentreFromBottom - new Vector2(0, 80), SpriteManager.drawOrderFwdLowPrio(6), false, white);
            spriteBonus.Additive = true;
            Sprites.Add(spriteBonus);

            foreach (pDrawable p in Sprites)
            {
                p.Transformations.Clear();
                p.Transform(new TransformationF(TransformationType.Fade, 0, 1, StartTime - DifficultyManager.FadeIn, StartTime));
                p.Transform(new TransformationF(TransformationType.Fade, 1, 0, EndTime, EndTime + (spriteScoreMetreForeground == p ? DifficultyManager.FadeOut / 4 : DifficultyManager.FadeOut / 2)));
            }

            SpriteSpin =
                new pSprite(TextureManager.Load(OsuTexture.spinner_spin),
                            FieldTypes.StandardSnapBottomCentre, OriginTypes.Centre, ClockTypes.Audio,
                            SpinnerCentreFromBottom, SpriteManager.drawOrderFwdLowPrio(5), false, white);
            SpriteSpin.Transform(new TransformationF(TransformationType.Fade, 0, 1, StartTime - DifficultyManager.FadeIn / 2, StartTime));
            SpriteSpin.Transform(new TransformationF(TransformationType.Fade, 1, 0, EndTime - Math.Min(400, endTime - startTime), EndTime));
            Sprites.Add(SpriteSpin);

            ApproachCircle.Transform(new TransformationF(TransformationType.Scale, GameBase.BaseSizeFixedWidth.Y * 0.47f, 0.1f, StartTime, EndTime));

            SpriteClear =
                new pSprite(TextureManager.Load(OsuTexture.spinner_clear),
                            FieldTypes.StandardSnapBottomCentre, OriginTypes.Centre, ClockTypes.Audio,
                            SpinnerCentreFromBottom + new Vector2(0, 80), SpriteManager.drawOrderFwdLowPrio(6), false, white);
            SpriteClear.Transform(new TransformationF(TransformationType.Fade, 0, 0, startTime, endTime));
            Sprites.Add(SpriteClear);

            currentRotationCount = 0;
            rotationRequirement = (int)((float)(EndTime - StartTime) / 1000 * DifficultyManager.SpinnerRotationRatio) * sensitivity_modifier;
            AccelerationCap = 0.00008 + Math.Max(0, (5000 - (double)(EndTime - StartTime)) / 1000 / 2000);
        }

        internal override int ComboNumber
        {
            get => 1;
            set { }
        }

        public override bool IncrementCombo => false;

        public override bool IsVisible
        {
            get
            {
                int now = ClockingNow;
                return now >= StartTime - DifficultyManager.FadeIn && now <= EndTime + DifficultyManager.FadeOut / 2;
            }
        }

        internal override void Shake()
        {
        }

        internal override int HittableEndTime => EndTime;

        private TrackingPoint cursorTrackingPoint;
        private Vector2 cursorTrackingPosition;

        private int lastScoreCheckTime;
        internal override ScoreChange CheckScoring()
        {
            //Update the angles
            ScoreChange change = base.CheckScoring();
            if (change != ScoreChange.Ignore)
            {
                hpMultiplier = 1;
                return change;
            }

            int now = ClockingNow;
            int elapsed = lastScoreCheckTime == 0 ? 0 : now - lastScoreCheckTime;
            lastScoreCheckTime = now;

            if (!Player.Autoplay)
            {
                velocityFromInputPerMillisecond = 0;

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

                    if (angleDiff < -MathHelper.Pi)
                        angleDiff = (2 * MathHelper.Pi) + angleDiff;
                    else if (oldAngle - newAngle < -MathHelper.Pi)
                        angleDiff = (-2 * MathHelper.Pi) - angleDiff;

                    velocityFromInputPerMillisecond = angleDiff / elapsed;
                }
            }

            if (IsActive)
            {
                if (Player.Autoplay)
                    velocityCurrent = 0.03;
                else
                    velocityCurrent = velocityFromInputPerMillisecond * (0.5f * Clock.ElapsedRatioToSixty) + velocityCurrent * (1 - 0.5 * Clock.ElapsedRatioToSixty);

                if (Math.Abs(velocityCurrent) > 0.0001f)
                {
                    if (sourceSpinning == null || !sourceSpinning.Playing)
                        StartSound();
                }
                else
                    StopSound(false);

                //hard rate limit
                velocityCurrent = Math.Max(-0.05, Math.Min(velocityCurrent, 0.05));

                double delta = velocityCurrent * elapsed;

                spriteCircle.Rotation += (float)delta;

                currentRotationCount += Math.Abs(delta) * sensitivity_modifier / (MathHelper.Pi * 2);
            }

            if (currentRotationCount >= rotationRequirement && !Cleared)
            {
                Cleared = true;

                if (SpriteSpin != null)
                {
                    SpriteSpin.FadeOut(100);

                    SpriteClear.Transformations.Clear();
                    SpriteClear.Transform(new TransformationF(TransformationType.Fade, 0, 1, now, Math.Min(EndTime, now + 400), EasingTypes.In));
                    SpriteClear.Transform(new TransformationF(TransformationType.Scale, 2, 0.8f, now, Math.Min(EndTime, now + 240), EasingTypes.In));
                    SpriteClear.Transform(new TransformationF(TransformationType.Scale, 0.8f, 1, Math.Min(EndTime, now + 240), Math.Min(EndTime, now + 400)));
                    SpriteClear.Transform(new TransformationF(TransformationType.Fade, 1, 0, EndTime - 50, EndTime));
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

                    spriteBonus.ShowInt((int)BonusScore);

                    if (currentRotationCount - lastSamplePlayedRotationCount > sensitivity_modifier)
                    {
                        hpMultiplier = 50;
                        AudioEngine.PlaySample(OsuSamples.SpinnerBonus, SampleSet.SampleSet, SampleSet.Volume);

                        spriteBonus.Transformations.Clear();
                        spriteBonus.Transform(new TransformationF(TransformationType.Scale, 2F, 1.28f, now, now + 600, EasingTypes.In));
                        spriteBonus.Transform(new TransformationF(TransformationType.Fade, 1, 0, now, now + 600, EasingTypes.Out));
                        //Ensure we don't recycle this too early.
                        spriteBonus.Transform(new TransformationF(TransformationType.Fade, 0, 0, EndTime + 800, EndTime + 800));

                        lastSamplePlayedRotationCount += sensitivity_modifier;
                    }
                    else
                    {
                        hpMultiplier *= 10;
                    }

                    BonusScore += hpMultiplier;
                }
                else if (currentRotationCount - lastScoredRotationCount > sensitivity_modifier / 4)
                {
                    score = ScoreChange.SpinnerSpinPoints;
                    lastScoredRotationCount += sensitivity_modifier / 4;
                    lastSamplePlayedRotationCount = currentRotationCount;
                }
            }

            lastRotationCount = currentRotationCount;

            return score;
        }

        internal float BonusScore;
        private float hpMultiplier = 1;
        public override float HpMultiplier => hpMultiplier;

        private double lastSamplePlayedRotationCount;
        private double lastScoredRotationCount;

        public override void Update()
        {
            base.Update();

            int now = ClockingNow;

            if (IsHit || now < StartTime)
                return;

            if (sourceSpinning != null)
                sourceSpinning.Pitch = 0.5f + 0.5f * (float)(currentRotationCount / rotationRequirement);

            Rpm = Rpm * 0.9 + 0.1 * (Math.Abs(velocityCurrent) * 60000) / (MathHelper.Pi * 2);

            SetScoreMeter((int)(currentRotationCount / rotationRequirement * 100));

            //Hide the "SPIN!" sprite once we have started spinning.
            if (currentRotationCount > 0 && !StartedSpinning)
            {
                if (SpriteSpin != null)
                {
                    if (now > StartTime + 500)
                    {
                        SpriteSpin.FadeOut(300);
                        StartedSpinning = true;
                    }
                }
            }
        }

        private Source sourceSpinning;

        private void StartSound()
        {
            if (AudioEngine.Effect != null)
            {
                if (sourceSpinning == null || sourceSpinning.BufferId == 0)
                    sourceSpinning = AudioEngine.Effect.LoadBuffer(AudioEngine.LoadSample(OsuSamples.SpinnerSpin), 1, true, true);
                if (sourceSpinning != null)
                    sourceSpinning.Play();
            }
        }

        internal override void StopSound(bool done = true)
        {
            if (sourceSpinning != null && sourceSpinning.Reserved)
            {
                sourceSpinning.Stop();
                if (done)
                {
                    sourceSpinning.Reserved = false;
                    sourceSpinning = null;
                }
            }
        }

        private void SetScoreMeter(int percent)
        {
            percent = Math.Min(99, percent);

            int randomAmount = percent % 9;
            int barCount = percent / 9;


            if (randomizer.NextDouble() < (float)randomAmount / 9) // || SkinManager.Current.SpinnerNoBlink)
                barCount++;

            spriteScoreMetreForeground.Scale.Y = 38.73f * barCount;
        }

        internal override bool HitTestInitial(TrackingPoint tracking)
        {
            return false;
        }

        protected override ScoreChange HitActionInitial()
        {
            if (ClockingNow < EndTime)
                return ScoreChange.Ignore;

            if (Cleared)
                //a quick hack to reduce fill during fade out. we have cleared so most of the bar is going to be orange already.
                spriteScoreMetreBackground.Bypass = true;

            StopSound();

            ScoreChange val = ScoreChange.Miss;
            if (currentRotationCount > rotationRequirement + 1)
                val = ScoreChange.Hit300;
            else if (currentRotationCount > rotationRequirement)
                val = ScoreChange.Hit100;
            else if (currentRotationCount > rotationRequirement - 1)
                val = ScoreChange.Hit50;

            if (val > ScoreChange.Miss)
                PlaySound();
            return val;
        }
    }
}
