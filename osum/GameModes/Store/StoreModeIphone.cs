using System;
using MonoTouch.StoreKit;
using System.Collections.Generic;

namespace osum.GameModes.Store
{
    public class StoreModeIphone : StoreMode
    {
        InAppPurchaseManager iap = new InAppPurchaseManager();

        public StoreModeIphone()
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            iap = new InAppPurchaseManager();
        }

        public override void Dispose ()
        {
            base.Dispose();
            if (iap != null) iap.Dispose();
        }

        protected override void handlePackInfo(string result, Exception e)
        {
            base.handlePackInfo(result, e);

            if (packs.Count > 0)
            {
                //grab information for any packs which require payment.
                List<string> ids = new List<string>();
                foreach (PackPanel pp in packs)
                {
                    if (!pp.IsFree)
                        ids.Add(pp.PackId);
                }

                iap.RequestProductData(ids, productsResponse);
            }
        }

        protected override void purchase(PackPanel pack)
        {
            if (pack.IsFree)
                download(pack);
            else
            {
                iap.PurchaseItem(pack.PackId);
            }
        }

        void productsResponse(SKProduct[] products)
        {
            GameBase.Scheduler.Add(delegate {
                foreach (SKProduct p in products)
                {
                    PackPanel associatedPack = packs.Find(pack => pack.PackId == p.ProductIdentifier);
                    associatedPack.s_Price.Text = p.LocalizedPrice();
                }
                //handle products. give packs prices, and stuff.
            });
        }
    }
}

