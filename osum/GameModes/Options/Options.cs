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

            int vPos = 10;

            pText text = new pText("About", 36, new Vector2(10, vPos), 1, true, Color4.YellowGreen) { Bold = true };
            smd.Add(text);

            vPos += 90;

            pButton button = new pButton("Credits", new Vector2(320, vPos), new Vector2(280, 50), Color4.SkyBlue, delegate
            {
                Director.ChangeMode(OsuMode.Credits);
            });
            smd.Add(button);

            vPos += 70;

            button = new pButton("Online Help", new Vector2(320, vPos), new Vector2(280, 50), Color4.SkyBlue, delegate
            {
                Process.Start("http://www.osustream.com");
            });
            smd.Add(button);

            vPos += 60;

            text = new pText("Difficulty", 36, new Vector2(10, vPos), 1, true, Color4.YellowGreen) { Bold = true };
            smd.Add(text);

            vPos += 90;

            button = new pButton("Finger Guide Display", new Vector2(320, vPos), new Vector2(280, 50), Color4.SkyBlue, delegate
            {
                DisplayFingerGuideDialog();
            });
            smd.Add(button);

            vPos += 70;

            button = new pButton("Easy Mode Default", new Vector2(320, vPos), new Vector2(280, 50), Color4.SkyBlue, delegate
            {
                DisplayEasyModeDialog();
            });
            smd.Add(button);

            vPos += 60;

            text = new pText("Audio", 36, new Vector2(10, vPos), 1, true, Color4.YellowGreen) { Bold = true };
            smd.Add(text);

            vPos += 60;

            soundEffectSlider = new SliderControl("Effect Volume", AudioEngine.Effect.Volume, new Vector2(GameBase.BaseSizeFixedWidth.Width / 2, vPos),
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

            soundEffectSlider = new SliderControl("Music Volume", AudioEngine.Music.MaxVolume, new Vector2(GameBase.BaseSizeFixedWidth.Width / 2, vPos),
                delegate(float v) { AudioEngine.Music.MaxVolume = v; });
            smd.Add(soundEffectSlider);

            vPos += 50;

            text = new pText("Scoring", 36, new Vector2(10, vPos), 1, true, Color4.YellowGreen) { Bold = true };
            smd.Add(text);

            vPos += 50;

            gameCentre = new pSprite(TextureManager.Load(OsuTexture.gamecentre), new Vector2(50, vPos));
            gameCentre.OnClick += delegate
            {
                OnlineHelper.Initialize();
            };
            smd.Add(gameCentre);

            vPos += 20;
            text = new pText(OnlineHelper.Available ? "You are logged in!" : "Tap to login to Game Centre!", 24, new Vector2(180, vPos), 1, true, Color4.White);
            smd.Add(text);

            vPos += 50;
        }

        int lastEffectSound;

        internal static void DisplayFingerGuideDialog()
        {
            Notification notification = new Notification(osum.Resources.Tutorial.UseFingerGuides, osum.Resources.Tutorial.UseGuideFingers_Explanation,
                        NotificationStyle.YesNo,
                        delegate(bool yes) { GameBase.Config.SetValue<bool>(@"GuideFingers", yes); });
            GameBase.Notify(notification);
        }

        internal static void DisplayEasyModeDialog()
        {
            Notification notification = new Notification(osum.Resources.Tutorial.DefaultToEasyMode, osum.Resources.Tutorial.DefaultToEasyMode_Explanation,
                        NotificationStyle.YesNo,
                        delegate(bool yes) { GameBase.Config.SetValue<bool>(@"EasyMode", yes); });
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

