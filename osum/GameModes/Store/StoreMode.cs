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
using osum.Audio;
using osum.Helpers;

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

            StringNetRequest netRequest = new StringNetRequest("http://d.osu.ppy.sh/osum/getpacks.php");
            netRequest.onFinish += netRequest_onFinish;
            NetManager.AddRequest(netRequest);

            loading = new pText("Loading...", 36, Vector2.Zero, 1, true, Color4.OrangeRed)
            {
                TextAlignment = TextAlignment.Centre,
                Origin = OriginTypes.Centre,
                Field = FieldTypes.StandardSnapCentre,
                Clocking = ClockTypes.Game,
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
            bool newPack = true;

            int i = 0;

            foreach (string line in _result.Split('\n'))
            {
                if (line.Length == 0)
                {
                    newPack = true;
                    Console.WriteLine("Reading new pack");
                    continue;
                }

                string[] split = line.Split('\t');

                if (newPack)
                {
                    GameBase.Scheduler.Add(delegate
                    {
                        if (pp != null && pp.BeatmapCount > 0)
                        {
                            spriteManager.Add(pp);
                            packs.Add(pp);
                        }


                        Console.WriteLine("Adding pack: " + split[0]);
                        pp = new PackPanel(split[0], split[1], null);
                    });

                    newPack = false;
                    continue;
                }

                string filename = split[0];
                string checksum = split[1];

                string path = SongSelectMode.BeatmapPath + "/" + filename;

                if (File.Exists(path))
                {
                    string checksumLocal = CryptoHelper.GetMd5(path);
                    if (checksumLocal == checksum) continue;
                }

                int thisY = y;

                Console.WriteLine("Adding beatmap: " + filename);

                GameBase.Scheduler.Add(delegate
                {
                    pp.Add(filename);
                });

                y++;
            }

            GameBase.Scheduler.Add(delegate
            {
                if (pp != null && pp.BeatmapCount > 0)
                    spriteManager.Add(pp); packs.Add(pp);

                loading.FadeOut(200);

                if (y == 0)
                {
                    GameBase.Notify("You already have all available song packs!", delegate { Director.ChangeMode(OsuMode.SongSelect); });
                }
            });
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override bool Draw()
        {
            return base.Draw();
        }

        bool playingPreview;


        public static void ResetAllPreviews(bool isPausing)
        {
            StoreMode instance = Director.CurrentMode as StoreMode;
            if (instance == null) return;

            foreach (PackPanel p in instance.packs)
                p.ResetPreviews();

            instance.playingPreview = false;

            if (isPausing)
            {
                SongSelectMode.InitializeBgm();
                instance.playingPreview = false;
            }
        }

        public static void PlayPreview(byte[] data)
        {
            StoreMode instance = Director.CurrentMode as StoreMode;
            if (instance == null) return;

            AudioEngine.Music.Load(data, false);
            AudioEngine.Music.Play();
            instance.playingPreview = true;
        }

        internal static void PurchaseInitiated(PackPanel packPanel)
        {
            StoreMode instance = Director.CurrentMode as StoreMode;
            if (instance == null) return;

            instance.s_ButtonBack.FadeOut(100);
        }

        public static void DownloadComplete(PackPanel pp)
        {
            StoreMode instance = Director.CurrentMode as StoreMode;
            if (instance == null) return;

            pp.Sprites.ForEach(s => s.FadeOut(100));
            instance.packs.Remove(pp);

            if (instance.packs.Count == 0)
                GameBase.Notify("You have all available packs.\nCheck back later!", delegate { Director.ChangeMode(OsuMode.SongSelect); });

            if (instance.packs.TrueForAll(p => !p.Downloading))
                instance.s_ButtonBack.FadeIn(100);
        }

        public static void EnsureVisible(pDrawable sprite)
        {
            StoreMode instance = Director.CurrentMode as StoreMode;
            if (instance == null) return;

            instance.scrollOffset = Math.Min(0, Math.Max(instance.offset_min, instance.scrollOffset - sprite.Position.Y + 40));
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

                return -totalHeight + GameBase.BaseSizeFixedWidth.Height - 80;
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
            if (playingPreview && !AudioEngine.Music.IsElapsing)
            {
                ResetAllPreviews(true);
            }

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
