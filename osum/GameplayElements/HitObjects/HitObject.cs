using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using osum.Graphics.Sprites;
using osum.Helpers;
using osum.Support;
using osum.Audio;
using osum.Graphics.Skins;
using osum.GameModes;

namespace osum.GameplayElements
{
    internal delegate void HitCircleDelegate(HitObject h);

    [Flags]
    public enum HitObjectType
    {
        Circle = 1,
        Slider = 2,
        NewCombo = 4,
        NormalNewCombo = 5,
        SliderNewCombo = 6,
        Spinner = 8
    }

    [Flags]
    public enum HitObjectSoundType
    {
        Normal = 0,
        Whistle = 2,
        Finish = 4,
        WhistleFinish = 6,
        Clap = 8
    }

    [Flags]
    internal enum ScoreChange
    {
        MissHpOnlyNoCombo = -524288,
        MissMinor = -262144,
        Miss = -131072,
        Ignore = 0,
        MuAddition = 1,
        KatuAddition = 2,
        GekiAddition = 4,
        SliderTick = 8,
        FruitTickTiny = 16,
        FruitTickTinyMiss = 32,
        SliderRepeat = 64,
        SliderEnd = 128,
        Hit50 = 256,
        Hit100 = 512,
        Hit300 = 1024,
        Hit50m = Hit50 | MuAddition,
        Hit100m = Hit100 | MuAddition,
        Hit300m = Hit300 | MuAddition,
        Hit100k = Hit100 | KatuAddition,
        Hit300k = Hit300 | KatuAddition,
        Hit300g = Hit300 | GekiAddition,
        FruitTick = 2048,
        SpinnerSpin = 4096,
        SpinnerSpinPoints = 8192,
        SpinnerBonus = 16384,
        TaikoDrumRoll = 32768,
        TaikoLargeHitBoth = 65536,
        TaikoDenDenHit = 1048576,
        TaikoDenDenComplete = 2097152,
        TaikoLargeHitFirst = 4194304,
        TaikoLargeHitSecond = 8388608,
        Shake = 16777216,
        HitValuesOnly = Hit50 | Hit100 | Hit300 | GekiAddition | KatuAddition,
        ComboAddition = MuAddition | KatuAddition | GekiAddition,
        NonScoreModifiers = TaikoLargeHitBoth | TaikoLargeHitFirst | TaikoLargeHitSecond
    }

    public enum Difficulty
    {
        None = -1,
        Easy = 0,
        Normal = 1,
        Hard = 2,
        Expert = 3
    }

    internal abstract class HitObject : pSpriteCollection, IComparable<HitObject>, IComparable<int>, IUpdateable
    {
        protected HitObjectManager m_HitObjectManager;

        public HitObject(HitObjectManager hitObjectManager, Vector2 position, int startTime, HitObjectSoundType soundType, bool newCombo, int comboOffset)
        {
            m_HitObjectManager = hitObjectManager;
            this.position = position;
            StartTime = startTime;
            EndTime = StartTime;
            SoundType = soundType;
            NewCombo = newCombo;
            ComboOffset = comboOffset;
        }

        #region General & Timing

        internal int StartTime;
        internal int EndTime;

        internal ScoreChange hitValue;

        internal HitObjectType Type;

        internal int Index;
        internal int ComboOffset;

        internal HitObject connectedObject;
        internal pSprite connectionSprite;

        /// <summary>
        /// Do any arbitrary updates for this hitObject.
        /// </summary>
        public virtual void Update()
        {
            UpdateDimming();
        }

        bool isDimmed;

        //todo: this is horribly memory inefficient.
        private void UpdateDimming()
        {
            bool shouldDim = Clock.AudioTime < StartTime &&
                Math.Abs(StartTime - Clock.AudioTime) > m_HitObjectManager.FirstBeatLength;

            if (shouldDim != isDimmed)
            {
                isDimmed = shouldDim;

                if (isDimmed)
                {
                    foreach (pDrawable p in SpriteCollectionDim)
                        p.FadeColour(ColourHelper.Darken(p.Colour, 0.3f), 0);
                }
                else
                {
                    foreach (pDrawable p in SpriteCollectionDim)
                        p.FadeColour(ColourHelper.Lighten(p.Colour, 0.7f), (int)m_HitObjectManager.FirstBeatLength);
                }
            }
        }

        internal virtual bool NewCombo { get; set; }

        private Color4 colour;
        internal virtual Color4 Colour
        {
            get
            {
                return colour;
            }

            set
            {
                colour = value;

                float dimFactor = 0.75f;
                ColourDim = new Color4(colour.R * dimFactor, colour.G * dimFactor, colour.B * dimFactor, 255);
            }
        }

        private int colour_index;
        internal virtual int ColourIndex
        {
            get
            {
                return colour_index;
            }
            set
            {
                if (value >= 4) throw new ArgumentOutOfRangeException();
                colour_index = value;
                Colour = TextureManager.DefaultColours[value];
            }
        }

        internal virtual bool IsHit { get; set; }

        /// <summary>
        /// This will cause the hitObject to get hit and scored.
        /// </summary>
        /// <returns>
        /// A <see cref="ScoreChange"/> representing what action was taken.
        /// </returns>
        internal ScoreChange Hit()
        {
            if (Clock.AudioTime < StartTime - 200)
            {
                Shake();
                return ScoreChange.Ignore;
            }

            if (IsHit)
                return ScoreChange.Ignore;

            ScoreChange action = HitActionInitial();

            if (action != ScoreChange.Ignore)
                IsHit = true;

            return action;
        }

        /// <summary>
        /// This is called every frame that this object is visible to pick up any intermediary scoring that is not associated with the initial hit.
        /// </summary>
        /// <returns></returns>
        internal virtual ScoreChange CheckScoring()
        {
            if (IsHit)
                return ScoreChange.Ignore;

            //check for miss
            if (Clock.AudioTime > (Player.Autoplay ? StartTime : HittableEndTime))
                return Hit(); //force a "hit" if we haven't yet.

            return ScoreChange.Ignore;
        }

        /// <summary>
        /// Trigger a hit animation showing the score overlay above the object.
        /// </summary>
        /// <param name="action">The ssociated score change action.</param>
        internal virtual void HitAnimation(ScoreChange action)
        {
            if (m_HitObjectManager == null) return; //is the case for sliders, where we don't want to display this stuff.

            float depth;
            //todo: should this be changed?
            if (this is Spinner)
                depth = SpriteManager.drawOrderBwd(EndTime - 4);
            else
                depth = SpriteManager.drawOrderFwdPrio(EndTime - 4);

            OsuTexture texture = OsuTexture.None;

            switch (action & ScoreChange.HitValuesOnly)
            {
                case ScoreChange.Hit300g:
                    texture = OsuTexture.hit300g;
                    break;
                case ScoreChange.Hit300k:
                    texture = OsuTexture.hit300k;
                    break;
                case ScoreChange.Hit300:
                    texture = OsuTexture.hit300;
                    break;
                case ScoreChange.Hit100k:
                    texture = OsuTexture.hit100k;
                    break;
                case ScoreChange.Hit100:
                    texture = OsuTexture.hit100;
                    break;
                case ScoreChange.Hit50:
                    texture = OsuTexture.hit50;
                    break;
                default:
                    if (action == ScoreChange.Miss)
                        texture = OsuTexture.hit0;
                    break;
            }

            if (texture == OsuTexture.None)
                return;

            //Draw the hit value
            pSprite p =
                new pSprite(TextureManager.Load(texture),
                            FieldTypes.GamefieldSprites,
                            OriginTypes.Centre,
                            ClockTypes.Game, EndPosition, depth, false, Color4.White);
            m_HitObjectManager.spriteManager.Add(p);

            const int HitFadeIn = 120;
            const int HitFadeOut = 600;
            const int PostEmpt = 500;

            if (action > 0)
            {
                p.Transform(
                    new TransformationBounce(Clock.Time, (int)(Clock.Time + (HitFadeIn * 1.4)), 1, 0.3f, 3));
                p.Transform(
                    new Transformation(TransformationType.Fade, 1, 0,
                                       Clock.Time + PostEmpt, Clock.Time + PostEmpt + HitFadeOut));
            }
            else
            {
                p.Transform(
                            new Transformation(TransformationType.Scale, 2, 1, Clock.Time,
                                               Clock.Time + HitFadeIn));
                p.Transform(
                    new Transformation(TransformationType.Fade, 1, 0, Clock.Time + PostEmpt,
                                       Clock.Time + PostEmpt + HitFadeOut));

                p.Transform(
                    new Transformation(TransformationType.Rotation, 0,
                                       (float)((GameBase.Random.NextDouble() - 0.5) * 0.2), Clock.Time,
                                       Clock.Time + HitFadeIn));
            }

        }

        /// <summary>
        /// Internal judging of a Hit() call. Is only called after preliminary checks have been completed.
        /// </summary>
        /// <returns>
        /// A <see cref="ScoreChange"/>
        /// </returns>
        protected abstract ScoreChange HitActionInitial();

        internal virtual void Dispose()
        {

        }

        /// <summary>
        /// Is this object currently within an active range?
        /// </summary>
        internal virtual bool IsActive
        {
            get { return !IsHit; }
        }

        #endregion

        #region Drawing

        /// <summary>
        /// Sprites which should be dimmed when not the active object.
        /// </summary>
        protected internal List<pDrawable> SpriteCollectionDim = new List<pDrawable>();

        protected Vector2 position;
        internal virtual Vector2 Position
        {
            get { return position; }
            set
            {
                position = value;
                Sprites.ForEach(s => { s.StartPosition = value; s.Position = value; });
            }
        }

        internal virtual Vector2 EndPosition
        {
            get { return Position; }
            set { throw new NotImplementedException(); }
        }

        internal int StackCount;

        internal virtual int ComboNumber { get; set; }

        /// <summary>
        /// Id this hitObject visible at the current audio time?
        /// </summary>
        internal abstract bool IsVisible { get; }

        #endregion

        #region Sound

        internal Color4 ColourDim;
        internal HitObjectSoundType SoundType;

        /// <summary>
        /// Whether to add this object's score to the counters (hit300 count etc.)
        /// </summary>
        public bool IsScorable = true;

        internal bool Whistle
        {
            get { return (HitObjectSoundType.Whistle & SoundType) > 0; }
            set
            {
                if (value)
                    SoundType |= HitObjectSoundType.Whistle;
                else
                    SoundType &= ~HitObjectSoundType.Whistle;
            }
        }


        internal bool Finish
        {
            get { return (HitObjectSoundType.Finish & SoundType) > 0; }
            set
            {
                if (value)
                    SoundType |= HitObjectSoundType.Finish;
                else
                    SoundType &= ~HitObjectSoundType.Finish;
            }
        }

        internal bool Clap
        {
            get { return (HitObjectSoundType.Clap & SoundType) > 0; }
            set
            {
                if (value)
                    SoundType |= HitObjectSoundType.Clap;
                else
                    SoundType &= ~HitObjectSoundType.Clap;
            }
        }

        internal virtual void PlaySound()
        {
            PlaySound(SoundType);
        }

        internal virtual void PlaySound(HitObjectSoundType type)
        {

            //HitObjectManager.OnHitSound(SoundType);

            if ((type & HitObjectSoundType.Finish) > 0)
                AudioEngine.PlaySample(OsuSamples.HitFinish);
            //AudioEngine.PlaySample(AudioEngine.s_HitFinish, AudioEngine.VolumeSample, 0, PositionalSound);

            if ((type & HitObjectSoundType.Whistle) > 0)
                AudioEngine.PlaySample(OsuSamples.HitWhistle);

            if ((type & HitObjectSoundType.Clap) > 0)
                AudioEngine.PlaySample(OsuSamples.HitClap);

            //if (SkinManager.Current.LayeredHitSounds || SoundType == HitObjectSoundType.Normal)
            AudioEngine.PlaySample(OsuSamples.HitNormal);

        }

        protected virtual float PositionalSound { get { return Position.X / GameBase.GamefieldBaseSize.Width - 0.5f; } }

        /// <summary>
        /// Gets the hittable end time (valid active object time for sliders etc. - used in taiko to extend when hits are valid).
        /// </summary>
        /// <value>The hittable end time.</value>
        internal virtual int HittableEndTime
        {
            get { return EndTime + DifficultyManager.HitWindow50; }
        }

        /// <summary>
        /// Gets the hittable end time (valid active object time for sliders etc. - used in taiko to extend when hits are valid).
        /// </summary>
        /// <value>The hittable end time.</value>
        internal virtual int HittableStartTime
        {
            get { return StartTime; }
        }

        #endregion

        #region IComparable<HitObject> Members

        public int CompareTo(HitObject other)
        {
            return StartTime.CompareTo(other.StartTime);
        }

        public int CompareTo(int time)
        {
            return StartTime.CompareTo(time);
        }

        #endregion

        public virtual bool IncrementCombo { get { return true; } }


        internal virtual void StopSound()
        {
        }

        internal virtual bool HitTestInitial(TrackingPoint tracking)
        {
            float radius = DifficultyManager.HitObjectRadiusSolidGamefieldHittable;

            return (IsVisible &&
                    StartTime - DifficultyManager.PreEmpt <= Clock.AudioTime &&
                    StartTime + DifficultyManager.HitWindow50 >= Clock.AudioTime &&
                    !IsHit &&
                    pMathHelper.DistanceSquared(tracking.GamefieldPosition, Position) <= radius * radius);
        }

        const int TAG_SHAKE_TRANSFORMATION = 54327;
        public Difficulty Difficulty;

        internal virtual void Shake()
        {
            foreach (pDrawable p in SpriteCollectionDim)
            {
                Transformation previousShake = p.Transformations.FindLast(t => t.Tag == TAG_SHAKE_TRANSFORMATION);

                float pos = previousShake != null ? previousShake.EndFloat : p.Position.X;

                const int shake_count = 6;
                const int shake_velocity = 8;
                const int shake_period = 40;

                for (int i = 0; i < shake_count; i++)
                {
                    int s = i == 0 ? 0 : shake_velocity;
                    if (i % 2 == 0) s = -s;

                    int e = i == shake_count - 1 ? 0 : -s;

                    p.Transform(new Transformation(TransformationType.MovementX, pos + s, pos + e,
                        Clock.AudioTime + i * shake_period, Clock.AudioTime + (i + 1) * shake_period) { Tag = TAG_SHAKE_TRANSFORMATION });
                }
            }

            if (connectedObject != null)
            {
                connectedObject.connectedObject = null;
                connectedObject.Shake();
                connectedObject.connectedObject = this;
            }

        }

        public override string ToString()
        {
            return this.Type + ": " + this.StartTime + "-" + this.EndTime + " stack:" + this.StackCount;
        }
    }
}
