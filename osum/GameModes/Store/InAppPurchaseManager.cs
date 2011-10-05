using System;
using MonoTouch.StoreKit;
using MonoTouch.Foundation;
using System.Globalization;
using System.Threading;
using System.Collections.Generic;

/// this is the first pass translation of the code used in the store kit from this tutorial http://troybrant.net/blog/2010/01/in-app-purchases-a-full-walkthrough/
namespace osum.GameModes.Store
{
    public delegate void ProductResponseDelegate(SKProduct[] products);
    public delegate void PurchaseCompleteDelegate(SKPaymentTransaction transaction, bool wasSuccessful);

    public class InAppPurchaseManager : SKProductsRequestDelegate, IDisposable
    {
        private SKProductsRequest productsRequest;
        private MySKPaymentObserver observer;

        public new void Dispose()
        {
            if (observer != null)
            {
                SKPaymentQueue.DefaultQueue.RemoveTransactionObserver(observer);
                observer = null;
            }

            if (productsRequest != null)
            {
                productsRequest.Delegate = null;
                productsRequest.Cancel();
                productsRequest = null;
            }

            base.Dispose();
        }

        ProductResponseDelegate responseDelegate;

        public void RequestProductData(List<string> productIds, ProductResponseDelegate responseDelegate)
        {
            NSMutableSet setIds = new NSMutableSet();

            foreach (string s in productIds)
                setIds.Add(new NSString(s));

            this.responseDelegate = responseDelegate;

            productsRequest = new SKProductsRequest(setIds);
            productsRequest.Delegate = this;
            productsRequest.Start();
        }

        public override void ReceivedResponse(SKProductsRequest request, SKProductsResponse response)
        {
#if !DIST
            foreach (SKProduct product in response.Products)
            {
                Console.WriteLine("Localised price:" + product.LocalizedPrice());
                Console.WriteLine("Product title: " + product.LocalizedTitle);
                Console.WriteLine("Product description: " + product.LocalizedDescription);
                Console.WriteLine("Product price: " + product.LocalizedPrice());
                Console.WriteLine("Product id: " + product.ProductIdentifier);
            }

            foreach (string invalidProductId in response.InvalidProducts)
                Console.WriteLine("Invalid product id: " + invalidProductId);
#endif

            if (responseDelegate != null)
            {
                responseDelegate(response.Products);
                responseDelegate = null;
            }

            productsRequest.Dispose();
            productsRequest = null;
        }

        public bool CanMakePurchases()
        {
            return SKPaymentQueue.CanMakePayments;
        }

        PurchaseCompleteDelegate purchaseCompleteDelegate;

        /// <summary>
        /// Begin the purchase process.
        /// </summary>
        public void PurchaseItem(string productId, PurchaseCompleteDelegate purchaseCompleteResponse)
        {
            purchaseCompleteDelegate = purchaseCompleteResponse;

#if !DIST
            Console.WriteLine("Beginning purchase of " + productId);
#endif

            if (observer == null)
            {
                observer = new MySKPaymentObserver(this);
                SKPaymentQueue.DefaultQueue.AddTransactionObserver(observer);
            }

            SKPayment payment = SKPayment.PaymentWithProduct(productId);
            SKPaymentQueue.DefaultQueue.AddPayment(payment);
        }

        //
        // removes the transaction from the queue and posts a notification with the transaction result
        //
        public void finishTransaction(SKPaymentTransaction transaction, bool wasSuccessful)
        {
#if !DIST
            Console.WriteLine("Purchase compete with " + (wasSuccessful ? "success" : "failure"));
#endif

            // remove the transaction from the payment queue.
            SKPaymentQueue.DefaultQueue.FinishTransaction(transaction);

            if (purchaseCompleteDelegate != null)
            {
                purchaseCompleteDelegate(transaction, wasSuccessful);
                purchaseCompleteDelegate = null;
            }
        }

        /// <summary>
        /// Callback from payment observer on successful completion.
        /// </summary>
        public void handleCompleteTransaction(SKPaymentTransaction transaction)
        {
            finishTransaction(transaction, true);
        }

        /// <summary>
        /// Callback from payment observer on successful completion (previously purchased).
        /// </summary>
        public void handleRestoreTransaction(SKPaymentTransaction transaction)
        {
            finishTransaction(transaction.OriginalTransaction, true);
        }

        /// <summary>
        /// Callback from payment observer on failure.
        /// </summary>
        public void handleFailedTransaction(SKPaymentTransaction transaction)
        {
#if !DIST
            Console.WriteLine("Transaction failed with error code:" + transaction.Error.Code);
#endif
            if (transaction.Error.Code != 2)
            {
                //there was an actual error during the purchase process.
            } else
            {
                //payment was cancelled by the user at the apple dialog.
                SKPaymentQueue.DefaultQueue.FinishTransaction(transaction);
            }

            finishTransaction(transaction, false);
        }
    }

    public class MySKPaymentObserver : SKPaymentTransactionObserver
    {
        private InAppPurchaseManager manager;

        public MySKPaymentObserver(InAppPurchaseManager manager)
        {
            this.manager = manager;
        }

        public override void UpdatedTransactions(SKPaymentQueue queue, SKPaymentTransaction[] transactions)
        {
            foreach (SKPaymentTransaction transaction in transactions)
            {
                switch (transaction.TransactionState)
                {
                    case SKPaymentTransactionState.Purchased:
                        manager.handleCompleteTransaction(transaction);
                        break;
                    case SKPaymentTransactionState.Failed:
                        manager.handleFailedTransaction(transaction);
                        break;
                    case SKPaymentTransactionState.Restored:
                        manager.handleRestoreTransaction(transaction);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    static class extensions
    {
        public static string LocalizedPrice(this SKProduct product)
        {
            using (NSNumberFormatter numberFormatter = new NSNumberFormatter())
            {
                numberFormatter.FormatterBehavior = NSNumberFormatterBehavior.Version_10_4;
                numberFormatter.NumberStyle = NSNumberFormatterStyle.Currency;
                numberFormatter.Locale = product.PriceLocale;
                return numberFormatter.StringFromNumber(product.Price).ToString();
            }
        }
    }
}