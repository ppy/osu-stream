using System;
using OpenTK;
using OpenTK.Graphics;
using osum.GameModes.Store;
using osum.Graphics;
using osum.Graphics.Sprites;
using osum.Helpers;
using osum.Libraries.NetLib;

namespace osum.GameModes.MainMenu
{
    internal class NewsButton : SpriteManager
    {
        private readonly pSprite newsButton;
        private readonly pSprite newsLight;

        public NewsButton()
        {
            newsButton = new pSprite(TextureManager.Load(OsuTexture.news_button), FieldTypes.StandardSnapBottomLeft, OriginTypes.BottomLeft, ClockTypes.Mode, Vector2.Zero, 0.8f, true, Color4.White);
            newsButton.OnClick += newsButton_OnClick;
            Add(newsButton);

            newsLight = new pSprite(TextureManager.Load(OsuTexture.news_light), FieldTypes.StandardSnapBottomLeft, OriginTypes.BottomLeft, ClockTypes.Mode, new Vector2(3,13f), 0.81f, true, Color4.White);
            newsLight.Additive = true;

            newsLight.Transform(new TransformationF(TransformationType.Fade, 0, 1, 0, 300, EasingTypes.Out) { Looping = true, LoopDelay = 1500 });
            newsLight.Transform(new TransformationF(TransformationType.Fade, 1, 0, 300, 1500, EasingTypes.Out) { Looping = true, LoopDelay = 600 });
            Add(newsLight);

            newsLight.Bypass = true;

            string lastRead = GameBase.Config.GetValue("NewsLastRead", string.Empty);
            string lastRetrieved = GameBase.Config.GetValue("NewsLastRetrieved", string.Empty);

            if (lastRead == string.Empty || lastRetrieved != lastRead)
                HasNews = true;

            StringNetRequest nr = new StringNetRequest(@"https://osustream.com/misc/news.php?v=2");
            nr.onFinish += newsCheck_onFinish;
            NetManager.AddRequest(nr);
        }

        private void newsButton_OnClick(object sender, EventArgs e)
        {
            HasNews = false;

            GameBase.Instance.ShowWebView(@"https://osustream.com/p/news", "News");

            GameBase.Config.SetValue("NewsLastRead", GameBase.Config.GetValue("NewsLastRetrieved", string.Empty));
            GameBase.Config.SaveConfig();
        }

        private bool hasNews;
        public bool HasNews
        {
            get => hasNews;
            set
            {
                hasNews = value;
                newsLight.Bypass = !hasNews;
            }
        }

        private void newsCheck_onFinish(string _result, Exception e)
        {
            if (e == null)
            {
                string[] split = _result.Split('\n');

                if (split.Length < 2)
                    return;

                string newsLastRead = GameBase.Config.GetValue("NewsLastRead", string.Empty);
                string storeLastRead = GameBase.Config.GetValue("StoreLastRead", string.Empty);

                foreach (string line in split)
                {
                    int index = line.IndexOf(':');
                    if (index < 0) continue;

                    string key = line.Remove(index);
                    string val = line.Substring(index + 1);

                    switch (key)
                    {
                        case "news":
                            GameBase.Config.SetValue("NewsLastRetrieved", val);
                            if (newsLastRead != val)
                                HasNews = true;
                            break;
                        case "store":
                            GameBase.Config.SetValue("StoreLastRetrieved", val);
                            if (storeLastRead != val)
                                StoreMode.HasNewStoreItems = true;
                            break;
                    }
                }
            }
        }
    }
}
