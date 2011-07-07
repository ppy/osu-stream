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

        public override void Initialize()
        {
            pDrawable background =
                new pSprite(TextureManager.Load(OsuTexture.songselect_background), FieldTypes.StandardSnapCentre, OriginTypes.Centre,
                            ClockTypes.Mode, Vector2.Zero, 0, true, new Color4(56, 56, 56, 255));
            background.AlphaBlend = false;
            spriteManager.Add(background);

            s_ButtonBack = new BackButton(delegate { Director.ChangeMode(OsuMode.MainMenu); s_ButtonBack.DimImmune = false; }, Director.LastOsuMode == OsuMode.MainMenu);
            if (Director.LastOsuMode != OsuMode.MainMenu) s_ButtonBack.DimImmune = true;
            smd.AddNonDraggable(s_ButtonBack);

            if (MainMenu.InitializeBgm())
                AudioEngine.Music.Play();

            int vPos = 10;

            pText text = new pText("About", 36, new Vector2(10, vPos), 1, true, Color4.YellowGreen) { Bold = true };
            smd.Add(text);

            vPos += 90;

            pButton button = new pButton("Credits", new Vector2(320, vPos), new Vector2(280, 50), Color4.SkyBlue, delegate
            {
                s_ButtonBack.DimImmune = true;
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

            vPos += 40;

            text = new pText("Sound effect volume", 24, new Vector2(180, vPos), 1, true, Color4.White);
            smd.Add(text);

            vPos += 40;

            text = new pText("Music volume", 24, new Vector2(180, vPos), 1, true, Color4.White);
            smd.Add(text);

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

