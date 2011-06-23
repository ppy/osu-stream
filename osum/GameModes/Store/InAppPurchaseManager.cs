using System;
using MonoTouch.StoreKit;
using MonoTouch.Foundation;
using System.Globalization;
using System.Threading;
/// this is the first pass translation of the code used in the store kit from this tutorial http://troybrant.net/blog/2010/01/in-app-purchases-a-full-walkthrough/


namespace osum.GameModes.Store
{
	public class InAppPurchaseManager : SKProductsRequestDelegate
	{
		public static string InAppPurchaseManagerTransactionFailedNotification  = "InAppPurchaseManagerTransactionFailedNotification";
		public static string InAppPurchaseManagerTransactionSucceededNotification  = "InAppPurchaseManagerTransactionSucceededNotification";
		public static string InAppPurchaseManagerProductsFetchedNotification  = "InAppPurchaseManagerProductsFetchedNotification";
		
		public static string InAppPurchaseProUpgradeProductId = "TouhouPack1";
		
		private MonoTouch.StoreKit.SKProduct proUpgradeProduct;
		private SKProductsRequest productsRequest;
		private MySKPaymentObserver theObserver;
		
		public InAppPurchaseManager ()
		{
			theObserver = new MySKPaymentObserver(this);
		}
		
		public void requestProductData(string productId)
		{
		 	NSSet productIdentifiers  = NSSet.MakeNSObjectSet<NSString>(new NSString[]{new NSString(InAppPurchaseProUpgradeProductId)});
		    productsRequest  = new SKProductsRequest(productIdentifiers);

			productsRequest.Delegate = this;
			productsRequest.Start();
		}
		
		public override void ReceivedResponse (SKProductsRequest request, SKProductsResponse response)
		{
			
			SKProduct[] products = response.Products;
			proUpgradeProduct = products.Length == 1 ? products[0] : null;
			if (proUpgradeProduct != null)
			{
				proUpgradeProduct.LocalizedPrice();
				Console.WriteLine("Product title: " + proUpgradeProduct.LocalizedTitle);
				Console.WriteLine("Product description: " + proUpgradeProduct.LocalizedDescription);
				Console.WriteLine("Product price: " + proUpgradeProduct.LocalizedPrice());
				Console.WriteLine("Product id: " + proUpgradeProduct.ProductIdentifier);
			}
			
			foreach(string invalidProductId in response.InvalidProducts)
			{
				Console.WriteLine("Invalid product id: " + invalidProductId );
			}
			
			// finally release the reqest we alloc/init’ed in requestProUpgradeProductData
			productsRequest.Dispose();
			
			
			NSNotificationCenter.DefaultCenter.PostNotificationName(InAppPurchaseManagerProductsFetchedNotification,this,null);
			
		
		}
		
		//
		// call this method once on startup
		//
		public void LoadStore()
		{
			SKPaymentQueue.DefaultQueue.AddTransactionObserver(theObserver);
			//this.requestProUpgradeProductData();
		}
		//
		// call this before making a purchase
		//
		public bool canMakeProUpgrade()
		{
			return SKPaymentQueue.CanMakePayments;	
		}
		//
		// kick off the upgrade transaction
		//
		public void PurchaseProUpgrade()
		{
			SKPayment payment = SKPayment.PaymentWithProduct(InAppPurchaseProUpgradeProductId);	
			SKPaymentQueue.DefaultQueue.AddPayment(payment);
		}
		
		//
		// saves a record of the transaction by storing the receipt to disk
		//
		public void recordTransaction(SKPaymentTransaction transaction)
		{
			if(transaction.Payment.ProductIdentifier == InAppPurchaseProUpgradeProductId)
			{
				NSUserDefaults.StandardUserDefaults.SetNativeField("proUpgradeTransactionReceipt",transaction.TransactionReceipt);
				NSUserDefaults.StandardUserDefaults.Synchronize();
			}
		}
		
		//
		// removes the transaction from the queue and posts a notification with the transaction result
		//
		public void finishTransaction(SKPaymentTransaction transaction, bool wasSuccessful)
		{
			// remove the transaction from the payment queue.
			SKPaymentQueue.DefaultQueue.FinishTransaction(transaction);
			NSDictionary userInfo = NSDictionary.FromObjectsAndKeys(new NSObject[] {transaction},new NSObject[] { new NSString("transaction")});
			if(wasSuccessful)
			{
				// send out a notification that we’ve finished the transaction
				NSNotificationCenter.DefaultCenter.PostNotificationName(InAppPurchaseManagerTransactionSucceededNotification,this,userInfo);
			}
			else
			{
				// send out a notification for the failed transaction
				NSNotificationCenter.DefaultCenter.PostNotificationName(InAppPurchaseManagerTransactionFailedNotification,this,userInfo);
			}
		}
		//
		// called when the transaction was successful
		//
		public void completeTransaction(SKPaymentTransaction transaction)
		{
			this.recordTransaction(transaction);
			//this.provideContent(transaction.Payment.ProductIdentifier);
			this.finishTransaction(transaction,true);
		}
		//
		// called when a transaction has been restored and and successfully completed
		//
		public void restoreTransaction(SKPaymentTransaction transaction)
		{
			this.recordTransaction(transaction.OriginalTransaction);
			//this.provideContent(transaction.OriginalTransaction.Payment.ProductIdentifier);
			this.finishTransaction(transaction.OriginalTransaction, true);
		}
		//
		// called when a transaction has failed
		//
		public void failedTransaction(SKPaymentTransaction transaction)
		{	
            //SKErrorPaymentCancelled == 2
			if (transaction.Error.Code != 2)
			{
				// error!
				this.finishTransaction(transaction,false);
			}
			else
			{
			// this is fine, the user just cancelled, so don’t notify
				SKPaymentQueue.DefaultQueue.FinishTransaction(transaction);	
			}
		}
		
		
	
		// Handles custom observer
		private class MySKPaymentObserver : SKPaymentTransactionObserver
		{
			private InAppPurchaseManager theManager;
			public MySKPaymentObserver(InAppPurchaseManager manager)
			{
				theManager = manager;
			}
			//
			// called when the transaction status is updated
			//
			public override void UpdatedTransactions (SKPaymentQueue queue, SKPaymentTransaction[] transactions)
			{
				foreach (SKPaymentTransaction transaction in transactions)
				{
				    switch (transaction.TransactionState)
				    {
				        case SKPaymentTransactionState.Purchased:
				           theManager.completeTransaction(transaction);
				            break;
				        case SKPaymentTransactionState.Failed:
				           theManager.failedTransaction(transaction);
				            break;
				        case SKPaymentTransactionState.Restored:
				            theManager.restoreTransaction(transaction);
				            break;
				        default:
				            break;
				    }
				}
			
			}
			
		}	
	}
	
	public static class SKProductExtender
	{
		public static string LocalizedPrice( this SKProduct product)
	    {
	        Console.WriteLine("product.PriceLocale.LocaleIdentifier="+product.PriceLocale.LocaleIdentifier);
	        // returns en_AU@currency=AUD for me
	        string localeIdString = product.PriceLocale.LocaleIdentifier;
	        string locale = localeIdString; // default
	        string currency = "USD";
	        if (localeIdString.IndexOf('@') > 0)
	        {
	            locale = localeIdString.Substring(0, localeIdString.IndexOf('@'));
	            currency = localeIdString.Substring(localeIdString.IndexOf('=')+1,3);
	        }
	        Console.WriteLine("locale " + locale);
	        Console.WriteLine("currency " + currency);

            Console.WriteLine("Price is: " + product.Price);

	        return product.Price.StringValue;
	    }
	}
}