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
        SpriteManagerDraggable scrollableSpriteManager = new SpriteManagerDraggable();
        SpriteManager topMostSpriteManager = new SpriteManager();

        bool playingPreview;
        private pSprite s_Header;

        private BackButton s_ButtonBack;

        protected List<PackPanel> packs = new List<PackPanel>();

        StringNetRequest fetchRequest;

        const int HEADER_PADDING = 60;
        float totalHeight = HEADER_PADDING;

        public override void Initialize()
        {
            spriteManager.CheckSpritesAreOnScreenBeforeRendering = true;

            pSprite background =
                new pSprite(TextureManager.Load(OsuTexture.songselect_background), FieldTypes.StandardSnapCentre, OriginTypes.Centre,
                            ClockTypes.Mode, Vector2.Zero, 0, true, new Color4(56, 56, 56, 255));
            background.AlphaBlend = false;
            spriteManager.Add(background);

            s_Header = new pSprite(TextureManager.Load(OsuTexture.store_header), new Vector2(0, 0));
            s_Header.OnClick += delegate { };
            topMostSpriteManager.Add(s_Header);

            s_ButtonBack = new BackButton(delegate {
                switch (Director.LastOsuMode)
                {
                    case OsuMode.SongSelect:
                        Director.ChangeMode(OsuMode.SongSelect);
                        break;
                    default:
                        Director.ChangeMode(OsuMode.MainMenu);
                        break;
                }
            }, true);
            topMostSpriteManager.Add(s_ButtonBack);

            GameBase.ShowLoadingOverlay = true;

            if (fetchRequest != null) fetchRequest.Abort();
            fetchRequest = new StringNetRequest("http://www.osustream.com/dl/list2.php");
            fetchRequest.onFinish += handlePackInfo;
            NetManager.AddRequest(fetchRequest);

            if (GameBase.IsSlowDevice)
                GameBase.ThrottleExecution = true;

            SongSelectMode.InitializeBgm();
        }

        public override void Restore()
        {
            SongSelectMode.InitializeBgm();
            base.Restore ();
        }

        public override void Dispose()
        {
            GameBase.ShowLoadingOverlay = false;

            scrollableSpriteManager.Dispose();
            topMostSpriteManager.Dispose();

            if (fetchRequest != null) fetchRequest.Abort();
            base.Dispose();
        }

        public override bool Draw()
        {
            base.Draw();
            scrollableSpriteManager.Draw();
            topMostSpriteManager.Draw();

            return true;
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

            float yOffset = 0;

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

                int length = split.Length;

                if (length == 0) continue;

                string filename = split[0];
                string checksum = split[1];
                string revision = length > 3 ? split[3] : "1.0";
                string title = length > 2 ? split[2] : null;
                string youtubeId = length > 4 ? split[4] : null;
                string updateChecksum = null;

                string path = SongSelectMode.BeatmapPath + "/" + filename;

                if (File.Exists(path))
                {
                    using (Beatmap b = new Beatmap(path))
                    {
                        if (b.Package != null)
                        {
                            string localRev = b.Package.GetMetadata(MapMetaType.Revision) ?? "1.0";
                            if (Path.GetFileNameWithoutExtension(b.ContainerFilename) != Path.GetFileNameWithoutExtension(b.Package.MapFiles[0]))
                                continue;

                            if (localRev == revision)
                                continue;

                            pp.SetPrice(LocalisationManager.GetString(OsuString.Update), true);
                            updateChecksum = CryptoHelper.GetMd5String(GameBase.Instance.DeviceIdentifier + (char)0x77 + filename + "-update");
                        }
                    }
                }

                int thisY = y;

#if DEBUG
                Console.WriteLine("Adding beatmap: " + filename);
#endif

                pp.AddItem(new PackItem(filename, title, updateChecksum) { YoutubeId = youtubeId });

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

            pp.Sprites.ForEach(s => s.Position.Y += totalHeight);
            pp.PackItemSprites.ForEach(s => s.FadeOut(0));
            totalHeight += pp.CondensedHeight;

            scrollableSpriteManager.Add(pp);
            packs.Add(pp);
        }

        void RemovePack(PackPanel pp)
        {
            if (pp == null)
                return;

            if (!packs.Remove(pp))
                return;

            pp.Sprites.ForEach(s => s.FadeOut(100));

            recalculateHeights();
        }

        private void recalculateHeights()
        {
            //recalculate all heights
            totalHeight = HEADER_PADDING;
            foreach (PackPanel p in packs)
            {
                p.MoveTo(new Vector2(0, totalHeight), 350);
                totalHeight += p.Height;
            }

            scrollableSpriteManager.SetMaxHeight(totalHeight);
        }

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
                GameBase.Notify("Can't download paid packs from this build!", null);
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

            instance.RemovePack(pp);

            if (instance.packs.Count == 0)
                GameBase.Notify(LocalisationManager.GetString(OsuString.HaveAllAvailableSongPacks), delegate { Director.ChangeMode(Director.LastOsuMode); });

            if (instance.packs.TrueForAll(p => !p.Downloading))
                instance.s_ButtonBack.FadeIn(100);
        }

        public static void ShowPack(PackPanel pack)
        {
            StoreMode instance = Director.CurrentMode as StoreMode;
            if (instance == null) return;

            foreach (PackPanel p in instance.packs)
                p.Expanded = p == pack;

            pack.StartPreviewing();

            instance.recalculateHeights();
            instance.scrollableSpriteManager.ScrollTo(pack.s_BackingPlate, HEADER_PADDING);
        }

        public override void Update()
        {
            if (playingPreview && !AudioEngine.Music.IsElapsing)
                ResetAllPreviews(true);

            scrollableSpriteManager.Update();
            topMostSpriteManager.Update();
            base.Update();
        }
    }
}
