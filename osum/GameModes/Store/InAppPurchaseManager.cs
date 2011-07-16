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
        private MonoTouch.StoreKit.SKProduct product;
        private SKProductsRequest productsRequest;
        private MySKPaymentObserver observer;

        public void Dispose()
        {
            Console.WriteLine("disposing iapm");
            SKPaymentQueue.DefaultQueue.RemoveTransactionObserver(observer);
            if (observer != null)
            {
                observer.Dispose();
                observer = null;
            }
        }

        ProductResponseDelegate responseDelegate;

        public void RequestProductData(List<string> productIds, ProductResponseDelegate responseDelegate)
        {
            NSMutableSet setIds = new NSMutableSet();

            foreach (string s in productIds)
                setIds.Add(new NSString(s));
            productsRequest = new SKProductsRequest(setIds);

            this.responseDelegate = responseDelegate;

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

            if (observer == null)
            {
                observer = new MySKPaymentObserver(this);
                SKPaymentQueue.DefaultQueue.AddTransactionObserver(observer);
            }

            Console.WriteLine("purchasing " + productId);
            SKPayment payment = SKPayment.PaymentWithProduct(productId);
            SKPaymentQueue.DefaultQueue.AddPayment(payment);


        }

        //
        // removes the transaction from the queue and posts a notification with the transaction result
        //
        public void finishTransaction(SKPaymentTransaction transaction, bool wasSuccessful)
        {
            // remove the transaction from the payment queue.
            SKPaymentQueue.DefaultQueue.FinishTransaction(transaction);

            if (purchaseCompleteDelegate != null)
            {
                purchaseCompleteDelegate(transaction, wasSuccessful);
                purchaseCompleteDelegate = null;
            }

            if (wasSuccessful)
            {
                Console.WriteLine("Successful purchase");
            }
            else
            {
                Console.WriteLine("Failed purchase");
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
            if (transaction.Error.Code != 2)
            {
                //there was an actual error during the purchase process.
                finishTransaction(transaction, false);
            } else
            {
                //payment was cancelled by the user at the apple dialog.
                SKPaymentQueue.DefaultQueue.FinishTransaction(transaction);  
            }
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
            Console.WriteLine("got here");
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