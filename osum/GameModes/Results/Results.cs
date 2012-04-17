using System;
using osum.GameplayElements.Scoring;
using osum.Graphics.Sprites;
using osum.Graphics.Skins;
using osum.Helpers;
using OpenTK;
using OpenTK.Graphics;
using System.Collections;
using System.Collections.Generic;
using osum.Audio;
using osum.GameModes.SongSelect;
using osum.Graphics;
using osu_common.Helpers;
using osum.GameplayElements;
using System.IO;
using osum.Resources;
using osum.UI;
using osu_common.Libraries.NetLib;
namespace osum.GameModes
{
    public class Results : GameMode
    {
        internal static Score RankableScore;

        List<pDrawable> fillSprites = new List<pDrawable>();
        List<pDrawable> fallingSprites = new List<pDrawable>();
        List<pDrawable> resultSprites = new List<pDrawable>();

        private BackButton s_ButtonBack;
        private pSprite s_Footer;
        private pSprite background;
        private pSprite rankingBackground;
        private pSprite modeGraphic;
        private pSprite rankGraphic;

        const int colour_change_length = 500;
        const int end_bouncing = 600;
        const int time_between_fills = 600;

        const float fill_height = 5;

        private pSpriteText countTotalScore;
        private pSpriteText countScoreHit;
        private pSpriteText countScoreCombo;
        private pSpriteText countScoreAccuracy;
        private pSpriteText countScoreSpin;

        SpriteManager layer1 = new SpriteManager();
        SpriteManager layer2 = new SpriteManager();
        SpriteManager topMostLayer = new SpriteManager();

        float offset;

        public override void Initialize()
        {
            background =
                new pSprite(TextureManager.Load(OsuTexture.songselect_background), FieldTypes.StandardSnapCentre, OriginTypes.Centre,
                            ClockTypes.Mode, Vector2.Zero, 0, true, new Color4(56, 56, 56, 255));

            background.AlphaBlend = false;
            spriteManager.Add(background);

            rankingBackground =
                new pSprite(TextureManager.Load(OsuTexture.ranking_background), FieldTypes.StandardSnapCentreLeft, OriginTypes.CentreLeft,
                            ClockTypes.Mode, Vector2.Zero, 0.4f, true, Color4.White);
            rankingBackground.Position = new Vector2(5, -20);
            rankingBackground.ScaleScalar = 0.85f;
            layer2.Add(rankingBackground);

            pText artist = new pText(Player.Beatmap.Artist, 30, new Vector2(10, fill_height + 5), 0.5f, true, Color4.OrangeRed) { TextShadow = true };
            layer1.Add(artist);
            pText title = new pText(Player.Beatmap.Title, 30, new Vector2(16 + artist.MeasureText().X / GameBase.BaseToNativeRatioAligned, fill_height + 5), 0.5f, true, Color4.White) { TextShadow = true };
            layer1.Add(title);

            pTexture modeTex;
            switch (Player.Difficulty)
            {
                case Difficulty.Easy:
                    modeTex = TextureManager.Load(OsuTexture.songselect_mode_easy);
                    break;
                case Difficulty.Expert:
                    modeTex = TextureManager.Load(OsuTexture.songselect_mode_expert);
                    break;
                default:
                    modeTex = TextureManager.Load(OsuTexture.songselect_mode_stream);
                    break;
            }

            modeGraphic = new pSprite(modeTex, FieldTypes.StandardSnapRight, OriginTypes.TopRight, ClockTypes.Mode, new Vector2(5, 7), 0.45f, true, Color4.White) { ScaleScalar = 0.5f };
            layer1.Add(modeGraphic);

            rankGraphic = new pSprite(RankableScore.RankingTexture, FieldTypes.StandardSnapBottomRight, OriginTypes.Centre, ClockTypes.Mode, new Vector2(120, 180), 0.46f, true, Color4.White) { Alpha = 0 };

            layer1.Add(rankGraphic);

            initializeTransition();

            //Scoring
            {
                float v_offset = -165;

                pText heading = new pText(LocalisationManager.GetString(OsuString.Score), 28, new Vector2(240, v_offset), 0.5f, true, Color4.White)
                {
                    Origin = OriginTypes.TopLeft,
                    Field = FieldTypes.StandardSnapCentreLeft,
                    Bold = true,
                };
                resultSprites.Add(heading);

                v_offset += 30;

                pSpriteText count = new pSpriteText("000,000", "score", 0, FieldTypes.StandardSnapCentreLeft, OriginTypes.TopRight, ClockTypes.Mode, new Vector2(445, v_offset), 0.9f, true, new Color4(255, 166, 0, 255));
                count.TextConstantSpacing = true;
                countTotalScore = count;

                resultSprites.Add(count);

                v_offset += 40;

                //Spin Bonus
                heading = new pText(LocalisationManager.GetString(OsuString.Hit), 20, new Vector2(240, v_offset), 0.5f, true, Color4.Gray)
                {
                    Origin = OriginTypes.TopLeft,
                    Field = FieldTypes.StandardSnapCentreLeft,
                    Bold = true
                };
                resultSprites.Add(heading);

                count = new pSpriteText("000000", "score", 0, FieldTypes.StandardSnapCentreLeft, OriginTypes.TopRight, ClockTypes.Mode, new Vector2(445, v_offset), 0.9f, true, new Color4(255, 166, 0, 255));
                count.TextConstantSpacing = true;
                count.ZeroAlpha = 0.5f;
                count.ScaleScalar = 0.7f;
                resultSprites.Add(count);

                countScoreHit = count;

                v_offset += 25;


                heading = new pText(LocalisationManager.GetString(OsuString.Combo), 20, new Vector2(240, v_offset), 0.5f, true, Color4.Gray)
                {
                    Origin = OriginTypes.TopLeft,
                    Field = FieldTypes.StandardSnapCentreLeft,
                    Bold = true
                };
                resultSprites.Add(heading);

                count = new pSpriteText("000000", "score", 0, FieldTypes.StandardSnapCentreLeft, OriginTypes.TopRight, ClockTypes.Mode, new Vector2(445, v_offset), 0.9f, true, new Color4(255, 166, 0, 255));
                count.TextConstantSpacing = true;
                count.ZeroAlpha = 0.5f;
                count.ScaleScalar = 0.7f;
                resultSprites.Add(count);

                countScoreCombo = count;

                v_offset += 25;

                heading = new pText(LocalisationManager.GetString(OsuString.Accuracy), 20, new Vector2(240, v_offset), 0.5f, true, Color4.Gray)
                {
                    Origin = OriginTypes.TopLeft,
                    Field = FieldTypes.StandardSnapCentreLeft,
                    Bold = true
                };
                resultSprites.Add(heading);

                count = new pSpriteText("000000", "score", 0, FieldTypes.StandardSnapCentreLeft, OriginTypes.TopRight, ClockTypes.Mode, new Vector2(445, v_offset), 0.9f, true, new Color4(255, 166, 0, 255));
                count.TextConstantSpacing = true;
                count.ZeroAlpha = 0.5f;
                count.ScaleScalar = 0.7f;
                resultSprites.Add(count);

                countScoreAccuracy = count;

                v_offset += 25;

                heading = new pText(LocalisationManager.GetString(OsuString.Spin), 20, new Vector2(240, v_offset), 0.5f, true, Color4.Gray)
                {
                    Origin = OriginTypes.TopLeft,
                    Field = FieldTypes.StandardSnapCentreLeft,
                    Bold = true
                };
                resultSprites.Add(heading);

                count = new pSpriteText("000000", "score", 0, FieldTypes.StandardSnapCentreLeft, OriginTypes.TopRight, ClockTypes.Mode, new Vector2(445, v_offset), 0.9f, true, new Color4(255, 166, 0, 255));
                count.TextConstantSpacing = true;
                count.ZeroAlpha = 0.5f;
                count.ScaleScalar = 0.7f;
                resultSprites.Add(count);

                countScoreSpin = count;

                v_offset += 30;

                heading = new pText(LocalisationManager.GetString(OsuString.Accuracy), 28, new Vector2(240, v_offset), 0.5f, true, Color4.White)
                {
                    Origin = OriginTypes.TopLeft,
                    Field = FieldTypes.StandardSnapCentreLeft,
                    Bold = true
                };
                resultSprites.Add(heading);

                v_offset += 34;

                count = new pSpriteText((RankableScore.accuracy * 100).ToString("00.00", GameBase.nfi) + "%", "score", 0, FieldTypes.StandardSnapCentreLeft, OriginTypes.TopRight, ClockTypes.Mode, new Vector2(445, v_offset), 0.9f, true, new Color4(0, 180, 227, 255));
                count.ScaleScalar = 0.7f;
                resultSprites.Add(count);

                v_offset += 20;

                heading = new pText(LocalisationManager.GetString(OsuString.MaxCombo), 28, new Vector2(240, v_offset), 0.5f, true, Color4.White)
                {
                    Origin = OriginTypes.TopLeft,
                    Field = FieldTypes.StandardSnapCentreLeft,
                    Bold = true
                };
                resultSprites.Add(heading);

                v_offset += 34;

                count = new pSpriteText(RankableScore.maxCombo.ToString("#,0", GameBase.nfi) + "x", "score", 0, FieldTypes.StandardSnapCentreLeft, OriginTypes.TopRight, ClockTypes.Mode, new Vector2(445, v_offset), 0.9f, true, new Color4(0, 180, 227, 255));
                count.ScaleScalar = 0.7f;
                resultSprites.Add(count);
            }

            {
                Vector2 pos = new Vector2(60, -130);
                Vector2 textOffset = new Vector2(150, 0);

                float spacing = 65;

                pSprite hitExplosion = new pSprite(TextureManager.Load(OsuTexture.hit300), pos) { Field = FieldTypes.StandardSnapCentreLeft, Origin = OriginTypes.Centre, ScaleScalar = 0.5f, DrawDepth = 0.9f };
                resultSprites.Add(hitExplosion);

                count300 = new pSpriteText("0x", "score", 0, FieldTypes.StandardSnapCentreLeft, OriginTypes.CentreRight, ClockTypes.Mode, pos + textOffset, 0.9f, true, Color4.White) { SpacingOverlap = 3, TextConstantSpacing = true };
                resultSprites.Add(count300);

                pos.Y += spacing;

                hitExplosion = new pSprite(TextureManager.Load(OsuTexture.hit100), pos) { Field = FieldTypes.StandardSnapCentreLeft, Origin = OriginTypes.Centre, ScaleScalar = 0.5f, DrawDepth = 0.9f };
                resultSprites.Add(hitExplosion);

                count100 = new pSpriteText("0x", "score", 0, FieldTypes.StandardSnapCentreLeft, OriginTypes.CentreRight, ClockTypes.Mode, pos + textOffset, 0.9f, true, Color4.White) { SpacingOverlap = 3, TextConstantSpacing = true };
                resultSprites.Add(count100);

                pos.Y += spacing;

                hitExplosion = new pSprite(TextureManager.Load(OsuTexture.hit50), pos) { Field = FieldTypes.StandardSnapCentreLeft, Origin = OriginTypes.Centre, ScaleScalar = 0.5f, DrawDepth = 0.9f };
                resultSprites.Add(hitExplosion);

                count50 = new pSpriteText("0x", "score", 0, FieldTypes.StandardSnapCentreLeft, OriginTypes.CentreRight, ClockTypes.Mode, pos + textOffset, 0.9f, true, Color4.White) { SpacingOverlap = 3, TextConstantSpacing = true };
                resultSprites.Add(count50);

                pos.Y += spacing;

                hitExplosion = new pSprite(TextureManager.Load(OsuTexture.hit0), pos) { Field = FieldTypes.StandardSnapCentreLeft, Origin = OriginTypes.Centre, ScaleScalar = 0.5f, DrawDepth = 0.9f };
                resultSprites.Add(hitExplosion);

                count0 = new pSpriteText("0x", "score", 0, FieldTypes.StandardSnapCentreLeft, OriginTypes.CentreRight, ClockTypes.Mode, pos + textOffset, 0.9f, true, Color4.White) { SpacingOverlap = 3, TextConstantSpacing = true };
                resultSprites.Add(count0);
            }

            if (Director.LastOsuMode == OsuMode.SongSelect)
                cameFromSongSelect = true;

            layer2.Add(resultSprites);

            s_ButtonBack = new BackButton(returnToSelect, false);
            s_ButtonBack.Alpha = 0;
            topMostLayer.Add(s_ButtonBack);

            s_Footer = new pSprite(TextureManager.Load(OsuTexture.ranking_footer), FieldTypes.StandardSnapBottomRight, OriginTypes.BottomRight, ClockTypes.Mode, new Vector2(0, -100), 0.98f, true, Color4.White);
            s_Footer.Alpha = 0;
            s_Footer.OnClick += delegate
            {
                Director.ChangeMode(OsuMode.Play);
                AudioEngine.PlaySample(OsuSamples.MenuHit);
            };
            topMostLayer.Add(s_Footer);

            if (!cameFromSongSelect)
            {
                InitializeBgm();

                //this is a bit of cheating to ensure that getting 100% will always result in 1mil. there are some race conditions with multitouch that may allow
                //for ever-so-slightly lower max scores, but this would piss people off.
                if (RankableScore.accuracy == 1 && RankableScore.totalScore - RankableScore.spinnerBonusScore != Score.MAX_SCORE)
                    RankableScore.comboBonusScore = Score.MAX_SCORE - RankableScore.accuracyBonusScore - RankableScore.hitScore;

                DifficultyScoreInfo bmi = BeatmapDatabase.GetDifficultyInfo(Player.Beatmap, Player.Difficulty);
                if (bmi.HighScore == null || RankableScore.totalScore > bmi.HighScore.totalScore)
                {
                    if (bmi.difficulty == Difficulty.Normal && RankableScore.Ranking >= Rank.A && (bmi.HighScore == null || bmi.HighScore.Ranking < Rank.A))
                        unlockedExpert = true;

                    isPersonalBest = true;
                    bmi.HighScore = RankableScore;
                    BeatmapDatabase.Write();
                }

                //Average Timing
                float avg = (float)RankableScore.hitOffsetMilliseconds / Math.Max(1, RankableScore.hitOffsetCount);
                pText heading = new pText(LocalisationManager.GetString(OsuString.AvgTiming) + avg + (RankableScore.hitOffsetMilliseconds > 0 ? "ms late" : "ms early"), 16, new Vector2(0, 20), 0.5f, true, Color4.White)
                {
                    Field = FieldTypes.StandardSnapBottomCentre,
                    Origin = OriginTypes.BottomCentre
                };
                layer1.Add(heading);

                int deviceType = 0;
#if iOS && DIST
                deviceType = (int)osum.Support.iPhone.HardwareDetection.Version;

                string check = CryptoHelper.GetMd5String("moocow" +
                    GameBase.Instance.DeviceIdentifier +
                    RankableScore.count100 +
                    RankableScore.count300 +
                    RankableScore.count50 +
                    RankableScore.countMiss +
                    RankableScore.maxCombo +
                    RankableScore.spinnerBonusScore +
                    RankableScore.comboBonusScore +
                    RankableScore.accuracyBonusScore +
                    RankableScore.Ranking +
                    Path.GetFileName(Player.Beatmap.ContainerFilename) +
                    deviceType +
                    RankableScore.hitScore +
                    (int)Player.Difficulty);

                string postString =
                    "udid=" + GameBase.Instance.DeviceIdentifier +
                    "&count300=" + RankableScore.count300 +
                    "&count100=" + RankableScore.count100 +
                    "&count50=" + RankableScore.count50 +
                    "&countMiss=" + RankableScore.countMiss +
                    "&maxCombo=" + RankableScore.maxCombo +
                    "&spinnerBonus=" + RankableScore.spinnerBonusScore +
                    "&comboBonus=" + RankableScore.comboBonusScore +
                    "&accuracyBonus=" + RankableScore.accuracyBonusScore +
                    "&hitScore=" + RankableScore.hitScore +
                    "&rank=" + RankableScore.Ranking +
                    "&filename=" + NetRequest.UrlEncode(Path.GetFileName(Player.Beatmap.ContainerFilename)) +
                    "&cc=" + GameBase.Config.GetValue<string>("hash", string.Empty) +
                    "&c=" + check +
                    "&difficulty=" + (int)Player.Difficulty +
                    "&username=" + GameBase.Config.GetValue<string>("username", string.Empty) +
                    "&dt=" + deviceType +
                    "&offset=" + avg;

                spriteSubmitting = new pSprite(TextureManager.Load(OsuTexture.songselect_audio_preview), FieldTypes.StandardSnapRight, OriginTypes.Centre, ClockTypes.Game, new Vector2(20, 20), 0.999f, true, Color4.White)
                {
                    ExactCoordinates = false,
                    DimImmune = true,
                    ScaleScalar = 0.7f
                };

                spriteSubmitting.Transform(new TransformationF(TransformationType.Rotation, 0, MathHelper.Pi * 2, Clock.Time, Clock.Time + 1500) { Looping = true });
                GameBase.MainSpriteManager.Add(spriteSubmitting);
                spriteSubmitting.FadeInFromZero(300);

                StringNetRequest nr = new StringNetRequest("http://www.osustream.com/score/submit.php", "POST", postString);
                nr.onFinish += delegate(string result, Exception e)
                {
                    spriteSubmitting.AlwaysDraw = false;
                    if (e == null)
                    {
                        spriteSubmitting.FadeOut(200);
                        spriteSubmitting.ScaleTo(3, 200);
                        spriteSubmitting.Colour = Color4.YellowGreen;
                    }
                    else
                    {
                        spriteSubmitting.FadeOut(1000);
                        spriteSubmitting.ScaleTo(1.2f, 200, EasingTypes.In);
                        spriteSubmitting.Colour = Color4.Red;
                    }

                    if (e == null && result != null && result.StartsWith("message:"))
                    {
                        rankingNotification = new Notification("Ranking", result.Replace("message:", string.Empty), NotificationStyle.Okay);
                        if (finishedDisplaying) GameBase.Notify(rankingNotification);
                    }
                };
                NetManager.AddRequest(nr);
#endif
            }
            else
            {
                if (!string.IsNullOrEmpty(RankableScore.Username))
                {
                    pText playerName = new pText("Played by " + RankableScore.Username, 30, new Vector2(10, 10), 0.5f, true, new Color4(255, 234, 0, 255))
                    {
                        TextShadow = true,
                        Field = FieldTypes.StandardSnapBottomRight,
                        Origin = OriginTypes.BottomRight
                    };

                    s_Footer.Bypass = true; //don't show footer. we need this room!

                    layer1.Add(playerName);
                }

                //displaying a previous high score (or online high score)
                finishDisplaying();
            }

            Director.OnTransitionEnded += Director_OnTransitionEnded;
            InputManager.OnMove += HandleInputManagerOnMove;
        }

        bool cameFromSongSelect;
        bool unlockedExpert;

        /// <summary>
        /// Initializes the song select BGM and starts playing. Static for now so it can be triggered from anywhere.
        /// </summary>
        internal void InitializeBgm()
        {
            //Start playing song select BGM.
#if iOS
            bool didLoad = AudioEngine.Music.Load("Skins/Default/results.m4a", true);
#else
            bool didLoad = AudioEngine.Music.Load("Skins/Default/results.mp3", true);
#endif
            if (didLoad)
                AudioEngine.Music.Play();
        }

        void HandleInputManagerOnMove(InputSource source, TrackingPoint trackingPoint)
        {
            if (InputManager.IsPressed && finishedDisplaying && !s_ButtonBack.IsHovering)
                offset += trackingPoint.WindowDelta.Y;
        }

        void Director_OnTransitionEnded()
        {
            //hit -> combo -> accuracy -> spin

            int time = cameFromSongSelect ? 0 : 500;
            int increment = cameFromSongSelect ? 0 : 500;

            GameBase.Scheduler.Add(delegate
            {
                AudioEngine.PlaySample(OsuSamples.RankingBam);
                countScoreHit.ShowInt(RankableScore.hitScore, 6, false);
                countScoreHit.AdditiveFlash(500, 1);

                addedScore += RankableScore.hitScore;
                countTotalScore.ShowInt(addedScore, 6, true);
                countTotalScore.FlashColour(Color4.White, 1000);
            }, time);

            time += increment;

            GameBase.Scheduler.Add(delegate
            {
                AudioEngine.PlaySample(OsuSamples.RankingBam);
                countScoreCombo.ShowInt(RankableScore.comboBonusScore, 6, false);
                countScoreCombo.AdditiveFlash(500, 1);


                addedScore += RankableScore.comboBonusScore;
                countTotalScore.ShowInt(addedScore, 6, true);
                countTotalScore.FlashColour(Color4.White, 1000);
            }, time);

            time += increment;

            GameBase.Scheduler.Add(delegate
            {
                AudioEngine.PlaySample(OsuSamples.RankingBam);
                countScoreAccuracy.ShowInt(RankableScore.accuracyBonusScore, 6, false);
                countScoreAccuracy.AdditiveFlash(500, 1);

                addedScore += RankableScore.accuracyBonusScore;
                countTotalScore.ShowInt(addedScore, 6, true);
                countTotalScore.FlashColour(Color4.White, 1000);
            }, time);

            time += increment;

            GameBase.Scheduler.Add(delegate
            {
                AudioEngine.PlaySample(OsuSamples.RankingBam);
                countScoreSpin.ShowInt(RankableScore.spinnerBonusScore, 6, false);
                countScoreSpin.AdditiveFlash(500, 1);

                addedScore += RankableScore.spinnerBonusScore;
                countTotalScore.ShowInt(addedScore, 6, true);
                countTotalScore.FlashColour(Color4.White, 1000);
            }, time);

            time += increment;

            GameBase.Scheduler.Add(delegate
            {
                if (RankableScore.Ranking == Rank.D)
                {
                    AudioEngine.PlaySample(OsuSamples.RankFail);
                    rankGraphic.FadeIn(2000);
                    rankGraphic.ScaleScalar = 0.7f;
                    rankGraphic.ScaleTo(1, 1400, EasingTypes.In);
                    GameBase.Scheduler.Add(delegate
                    {
                        int interval = 100;
                        rankGraphic.Transform(new TransformationF(TransformationType.Rotation, 0, 0.3f, Clock.Time, Clock.Time + interval));
                        rankGraphic.Transform(new TransformationF(TransformationType.Rotation, 0.3f, -0.3f, Clock.Time + interval, Clock.Time + interval * 3));
                        rankGraphic.Transform(new TransformationF(TransformationType.Rotation, -0.3f, 0, Clock.Time + interval * 3, Clock.Time + interval * 4));
                    }, 1550);
                }
                else
                {
                    AudioEngine.PlaySample(OsuSamples.RankPass);
                    rankGraphic.FadeIn(4000);

                    GameBase.Scheduler.Add(delegate
                    {
                        rankGraphic.Transformations.Clear();
                        rankGraphic.Alpha = 1;
                        rankGraphic.AdditiveFlash(1500, 1);
                    }, 1400);
                }
            }, time);

            time += increment + 1200;

            if (isPersonalBest)
            {
                GameBase.Scheduler.Add(delegate
                {
                    pSprite personalBest = new pSprite(TextureManager.Load(OsuTexture.personalbest), FieldTypes.StandardSnapBottomRight, OriginTypes.Centre, ClockTypes.Mode, new Vector2(80, 250),
                            1, true, Color4.White);
                    personalBest.FadeInFromZero(250);
                    personalBest.ScaleScalar = 1.6f;
                    personalBest.RotateTo(0.2f, 250);
                    personalBest.ScaleTo(1, 250, EasingTypes.Out);

                    GameBase.Scheduler.Add(delegate { personalBest.AdditiveFlash(1000, 1).ScaleTo(1.05f, 1000); }, 250);

                    layer1.Add(personalBest);
                }, time);
            }

            time += increment;

            if (!cameFromSongSelect)
                GameBase.Scheduler.Add(finishDisplaying, time);
        }

        private void finishDisplaying()
        {
            if (unlockedExpert)
            {
                GameBase.Notify(new Notification(LocalisationManager.GetString(OsuString.Congratulations), LocalisationManager.GetString(OsuString.UnlockedExpert), NotificationStyle.Okay, delegate
                {
                    unlockedExpert = false; //reset and run again.
                    finishDisplaying();
                }));
                return;
            }

            if (rankingNotification != null)
                GameBase.Notify(rankingNotification);
            finishedDisplaying = true;

                showNavigation();
        }

        private void showNavigation()
        {
            if (s_Footer.Alpha != 1)
            {
                s_ButtonBack.FadeIn(500);

                s_Footer.Alpha = 1;
                s_Footer.Transform(new TransformationV(new Vector2(-60, -85), Vector2.Zero, Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In));
                s_Footer.Transform(new TransformationF(TransformationType.Rotation, 0.04f, 0, Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In));
            }
        }

        bool finishedDisplaying;


        int addedScore;
        private pSpriteText count300;
        private pSpriteText count100;
        private pSpriteText count50;
        private pSpriteText count0;
        private bool isPersonalBest;

        private void initializeTransition()
        {
            pDrawable fill = pSprite.FullscreenWhitePixel;
            fill.Scale.X *= (float)RankableScore.count300 / RankableScore.totalHits + 0.001f;
            fill.Scale.Y = fill_height;
            fill.DrawDepth = 0.9f;
            fill.Alpha = 1;
            fill.AlwaysDraw = true;
            fill.Colour = new Color4(1, 0.63f, 0.01f, 1);
            fillSprites.Add(fill);

            fill = pSprite.FullscreenWhitePixel;
            fill.Position.X = fillSprites[fillSprites.Count - 1].Position.X + fillSprites[fillSprites.Count - 1].Scale.X;
            fill.Scale.X *= (float)RankableScore.count100 / RankableScore.totalHits + 0.001f;
            fill.Scale.Y = fill_height;
            fill.DrawDepth = 0.9f;
            fill.Alpha = 1;
            fill.AlwaysDraw = true;
            fill.Colour = new Color4(0.55f, 0.84f, 0, 1);
            fillSprites.Add(fill);

            fill = pSprite.FullscreenWhitePixel;
            fill.Position.X = fillSprites[fillSprites.Count - 1].Position.X + fillSprites[fillSprites.Count - 1].Scale.X;
            fill.Scale.X *= (float)RankableScore.count50 / RankableScore.totalHits + 0.001f;
            fill.Scale.Y = fill_height;
            fill.DrawDepth = 0.9f;
            fill.Alpha = 1;
            fill.AlwaysDraw = true;
            fill.Colour = new Color4(0.50f, 0.29f, 0.635f, 1);
            fillSprites.Add(fill);

            fill = pSprite.FullscreenWhitePixel;
            fill.Position.X = fillSprites[fillSprites.Count - 1].Position.X + fillSprites[fillSprites.Count - 1].Scale.X;
            fill.Scale.X *= (float)RankableScore.countMiss / RankableScore.totalHits + 0.001f;
            fill.Scale.Y = fill_height;
            fill.DrawDepth = 0.9f;
            fill.Alpha = 1;
            fill.AlwaysDraw = true;
            fill.Colour = new Color4(0.10f, 0.10f, 0.10f, 1);
            fillSprites.Add(fill);

            spriteManager.Add(fillSprites);
        }

        void returnToSelect(object sender, EventArgs args)
        {
            Director.ChangeMode(OsuMode.SongSelect);
        }

        public override void Dispose()
        {
            TextureManager.Dispose(OsuTexture.ranking_background);
            InputManager.OnMove -= HandleInputManagerOnMove;
            base.Dispose();
        }

        public Results()
        {
        }

        public override bool Draw()
        {
            base.Draw();
            layer1.Draw();
            layer2.Draw();
            topMostLayer.Draw();

            return true;
        }

        int frameCount = 0;
        private pSprite spriteSubmitting;
        private Notification rankingNotification;

        public override void Update()
        {
            if (!AudioEngine.Music.IsElapsing)
                InitializeBgm();

            if (!Director.IsTransitioning)
            {
                if (finishedDisplaying)
                {
                    if (!InputManager.IsPressed)
                        offset *= 0.90f;

                    float thisOffset = 0;
                    if (offset != 0)
                        thisOffset = (offset > 0 ? 1 : -1) * (float)Math.Pow(Math.Abs(offset), 0.8);


                    foreach (pDrawable p in fillSprites)
                        p.Scale.Y = fill_height + thisOffset * 0.5f;
                    layer1.Position.Y = thisOffset * 0.6f;
                    layer2.Position.Y = thisOffset;

                    layer1.ExactCoordinates = thisOffset == 0;
                    layer2.ExactCoordinates = thisOffset == 0;
                }

                fallingSprites.RemoveAll(p => p.Transformations.Count == 0);
                foreach (pSprite p in fallingSprites)
                {
                    p.Position.Y += p.TagNumeric * 0.003f * (float)Clock.ElapsedMilliseconds;
                    p.TagNumeric++;
                }

                if (fallingSprites.Count < 20 && frameCount++ % 5 == 0)
                {
                    float pos = (float)GameBase.Random.NextDouble() * GameBase.BaseSizeFixedWidth.Width;

                    pTexture tex = null;
                    if (pos < fillSprites[1].Position.X)
                        tex = TextureManager.Load(OsuTexture.hit300);
                    else if (pos < fillSprites[2].Position.X)
                        tex = TextureManager.Load(OsuTexture.hit100);
                    else if (pos < fillSprites[3].Position.X)
                        tex = TextureManager.Load(OsuTexture.hit50);

                    if (tex != null)
                    {
                        pSprite f = new pSprite(tex, FieldTypes.Standard, OriginTypes.TopLeft, ClockTypes.Mode, new Vector2(pos, fillSprites[0].Scale.Y - 30), 0.3f, false, Color4.White);
                        f.ScaleScalar = 0.2f;
                        f.Transform(new TransformationF(TransformationType.Fade, 0, 1, Clock.ModeTime, Clock.ModeTime + 150));
                        f.Transform(new TransformationF(TransformationType.Fade, 1, 0, Clock.ModeTime + 250, Clock.ModeTime + 1000 + (int)(GameBase.Random.NextDouble() * 1000)));
                        fallingSprites.Add(f);
                        spriteManager.Add(f);
                    }
                }
            }

            int increaseAmount = (int)Math.Max(1, Clock.ElapsedMilliseconds / 8);
            if (count300.LastInt < RankableScore.count300)
                count300.ShowInt(Math.Min(RankableScore.count300, count300.LastInt + increaseAmount), 0, false, 'x');
            else if (count100.LastInt < RankableScore.count100)
                count100.ShowInt(Math.Min(RankableScore.count100, count100.LastInt + increaseAmount), 0, false, 'x');
            else if (count50.LastInt < RankableScore.count50)
                count50.ShowInt(Math.Min(RankableScore.count50, count50.LastInt + increaseAmount), 0, false, 'x');
            else if (count0.LastInt < RankableScore.countMiss)
                count0.ShowInt(Math.Min(RankableScore.countMiss, count0.LastInt + increaseAmount), 0, false, 'x');

            base.Update();
            layer1.Update();
            layer2.Update();
            topMostLayer.Update();
        }
    }
}

