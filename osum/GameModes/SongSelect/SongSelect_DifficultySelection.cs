using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using osum.Audio;
using osum.GameModes;
using osum.GameplayElements.Beatmaps;
using osum.Graphics.Sprites;
using osum.Graphics.Skins;
using osum.Helpers;
using osum.GameModes.SongSelect;
using OpenTK.Graphics;
using osum.GameModes.Play.Components;
using osum.Graphics.Drawables;
using osum.GameplayElements;
using System.Threading;
using osum.Graphics.Renderers;
using osu_common.Libraries.Osz2;
using osum.Resources;
using osum.GameplayElements.Scoring;
using osum.Graphics;

namespace osum.GameModes
{
    public partial class SongSelectMode : GameMode
    {
        private pSprite s_ModeButtonStream;

        private pSprite s_ModeArrowLeft;
        private pSprite s_ModeArrowRight;
        private pSprite s_ModeButtonEasy;
        private pSprite s_ModeButtonExpert;
        private pText s_ModeDescriptionText;

        private DifficultyScoreInfo bmi;

        /// <summary>
        /// True when expert mode is not yet unlocked for the current map.
        /// </summary>
        bool mapRequiresUnlock
        {
            get
            {
#if !DIST
                return false;
#else
                //todo: use the bmi field.
                BeatmapInfo bmi = BeatmapDatabase.GetBeatmapInfo(Player.Beatmap, Difficulty.Normal);
                return bmi.HighScore == null || bmi.HighScore.Ranking < Rank.A;
#endif
            }
        }

        private void showDifficultySelection()
        {
            if (State != SelectState.LoadingPreview && State != SelectState.SongInfo) return;

            if (!AudioEngine.Music.IsElapsing)
                playFromPreview();

            //do a second callback so we account for lost gametime due to the above audio load.
            //todo: can probably remove this.
            GameBase.Scheduler.Add(delegate
            {
                if (State != SelectState.LoadingPreview && State != SelectState.SongInfo) return;

                if (s_ModeButtonStream == null)
                    initializeDifficultySelection();

                s_ModeButtonExpert.Colour = mapRequiresUnlock ? Color4.Gray : Color4.White;

                //preview has finished loading.
                State = SelectState.DifficultySelect;

                foreach (pDrawable s in SelectedPanel.Sprites)
                    s.MoveTo(new Vector2(0, 0), 500, EasingTypes.InDouble);

                s_Header.Transform(new TransformationV(s_Header.Position, new Vector2(0, -63), Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In));
                s_Header.Transform(new TransformationF(TransformationType.Rotation, s_Header.Rotation, 0.03f, Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In));

                s_Footer.Transform(new TransformationV(s_Footer.Position, Vector2.Zero, Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In));
                s_Footer.Transform(new TransformationF(TransformationType.Rotation, s_Footer.Rotation, 0, Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In));
                s_Footer.Alpha = 1;

                s_SongInfo.Transform(new TransformationF(TransformationType.Fade, s_SongInfo.Alpha, 1, Clock.ModeTime + 500, Clock.ModeTime + 750));

                spriteManagerDifficultySelect.ScaleScalar = 1;
                spriteManagerDifficultySelect.Transformations.Clear();
                spriteManagerDifficultySelect.FadeInFromZero(250);

                SetDifficulty(GameBase.Config.GetValue<bool>("EasyMode", false) ? Difficulty.Easy : Difficulty.Normal, true);
            }, true);
        }

        private void playFromPreview()
        {
            AudioEngine.Music.DimmableVolume = 0;
            AudioEngine.Music.SeekTo(Player.Beatmap.PreviewPoint);
            AudioEngine.Music.Play();
        }

        private void initializeDifficultySelection()
        {
            const float yOffset = 0;

            s_ModeButtonStream = new pSprite(TextureManager.Load(OsuTexture.songselect_mode_stream), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, new Vector2(0, 0), 0.4f, true, Color4.White) { Offset = new Vector2(0, yOffset), HandleClickOnUp = true };
            s_ModeButtonStream.OnClick += onModeButtonClick;
            spriteManagerDifficultySelect.Add(s_ModeButtonStream);

            s_SongInfo = new pSprite(TextureManager.Load(OsuTexture.songselect_songinfo), FieldTypes.StandardSnapRight, OriginTypes.TopRight, ClockTypes.Mode, new Vector2(0, 0), 0.95f, true, Color4.White);
            s_SongInfo.Alpha = 0;
            s_SongInfo.OnClick += new EventHandler(onSongInfoClick);
            spriteManager.Add(s_SongInfo);

            s_ModeButtonEasy = new pSprite(TextureManager.Load(OsuTexture.songselect_mode_easy), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, new Vector2(0, 0), 0.4f, true, Color4.White) { Offset = new Vector2(-mode_button_width, yOffset), HandleClickOnUp = true };
            s_ModeButtonEasy.OnClick += onModeButtonClick;
            spriteManagerDifficultySelect.Add(s_ModeButtonEasy);

            s_ModeButtonExpert = new pSprite(TextureManager.Load(OsuTexture.songselect_mode_expert), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, new Vector2(0, 0), 0.4f, true, Color4.White) { Offset = new Vector2(mode_button_width, yOffset), HandleClickOnUp = true };
            s_ModeButtonExpert.OnClick += onModeButtonClick;
            spriteManagerDifficultySelect.Add(s_ModeButtonExpert);

            const float arrow_spread = 180;

            s_ModeArrowLeft = new pSprite(TextureManager.Load(OsuTexture.songselect_mode_arrow), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, new Vector2(-arrow_spread, yOffset), 0.45f, true, Color4.White);
            s_ModeArrowLeft.OnHover += delegate { s_ModeArrowLeft.ScaleTo(1.2f, 100, EasingTypes.In); };
            s_ModeArrowLeft.OnHoverLost += delegate { s_ModeArrowLeft.ScaleTo(1f, 100, EasingTypes.In); };
            s_ModeArrowLeft.OnClick += onSelectPreviousMode;

            spriteManagerDifficultySelect.Add(s_ModeArrowLeft);

            s_ModeArrowRight = new pSprite(TextureManager.Load(OsuTexture.songselect_mode_arrow), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, new Vector2(arrow_spread, yOffset), 0.45f, true, Color4.DarkGray);
            s_ModeArrowRight.OnHover += delegate { s_ModeArrowRight.ScaleTo(1.2f, 100, EasingTypes.In); };
            s_ModeArrowRight.OnHoverLost += delegate { s_ModeArrowRight.ScaleTo(1f, 100, EasingTypes.In); };
            s_ModeArrowRight.OnClick += onSelectNextMode;

            s_ModeArrowRight.Rotation = 1;
            spriteManagerDifficultySelect.Add(s_ModeArrowRight);

            s_ModeDescriptionText = new pText(string.Empty, 30, new Vector2(0, 110), 1, true, Color4.White) { Field = FieldTypes.StandardSnapCentre, Origin = OriginTypes.Centre, TextAlignment = TextAlignment.Centre };
            spriteManagerDifficultySelect.Add(s_ModeDescriptionText);

            s_ScoreInfo = new pText(null, 24, new Vector2(0, 64), Vector2.Zero, 1, true, Color4.White, true);
            s_ScoreInfo.OnClick += Handle_ScoreInfoOnClick;
            spriteManagerDifficultySelect.Add(s_ScoreInfo);

            s_ScoreRank = new pSprite(null, new Vector2(0, 72)) { DrawDepth = 0.95f };
            s_ScoreRank.OnClick += Handle_ScoreInfoOnClick;
            spriteManagerDifficultySelect.Add(s_ScoreRank);
        }

        void onSongInfoClick(object sender, EventArgs e)
        {
            s_SongInfo.AdditiveFlash(1000, 0.8f);
            AudioEngine.PlaySample(OsuSamples.MenuBling);

            if (State != SelectState.DifficultySelect)
                return;

            showSongInfo();
        }

        private void showSongInfo()
        {
            spriteManagerDifficultySelect.ScaleTo(0.5f, 300, EasingTypes.Out);
            spriteManagerDifficultySelect.FadeOut(300);
            background.FadeOut(300);

            SelectedPanel.Sprites.ForEach(s => s.MoveTo(new Vector2(0, -100), 400));

            spriteManagerSongInfo.Clear();

            spriteManagerSongInfo.Alpha = 0;
            spriteManagerSongInfo.Position = Vector2.Zero;
            spriteManagerSongInfo.Transformations.Clear();
            spriteManagerSongInfo.Transform(new TransformationBounce(Clock.Time + 200, Clock.Time + 700, 1, 0.5f, 2));
            spriteManagerSongInfo.Transform(new TransformationF(TransformationType.Fade, 0, 1, Clock.Time + 200, Clock.Time + 500));

            Beatmap beatmap = SelectedPanel.Beatmap;

            //256x172
            float aspectAdjust = GameBase.BaseSize.Height / (172 * GameBase.SpriteToBaseRatio);

            pSprite thumbSprite = new pSpriteDynamic()
            {
                LoadDelegate = delegate
                {
                    pTexture thumb = null;
                    byte[] bytes = beatmap.GetFileBytes("thumb-256.jpg");
                    if (bytes != null)
                        thumb = pTexture.FromBytes(bytes);
                    return thumb;
                },
                DrawDepth = 0.49f,
                Field = FieldTypes.StandardSnapCentre,
                Origin = OriginTypes.Centre,
                ScaleScalar = aspectAdjust,
                Alpha = 0.3f
            };
            spriteManagerSongInfo.Add(thumbSprite);

            float vPos = 60;

            string unicodeTitle = beatmap.Package.GetMetadata(MapMetaType.TitleUnicode);
            string normalTitle = beatmap.Title;

            if (unicodeTitle != normalTitle)
            {
                pText titleUnicode = new pText(unicodeTitle, 30, new Vector2(0, vPos), 1, true, Color4.White)
                {
                    Field = FieldTypes.StandardSnapTopCentre,
                    Origin = OriginTypes.Centre
                };
                spriteManagerSongInfo.Add(titleUnicode);
                vPos += 40;
            }

            pText title = new pText(normalTitle, 30, new Vector2(0, vPos), 1, true, Color4.LightYellow)
            {
                Field = FieldTypes.StandardSnapTopCentre,
                Origin = OriginTypes.Centre,
                TextShadow = true
            };
            spriteManagerSongInfo.Add(title);

            vPos += 40;

            string unicodeArtist = beatmap.Package.GetMetadata(MapMetaType.ArtistUnicode);

            pText artist = new pText("by " + beatmap.Package.GetMetadata(MapMetaType.ArtistFullName), 24, new Vector2(0, vPos), 1, true, Color4.LightYellow)
            {
                Field = FieldTypes.StandardSnapTopCentre,
                Origin = OriginTypes.Centre,
                TextShadow = true
            };
            spriteManagerSongInfo.Add(artist);

            vPos += 40;

            string artistTwitter = beatmap.Package.GetMetadata(MapMetaType.ArtistTwitter);
            string artistWeb = beatmap.Package.GetMetadata(MapMetaType.ArtistUrl);

            if (artistWeb != null)
            {
                pText info = new pText(artistWeb, 20, new Vector2(0, vPos), 1, true, Color4.SkyBlue)
                {
                    Field = FieldTypes.StandardSnapTopCentre,
                    Origin = OriginTypes.Centre
                };

                info.OnClick += delegate
                {
                    GameBase.Instance.OpenUrl(artistWeb);
                };
                spriteManagerSongInfo.Add(info);
                vPos += 40;
            }

            if (artistTwitter != null)
            {
                pText info = new pText(artistTwitter, 20, new Vector2(0, vPos), 1, true, Color4.SkyBlue)
                {
                    Field = FieldTypes.StandardSnapTopCentre,
                    Origin = OriginTypes.Centre
                };

                info.OnClick += delegate
                {
                    GameBase.Instance.OpenUrl(artistTwitter.Replace(@"@", @"http://twitter.com/"));
                };
                spriteManagerSongInfo.Add(info);
                vPos += 40;
            }

            string unicodeSource = beatmap.Package.GetMetadata(MapMetaType.SourceUnicode);
            string normalSource = beatmap.Package.GetMetadata(MapMetaType.Source);

            

            if (normalSource != null)
            {
                vPos += 40;
                pText source = new pText(normalSource, 24, new Vector2(0, vPos), 1, true, Color4.LightYellow)
                {
                    Field = FieldTypes.StandardSnapTopCentre,
                    Origin = OriginTypes.Centre,
                    TextShadow = true
                };
                spriteManagerSongInfo.Add(source);
            }

            if (normalSource != unicodeSource)
            {
                vPos += 40;
                pText source = new pText(unicodeSource, 24, new Vector2(0, vPos), 1, true, Color4.LightYellow)
                {
                    Field = FieldTypes.StandardSnapTopCentre,
                    Origin = OriginTypes.Centre,
                    TextShadow = true
                };
                spriteManagerSongInfo.Add(source);
            }

            pText mapper = new pText("Level design by " + beatmap.Creator, 18, new Vector2(0, 0), 1, true, Color4.White)
            {
                Field = FieldTypes.StandardSnapBottomCentre,
                Origin = OriginTypes.BottomCentre
            };
            spriteManagerSongInfo.Add(mapper);

            







            State = SelectState.SongInfo;

            footerHide();
        }

        private void hideSongInfo()
        {
            background.FadeIn(300);
            spriteManagerSongInfo.Transformations.Clear();
            spriteManagerSongInfo.FadeOut(400);
            spriteManagerSongInfo.MoveTo(new Vector2(0, 600), 1500, EasingTypes.Out);
        }

        private void footerHide()
        {
            s_Footer.Transformations.Clear();
            s_Footer.Transform(new TransformationV(s_Footer.Position, new Vector2(-60, -85), Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In));
            s_Footer.Transform(new TransformationF(TransformationType.Rotation, s_Footer.Rotation, 0.04f, Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In));
            s_Footer.Transform(new TransformationF(TransformationType.Fade, 1, 0, Clock.ModeTime + 500, Clock.ModeTime + 500));
        }

        void Handle_ScoreInfoOnClick(object sender, EventArgs e)
        {
            if (bmi == null || bmi.HighScore.totalScore == 0)
                return;

            AudioEngine.PlaySample(OsuSamples.MenuHit);
            Results.RankableScore = bmi.HighScore;
            Director.ChangeMode(OsuMode.Results);
        }

        void onModeButtonClick(object sender, EventArgs e)
        {
            if (State == SelectState.Starting) return;

            pDrawable d = sender as pDrawable;
            if (d == null) return;

            Difficulty newDifficulty;

            if (sender == s_ModeButtonEasy)
                newDifficulty = Difficulty.Easy;
            else if (sender == s_ModeButtonExpert)
                newDifficulty = Difficulty.Expert;
            else
                newDifficulty = Difficulty.Normal;

            if (newDifficulty == Player.Difficulty)
                onStartButtonPressed(sender, e);
            else
            {
                SetDifficulty(newDifficulty);
            }
        }

        private void SetDifficulty(Difficulty newDifficulty, bool force = false)
        {
            bool isNewDifficulty = Player.Difficulty != newDifficulty || force;
            velocity = 0;

            if (isNewDifficulty)
            {
                string versions = Player.Beatmap.Package.GetMetadata(MapMetaType.Version);
                if (versions != null && !versions.Contains(newDifficulty.ToString()))
                {

                    if (Player.Difficulty == Difficulty.Easy)
                        //came from easy -> expert; drop back on normal!
                        Player.Difficulty = Difficulty.Normal;
                    else
                    {
                        isNewDifficulty = false;
                        pendingModeChange = false;
                    }
                }
                else if (newDifficulty == Difficulty.Expert && mapRequiresUnlock)
                {
                    if (Player.Difficulty == Difficulty.Easy)
                        //came from easy -> expert; drop back on normal!
                        Player.Difficulty = Difficulty.Normal;
                    else
                    {
                        isNewDifficulty = false;
                        GameBase.Notify(LocalisationManager.GetString(OsuString.ExpertUnlock), delegate { pendingModeChange = false; });
                    }


                }
                else
                    Player.Difficulty = newDifficulty;
            }

            updateModeSelectionArrows(isNewDifficulty);
        }

        void onSelectPreviousMode(object sender, EventArgs e)
        {

            if (State == SelectState.Starting) return;

            AudioEngine.PlaySample(OsuSamples.ButtonTap);

            switch (Player.Difficulty)
            {
                case Difficulty.Normal:
                    SetDifficulty(Difficulty.Easy);
                    break;
                case Difficulty.Expert:
                    SetDifficulty(Difficulty.Normal);
                    break;
            }
        }

        void onSelectNextMode(object sender, EventArgs e)
        {
            if (State == SelectState.Starting) return;

            AudioEngine.PlaySample(OsuSamples.ButtonTap);

            switch (Player.Difficulty)
            {
                case Difficulty.Easy:
                    SetDifficulty(Difficulty.Normal);
                    break;
                case Difficulty.Normal:
                    SetDifficulty(Difficulty.Expert);
                    break;
            }
        }

        const float mode_button_width = 300;
        private pText s_ScoreInfo;
        private pSprite s_ScoreRank;
        private pSprite s_SongInfo;

        /// <summary>
        /// Updates the states of mode selection arrows depending on the current mode selection.
        /// </summary>
        private void updateModeSelectionArrows(bool isNewDifficulty = true)
        {
            bool hasPrevious = false;
            bool hasNext = false;

            string text = null;

            velocity = 0;

            switch (Player.Difficulty)
            {
                case Difficulty.Easy:
                    hasNext = true;
                    difficultySelectOffset = mode_button_width;
                    text = LocalisationManager.GetString(OsuString.YouCantFail);
                    background.FadeColour(new Color4(110, 110, 110, 255), 500);
                    break;
                case Difficulty.Normal:
                    hasPrevious = true;
                    hasNext = !mapRequiresUnlock;
                    difficultySelectOffset = 0;
                    text = LocalisationManager.GetString(OsuString.DynamicStreamSwitching);
                    background.FadeColour(new Color4(70, 70, 70, 255), 500);
                    break;
                case Difficulty.Expert:
                    hasPrevious = true;
                    difficultySelectOffset = -mode_button_width;
                    text = LocalisationManager.GetString(OsuString.NotForTheFaintHearted);
                    background.FadeColour(new Color4(30, 30, 30, 255), 500);
                    break;
            }

            s_ModeArrowLeft.Colour = hasPrevious ? Color4.White : Color4.DarkGray;
            s_ModeArrowRight.Colour = hasNext ? Color4.White : Color4.DarkGray;

            if (isNewDifficulty)
            {
                if (s_ModeDescriptionText.Text != text)
                    s_ModeDescriptionText.FadeOut(100);

                GameBase.Scheduler.Add(delegate
                {
                    //could have hit the back button really fast.
                    if (State == SelectState.DifficultySelect)
                    {
                        if (s_ModeDescriptionText.Text != text)
                        {
                            s_ModeDescriptionText.Text = text;
                            s_ModeDescriptionText.FadeInFromZero(300);
                        }

                        bmi = BeatmapDatabase.GetDifficultyInfo(Player.Beatmap, Player.Difficulty);
                        s_ScoreInfo.Transform(new TransformationBounce(Clock.ModeTime, Clock.ModeTime + 200, 1, 0.05f, 2));
                        s_ScoreInfo.Text = LocalisationManager.GetString(OsuString.PlayCount) + " " + bmi.Playcount.ToString().PadLeft(3, '0') + '\n' + LocalisationManager.GetString(OsuString.HighScore) + " ";
                        s_ScoreInfo.Text += bmi.HighScore.totalScore.ToString().PadLeft(6, '0');
                        s_ScoreRank.Texture = bmi.HighScore.RankingTextureSmall;

                        if (s_ScoreRank.Texture != null)
                        {
                            s_ScoreRank.AdditiveFlash(500, 0.5f);
                            s_ScoreInfo.MoveTo(new Vector2(40, 64), 200, EasingTypes.In);
                        }
                        else
                            s_ScoreInfo.MoveTo(new Vector2(0, 64), 200, EasingTypes.In);
                    }
                }, 100);
            }
        }

        private void leaveDifficultySelection(object sender, EventArgs args)
        {
            touchingBegun = false;
            velocity = 0;

            State = SelectState.SongSelect;

            InitializeBgm();

            GameBase.Scheduler.Add(delegate
            {
                background.FadeColour(new Color4(56, 56, 56, 255), 200);

                if (SelectedPanel != null)
                    SelectedPanel.s_BackingPlate2.FadeColour(Color4.Transparent, 150);

                foreach (BeatmapPanel p in panels)
                {
                    p.s_BackingPlate.HandleInput = true;

                    foreach (pDrawable d in p.Sprites)
                        d.FadeIn(500);
                }

                spriteManagerDifficultySelect.FadeOut(250);

                s_Header.Transform(new TransformationV(s_Header.Position, Vector2.Zero, Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In));
                s_Header.Transform(new TransformationF(TransformationType.Rotation, s_Header.Rotation, 0, Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In));

                s_SongInfo.FadeOut(250);

                footerHide();
            }, true);
        }

        private void onStartButtonPressed(object sender, EventArgs args)
        {
            if (State == SelectState.Starting) return;

            AudioEngine.PlaySample(OsuSamples.MenuHit);

            State = SelectState.Starting;

            pDrawable activatedSprite = null;

            switch (Player.Difficulty)
            {
                case Difficulty.Easy:
                    s_ModeButtonStream.FadeOut(200);
                    s_ModeButtonExpert.FadeOut(200);
                    activatedSprite = s_ModeButtonEasy;
                    break;
                case Difficulty.Normal:
                    s_ModeButtonEasy.FadeOut(200);
                    s_ModeButtonExpert.FadeOut(200);
                    activatedSprite = s_ModeButtonStream;
                    break;
                case Difficulty.Expert:
                    s_ModeButtonStream.FadeOut(200);
                    s_ModeButtonEasy.FadeOut(200);
                    activatedSprite = s_ModeButtonExpert;
                    break;
            }

            activatedSprite.Transform(new TransformationBounce(Clock.ModeTime, Clock.ModeTime + 500, 1.2f, 0.4f, 1));

            activatedSprite.AdditiveFlash(800, 0.8f).Transform(new TransformationF(TransformationType.Scale, 1, 1.5f, Clock.Time, Clock.Time + 800, EasingTypes.In));

            background.FlashColour(new Color4(200, 200, 200, 255), 800);

            s_ModeArrowLeft.FadeOut(200);
            s_ModeArrowRight.FadeOut(200);

            s_Footer.AdditiveFlash(500, 0.5f);

            GameBase.Scheduler.Add(delegate
            {
                Director.ChangeMode(OsuMode.Play);
            }, 800);
        }
    }
}
