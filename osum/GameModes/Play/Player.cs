//  Play.cs
//  Author: Dean Herbert <pe@ppy.sh>
//  Copyright (c) 2010 2010 Dean Herbert
using System;
using osum.GameplayElements;
using osum.GameplayElements.Beatmaps;
using osum.Helpers;
//using osu.Graphics.Renderers;
using osum.Graphics.Primitives;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using System.Drawing;
using osum.Audio;
using osum.Graphics.Renderers;
using osum.GameplayElements.Scoring;
using osum.GameModes.Play.Components;
using osum.Graphics.Sprites;
using osum.Graphics.Skins;

namespace osum.GameModes
{
    public class Player : GameMode
    {
        HitObjectManager hitObjectManager;

        HealthBar healthBar;

        ScoreDisplay scoreDisplay;

        ComboCounter comboCounter;

        /// <summary>
        /// Score which is being played (or watched?)
        /// </summary>
        Score currentScore;

        /// <summary>
        /// The beatmap currently being played.
        /// </summary>
        internal static Beatmap Beatmap;

        /// <summary>
        /// The difficulty which will be used for play mode (Easy/Standard/Expert).
        /// </summary>
        internal static Difficulty Difficulty;

        /// <summary>
        /// Is autoplay activated?
        /// </summary>
        public static bool Autoplay;

        private PlayfieldBackground s_Playfield;

        private bool stateCompleted; //todo: make this an enum state

        /// <summary>
        /// If we are currently in the process of switching to another stream, this is when it should happen.
        /// </summary>
        private int queuedStreamSwitchTime;

        /// <summary>
        /// Warning graphic which appears when a stream change is in process.
        /// </summary>
        private pSprite s_streamSwitchWarningArrow;


        public Player()
            : base()
        {
        }

        internal override void Initialize()
        {
            TextureManager.PopulateSurfaces();

            InputManager.OnDown += InputManager_OnDown;
            InputManager.OnMove += InputManager_OnMove;

            TextureManager.RequireSurfaces = true;

            hitObjectManager = new HitObjectManager(Beatmap);
            hitObjectManager.OnScoreChanged += hitObjectManager_OnScoreChanged;
            hitObjectManager.OnStreamChanged += hitObjectManager_OnStreamChanged;

            hitObjectManager.LoadFile();

            switch (Difficulty)
            {
                default:
                    hitObjectManager.SetActiveStream();
                    break;
                case Difficulty.Expert:
                    hitObjectManager.SetActiveStream(Difficulty.Expert);
                    break;
                case Difficulty.Easy:
                    hitObjectManager.SetActiveStream(Difficulty.Easy);
                    break;
            }

            healthBar = new HealthBar();

            scoreDisplay = new ScoreDisplay();

            comboCounter = new ComboCounter();

            currentScore = new Score();

            s_Playfield = new PlayfieldBackground();
            spriteManager.Add(s_Playfield);

            AudioEngine.Music.Stop();
            //AudioEngine.Music.Load(Beatmap.GetFileBytes(Beatmap.AudioFilename), false);
            Director.OnTransitionEnded += new VoidDelegate(Director_OnTransitionEnded);

            //if (fpsTotalCount != null)
            //{
            //    fpsTotalCount.AlwaysDraw = false;
            //    fpsTotalCount = null;
            //}

            //gcAtStart = GC.CollectionCount(0);

            s_streamSwitchWarningArrow = new pSprite(TextureManager.Load(OsuTexture.stream_changing), FieldTypes.StandardSnapBottomRight, OriginTypes.Centre, ClockTypes.Audio, new Vector2(50, GameBase.BaseSizeHalf.Height), 1, true, Color.White);
            s_streamSwitchWarningArrow.Additive = true;
            s_streamSwitchWarningArrow.Alpha = 0;

            spriteManager.Add(s_streamSwitchWarningArrow);

            
        }

        //static pSprite fpsTotalCount;
        //int gcAtStart;

        public override void Dispose()
        {
            InputManager.OnDown -= InputManager_OnDown;

            TextureManager.RequireSurfaces = false;

            hitObjectManager.Dispose();

            healthBar.Dispose();
            scoreDisplay.Dispose();

            base.Dispose();

            //Performance testing code.
            //fpsTotalCount = new pText("Total Player.cs frames: " + frameCount + " of " + Math.Round(msCount / 16.666667f) + " (GC: " + (GC.CollectionCount(0) - gcAtStart) + ")", 16, new Vector2(0, 100), new Vector2(512, 256), 0, false, Color4.White, false);
            //fpsTotalCount.FadeOutFromOne(15000);
            //GameBase.MainSpriteManager.Add(fpsTotalCount);
        }

        void InputManager_OnDown(InputSource source, TrackingPoint point)
        {
            //pass on the event to hitObjectManager for handling.
            if (hitObjectManager.HandlePressAt(point))
                return;

            if (InputManager.TrackingPoints.Count == 2)
            {
                Vector2 p1 = InputManager.TrackingPoints[0].BasePosition;
                Vector2 p2 = InputManager.TrackingPoints[1].BasePosition;

                if (Math.Max(p1.X, p2.X) > (GameBase.BaseSizeFixedWidth.Width - 40) &&
                    Math.Min(p1.X, p2.X) < 40 &&
                    p1.Y + p2.Y < 80)
                {
                    Director.ChangeMode(OsuMode.SongSelect);
                }
                else if (Math.Max(p1.X, p2.X) > (GameBase.BaseSizeFixedWidth.Width - 40) &&
                    Math.Min(p1.X, p2.X) < 40 &&
                    p1.Y + p2.Y > (GameBase.BaseSizeFixedWidth.Height * 2) - 40)
                {
                    Player.Autoplay = !Autoplay;
                }

            }
        }

        int lastSeek;
        void InputManager_OnMove(InputSource source, TrackingPoint point)
        {
            // fast forward for iphone
            if (InputManager.TrackingPoints.Count >= 4 && Clock.Time - lastSeek > 250)
            {
                lastSeek = Clock.Time;
                AudioEngine.Music.SeekTo(Clock.AudioTime + 2000);
            }
        }

        void hitObjectManager_OnStreamChanged(Difficulty newStream)
        {
            s_Playfield.ChangeColour(hitObjectManager.ActiveStream);
            healthBar.SetCurrentHp(HealthBar.HP_BAR_MAXIMUM / 2);

            queuedStreamSwitchTime = 0;
        }

        void Director_OnTransitionEnded()
        {
            AudioEngine.Music.Play();
        }

        void hitObjectManager_OnScoreChanged(ScoreChange change, HitObject hitObject)
        {
            if (currentScore.totalHits == 0)
                s_Playfield.ChangeColour(Difficulty);

            double healthChange = 0;
            bool increaseCombo = false;

            //handle the score addition
            switch (change & ~ScoreChange.ComboAddition)
            {
                case ScoreChange.SpinnerBonus:
                    currentScore.totalScore += 1000;
                    healthChange = 2;
                    break;
                case ScoreChange.SpinnerSpinPoints:
                    currentScore.totalScore += 500;
                    healthChange = 1;
                    break;
                case ScoreChange.SpinnerSpin:
                    break;
                case ScoreChange.SliderRepeat:
                case ScoreChange.SliderEnd:
                    currentScore.totalScore += 30;
                    increaseCombo = true;
                    healthChange = 4;
                    break;
                case ScoreChange.SliderTick:
                    currentScore.totalScore += 10;
                    increaseCombo = true;
                    healthChange = 1;
                    break;
                case ScoreChange.Hit50:
                    currentScore.totalScore += 50;
                    currentScore.count50++;
                    increaseCombo = true;
                    healthChange = -3;
                    break;
                case ScoreChange.Hit100:
                    currentScore.totalScore += 100;
                    currentScore.count100++;
                    increaseCombo = true;
                    healthChange = 0.5;
                    break;
                case ScoreChange.Hit300:
                    currentScore.totalScore += 300;
                    currentScore.count300++;
                    increaseCombo = true;
                    healthChange = 5;
                    break;
                case ScoreChange.MissMinor:
                    comboCounter.SetCombo(0);
                    healthChange = -4;
                    break;
                case ScoreChange.Miss:
                    currentScore.countMiss++;
                    comboCounter.SetCombo(0);
                    healthChange = -20;
                    break;
            }

            if (increaseCombo)
            {
                comboCounter.IncreaseCombo();
                currentScore.maxCombo = comboCounter.currentCombo;
            }

            //then handle the hp addition
            if (healthChange < 0)
                healthBar.ReduceCurrentHp(-healthChange);
            else
                healthBar.IncreaseCurrentHp(healthChange);

            scoreDisplay.SetScore(currentScore.totalScore);
            scoreDisplay.SetAccuracy(currentScore.accuracy * 100);
        }

        int frameCount = 0;
        double msCount = 0;

        public override bool Draw()
        {
            base.Draw();

            hitObjectManager.Draw();

            scoreDisplay.Draw();
            healthBar.Draw();
            comboCounter.Draw();

            frameCount++;

            msCount += GameBase.ElapsedMilliseconds;

            return true;
        }

        public override void Update()
        {
            //check whether the map is finished
            if (hitObjectManager.AllNotesHit && !Director.IsTransitioning && !stateCompleted)
            {
                stateCompleted = true;
                GameBase.Scheduler.Add(delegate
                {
                    Ranking.RankableScore = currentScore;
                    Director.ChangeMode(OsuMode.Ranking);
                }, 2000);
            }

            hitObjectManager.Update();

            Spinner s = hitObjectManager.ActiveObject as Spinner;
            if (s != null)
                s_Playfield.Alpha = 1 - s.SpriteBackground.Alpha;
            else
                s_Playfield.Alpha = 1;

            healthBar.Update();

            UpdateStream();

            scoreDisplay.Update();
            comboCounter.Update();

            base.Update();
        }

        private void UpdateStream()
        {
            if (Difficulty == Difficulty.Easy)
                //easy can't fail, nor switch streams.
                return;

            if (!hitObjectManager.StreamChanging)
            {
                if (hitObjectManager.IsLowestStream &&
                    currentScore.totalHits > 0 &&
                    healthBar.CurrentHp < HealthBar.HP_BAR_MAXIMUM)
                {
                    //we are on the lowest available stream difficulty and in failing territory.
                    if (healthBar.CurrentHp == 0)
                    {
                        s_Playfield.ChangeColour(PlayfieldBackground.COLOUR_INTRO);

                        if (!stateCompleted)
                        {
                            stateCompleted = true;

                            AudioEngine.Music.Pause();
                            GameBase.Scheduler.Add(delegate
                            {
                                Ranking.RankableScore = currentScore;
                                Director.ChangeMode(OsuMode.SongSelect);
                            }, 2000);
                        }
                    }
                    else if (healthBar.CurrentHp < HealthBar.HP_BAR_MAXIMUM / 3)
                        s_Playfield.ChangeColour(PlayfieldBackground.COLOUR_WARNING);
                    else
                        s_Playfield.ChangeColour(hitObjectManager.ActiveStream);
                }
                else if (healthBar.CurrentHp == HealthBar.HP_BAR_MAXIMUM)
                {
                    switchStream(true);
                }
                else if (healthBar.CurrentHp == 0)
                {
                    switchStream(false);
                }
            }
        }

        private bool switchStream(bool increase)
        {
            if (increase && hitObjectManager.IsHighestStream)
                return false;
            if (!increase && hitObjectManager.IsLowestStream)
                return false;

            int switchTime = hitObjectManager.SetActiveStream((Difficulty)(hitObjectManager.ActiveStream + (increase ? 1 : -1)));

            if (switchTime < 0)
                return false;

            const int animation_time = 250;

            //rotate the warning arrow to the correct direction.
            if (increase)
            {
                s_streamSwitchWarningArrow.Transform(
                    new Transformation(TransformationType.Rotation, s_streamSwitchWarningArrow.Rotation, 0, Clock.AudioTime, Clock.AudioTime + animation_time * 2, EasingTypes.In));
                s_streamSwitchWarningArrow.MoveTo(s_streamSwitchWarningArrow.Position + new Vector2(0, 20), animation_time * 4, EasingTypes.In);
            }
            else
            {
                s_streamSwitchWarningArrow.Transform(
                    new Transformation(TransformationType.Rotation, s_streamSwitchWarningArrow.Rotation, (float)Math.PI, Clock.AudioTime, Clock.AudioTime + animation_time * 2, EasingTypes.In));
                s_streamSwitchWarningArrow.MoveTo(s_streamSwitchWarningArrow.Position + new Vector2(0, -20), animation_time * 4, EasingTypes.In);
            }

            s_streamSwitchWarningArrow.ScaleScalar = 1;
            s_streamSwitchWarningArrow.FadeIn(animation_time);

            s_streamSwitchWarningArrow.Transform(new Transformation(TransformationType.Fade, 1, 0, switchTime, switchTime + animation_time));
            s_streamSwitchWarningArrow.Transform(new Transformation(TransformationType.Scale, 1, 2f, switchTime, switchTime + animation_time, EasingTypes.In));

            queuedStreamSwitchTime = switchTime;
            return true;
        }
    }
}

