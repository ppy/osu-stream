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
                            ClockTypes.Mode, Vector2.Zero, 0, true, new Color4(56,56,56,255));
            background.AlphaBlend = false;
            spriteManager.Add(background);

            s_ButtonBack = new BackButton (delegate { Director.ChangeMode(OsuMode.MainMenu); }, Director.LastOsuMode == OsuMode.MainMenu);
            smd.AddNonDraggable(s_ButtonBack);

            if (MainMenu.InitializeBgm())
                AudioEngine.Music.Play();

            int vPos = 10;

            pText text = new pText("About", 36, new Vector2(10,vPos),1, true, Color4.YellowGreen) { Bold = true };
            smd.Add(text);

            vPos += 50;

            pButton button = new pButton("Credits", new Vector2(30,vPos), new Vector2(280,50), Color4.SkyBlue, delegate {
                Director.ChangeMode(OsuMode.Credits);
            });
            smd.Add(button);

            button = new pButton("Online Help", new Vector2(330,vPos), new Vector2(280,50), Color4.SkyBlue, delegate {
                Process.Start("http://www.osustream.com");
            });
            smd.Add(button);

            vPos += 60;

            text = new pText("Difficulty", 36, new Vector2(10,vPos),1, true, Color4.YellowGreen) { Bold = true };
            smd.Add(text);

            vPos += 40;

            text = new pText("Finger guide display", 24, new Vector2(180,vPos),1, true, Color4.White);
            smd.Add(text);

            vPos += 40;

            text = new pText("Easy mode default", 24, new Vector2(180,vPos),1, true, Color4.White);
            smd.Add(text);

            vPos += 50;

            text = new pText("Audio", 36, new Vector2(10,vPos),1, true, Color4.YellowGreen) { Bold = true };
            smd.Add(text);

            vPos += 40;

            text = new pText("Sound effect volume", 24, new Vector2(180,vPos),1, true, Color4.White);
            smd.Add(text);

            vPos += 40;

            text = new pText("Music volume", 24, new Vector2(180,vPos),1, true, Color4.White);
            smd.Add(text);

            vPos += 50;

            text = new pText("Scoring", 36, new Vector2(10,vPos),1, true, Color4.YellowGreen) { Bold = true };
            smd.Add(text);

            vPos += 50;

            gameCentre = new pSprite (TextureManager.Load(OsuTexture.gamecentre), new Vector2(50,vPos));
            gameCentre.OnClick += delegate {
                OnlineHelper.Initialize();
            };
            smd.Add(gameCentre);

            vPos += 20;
            text = new pText(OnlineHelper.Available ? "You are logged in!" : "Tap to login to Game Centre!", 24, new Vector2(180,vPos),1, true, Color4.White);
            smd.Add(text);

            vPos += 50;


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

