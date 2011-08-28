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
using osum.Support;
using System.IO;
using osu_common.Helpers;
using osum.GameplayElements.HitObjects.Osu;

namespace osum.GameModes
{
    public class Player : GameMode
    {
        private HitObjectManager hitObjectManager;
        public HitObjectManager HitObjectManager
        {
            get { return hitObjectManager; }
            set
            {
                hitObjectManager = value;
                if (GuideFingers != null) GuideFingers.HitObjectManager = value;
            }
        }

        public HealthBar healthBar;

        internal ScoreDisplay scoreDisplay;

        internal ComboCounter comboCounter;

        private int firstObjectTime;
        private int lastObjectTime;

        /// <summary>
        /// Score which is being played (or watched?)
        /// </summary>
        public Score CurrentScore;

        /// <summary>
        /// The beatmap currently being played.
        /// </summary>
        public static Beatmap Beatmap;

        /// <summary>
        /// The difficulty which will be used for play mode (Easy/Standard/Expert).
        /// </summary>
        public static Difficulty Difficulty;

        /// <summary>
        /// Is autoplay activated?
        /// </summary>
        public static bool Autoplay;

        internal PauseMenu menu;

        internal PlayfieldBackground playfieldBackground;

        internal CountdownDisplay countdown;

        public bool Completed; //todo: make this an enum state

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

        internal TouchBurster touchBurster;

        bool isIncreasingStream;
        protected bool Failed;

        public Player()
            : base()
        { }

        int frameCount;

        public override void Initialize()
        {
            if (GameBase.Instance != null) GameBase.Instance.DisableDimming = true;

#if SCORE_TESTING
            File.WriteAllText("score.txt","");
#endif

            InputManager.OnDown += InputManager_OnDown;

            if (GameBase.Instance != null)
                TextureManager.RequireSurfaces = true;

            if (!GameBase.IsSlowDevice)
                touchBurster = new TouchBurster(!Player.Autoplay);

            loadBeatmap();

            initializeUIElements();

            if (HitObjectManager != null)
            {
                GuideFingers = new GuideFinger() { TouchBurster = touchBurster, HitObjectManager = hitObjectManager };
                ShowGuideFingers = Autoplay || GameBase.Config.GetValue<bool>("GuideFingers", false);

                switch (Difficulty)
                {
                    default:
                        HitObjectManager.SetActiveStream();
                        break;
                    case Difficulty.Expert:
                        HitObjectManager.SetActiveStream(Difficulty.Expert);
                        break;
                    case Difficulty.Easy:
                        HitObjectManager.SetActiveStream(Difficulty.Easy);
                        break;
                }

                BeatmapDifficultyInfo diff = null;
                if (Beatmap.DifficultyInfo.TryGetValue(Difficulty, out diff))
                    DifficultyComboMultiplier = diff.ComboMultiplier;
                //isn't being read. either i'm playing with the wrong osz2 file or the loading code isn't working

                if (HitObjectManager.ActiveStreamObjects == null)
                {
                    GameBase.Scheduler.Add(delegate { GameBase.Notify("Could not load difficulty!\nIt has likely not been mapped yet."); }, 500);
                    Director.ChangeMode(OsuMode.SongSelect);
                    //error while loading.
                    return;
                }

                //countdown
                int firstObjectTime = HitObjectManager.ActiveStreamObjects[0].StartTime;

                if ((AudioEngine.Music != null) && (AudioEngine.Music.lastLoaded != Beatmap.AudioFilename)) //could have switched to the results screen bgm.
                    AudioEngine.Music.Load(Beatmap.GetFileBytes(Beatmap.AudioFilename), false, Beatmap.AudioFilename);

                if (AudioEngine.Music != null)
                    AudioEngine.Music.Stop(true);

                Resume(firstObjectTime, 8, true);

                List<HitObject> objects = hitObjectManager.ActiveStreamObjects;

                firstObjectTime = objects[0].StartTime;
                lastObjectTime = objects[objects.Count - 1].EndTime;
            }

            resetScore();

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

#if VIDEO
            t_currentStream = new pText(HitObjectManager.ActiveStream.ToString(), 64, new Vector2(20, 20), 1, true, Color4.White);
            t_currentStream.Field = FieldTypes.StandardSnapBottomRight;
            t_currentStream.Origin = OriginTypes.BottomRight;
            t_currentStream.TextShadow = true;
            spriteManager.Add(t_currentStream);
#endif
        }

#if VIDEO
        pText t_currentStream;
#endif

        protected virtual void initializeUIElements()
        {
#if VIDEO
            healthBar = new HealthBar();
            healthBar.SetCurrentHp(200);
#else
            if (Difficulty != Difficulty.Easy) healthBar = new HealthBar();
            scoreDisplay = new ScoreDisplay();
#endif

            comboCounter = new ComboCounter();
            streamSwitchDisplay = new StreamSwitchDisplay();
            countdown = new CountdownDisplay();

#if !VIDEO
            menu = new PauseMenu();
#endif
            progressDisplay = new ProgressDisplay();
        }

        protected virtual void resetScore()
        {
            if (comboCounter != null)
                comboCounter.SetCombo(0);

            if (healthBar != null)
                healthBar.SetCurrentHp(DifficultyManager.InitialHp, true);

            if (scoreDisplay != null)
            {
                scoreDisplay.SetAccuracy(0);
                scoreDisplay.SetScore(0);
            }

            CurrentScore = new Score() { UseAccuracyBonus = false };
        }

        protected void loadBeatmap()
        {
            if (Beatmap == null)
                return;

            if (HitObjectManager != null)
                HitObjectManager.Dispose();

            HitObjectManager = new HitObjectManager(Beatmap);

            HitObjectManager.OnScoreChanged += hitObjectManager_OnScoreChanged;
            HitObjectManager.OnStreamChanged += hitObjectManager_OnStreamChanged;

            if (Beatmap.ContainerFilename != null)
                HitObjectManager.LoadFile();
#if VIDEO
            Player.Difficulty = Difficulty.Easy;
            //force back to stream difficulty, as it may be modified during load to get correct AR etc. variables.
#endif
        }

        /// <summary>
        /// Setup a new countdown process.
        /// </summary>
        /// <param name="startTime">AudioTime of the point at which the countdown finishes (the "go"+1 beat)</param>
        /// <param name="beats">How many beats we should count in.</param>
        internal void Resume(int startTime, int beats, bool forceCountdown = false)
        {
            double beatLength = Beatmap.beatLengthAt(startTime);

            int countdownStartTime;
            if (!countdown.HasFinished)
                countdownStartTime = countdown.StartTime - (int)(beatLength * beats);
            else
            {
                countdown.SetStartTime(startTime, beatLength);
                countdownStartTime = startTime - (int)(beatLength * beats);
            }

            if (AudioEngine.Music != null)
                AudioEngine.Music.Play();
        }

        //static pSprite fpsTotalCount;
        //int gcAtStart;

        public override void Dispose()
        {
#if !DIST && !MONO
            Console.WriteLine("Player.cs produced " + frameCount + " frames.");
#endif
            if (GameBase.Instance != null) GameBase.Instance.DisableDimming = false;

            if (Beatmap != null)
            {
                if (Clock.AudioTime > 5000 && !Autoplay)
                {
                    BeatmapDatabase.GetDifficultyInfo(Beatmap, Difficulty).Playcount++;
                    BeatmapDatabase.Write();
                }
            }

            InputManager.OnDown -= InputManager_OnDown;

            if (HitObjectManager != null) HitObjectManager.Dispose();

            TextureManager.RequireSurfaces = false;

            if (healthBar != null) healthBar.Dispose();
            if (scoreDisplay != null) scoreDisplay.Dispose();
            if (countdown != null) countdown.Dispose();
            if (GuideFingers != null) GuideFingers.Dispose();
            if (menu != null) menu.Dispose();
            if (touchBurster != null) touchBurster.Dispose();
            if (streamSwitchDisplay != null) streamSwitchDisplay.Dispose();

            if (topMostSpriteManager != null) topMostSpriteManager.Dispose();

            if (progressDisplay != null) progressDisplay.Dispose();

            if (Director.PendingOsuMode != OsuMode.Play)
            {
                RestartCount = 0;
                Player.Autoplay = false;
            }
            else
                RestartCount++;

            base.Dispose();

            //Performance testing code.
            //fpsTotalCount = new pText("Total Player.cs frames: " + frameCount + " of " + Math.Round(msCount / 16.666667f) + " (GC: " + (GC.CollectionCount(0) - gcAtStart) + ")", 16, new Vector2(0, 100), new Vector2(512, 256), 0, false, Color4.White, false);
            //fpsTotalCount.FadeOutFromOne(15000);
            //GameBase.MainSpriteManager.Add(fpsTotalCount);
        }

        protected virtual void InputManager_OnDown(InputSource source, TrackingPoint point)
        {
            if (menu != null && menu.MenuDisplayed)
                return;

            if (!(Clock.AudioTime > 0 && !AudioEngine.Music.IsElapsing))
            {
                //pass on the event to hitObjectManager for handling.
                if (HitObjectManager != null && !Failed && !Player.Autoplay && HitObjectManager.HandlePressAt(point))
                    return;
            }

            

            //before passing on input to the menu, do some other checks to make sure we don't accidentally trigger.
            if (hitObjectManager != null && !Autoplay)
            {
                Slider s = hitObjectManager.ActiveObject as Slider;
                if (s != null && s.IsTracking)
                    return;

                List<HitObject> objects = hitObjectManager.ActiveStreamObjects;
                for (int i = hitObjectManager.ProcessFrom; i <= hitObjectManager.ProcessTo; i++)
                {
                    HitObject h = objects[i];
                    if (h.IsVisible && h.TrackingPosition.Y < 50)
                        return;
                }
            }

            if (menu != null)
                menu.handleInput(source, point);
        }

        void hitObjectManager_OnStreamChanged(Difficulty newStream)
        {
            playfieldBackground.ChangeColour(HitObjectManager.ActiveStream);
            healthBar.SetCurrentHp(DifficultyManager.InitialHp);

            streamSwitchDisplay.EndSwitch();

#if VIDEO
            t_currentStream.Text = HitObjectManager.ActiveStream.ToString();
#endif

            queuedStreamSwitchTime = 0;
        }

        void Director_OnTransitionEnded()
        {
        }


        private void comboPain(bool harsh)
        {
            if (Failed) return;

            playfieldBackground.FlashColour(Color4.Red, 500).Offset(-250);

            if (harsh)
            {
                AudioEngine.PlaySample(OsuSamples.miss);

                HitObjectManager.ActiveStreamSpriteManager.ScaleScalar = 0.9f;
                HitObjectManager.ActiveStreamSpriteManager.ScaleTo(1, 400, EasingTypes.In);
            }
        }

        void hitObjectManager_OnScoreChanged(ScoreChange change, HitObject hitObject)
        {
            double healthChange = 0;
            bool increaseCombo = false;

            if (hitObject is HitCircle && change > 0)
            {
                CurrentScore.hitOffsetMilliseconds += (Clock.AudioTimeInputAdjust - hitObject.StartTime);
                CurrentScore.hitOffsetCount++;
            }

            bool comboMultiplier = true;
            bool addHitScore = true;

            int scoreChange = 0;

            //handle the score addition
            switch (change & ~ScoreChange.ComboAddition)
            {
                case ScoreChange.SpinnerBonus:
                    scoreChange = (int)hitObject.HpMultiplier;
                    comboMultiplier = false;
                    addHitScore = false;
                    CurrentScore.spinnerBonusScore += (int)hitObject.HpMultiplier;
                    healthChange = hitObject.HpMultiplier * 0.04f;
                    break;
                case ScoreChange.SpinnerSpinPoints:
                    scoreChange = 10;
                    comboMultiplier = false;
                    healthChange = 0.4f * hitObject.HpMultiplier;
                    break;
                case ScoreChange.SliderRepeat:
                    scoreChange = 30;
                    comboMultiplier = false;
                    increaseCombo = true;
                    healthChange = 2 * hitObject.HpMultiplier;
                    break;
                case ScoreChange.SliderEnd:
                    scoreChange = 30;
                    comboMultiplier = false;
                    increaseCombo = true;
                    healthChange = 3 * hitObject.HpMultiplier;
                    break;
                case ScoreChange.SliderTick:
                    scoreChange = 10;
                    comboMultiplier = false;
                    increaseCombo = true;
                    healthChange = 1 * hitObject.HpMultiplier;
                    break;
                case ScoreChange.Hit50:
                    scoreChange = 50;
                    CurrentScore.count50++;
                    increaseCombo = true;
                    healthChange = -8;
                    break;
                case ScoreChange.Hit100:
                    scoreChange = 100;
                    CurrentScore.count100++;
                    increaseCombo = true;
                    healthChange = 0.5;
                    break;
                case ScoreChange.Hit300:
                    scoreChange = 300;
                    CurrentScore.count300++;
                    increaseCombo = true;
                    healthChange = 5;
                    break;
                case ScoreChange.MissMinor:
                    if (comboCounter != null)
                    {
                        comboPain(comboCounter.currentCombo >= 30);
                        comboCounter.SetCombo(0);
                    }
                    healthChange = -20 * hitObject.HpMultiplier;
                    break;
                case ScoreChange.Miss:
                    CurrentScore.countMiss++;
                    if (comboCounter != null)
                    {
                        //if (comboCounter.currentCombo >= 30)
                        comboPain(comboCounter.currentCombo >= 30);
                        comboCounter.SetCombo(0);
                    }
                    healthChange = -40;
                    break;
            }

#if !DIST
            /*if (hitObject is HitCircle || Math.Abs(Clock.AudioTime - hitObject.StartTime) < 30)
            {
                pSpriteText st = new pSpriteText(Math.Abs(Clock.AudioTime - hitObject.StartTime).ToString(), "default", 0, FieldTypes.GamefieldSprites, OriginTypes.TopCentre,
                    ClockTypes.Audio, hitObject.Position + new Vector2(0,60), 1, false, Clock.AudioTime > hitObject.StartTime ? Color.OrangeRed : Color4.YellowGreen);
                st.FadeOutFromOne(900);
                spriteManager.Add(st);
            }*/
#endif

            if (scoreChange > 0 && addHitScore)
                CurrentScore.hitScore += scoreChange;

            if (increaseCombo && comboCounter != null)
            {
                comboCounter.IncreaseCombo();
                CurrentScore.maxCombo = (ushort)Math.Max(comboCounter.currentCombo, CurrentScore.maxCombo);

                if (comboMultiplier)
                {
                    int comboAmount = (int)Math.Max(0, (scoreChange / 10 * Math.Min(comboCounter.currentCombo - 4, 60) / 2 * DifficultyComboMultiplier));

                    CurrentScore.comboBonusScore += comboAmount;

                    //check we don't exceed 0.6mil total (before accuracy bonus).
                    //null check makes sure we aren't doing score calculations via combinator.
                    if (GameBase.Instance != null && CurrentScore.hitScore + CurrentScore.comboBonusScore > Score.HIT_PLUS_COMBO_BONUS_AMOUNT)
                    {
                        if (CurrentScore.comboBonusScore + CurrentScore.hitScore > Score.HIT_PLUS_COMBO_BONUS_AMOUNT)
                        {
#if !DIST
                            Console.WriteLine("WARNING: Score exceeded limits at " + CurrentScore.totalScore);
#if SCORE_TESTING
                            File.AppendAllText("score.txt", "WARNING: Score exceeded limits at " + CurrentScore.totalScore + "\n");
#endif
#endif
                            CurrentScore.comboBonusScore = Math.Min(Score.HIT_PLUS_COMBO_BONUS_AMOUNT - CurrentScore.hitScore, CurrentScore.comboBonusScore);
                        }
                    }
                }
            }

#if SCORE_TESTING
            File.AppendAllText("score.txt", "at " + Clock.AudioTime + " : " + change + "\t" + CurrentScore.hitScore + "\t" + CurrentScore.comboBonusScore + "\n");
#endif

            if (healthBar != null)
            {
                if (HitObjectManager == null || !HitObjectManager.StreamChanging)
                {
                    //then handle the hp addition
                    if (healthChange < 0)
                    {
                        Difficulty streamDifficulty = hitObjectManager.ActiveStream;
                        float streamMultiplier = 1;

                        switch (streamDifficulty)
                        {
                            case Difficulty.Hard:
                                streamMultiplier = 1.3f;
                                break;
                            case Difficulty.Normal:
                                streamMultiplier = 1.1f;
                                break;
                        }


                        healthBar.ReduceCurrentHp(DifficultyManager.HpAdjustment * -healthChange);
                    }
                    else
                        healthBar.IncreaseCurrentHp(healthChange * Beatmap.HpStreamAdjustmentMultiplier);
                }
            }

            if (scoreDisplay != null)
            {
                scoreDisplay.SetScore(CurrentScore.totalScore);
                scoreDisplay.SetAccuracy(CurrentScore.accuracy * 100);
            }
        }

        private pSprite failSprite;
        private double DifficultyComboMultiplier = 1;

        internal GuideFinger GuideFingers;
        public static int RestartCount;

        public override bool Draw()
        {
            base.Draw();

            frameCount++;

            if (streamSwitchDisplay != null) streamSwitchDisplay.Draw();

            if (progressDisplay != null) progressDisplay.Draw();

            if (comboCounter != null) comboCounter.Draw();

            if (countdown != null) countdown.Draw();

            if (HitObjectManager != null)
                HitObjectManager.Draw();

            if (scoreDisplay != null) scoreDisplay.Draw();

#if !VIDEO
            if (healthBar != null) healthBar.Draw();
#endif

            if (GuideFingers != null && ShowGuideFingers) GuideFingers.Draw();

            if (menu != null) menu.Draw();

            if (touchBurster != null) touchBurster.Draw();

            topMostSpriteManager.Draw();

            return true;
        }

        public override void Update()
        {
            if (Failed)
            {
                if (AudioEngine.Music != null)
                {
                    float vol = AudioEngine.Music.DimmableVolume;
                    if (vol == 0 && AudioEngine.Music.IsElapsing)
                        AudioEngine.Music.Pause();
                    else
                        AudioEngine.Music.DimmableVolume -= (float)(Clock.ElapsedMilliseconds) * 0.001f;
                }
            }

            if (GuideFingers != null && ShowGuideFingers) GuideFingers.Update();

            if (HitObjectManager != null)
            {
                //this needs to be run even when paused to draw sliders on resuming from resign.
                HitObjectManager.Update();

                //check whether the map is finished
                CheckForCompletion();

                Spinner s = HitObjectManager.ActiveObject as Spinner;
                if (s != null)
                    playfieldBackground.Alpha = 1 - s.SpriteBackground.Alpha;
                else
                    playfieldBackground.Alpha = 1;
            }

            if (healthBar != null) healthBar.Update();

            UpdateStream();

            if (scoreDisplay != null) scoreDisplay.Update();
            if (comboCounter != null) comboCounter.Update();

            if (touchBurster != null) touchBurster.Update();

            if (countdown != null) countdown.Update();

            topMostSpriteManager.Update();

            if (streamSwitchDisplay != null) streamSwitchDisplay.Update();

            if (menu != null && (!Completed || Failed)) menu.Update();

            if (progressDisplay != null)
            {
                progressDisplay.SetProgress(Progress);
                progressDisplay.Update();
            }

            base.Update();
        }

        protected virtual bool CheckForCompletion()
        {
            if (HitObjectManager.AllNotesHit && !Director.IsTransitioning && !Completed)
            {
                Completed = true;

#if iOS || true
                if (Player.Autoplay)
                {
                    Director.ChangeMode(OsuMode.Empty,new FadeTransition(3000, FadeTransition.DEFAULT_FADE_IN));
                }
                else
#endif
                {
                    Results.RankableScore = CurrentScore;
                    Results.RankableScore.UseAccuracyBonus = true;

                    GameBase.Scheduler.Add(delegate
                    {

                        Director.ChangeMode(OsuMode.Results, new ResultTransition());
                    }, 500);
                }
            }

            return Completed;
        }

        protected virtual void UpdateStream()
        {
#if !VIDEO
            if (Difficulty == Difficulty.Easy || HitObjectManager == null)
                //easy can't fail, nor switch streams.
                return;
#endif

            if (HitObjectManager != null && !HitObjectManager.StreamChanging)
            {
                if (HitObjectManager.IsLowestStream &&
                    CurrentScore.totalHits > 0 &&
                    healthBar.CurrentHp < HealthBar.HP_BAR_MAXIMUM)
                {
                    //we are on the lowest available stream difficulty and in failing territory.
                    if (healthBar.CurrentHp == 0 && !Autoplay)
                    {
                        playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_INTRO);

                        if (!Completed)
                        {
                            Completed = true;

                            showFailScreen();

                            menu.Failed = true; //set this now so the menu will be in fail state if interacted with early.

                            GameBase.Scheduler.Add(delegate
                            {
                                Results.RankableScore = CurrentScore;
                                menu.ShowFailMenu();
                            }, 1500);
                        }
                    }
                    else if (healthBar.CurrentHp < HealthBar.HP_BAR_MAXIMUM / 2)
                        playfieldBackground.ChangeColour(HitObjectManager.ActiveStream, 1 - (float)healthBar.CurrentHp / (HealthBar.HP_BAR_MAXIMUM / 2));
                    else
                        playfieldBackground.ChangeColour(HitObjectManager.ActiveStream, false);
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
#if DEBUG
                DebugOverlay.AddLine("Stream changing at " + HitObjectManager.nextStreamChange + " to " + HitObjectManager.ActiveStream);
#endif
                playfieldBackground.Move((isIncreasingStream ? 1 : -1) * Math.Max(0, (2000f - (queuedStreamSwitchTime - Clock.AudioTime)) / 200));
            }

        }

        protected void hideFailScreen()
        {
            Failed = false;

            if (GameBase.Instance != null) GameBase.Instance.DisableDimming = true;

            if (HitObjectManager != null)
            {
                HitObjectManager.spriteManager.Transformations.Clear();
                HitObjectManager.spriteManager.Position = Vector2.Zero;
            }

            if (failSprite != null)
                failSprite.FadeOut(100);
        }

        protected void showFailScreen()
        {
            Failed = true;
            playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_INTRO);
            AudioEngine.PlaySample(OsuSamples.fail);

            if (GameBase.Instance != null) GameBase.Instance.DisableDimming = false;

            if (HitObjectManager != null)
                HitObjectManager.StopAllSounds();

            if (HitObjectManager != null)
            {
                HitObjectManager.spriteManager.MoveTo(new Vector2(0, 700), 5000, EasingTypes.OutDouble);
                HitObjectManager.spriteManager.RotateTo(0.1f, 5000);
                HitObjectManager.spriteManager.FadeOut(1000);
                HitObjectManager.ActiveStreamSpriteManager.MoveTo(new Vector2(0, 700), 5000, EasingTypes.OutDouble);
                HitObjectManager.ActiveStreamSpriteManager.FadeOut(5000);
                HitObjectManager.ActiveStreamSpriteManager.RotateTo(0.1f, 5000);
            }

            failSprite = new pSprite(TextureManager.Load(OsuTexture.failed), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, Vector2.Zero, 0.5f, true, Color4.White);

            pDrawable failGlow = failSprite.Clone();

            failSprite.FadeInFromZero(500);
            failSprite.Transform(new TransformationF(TransformationType.Scale, 1.8f, 1, Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.Out));
            failSprite.Transform(new TransformationF(TransformationType.Rotation, 0.1f, 0, Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.Out));

            failGlow.DrawDepth = 0.51f;
            failGlow.AlwaysDraw = false;
            failGlow.ScaleScalar = 1.04f;
            failGlow.Additive = true;
            failGlow.Transform(new TransformationF(TransformationType.Fade, 0, 0, Clock.ModeTime, Clock.ModeTime + 500));
            failGlow.Transform(new TransformationF(TransformationType.Fade, 1, 0, Clock.ModeTime + 500, Clock.ModeTime + 2000));

            topMostSpriteManager.Add(failSprite);
            topMostSpriteManager.Add(failGlow);
        }

        internal bool IsPaused
        {
            get { return menu != null && menu.MenuDisplayed; }
        }

        internal void Pause()
        {
            if (!Failed) AudioEngine.Music.Pause();

            if (HitObjectManager != null)
                HitObjectManager.StopAllSounds();

            if (menu != null) menu.MenuDisplayed = true;
        }

        private bool switchStream(bool increase)
        {
            isIncreasingStream = increase;
            if (increase && HitObjectManager.IsHighestStream)
                return false;
            if (!increase && HitObjectManager.IsLowestStream)
                return false;

            int switchTime = HitObjectManager.SetActiveStream((Difficulty)(HitObjectManager.ActiveStream + (increase ? 1 : -1)));

            if (switchTime < 0)
                return false;

            streamSwitchDisplay.BeginSwitch(increase);

            queuedStreamSwitchTime = switchTime;
            return true;
        }

        internal static string SubmitString
        {
            get { return CryptoHelper.GetMd5String(Path.GetFileName(Player.Beatmap.ContainerFilename) + "-" + Player.Difficulty.ToString()); }
        }

        public float Progress
        {
            get
            {
                return pMathHelper.ClampToOne((float)Clock.AudioTime / lastObjectTime);
            }
        }

        protected bool ShowGuideFingers;
        private ProgressDisplay progressDisplay;
    }
}

