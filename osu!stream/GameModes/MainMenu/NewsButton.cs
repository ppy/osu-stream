using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Graphics.Sprites;
using osum.Graphics.Skins;
using osum.Helpers;
using OpenTK;
using OpenTK.Graphics;
using System.Text.RegularExpressions;
using osu_common.Libraries.NetLib;
using osum.GameModes.Store;

namespace osum.GameModes
{
    internal class NewsButton : SpriteManager
    {
        private pSprite newsButton;
        private pSprite newsLight;

        public NewsButton()
        {
            newsButton = new pSprite(TextureManager.Load(OsuTexture.news_button), FieldTypes.StandardSnapBottomLeft, OriginTypes.BottomLeft, ClockTypes.Mode, Vector2.Zero, 0.8f, true, Color4.White);
            newsButton.OnClick += new EventHandler(newsButton_OnClick);
            Add(newsButton);

            newsLight = new pSprite(TextureManager.Load(OsuTexture.news_light), FieldTypes.StandardSnapBottomLeft, OriginTypes.BottomLeft, ClockTypes.Mode, new Vector2(3,13f), 0.81f, true, Color4.White);
            newsLight.Additive = true;

            newsLight.Transform(new TransformationF(TransformationType.Fade, 0, 1, 0, 300, EasingTypes.Out) { Looping = true, LoopDelay = 1500 });
            newsLight.Transform(new TransformationF(TransformationType.Fade, 1, 0, 300, 1500, EasingTypes.Out) { Looping = true, LoopDelay = 600 });
            Add(newsLight);

            newsLight.Bypass = true;

            string lastRead = GameBase.Config.GetValue<string>("NewsLastRead", string.Empty);
            string lastRetrieved = GameBase.Config.GetValue<string>("NewsLastRetrieved", string.Empty);

            if (lastRead == string.Empty || lastRetrieved != lastRead)
                HasNews = true;

            StringNetRequest nr = new StringNetRequest(@"http://osustream.com/misc/news.php?v=2");
            nr.onFinish += new StringNetRequest.RequestCompleteHandler(newsCheck_onFinish);
            NetManager.AddRequest(nr);
        }

        void newsButton_OnClick(object sender, EventArgs e)
        {
            HasNews = false;

            GameBase.Instance.ShowWebView(@"http://osustream.com/p/news", "News");

            GameBase.Config.SetValue<string>("NewsLastRead", GameBase.Config.GetValue<string>("NewsLastRetrieved", string.Empty));
            GameBase.Config.SaveConfig();
        }

        private bool hasNews;
        public bool HasNews
        {
            get { return hasNews; }
            set
            {
                hasNews = value;
                newsLight.Bypass = !hasNews;
            }
        }

        void newsCheck_onFinish(string _result, Exception e)
        {
            if (e == null)
            {
                string[] split = _result.Split('\n');

                if (split.Length < 2)
                    return;

                string newsLastRead = GameBase.Config.GetValue<string>("NewsLastRead", string.Empty);
                string storeLastRead = GameBase.Config.GetValue<string>("StoreLastRead", string.Empty);

                foreach (string line in split)
                {
                    int index = line.IndexOf(':');
                    if (index < 0) continue;

                    string key = line.Remove(index);
                    string val = line.Substring(index + 1);

                    switch (key)
                    {
                        case "news":
                            GameBase.Config.SetValue<string>("NewsLastRetrieved", val);
                            if (newsLastRead != val)
                                HasNews = true;
                            break;
                        case "store":
                            GameBase.Config.SetValue<string>("StoreLastRetrieved", val);
                            if (storeLastRead != val)
                                StoreMode.HasNewStoreItems = true;
                            break;
                    }
                }
            }
        }
    }
}
