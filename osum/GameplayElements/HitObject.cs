using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using osum.Graphics.Sprites;
using osum.Helpers;

namespace osum.GameplayElements
{
    internal delegate void HitCircleDelegate(HitObject h);

    [Flags]
    internal enum HitObjectType
    {
        Normal = 1,
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
    internal enum IncreaseScoreType
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
        HitValuesOnly = Hit50 | Hit100 | Hit300 | GekiAddition | KatuAddition,
        ComboAddition = MuAddition | KatuAddition | GekiAddition,
        NonScoreModifiers = TaikoLargeHitBoth | TaikoLargeHitFirst | TaikoLargeHitSecond
    }

    internal abstract class HitObject : pSpriteCollection, IComparable<HitObject>, IComparable<int>
    {
        #region General & Timing

        //private bool IsSelected; // editor

        internal bool IsHit;
        internal double MaxHp;
        internal int StartTime;
        internal HitObjectType Type;
        internal int EndTime;

        internal virtual bool NewCombo
        {
            get { return (Type & HitObjectType.NewCombo) > 0; }
            set
            {
                if (value)
                    Type |= HitObjectType.NewCombo;
                else
                    Type &= ~HitObjectType.NewCombo;
            }
        }

        /*
        internal bool Selected
        {
            get { return IsSelected; }

            set
            {
                if (IsSelected != value)
                {
                    IsSelected = value;
                    if (IsSelected)
                        Select();
                    else
                        Deselect();
                }
            }
        }
        */

        internal abstract void SetColour(Color4 color);
        internal abstract IncreaseScoreType Hit();

        internal virtual void Dispose()
        {
        }

        internal abstract HitObject Clone();

        /* // editor?
        internal abstract void Select();
        internal abstract void Deselect();
        internal abstract void ModifyTime(int newTime);
        internal abstract void ModifyPosition(Vector2 newPosition);
        */

        /*
        internal virtual void Update()
        {
            return;
        }

        internal virtual void Draw()
        {
            return;
        }
        */
        #endregion

        #region Drawing

        internal Color4 Colour;
        protected internal List<pSprite> DimCollection = new List<pSprite>();
        internal Vector2 Position;
        internal int StackCount;

        internal abstract int ComboNumber { get; set; }
        internal abstract Vector2 EndPosition { get; set; }

        internal abstract bool IsVisible { get; }

        internal virtual Vector2 Position2
        {
            get { return Position; }
            set { }
        }

        internal pSprite[] Sprites
        {
            get { return SpriteCollection.ToArray(); }
        }

        #endregion

        #region Sound

        internal Color4 ColourDim;
        internal bool Dimmed;
        internal bool Sounded;
        internal HitObjectSoundType SoundType;
        internal bool Drawable;
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
            /*
            HitObjectManager.OnHitSound(SoundType);

            if ((SoundType & HitObjectSoundType.Finish) > 0)
                AudioEngine.PlaySample(AudioEngine.s_HitFinish, AudioEngine.VolumeSample, 0, PositionalSound);

            if ((SoundType & HitObjectSoundType.Whistle) > 0)
                AudioEngine.PlaySample(AudioEngine.s_HitWhistle, (int)(AudioEngine.VolumeSample * 0.85), 0, PositionalSound);

            if ((SoundType & HitObjectSoundType.Clap) > 0)
                AudioEngine.PlaySample(AudioEngine.s_HitClap, (int)(AudioEngine.VolumeSample * 0.85), 0, PositionalSound);

            if (SkinManager.Current.LayeredHitSounds || SoundType == HitObjectSoundType.Normal)
                AudioEngine.PlaySample(AudioEngine.s_HitNormal, (int)(AudioEngine.VolumeSample * 0.8), 0, PositionalSound);
            */
        }

        protected virtual float PositionalSound { get { return Position.X / 512f - 0.5f; } }

        /// <summary>
        /// Gets the hittable end time (valid active object time for sliders etc. - used in taiko to extend when hits are valid).
        /// </summary>
        /// <value>The hittable end time.</value>
        internal virtual int HittableEndTime
        {
            get { return EndTime; }
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

        #endregion

        internal abstract IncreaseScoreType GetScorePoints(Vector2 currentMousePos);

        internal virtual void StopSound()
        {
        }

        internal abstract void SetEndTime(int time);

        public int CompareTo(int other)
        {
            return EndTime.CompareTo(other);
        }

        internal virtual bool HitTest(Vector2 testPosition, bool hittableRangeOnly, float radius)
        {
            return ((!hittableRangeOnly && IsVisible) ||
                  (StartTime - DifficultyManager.PreEmpt <= Clock.AudioTime &&
                   StartTime + DifficultyManager.HitWindow50 >= Clock.AudioTime && !IsHit)) &&
                 (OsumMathHelper.DistanceSquared(testPosition, Position) <= radius * radius ||
                  (!hittableRangeOnly && OsumMathHelper.DistanceSquared(testPosition, Position2) <= radius * radius));
        }

        internal virtual void Shake()
        {
            foreach (pSprite p in SpriteCollection)
            {
                Transform previousShake = p.Transformations.FindLast(t => t.Type == TransformType.Movement);

                Vector2 startPos = previousShake != null ? previousShake.EndVector : p.Position;

                p.Transform(new Transform(startPos, startPos + new Vector2(8, 0), 
                    Clock.AudioTime, Clock.AudioTime + 20));
                p.Transform(new Transform(startPos + new Vector2(8, 0), startPos - new Vector2(8, 0), 
                    Clock.AudioTime + 20, Clock.AudioTime + 40));
                p.Transform(new Transform(startPos - new Vector2(8, 0), startPos + new Vector2(8, 0), 
                    Clock.AudioTime + 40, Clock.AudioTime + 60));
                p.Transform(new Transform(startPos + new Vector2(8, 0), startPos - new Vector2(8, 0), 
                    Clock.AudioTime + 60, Clock.AudioTime + 80));
                p.Transform(new Transform(startPos + new Vector2(8, 0), startPos - new Vector2(8, 0), 
                    Clock.AudioTime + 80, Clock.AudioTime + 100));
                p.Transform(new Transform(startPos + new Vector2(8, 0), startPos, 
                    Clock.AudioTime + 100, Clock.AudioTime + 120));
            }
        }

        public override string ToString()
        {
            return this.Type + ": " + this.StartTime + "-" + this.EndTime + " stack:" + this.StackCount;
        }

        public int Length { get { return EndTime - StartTime; } }
    }
}
