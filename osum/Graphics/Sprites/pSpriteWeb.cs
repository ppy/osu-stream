using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Helpers;
using osum.Graphics.Skins;
using OpenTK;
using osu_common.Libraries.NetLib;

namespace osum.Graphics.Sprites
{
    class pSpriteWeb : pSprite
    {
        string Url;

        public pSpriteWeb(string url)
            : base(null, Vector2.Zero)
        {
            Url = url;
        }

        internal override pTexture Texture
        {
            get
            {
                if (isLoading || failedLoad)
                    return null;

                pTexture t = base.Texture;

                if (t == null || t.IsDisposed)
                    LoadTexture();

                return base.Texture;
            }
            set
            {
                base.Texture = value;
            }
        }

        bool failedLoad;
        bool isLoading;

        private void LoadTexture()
        {
            if (failedLoad || isLoading) return;

            isLoading = true;

            DataNetRequest dnr = new DataNetRequest(Url);
            dnr.onFinish += delegate(Byte[] data, Exception e) {
                if (e != null || data.Length == 0)
                {
                    failedLoad = true;
                }
                else
                {
                    pTexture t = pTexture.FromBytes(data);

                    if (t == null)
                        failedLoad = true;
                    else
                    {
                        Texture = t;
                        TextureManager.RegisterDisposable(t);
                    }
                }

                isLoading = false;
            };
            NetManager.AddRequest(dnr);
        }
    }
}
