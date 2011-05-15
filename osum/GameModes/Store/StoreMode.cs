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

        List<PackPanel> packs = new List<PackPanel>();

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

            InputManager.OnMove += new Helpers.InputHandler(InputManager_OnMove);
        }

        void InputManager_OnMove(InputSource source, TrackingPoint trackingPoint)
        {
            if (!InputManager.IsPressed || InputManager.PrimaryTrackingPoint == null) return;
            {
                float change = InputManager.PrimaryTrackingPoint.WindowDelta.Y;
                float bound = offsetBound;

                if ((scrollOffset - bound < 0 && change < 0) || (scrollOffset - bound > 0 && change > 0))
                    change *= Math.Min(1, 10 / Math.Max(0.1f, Math.Abs(scrollOffset - bound)));
                scrollOffset = scrollOffset + change;
                velocity = change;
            }
        }

        void netRequest_onFinish(string _result, Exception e)
        {
            if (e != null || string.IsNullOrEmpty(_result))
            {
                GameBase.Notify("Error while downloading song listing.", delegate { Director.ChangeMode(OsuMode.SongSelect); });
                return;
            }

            int y = 0;


            PackPanel pp = null;

            int i = 0;

            foreach (string line in _result.Split('\n'))
            {
                string[] split = line.Split('\t');

                if (split.Length < 2) continue;

                string filename = split[0];
                string checksum = split[1];

                string path = SongSelectMode.BeatmapPath + "/" + filename;

                if (i++ % 3 == 0)
                {
                    GameBase.Scheduler.Add(delegate
                    {
                        if (pp != null)
                        {
                            spriteManager.Add(pp);
                            packs.Add(pp);
                        }

                        pp = new PackPanel("Free Pack #" + (i / 3 + 1), "Free", delegate { });
                    });
                }

                if (File.Exists(path))
                {
                    string checksumLocal = CryptoHelper.GetMd5(path);
                    if (checksumLocal == checksum) continue;
                }

                int thisY = y;

                GameBase.Scheduler.Add(delegate
                {

                    pp.Add(filename);


                    /*pText text = new pText(filename, 20, new Vector2(10, 50 + thisY * 50), 0.5f, true, Color4.White);
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
    
                    spriteManager.Add(text);*/
                });

                y++;
            }

            GameBase.Scheduler.Add(delegate { spriteManager.Add(pp); packs.Add(pp); });

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

        public static void ResetAllPreviews()
        {
            StoreMode instance = Director.CurrentMode as StoreMode;
            if (instance == null) return;

            foreach (PackPanel p in instance.packs)
                p.ResetPreviews();
        }

        private float offset_min
        {
            get
            {
                if (packs.Count == 0)
                    return 0;

                float totalHeight = 0;
                foreach (PackPanel p in packs)
                    totalHeight += p.Height;

                return -totalHeight + GameBase.BaseSize.Height - 80;
            }
        }
        private float offset_max = 0;
        float scrollOffset = 0;
        private float velocity;
        /// <summary>
        /// Offset bound to visible limits.
        /// </summary>
        private float offsetBound
        {
            get
            {
                return Math.Min(offset_max, Math.Max(offset_min, scrollOffset));
            }
        }

        public override void Update()
        {
            if (!InputManager.IsPressed)
            {
                float bound = offsetBound;

                scrollOffset = scrollOffset * 0.8f + bound * 0.2f + velocity;

                if (scrollOffset != bound)
                    velocity *= 0.7f;
                else
                    velocity *= 0.94f;
            }

            base.Update();

            if (Director.PendingMode == OsuMode.Unknown)
            {
                Vector2 pos = new Vector2(0, scrollOffset);
                foreach (PackPanel p in packs)
                {
                    p.MoveTo(pos, 40);
                    pos.Y += p.Height;
                }
            }

            
        }
    }
}
