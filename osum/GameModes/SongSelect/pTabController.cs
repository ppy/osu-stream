using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Graphics.Skins;
using osum.Graphics.Sprites;
using OpenTK.Graphics;
using OpenTK;
using osum.Helpers;

namespace osum.GameModes.SongSelect
{
    internal class pTabController : GameMode
    {
        private pSprite s_TabBarBackground;

        internal List<pDrawable> Sprites { get { return spriteManager.Sprites; } }

        List<pDrawable> tabs = new List<pDrawable>();

        internal pTabController()
        {
            Initialize();
        }


        internal override void Initialize()
        {
            s_TabBarBackground = new pSprite(TextureManager.Load(OsuTexture.songselect_tab_bar_background), FieldTypes.StandardSnapTopCentre, OriginTypes.TopCentre, ClockTypes.Mode, new Vector2(0, -100), 0.4f, true, Color4.White);
            s_TabBarBackground.Scale = new Vector2(GameBase.BaseSize.Width, 1); //this isn't perfectly window width, for what it's worth.
            spriteManager.Add(s_TabBarBackground);
        }

        internal void Add(OsuTexture tabTexture)
        {
            pSprite tab = new pSprite(TextureManager.Load(tabTexture), FieldTypes.StandardSnapTopCentre, OriginTypes.TopCentre, ClockTypes.Mode, new Vector2(0, -100), 0.41f, true, Color4.Gray);
            tab.OnClick += onTabClick;
            tab.OnHover += delegate { tab.FadeColour(Color4.White, 100); };
            tab.OnHoverLost += delegate { tab.FadeColour(Color4.Gray, 100); };

            spriteManager.Add(tab);

            if (tabs.Count == 0) tab.Click();

            tabs.Add(tab);
            
            float x = -(Math.Max(0,tabs.Count - 1) * 200f) / 2;
            for (int i = 0; i < tabs.Count; i++)
            {
                tabs[i].Offset = new Vector2(x, 0);
                x += 200;
            }
        }

        pSprite selectedTab;
        void onTabClick(object sender, EventArgs e)
        {
            if (selectedTab != null)
            {
                if (selectedTab == sender) return;
                selectedTab.HandleInput = true;
            }

            pSprite s = sender as pSprite;
            if (s == null) return;

            selectedTab = s;
            s.HandleInput = false;
        }

    }
}
