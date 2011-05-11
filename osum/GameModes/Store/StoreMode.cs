using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Graphics.Sprites;
using osum.GameModes.SongSelect;
using osu_common.Libraries.NetLib;
using System.IO;
using osu_common.Helpers;
using OpenTK;
using OpenTK.Graphics;
using osum.Graphics.Renderers;
using osum.Graphics.Drawables;

namespace osum.GameModes.Store
{
    class StoreMode : GameMode
    {
        private pText loading;
        private pRectangle loadingRect;
        private BackButton s_ButtonBack;
        internal override void Initialize()
        {
            s_ButtonBack = new BackButton(delegate { Director.ChangeMode(OsuMode.SongSelect); });
            spriteManager.Add(s_ButtonBack);

            StringNetRequest netRequest = new StringNetRequest("http://osu.ppy.sh/osum/");
            netRequest.onFinish += netRequest_onFinish;
            NetManager.AddRequest(netRequest);

            loading = new pText("Loading...", 36, Vector2.Zero, 1, true, Color4.OrangeRed)
            {
                TextAlignment = TextAlignment.Centre,
                Origin = OriginTypes.Centre,
                Field = FieldTypes.StandardSnapCentre,
                Bold = true
            };

            spriteManager.Add(loading);
        }

        void netRequest_onFinish(string _result, Exception e)
        {
            if (e != null || string.IsNullOrEmpty(_result))
            {
                GameBase.Notify("Error while downloading song listing.", delegate { Director.ChangeMode(OsuMode.SongSelect); });
				return;
            }

            int y = 0;

            foreach (string line in _result.Split('\n'))
            {
                string[] split = line.Split('\t');

                if (split.Length < 2) continue;

                string filename = split[0];
                string checksum = split[1];
				
                string path = SongSelectMode.BeatmapPath + "/" + filename;

				if (File.Exists(path))
				{
                    string checksumLocal = CryptoHelper.GetMd5(path);
                    if (checksumLocal == checksum) continue;
				}

                GameBase.Scheduler.Add(delegate {
                    pText text = new pText(filename, 20, new Vector2(10, 50 + y * 50), 0.5f, true, Color4.White);
                    text.BackgroundColour = Color4.SkyBlue;
                    text.TextShadow = true;
                    text.FadeInFromZero(200);

                    text.OnClick += delegate
                    {
                        FileNetRequest fnr = new FileNetRequest(path, "http://osu.ppy.sh/osum/" + filename);
                        fnr.onFinish += delegate
                        {
                            loadingRect.FadeOut(200);
                            loading.FadeOut(200);
                            text.FadeOut(200);
                            s_ButtonBack.FadeIn(200);
                        };
    
                        fnr.onUpdate += fnr_onUpdate;
                        NetManager.AddRequest(fnr);
    
                        s_ButtonBack.FadeOut(200);
    
                        loading.FadeIn(200);
                        loading.Text = "Starting download...";
    
                        loadingRect = new pRectangle(Vector2.Zero, new Vector2(GameBase.BaseSize.Width, GameBase.BaseSize.Height), true, 0.96f, Color4.Black);
                        loadingRect.FadeInFromZero(200);
                        spriteManager.Add(loadingRect);
                    };
    
                    spriteManager.Add(text);
                });

                y++;
            }
			
			loading.FadeOut(200);

            if (y == 0)
            {
                GameBase.Notify("You already have all available maps!", delegate { Director.ChangeMode(OsuMode.SongSelect); });
            }
        }

        void fnr_onUpdate(object sender, long current, long total)
        {
            if (loadingRect != null)
            {
                float completion = (float)current / total;
                loadingRect.Colour = new Color4(completion, completion, completion, 1.0f);
                loading.Text = "Downloading... " + Math.Round(completion * 100, 0) + "%";
            }
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override bool Draw()
        {
            return base.Draw();
        }

        public override void Update()
        {
            base.Update();
        }
    }
}
