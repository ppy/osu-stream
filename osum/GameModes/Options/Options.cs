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
using osu_common.Libraries.NetLib;

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
                GameBase.Instance.ShowWebView("http://www.osustream.com/help/","Online Help");
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

            if (GameBase.Config.GetValue<string>("hash",null) == null)
            {
                button = new pButton("Link to Twitter", new Vector2(button_x_offset, vPos), new Vector2(280, 50), Color4.SkyBlue, delegate
                {
                    GameBase.Instance.ShowWebView("http://osustream.com/twitter/connect.php?udid=" + GameBase.Instance.DeviceIdentifier,
                    "Login with Twitter",
                    delegate (string url)
                    {
                        if (url.StartsWith("finished://"))
                        {
                            string[] split = url.Replace("finished://","").Split('/');

                            GameBase.Config.SetValue<string>("username",split[0]);
                            GameBase.Config.SetValue<string>("hash",split[1]);
                            GameBase.Config.SetValue<string>("twitterId",split[2]);
                            GameBase.Config.SaveConfig();

                            Director.ChangeMode(Director.CurrentOsuMode);
                            return true;
                        }
                        return false;
                    });
                });
                smd.Add(button);
            }
            else
            {
                button = new pButton("Unlink '" + GameBase.Config.GetValue<string>("username",null) + "'", new Vector2(button_x_offset, vPos), new Vector2(280, 50), Color4.SkyBlue, delegate
                {
                    StringNetRequest nr = new StringNetRequest("http://osustream.com/twitter/disconnect.php?udid="
                        + GameBase.Instance.DeviceIdentifier + "&cc=" + GameBase.Config.GetValue<string>("hash",null));
                    nr.onFinish += delegate(string _result, Exception e) {
                            GameBase.GloballyDisableInput = false;

                            if (e != null || _result != "success")
                                GameBase.Notify("An error occurred during unlinking. Please check your internet connection and try again");
                            else
                            {
                                GameBase.Config.SetValue<string>("username",null);
                                GameBase.Config.SetValue<string>("hash",null);
                                GameBase.Config.SetValue<string>("twitterId",null);
                                GameBase.Config.SaveConfig();

                                Director.ChangeMode(Director.CurrentOsuMode);
                            }
                    };

                    GameBase.GloballyDisableInput = true;

                    NetManager.AddRequest(nr);
                });

                smd.Add(button);
            }

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

