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

namespace osum.GameplayElements
{
    internal delegate void HitCircleDelegate(HitObject h);

    [Flags]
    internal enum HitObjectType
    {
        Circle = 1,
        Slider = 2,
        NewCombo = 4,
        NormalNewCombo = 5,
        SliderNewCombo = 6,
        Spinner = 8
    }

    [Flags]
    internal enum HitObjectSoundType
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
        MissHpOnly = -262144,
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

    internal abstract class HitObject : pSpriteCollection, IComparable<HitObject>, IComparable<int>, IUpdateable
    {
        protected HitObjectManager m_HitObjectManager;

        public HitObject(HitObjectManager hitObjectManager, Vector2 position, int startTime, HitObjectSoundType soundType, bool newCombo)
        {
            m_HitObjectManager = hitObjectManager;
            this.position = position;
            StartTime = startTime;
            EndTime = StartTime;
            SoundType = soundType;
            NewCombo = newCombo;
        }

        #region General & Timing

        internal int StartTime;
        internal int EndTime;

        internal ScoreChange hitValue;

        internal HitObjectType Type;

        /// <summary>
        /// Do any arbitrary updates for this hitObject.
        /// </summary>
        public virtual void Update()
        {
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
                Colour = SkinManager.DefaultColours[value];
            }
        }

        internal bool IsHit { get; private set; }

        /// <summary>
        /// This will cause the hitObject to get hit and scored.
        /// </summary>
        /// <returns>
        /// A <see cref="ScoreChange"/> representing what action was taken.
        /// </returns>
        internal ScoreChange Hit()
        {
            if (Clock.AudioTime < StartTime - 400)
            {
                Shake();
                return ScoreChange.Shake;
            }

            if (IsHit)
                return ScoreChange.Ignore;

            ScoreChange action = HitAction();

            if (action != ScoreChange.Ignore)
            {
                IsHit = true;
                HitAnimation(action);
            }

            return action;
        }

        /// <summary>
        /// This is called every frame that this object is visible to pick up any intermediary scoring that is not associated with the initial hit.
        /// </summary>
        /// <returns></returns>
        internal virtual ScoreChange CheckScoring()
        {
            //check for miss
            if (Clock.AudioTime > HittableEndTime)
                return Hit(); //force a "hit" if we haven't yet.

            return ScoreChange.Ignore;
        }

        /// <summary>
        /// Trigger a hit animation showing the score overlay above the object.
        /// </summary>
        /// <param name="action">The ssociated score change action.</param>
        protected virtual void HitAnimation(ScoreChange action)
        {
            if (m_HitObjectManager == null) return; //is the case for sliders, where we don't want to display this stuff.

            float depth;
            //todo: should this be changed?
            if (this is Spinner)
                depth = SpriteManager.drawOrderBwd(EndTime - 4);
            else
                depth = SpriteManager.drawOrderFwdPrio(EndTime - 4);

            string spriteName;
            string specialAddition = "";

            switch (action & ScoreChange.HitValuesOnly)
            {
                case ScoreChange.Hit100:
                    spriteName = "hit100";
                    break;
                case ScoreChange.Hit300:
                    spriteName = "hit300";
                    break;
                case ScoreChange.Hit50:
                    spriteName = "hit50";
                    break;
                case ScoreChange.Hit100k:
                    spriteName = "hit100k";
                    break;
                case ScoreChange.Hit300g:
                    spriteName = "hit300g";
                    break;
                case ScoreChange.Hit300k:
                    spriteName = "hit300k";
                    break;
                case ScoreChange.Miss:
                    spriteName = "hit0";
                    break;
                default:
                    spriteName = string.Empty;
                    break;
            }

            if (action < 0)
                spriteName = "hit0"; //todo: this sounds bad

            //Draw the hit value
            pSprite p =
                new pSprite(SkinManager.Load(spriteName + specialAddition),
                            FieldTypes.Gamefield512x384,
                            OriginTypes.Centre,
                            ClockTypes.Game, EndPosition, depth, false, Color4.White);
            m_HitObjectManager.spriteManager.Add(p);

            int HitFadeIn = 120;
            int HitFadeOut = 600;
            int PostEmpt = 500;

            if (action > 0)
            {
                p.Transform(
                    new Transformation(TransformationType.Scale, 0.6F, 1.1F, Clock.Time,
                                       (int)(Clock.Time + (HitFadeIn * 0.8))));

                p.Transform(
                    new Transformation(TransformationType.Fade, 0, 1, Clock.Time,
                                       Clock.Time + HitFadeIn));

                p.Transform(
                    new Transformation(TransformationType.Scale, 1.1F, 0.9F, Clock.Time + HitFadeIn,
                                       (int)(Clock.Time + (HitFadeIn * 1.2))));
                p.Transform(
                    new Transformation(TransformationType.Scale, 0.9F, 1F, Clock.Time + HitFadeIn,
                                       (int)(Clock.Time + (HitFadeIn * 1.4))));

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
        protected abstract ScoreChange HitAction();

        internal virtual void Dispose()
        {
        }

        #endregion

        #region Drawing

        /// <summary>
        /// Sprites which should be dimmed when not the active object.
        /// </summary>
        protected internal List<pSprite> DimCollection = new List<pSprite>();

        protected Vector2 position;
        internal virtual Vector2 Position
        {
            get { return position; }
            set
            {
                position = value;
                SpriteCollection.ForEach(s => { s.StartPosition = value; s.Position = value; });
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
        internal bool Dimmed;
        internal bool Sounded;
        internal HitObjectSoundType SoundType;
        /// <summary>
        /// Whether to add this object's score to the counters (hit300 count etc.)
        /// </summary>
        public bool IsScorable = true;
        public int TagNumeric;
        public int scoreValue;
        public bool LastInCombo;

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

            if ((SoundType & HitObjectSoundType.Finish) > 0)
                AudioEngine.PlaySample(OsuSamples.HitFinish);
            //AudioEngine.PlaySample(AudioEngine.s_HitFinish, AudioEngine.VolumeSample, 0, PositionalSound);

            if ((SoundType & HitObjectSoundType.Whistle) > 0)
                AudioEngine.PlaySample(OsuSamples.HitWhistle);

            if ((SoundType & HitObjectSoundType.Clap) > 0)
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
            return EndTime.CompareTo(other.EndTime);
        }

        public int CompareTo(int time)
        {
            return EndTime.CompareTo(time);
        }

        #endregion

        internal virtual void StopSound()
        {
        }

        internal virtual bool HitTest(TrackingPoint tracking)
        {
            float radius = 50;

            return (IsVisible &&
                    StartTime - DifficultyManager.PreEmpt <= Clock.AudioTime &&
                    StartTime + DifficultyManager.HitWindow50 >= Clock.AudioTime &&
                    !IsHit &&
                    pMathHelper.DistanceSquared(tracking.GamefieldPosition, Position) <= radius * radius);
        }

        internal virtual void Shake()
        {
            foreach (pSprite p in SpriteCollection)
            {
                Transformation previousShake = p.Transformations.FindLast(t => t.Type == TransformationType.Movement);

                Vector2 startPos = previousShake != null ? previousShake.EndVector : p.Position;

                p.Transform(new Transformation(startPos, startPos + new Vector2(8, 0),
                    Clock.AudioTime, Clock.AudioTime + 20));
                p.Transform(new Transformation(startPos + new Vector2(8, 0), startPos - new Vector2(8, 0),
                    Clock.AudioTime + 20, Clock.AudioTime + 40));
                p.Transform(new Transformation(startPos - new Vector2(8, 0), startPos + new Vector2(8, 0),
                    Clock.AudioTime + 40, Clock.AudioTime + 60));
                p.Transform(new Transformation(startPos + new Vector2(8, 0), startPos - new Vector2(8, 0),
                    Clock.AudioTime + 60, Clock.AudioTime + 80));
                p.Transform(new Transformation(startPos + new Vector2(8, 0), startPos - new Vector2(8, 0),
                    Clock.AudioTime + 80, Clock.AudioTime + 100));
                p.Transform(new Transformation(startPos + new Vector2(8, 0), startPos,
                    Clock.AudioTime + 100, Clock.AudioTime + 120));
            }
        }

        public override string ToString()
        {
            return this.Type + ": " + this.StartTime + "-" + this.EndTime + " stack:" + this.StackCount;
        }
    }
}
