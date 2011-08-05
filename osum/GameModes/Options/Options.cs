using System;
using osum.GameModes.SongSelect;
using osum.Graphics.Skins;
using OpenTK;
using osum.Online;
using osum.Graphics.Sprites;
using OpenTK.Graphics.ES11;
using OpenTK.Graphics;
using osum.Helpers;
using System.Diagnostics;
using osum.Audio;
using osum.UI;
using osum.Resources;

namespace osum.GameModes.Options
{
    public class Options : GameMode
    {
        BackButton s_ButtonBack;
        pSprite gameCentre;
        SpriteManagerDraggable smd = new SpriteManagerDraggable()
        {
            ShowScrollbar = true
        };
        private SliderControl soundEffectSlider;

        public override void Initialize()
        {
            pDrawable background =
                new pSprite(TextureManager.Load(OsuTexture.songselect_background), FieldTypes.StandardSnapCentre, OriginTypes.Centre,
                            ClockTypes.Mode, Vector2.Zero, 0, true, new Color4(56, 56, 56, 255));
            background.AlphaBlend = false;
            spriteManager.Add(background);

            s_ButtonBack = new BackButton(delegate { Director.ChangeMode(OsuMode.MainMenu); }, Director.LastOsuMode == OsuMode.MainMenu);
            smd.AddNonDraggable(s_ButtonBack);

            if (MainMenu.InitializeBgm())
                AudioEngine.Music.Play();

            const int header_x_offset = 60;

            int button_x_offset = GameBase.BaseSize.Width / 2;

            int vPos = 10;

            pText text = new pText(LocalisationManager.GetString(OsuString.About), 36, new Vector2(header_x_offset, vPos), 1, true, Color4.White) { Bold = true };
            smd.Add(text);

            vPos += 90;

            pButton button = new pButton(LocalisationManager.GetString(OsuString.Credits), new Vector2(button_x_offset, vPos), new Vector2(280, 50), Color4.SkyBlue, delegate
            {
                Director.ChangeMode(OsuMode.Credits);
            });
            smd.Add(button);

            vPos += 70;

            button = new pButton(LocalisationManager.GetString(OsuString.OnlineHelp), new Vector2(button_x_offset, vPos), new Vector2(280, 50), Color4.SkyBlue, delegate
            {
                GameBase.Instance.OpenUrl("http://www.osustream.com/help/");
            });

            smd.Add(button);

            vPos += 60;

            text = new pText(LocalisationManager.GetString(OsuString.DifficultySettings), 36, new Vector2(header_x_offset, vPos), 1, true, Color4.White) { Bold = true };
            smd.Add(text);

            vPos += 90;

            buttonFingerGuides = new pButton(LocalisationManager.GetString(OsuString.UseFingerGuides), new Vector2(button_x_offset, vPos), new Vector2(280, 50), Color4.SkyBlue, delegate
            {
                DisplayFingerGuideDialog();
            });
            smd.Add(buttonFingerGuides);

            vPos += 70;

            buttonEasyMode = new pButton(LocalisationManager.GetString(OsuString.DefaultToEasyMode), new Vector2(button_x_offset, vPos), new Vector2(280, 50), Color4.SkyBlue, delegate
            {
                DisplayEasyModeDialog();
            });
            smd.Add(buttonEasyMode);

            vPos += 60;

            text = new pText(LocalisationManager.GetString(OsuString.Audio), 36, new Vector2(header_x_offset, vPos), 1, true, Color4.White) { Bold = true };
            smd.Add(text);

            vPos += 80;

            soundEffectSlider = new SliderControl(LocalisationManager.GetString(OsuString.EffectVolume), AudioEngine.Effect.Volume, new Vector2(button_x_offset, vPos),
                delegate(float v)
                {
                    AudioEngine.Effect.Volume = v;
                    if (Clock.ModeTime / 200 != lastEffectSound)
                    {
                        lastEffectSound = Clock.ModeTime / 200;
                        switch (lastEffectSound % 4)
                        {
                            case 0:
                                AudioEngine.PlaySample(OsuSamples.HitNormal);
                                break;
                            case 1:
                            case 3:
                                AudioEngine.PlaySample(OsuSamples.HitWhistle);
                                break;
                            case 2:
                                AudioEngine.PlaySample(OsuSamples.HitFinish);
                                break;

                        }
                    }
                });
            smd.Add(soundEffectSlider);

            vPos += 60;

            soundEffectSlider = new SliderControl(LocalisationManager.GetString(OsuString.MusicVolume), AudioEngine.Music.MaxVolume, new Vector2(button_x_offset, vPos),
                delegate(float v) { AudioEngine.Music.MaxVolume = v; });
            smd.Add(soundEffectSlider);

            vPos += 50;

            text = new pText(LocalisationManager.GetString(OsuString.OnlineOptions), 36, new Vector2(header_x_offset, vPos), 1, true, Color4.White) { Bold = true };
            smd.Add(text);

            vPos += 80;

            text = new pText("Coming soon!", 24, new Vector2(0, vPos), new Vector2(GameBase.BaseSize.Width * 0.9f, 0), 1, true, Color4.White, false)
            {
                Field = FieldTypes.StandardSnapTopCentre,
                Origin = OriginTypes.Centre
            };
            text.MeasureText();
            smd.Add(text);

            /*gameCentre = new pSprite(TextureManager.Load(OsuTexture.gamecentre), new Vector2(0, vPos))
            {
                Field = FieldTypes.StandardSnapTopCentre,
                Origin = OriginTypes.Centre
            };

            gameCentre.OnClick += delegate {  OnlineHelper.Initialize(true); };
            smd.Add(gameCentre);

            vPos += 60;
            text = new pText(LocalisationManager.GetString(OnlineHelper.Available ? OsuString.GameCentreLoggedIn : OsuString.GameCentreTapToLogin), 24, new Vector2(0, vPos), 1, true, Color4.White)
            {
                Field = FieldTypes.StandardSnapTopCentre,
                Origin = OriginTypes.Centre
            };
            smd.Add(text);*/

            UpdateButtons();

            vPos += 50;
        }

        int lastEffectSound;
        private pButton buttonFingerGuides;
        private pButton buttonEasyMode;

        internal static void DisplayFingerGuideDialog()
        {
            Notification notification = new Notification(LocalisationManager.GetString(OsuString.UseFingerGuides), LocalisationManager.GetString(OsuString.UseGuideFingers_Explanation),
                        NotificationStyle.YesNo,
                        delegate(bool yes)
                        {
                            GameBase.Config.SetValue<bool>(@"GuideFingers", yes);

                            Options o = Director.CurrentMode as Options;
                            if (o != null) o.UpdateButtons();
                        });
            GameBase.Notify(notification);
        }

        private void UpdateButtons()
        {
            buttonEasyMode.Colour = GameBase.Config.GetValue<bool>(@"EasyMode", false) ? Color4.White : new Color4(255, 100, 100, 255);
            buttonFingerGuides.Colour = GameBase.Config.GetValue<bool>(@"GuideFingers", false) ? Color4.White : new Color4(255, 100, 100, 255);
        }

        internal static void DisplayEasyModeDialog()
        {
            Notification notification = new Notification(LocalisationManager.GetString(OsuString.DefaultToEasyMode), LocalisationManager.GetString(OsuString.DefaultToEasyMode_Explanation),
                        NotificationStyle.YesNo,
                        delegate(bool yes)
                        {
                            GameBase.Config.SetValue<bool>(@"EasyMode", yes);

                            Options o = Director.CurrentMode as Options;
                            if (o != null) o.UpdateButtons();
                        });
            GameBase.Notify(notification);
        }

        public override void Dispose()
        {
            GameBase.Config.SetValue<int>("VolumeEffect", (int)(AudioEngine.Effect.Volume * 100));
            GameBase.Config.SetValue<int>("VolumeMusic", (int)(AudioEngine.Music.MaxVolume * 100));
            GameBase.Config.SaveConfig();

            smd.Dispose();
            base.Dispose();
        }

        public override bool Draw()
        {
            base.Draw();
            smd.Draw();
            return true;
        }

        public override void Update()
        {
            smd.Update();
            base.Update();
        }
    }
}

