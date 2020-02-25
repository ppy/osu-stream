using System;
using OpenTK;
using osum.Libraries.NetLib;

namespace osum.Graphics.Sprites
{
    internal class pSpriteWeb : pSprite
    {
        private readonly string Url;

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
            set => base.Texture = value;
        }

        private bool failedLoad;
        private bool isLoading;

        private void LoadTexture()
        {
            if (failedLoad || isLoading) return;

            isLoading = true;

            DataNetRequest dnr = new DataNetRequest(Url);
            dnr.onFinish += delegate(byte[] data, Exception e)
            {
                if (e != null || data == null || data.Length == 0)
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

                        if (IsOnScreen && Alpha > 0 && AlwaysDraw)
                            FadeInFromZero(250);
                    }
                }

                isLoading = false;
            };
            NetManager.AddRequest(dnr);
        }
    }
}