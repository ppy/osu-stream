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
using osum.Graphics.Skins;
using osum.Resources;
using osum.GameplayElements.Beatmaps;
using osu_common.Libraries.Osz2;

namespace osum.GameModes.Store
{
    public class StoreMode : GameMode
    {
        private BackButton s_ButtonBack;

        protected List<PackPanel> packs = new List<PackPanel>();

        StringNetRequest fetchRequest;

        public override void Initialize()
        {
            spriteManager.CheckSpritesAreOnScreenBeforeRendering = true;

            background =
                new pSprite(TextureManager.Load(OsuTexture.songselect_background), FieldTypes.StandardSnapCentre, OriginTypes.Centre,
                            ClockTypes.Mode, Vector2.Zero, 0, true, new Color4(56, 56, 56, 255));
            background.AlphaBlend = false;
            spriteManager.Add(background);

            s_ButtonBack = new BackButton(delegate { Director.ChangeMode(Director.LastOsuMode); }, true);
            spriteManager.Add(s_ButtonBack);

            InputManager.OnMove += new Helpers.InputHandler(InputManager_OnMove);

            GameBase.ShowLoadingOverlay = true;

            if (fetchRequest != null) fetchRequest.Abort();
            fetchRequest = new StringNetRequest("http://www.osustream.com/dl/list.php");
            fetchRequest.onFinish += handlePackInfo;
            NetManager.AddRequest(fetchRequest);

            if (GameBase.IsSlowDevice)
                GameBase.ThrottleExecution = true;
        }

        public override void Dispose()
        {
            GameBase.ShowLoadingOverlay = false;

            if (fetchRequest != null) fetchRequest.Abort();
            base.Dispose();
        }

        protected virtual void handlePackInfo(string result, Exception e)
        {
            if (fetchRequest.AbortRequested)
                return;

            if (e != null || string.IsNullOrEmpty(result))
            {
                //error has occurred!
                GameBase.Notify(LocalisationManager.GetString(OsuString.ErrorWhileDownloadingSongListing), delegate { Director.ChangeMode(OsuMode.SongSelect); });
                return;
            }

            PackPanel pp = null;
            bool newPack = true;

            int y = 0;
            foreach (string line in result.Split('\n'))
            {
                if (line.Length == 0)
                {
                    newPack = true;
                    continue;
                }

                string[] split = line.Split('\t');

                if (newPack)
                {
                    AddPack(pp);
#if DEBUG
                    Console.WriteLine("Adding pack: " + split[0]);
#endif

                    string packId = split[0];
                    string packName = split[1];
                    bool isFree = packId.Contains("Free");

                    pp = new PackPanel(packName, packId, isFree);

                    newPack = false;
                    continue;
                }

                string filename = split[0];
                string checksum = split[1];
                string revision = split.Length > 3 ? split[3] : "1.0";
                string title = split.Length > 2 ? split[2] : null;

                string path = SongSelectMode.BeatmapPath + "/" + filename;

                if (File.Exists(path))
                {
                    using (Beatmap b = new Beatmap(path))
                    {
                        if (b.Package != null)
                        {
                            string localRev = b.Package.GetMetadata(MapMetaType.Revision) ?? "1.0";

                            if (localRev == revision)
                                continue;

                            pp.SetPrice(LocalisationManager.GetString(OsuString.Update), true);

                            pp.UpdateChecksum = CryptoHelper.GetMd5(GameBase.Instance.DeviceIdentifier + 0x90 + filename + "-update");
                        }
                    }
                }

                int thisY = y;

#if DEBUG
                Console.WriteLine("Adding beatmap: " + filename);
#endif

                pp.Add(filename, title);

                y++;
            }

            if ((Director.IsTransitioning && Director.ActiveTransition.FadeInDone) || Director.CurrentOsuMode != OsuMode.Store)
                return;

            AddPack(pp);

            GameBase.ShowLoadingOverlay = false;

            if (packs.Count == 0)
                GameBase.Notify(LocalisationManager.GetString(OsuString.HaveAllAvailableSongPacks), delegate { Director.ChangeMode(Director.LastOsuMode); });
        }

        void AddPack(PackPanel pp)
        {
            if (pp == null || pp.BeatmapCount == 0)
                return;
            spriteManager.Add(pp);
            packs.Add(pp);
        }

        void InputManager_OnMove(InputSource source, TrackingPoint trackingPoint)
        {
            if (!InputManager.IsPressed || InputManager.PrimaryTrackingPoint == null || InputManager.PrimaryTrackingPoint.HoveringObject is BackButton)
                return;

            float change = InputManager.PrimaryTrackingPoint.WindowDelta.Y;
            float bound = offsetBound;

            if ((scrollOffset - bound < 0 && change < 0) || (scrollOffset - bound > 0 && change > 0))
                change *= Math.Min(1, 10 / Math.Max(0.1f, Math.Abs(scrollOffset - bound)));
            scrollOffset = scrollOffset + change;
            velocity = change;
        }

        bool playingPreview;

        public static void ResetAllPreviews(bool isPausing, bool isUserDeselect)
        {
            StoreMode instance = Director.CurrentMode as StoreMode;
            if (instance == null) return;

            foreach (PackPanel p in instance.packs)
                p.ResetPreviews(isUserDeselect || !isPausing);

            instance.playingPreview = false;

            if (isPausing)
            {
                SongSelectMode.InitializeBgm();
                AudioEngine.Music.Play();
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

            instance.purchase(packPanel);
        }

        /// <summary>
        /// Initiate the "purchase" procedure (which may not involve a purchase in the case of a free pack.
        /// </summary>
        protected virtual void purchase(PackPanel pack)
        {
            if (pack.IsFree)
                download(pack);
            else
                GameBase.Notify("Can't download paid packs from this build!",null);
        }

        /// <summary>
        /// Download the specified pack.
        /// </summary>
        protected virtual void download(PackPanel pack)
        {
            pack.Download();
            s_ButtonBack.FadeOut(100);
        }

        public static void DownloadComplete(PackPanel pp)
        {
            StoreMode instance = Director.CurrentMode as StoreMode;
            if (instance == null) return;

            pp.Sprites.ForEach(s => s.FadeOut(100));
            instance.packs.Remove(pp);

            if (instance.packs.Count == 0)
                GameBase.Notify(LocalisationManager.GetString(OsuString.HaveAllAvailableSongPacks), delegate { Director.ChangeMode(Director.LastOsuMode); });

            if (instance.packs.TrueForAll(p => !p.Downloading))
            {
                instance.s_ButtonBack.FadeIn(100);
            }
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
        private float offset_max;
        float scrollOffset;
        private float velocity;
        private pSprite background;

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
                ResetAllPreviews(true, false);

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

            if (Director.PendingOsuMode == OsuMode.Unknown)
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
