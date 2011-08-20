using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Graphics.Sprites;
using osum.Graphics.Skins;
using osum.Helpers;
using OpenTK;
using OpenTK.Graphics;

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
        }

        void newsButton_OnClick(object sender, EventArgs e)
        {
            Click(true);
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
    }
}
