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
using osum.Graphics;

namespace osum.GameModes
{
    public class Player : GameMode
    {
        internal HitObjectManager hitObjectManager;

        internal HealthBar healthBar;

        internal ScoreDisplay scoreDisplay;

        internal ComboCounter comboCounter;

        /// <summary>
        /// Score which is being played (or watched?)
        /// </summary>
        internal Score currentScore;

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

        internal PauseMenu menu;

        internal PlayfieldBackground playfieldBackground;

        internal CountdownDisplay countdown;

        private bool stateCompleted; //todo: make this an enum state

        /// <summary>
        /// If we are currently in the process of switching to another stream, this is when it should happen.
        /// </summary>
        private int queuedStreamSwitchTime;

        /// <summary>
        /// Warning graphic which appears when a stream change is in process.
        /// </summary>
        private pSprite s_streamSwitchWarningArrow;

        internal SpriteManager topMostSpriteManager;

        internal StreamSwitchDisplay streamSwitchDisplay;

        private TouchBurster touchBurster;

        bool isIncreasingStream;
        protected bool Failed;

        public Player()
            : base()
        { }

        internal override void Initialize()
        {
            TextureManager.PopulateSurfaces();

            InputManager.OnDown += InputManager_OnDown;

            TextureManager.RequireSurfaces = true;

            loadBeatmap();

            touchBurster = new TouchBurster();

            initializeUIElements();

            if (hitObjectManager != null)
            {
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

                if (hitObjectManager.ActiveStreamObjects == null)
                {
                    GameBase.Scheduler.Add(delegate { GameBase.Notify("Could not load difficulty!\nIt has likely not been mapped yet."); }, 500);
                    Director.ChangeMode(OsuMode.SongSelect);
                    //error while loading.
                    return;
                }

                //countdown and lead-in time
                int firstObjectTime = hitObjectManager.ActiveStreamObjects[0].StartTime;
                AudioEngine.Music.Stop();
                CountdownResume(firstObjectTime, 8);
                firstCountdown = true;
            }

            currentScore = new Score();

            playfieldBackground = new PlayfieldBackground();
            playfieldBackground.ChangeColour(Difficulty);
            spriteManager.Add(playfieldBackground);

            Director.OnTransitionEnded += Director_OnTransitionEnded;

            //if (fpsTotalCount != null)
            //{
            //    fpsTotalCount.AlwaysDraw = false;
            //    fpsTotalCount = null;
            //}

            //gcAtStart = GC.CollectionCount(0);

            s_streamSwitchWarningArrow = new pSprite(TextureManager.Load(OsuTexture.stream_changing_down), FieldTypes.StandardSnapBottomRight, OriginTypes.Centre, ClockTypes.Audio, new Vector2(50, GameBase.BaseSizeHalf.Height), 1, true, Color.White);
            s_streamSwitchWarningArrow.Additive = true;
            s_streamSwitchWarningArrow.Alpha = 0;

            spriteManager.Add(s_streamSwitchWarningArrow);

            topMostSpriteManager = new SpriteManager();
        }

        protected virtual void initializeUIElements()
        {
            if (Difficulty != Difficulty.Easy) healthBar = new HealthBar();
            scoreDisplay = new ScoreDisplay();
            comboCounter = new ComboCounter();
            streamSwitchDisplay = new StreamSwitchDisplay();
            countdown = new CountdownDisplay();
            menu = new PauseMenu();
        }

        protected void resetScore()
        {
            if (comboCounter != null) comboCounter.SetCombo(0);
            if (healthBar != null) healthBar.SetCurrentHp(100);
            if (scoreDisplay != null)
            {
                scoreDisplay.SetAccuracy(0);
                scoreDisplay.SetScore(0);
            }

            currentScore = new Score();
        }

        protected void loadBeatmap()
        {
            if (Beatmap == null)
                return;

            if (hitObjectManager != null)
                hitObjectManager.Dispose();

            hitObjectManager = new HitObjectManager(Beatmap);

            hitObjectManager.OnScoreChanged += hitObjectManager_OnScoreChanged;
            hitObjectManager.OnStreamChanged += hitObjectManager_OnStreamChanged;

            if (Beatmap.ContainerFilename != null)
                hitObjectManager.LoadFile();
        }

        /// <summary>
        /// Set to true after the initial countdown is set to ensure it is not overridden by a pause menu countdown.
        /// </summary>
        protected bool firstCountdown;

        /// <summary>
        /// Abort (and hide) the active countdown display. Is ignored for the initial countdown.
        /// </summary>
        internal void CountdownAbort()
        {
            if (firstCountdown) return;

            if (countdown != null)
                countdown.Hide();
        }

        /// <summary>
        /// Setup a new countdown process.
        /// </summary>
        /// <param name="startTime">AudioTime of the point at which the countdown finishes (the "go"+1 beat)</param>
        /// <param name="beats">How many beats we should count in.</param>
        internal void CountdownResume(int startTime, int beats)
        {
            if (firstCountdown)
            {
                if (Clock.AudioTime > countdown.StartTime)
                    firstCountdown = false;
                else
                {
                    AudioEngine.Music.Play();
                    return;
                }
            }

            double beatLength = Beatmap.beatLengthAt(startTime);
            int countdownStartTime = startTime - (int)(beatLength * beats);

            countdown.SetStartTime(startTime, beatLength);

            if (countdownStartTime < Clock.AudioTime)
                Clock.BeginLeadIn(countdownStartTime);
            else
                AudioEngine.Music.Play();
        }

        //static pSprite fpsTotalCount;
        //int gcAtStart;

        public override void Dispose()
        {
            if (Beatmap != null)
            {
                if (Clock.AudioTime > 5000)
                    BeatmapDatabase.GetBeatmapInfo(Beatmap, Difficulty).Playcount++;
            }

            InputManager.OnDown -= InputManager_OnDown;

            if (hitObjectManager != null) hitObjectManager.Dispose();

            TextureManager.RequireSurfaces = false;

            if (healthBar != null) healthBar.Dispose();
            if (scoreDisplay != null) scoreDisplay.Dispose();
            if (countdown != null) countdown.Dispose();
            if (menu != null) menu.Dispose();
            if (touchBurster != null) touchBurster.Dispose();
            if (streamSwitchDisplay != null) streamSwitchDisplay.Dispose();

            if (topMostSpriteManager != null) topMostSpriteManager.Dispose();

            base.Dispose();

            //Performance testing code.
            //fpsTotalCount = new pText("Total Player.cs frames: " + frameCount + " of " + Math.Round(msCount / 16.666667f) + " (GC: " + (GC.CollectionCount(0) - gcAtStart) + ")", 16, new Vector2(0, 100), new Vector2(512, 256), 0, false, Color4.White, false);
            //fpsTotalCount.FadeOutFromOne(15000);
            //GameBase.MainSpriteManager.Add(fpsTotalCount);
        }

        protected virtual void InputManager_OnDown(InputSource source, TrackingPoint point)
        {
            if ((menu != null && menu.MenuDisplayed) || !AudioEngine.Music.IsElapsing)
                return;

            //pass on the event to hitObjectManager for handling.
            if (hitObjectManager != null && hitObjectManager.HandlePressAt(point))
                return;
        }

        void hitObjectManager_OnStreamChanged(Difficulty newStream)
        {
            playfieldBackground.ChangeColour(hitObjectManager.ActiveStream);
            healthBar.SetCurrentHp(HealthBar.HP_BAR_MAXIMUM / 2);

            streamSwitchDisplay.EndSwitch();

            queuedStreamSwitchTime = 0;
        }

        void Director_OnTransitionEnded()
        {
        }

        void hitObjectManager_OnScoreChanged(ScoreChange change, HitObject hitObject)
        {
            double healthChange = 0;
            bool increaseCombo = false;

            if (hitObject is HitCircle && change > 0)
            {
                currentScore.hitOffsetMilliseconds += (Clock.AudioTime - hitObject.StartTime);
                currentScore.hitOffsetCount++;
            }

            //handle the score addition
            switch (change & ~ScoreChange.ComboAddition)
            {
                case ScoreChange.SpinnerBonus:
                    currentScore.totalScore += (int)hitObject.HpMultiplier;
                    currentScore.spinnerBonus += (int)hitObject.HpMultiplier;
                    healthChange = hitObject.HpMultiplier * 0.04f;
                    break;
                case ScoreChange.SpinnerSpinPoints:
                    currentScore.totalScore += 10;
                    healthChange = 0.4f * hitObject.HpMultiplier;
                    break;
                case ScoreChange.SliderRepeat:
                    currentScore.totalScore += 30;
                    increaseCombo = true;
                    healthChange = 2 * hitObject.HpMultiplier;
                    break;
                case ScoreChange.SliderEnd:
                    currentScore.totalScore += 30;
                    increaseCombo = true;
                    healthChange = 3 * hitObject.HpMultiplier;
                    break;
                case ScoreChange.SliderTick:
                    currentScore.totalScore += 10;
                    increaseCombo = true;
                    healthChange = 1 * hitObject.HpMultiplier;
                    break;
                case ScoreChange.Hit50:
                    currentScore.totalScore += 50;
                    currentScore.count50++;
                    increaseCombo = true;
                    healthChange = -8;
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
                    if (comboCounter != null) comboCounter.SetCombo(0);
                    healthChange = -20 * hitObject.HpMultiplier;
                    break;
                case ScoreChange.Miss:
                    currentScore.countMiss++;
                    if (comboCounter != null) comboCounter.SetCombo(0);
                    healthChange = -40;
                    break;
            }

            if (increaseCombo && comboCounter != null)
            {
                comboCounter.IncreaseCombo();
                currentScore.maxCombo = Math.Max(comboCounter.currentCombo, currentScore.maxCombo);
            }

            if (healthBar != null)
            {
                //then handle the hp addition
                if (healthChange < 0)
                    healthBar.ReduceCurrentHp(DifficultyManager.HpAdjustment * -healthChange);
                else
                    healthBar.IncreaseCurrentHp(healthChange);
            }

            if (scoreDisplay != null)
            {
                scoreDisplay.SetScore(currentScore.totalScore);
                scoreDisplay.SetAccuracy(currentScore.accuracy * 100);
            }
        }

        int frameCount = 0;
        double msCount = 0;
        private pSprite failSprite;

        public override bool Draw()
        {
            base.Draw();

            if (streamSwitchDisplay != null) streamSwitchDisplay.Draw();


            if (comboCounter != null) comboCounter.Draw();

            if (hitObjectManager != null)
                hitObjectManager.Draw();

            if (scoreDisplay != null) scoreDisplay.Draw();
            if (healthBar != null) healthBar.Draw();


            if (menu != null) menu.Draw();

            if (countdown != null) countdown.Draw();

            touchBurster.Draw();

            topMostSpriteManager.Draw();

            frameCount++;

            msCount += GameBase.ElapsedMilliseconds;

            return true;
        }

        public override void Update()
        {
            if (hitObjectManager != null)
            {
                CheckForCompletion();
                //check whether the map is finished

                //this needs to be run even when paused to draw sliders on resuming from resign.
                hitObjectManager.Update();

                Spinner s = hitObjectManager.ActiveObject as Spinner;
                if (s != null)
                    playfieldBackground.Alpha = 1 - s.SpriteBackground.Alpha;
                else
                    playfieldBackground.Alpha = 1;
            }

            if (healthBar != null) healthBar.Update();
            UpdateStream();

            if (scoreDisplay != null) scoreDisplay.Update();
            if (comboCounter != null) comboCounter.Update();

            touchBurster.Update();

            if (countdown != null) countdown.Update();

            topMostSpriteManager.Update();

            if (streamSwitchDisplay != null) streamSwitchDisplay.Update();

            if (menu != null) menu.Update();

            base.Update();
        }

        protected virtual void CheckForCompletion()
        {
            if (hitObjectManager.AllNotesHit && !Director.IsTransitioning && !stateCompleted)
            {
                stateCompleted = true;
                GameBase.Scheduler.Add(delegate
                {
                    Ranking.RankableScore = currentScore;
                    Director.ChangeMode(OsuMode.Ranking);
                }, 2000);
            }
        }

        protected virtual void UpdateStream()
        {
            if (Difficulty == Difficulty.Easy || hitObjectManager == null)
                //easy can't fail, nor switch streams.
                return;

            if (hitObjectManager != null && !hitObjectManager.StreamChanging)
            {
                if (hitObjectManager.IsLowestStream &&
                    currentScore.totalHits > 0 &&
                    healthBar.CurrentHp < HealthBar.HP_BAR_MAXIMUM)
                {
                    //we are on the lowest available stream difficulty and in failing territory.
                    if (healthBar.CurrentHp == 0 && !Autoplay)
                    {
                        playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_INTRO);

                        if (!stateCompleted)
                        {
                            stateCompleted = true;
                            Failed = true;

                            showFailSprite();

                            AudioEngine.Music.Pause();

                            menu.Failed = true; //set this now so the menu will be in fail state if interacted with early.

                            GameBase.Scheduler.Add(delegate
                            {
                                Ranking.RankableScore = currentScore;
                                menu.ShowFailMenu();
                            }, 1500);
                        }
                    }
                    else if (healthBar.CurrentHp < HealthBar.HP_BAR_MAXIMUM / 3)
                        playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_WARNING);
                    else
                        playfieldBackground.ChangeColour(hitObjectManager.ActiveStream);
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
            else
            {
                playfieldBackground.Move((isIncreasingStream ? 1 : -1) * Math.Max(0, (2000f - (queuedStreamSwitchTime - Clock.AudioTime)) / 400));
            }

        }

        protected void hideFailSprite()
        {
            if (failSprite != null)
                failSprite.FadeOut(100);
        }

        protected void showFailSprite()
        {
            failSprite = new pSprite(TextureManager.Load(OsuTexture.failed), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, Vector2.Zero, 0.5f, true, Color4.White);

            pDrawable failGlow = failSprite.Clone();

            failSprite.FadeInFromZero(500);
            failSprite.Transform(new Transformation(TransformationType.Scale, 1.8f, 1, Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.Out));
            failSprite.Transform(new Transformation(TransformationType.Rotation, 0.1f, 0, Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.Out));

            failGlow.DrawDepth = 0.51f;
            failGlow.AlwaysDraw = false;
            failGlow.ScaleScalar = 1.04f;
            failGlow.Additive = true;
            failGlow.Transform(new Transformation(TransformationType.Fade, 0, 0, Clock.ModeTime, Clock.ModeTime + 500));
            failGlow.Transform(new Transformation(TransformationType.Fade, 1, 0, Clock.ModeTime + 500, Clock.ModeTime + 2000));

            topMostSpriteManager.Add(failSprite);
            topMostSpriteManager.Add(failGlow);
        }

        internal void Pause()
        {
            if (menu != null)
                menu.ShowMenu();

            if (hitObjectManager != null)
            {
                HitObject activeObject = hitObjectManager.ActiveObject;
                if (activeObject != null)
                    activeObject.StopSound(false);
            }
        }

        private bool switchStream(bool increase)
        {
            isIncreasingStream = increase;
            if (increase && hitObjectManager.IsHighestStream)
                return false;
            if (!increase && hitObjectManager.IsLowestStream)
                return false;

            int switchTime = hitObjectManager.SetActiveStream((Difficulty)(hitObjectManager.ActiveStream + (increase ? 1 : -1)));

            if (switchTime < 0)
                return false;

            streamSwitchDisplay.BeginSwitch(increase);

            queuedStreamSwitchTime = switchTime;
            return true;
        }
    }
}

