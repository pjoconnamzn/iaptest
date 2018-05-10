using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.Purchasing;
using GameSparks.Core;
using GameSparks.Api.Requests;
using GameSparks.Api.Responses;
using Steamworks;


public class shop : MonoBehaviour, IStoreListener
{
	private static IStoreController m_StoreController;          // The Unity Purchasing system.
	private static IExtensionProvider m_StoreExtensionProvider; // The store-specific Purchasing subsystems.




	//replace with google/apple product id 
	// replace hardcoded with  ListVirtualGoodsRequest
	public static string kProductIDConsumable = "android.test.purchased";
	public static string iosProductIDConsumable = "iostestBasicConsumable";
	public static string kProductIDNonConsumable = "nonconsumable";
	[Header("Buttons")]
	public Button GoogleBttn;
	public Button DeviceAuthBttn;
	public Button TwitchAuthBttn;
	public Button SteamButton;

	// Steam Stuff
	[Header("Steam Account Details")]
    public string steamDisplayName;
    public CSteamID steamID;

    [Header("Steam Session Ticket")]
    public bool haveSessionID = false;
    public string steamSessionTicket; //as hex, for sending to GS

    public string[] stats;




	void Start()
	{
		GoogleBttn.onClick.AddListener(() =>
			{
				Debug.Log("Buy VG");
				BuyConsumable();
			});

		DeviceAuthBttn.onClick.AddListener (() => {
			Debug.Log("Device Auth");
			DeviceAuth();
		});
		TwitchAuthBttn.onClick.AddListener(()=> {
			Debug.Log("Twitch Auth");
			TwitchAuth();
		});
		SteamButton.onClick.AddListener(()=> {
			Debug.Log("Steam");
			SteamRequest();
		});

		// If we haven't set up the Unity Purchasing reference
		if (m_StoreController == null)
		{
			// Begin to configure our connection to Purchasing
			InitializePurchasing();
		}

		if (!SteamManager.Initialized)
        {
        	Debug.Log("Steam Not Initialized");
            return;
        }
        else
        {
        	Debug.Log("Steam Initialized");
        	steamID = SteamUser.GetSteamID();
        	steamDisplayName = SteamFriends.GetPersonaName();
        	Debug.Log("Steam ID -" + steamID);	
        	Debug.Log("Steam Name -" + steamDisplayName);
        	if (Get_SteamSessionTicket() != string.Empty || Get_SteamSessionTicket() != null)
            {
                steamSessionTicket = Get_SteamSessionTicket();
                haveSessionID = true;
                Debug.Log("Steam Ticket -" + steamSessionTicket);	
            }


        }
	}

	public void InitializePurchasing()
	{
		// If we have already connected to Purchasing ...
		if (IsInitialized())
		{
			// ... we are done here.
			return;
		}

		// Create a builder, first passing in a suite of Unity provided stores.
		var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

		// Add a product to sell / restore by way of its identifier, associating the general identifier
		// with its store-specific identifiers.
		builder.AddProduct(kProductIDConsumable, ProductType.Consumable);
		builder.AddProduct(iosProductIDConsumable, ProductType.Consumable);
		builder.AddProduct(kProductIDNonConsumable, ProductType.NonConsumable);
		UnityPurchasing.Initialize(this, builder);
	}

	public void DeviceAuth()
	{
		new DeviceAuthenticationRequest ().SetDisplayName ("Padraig").Send ((response)=>
			{
				if(!response.HasErrors)
					Debug.Log("Device Authenticated");
				else
					Debug.Log("Error during Device Auth");
			});
	}

	public void TwitchAuth()
	{
		string url = "https://id.twitch.tv/oauth2/authorize?client_id=lzu0rq7ept7x3z7kk4dhjzykr0fuae&redirect_uri=https://u346603hlEWD.preview.gamesparks.net/callback/u346603hlEWD/twitchOAuth/mXk3cqBVnDiRD9iazLWoEqG1Bx1npY3r&response_type=token&scope=user_read";
		Application.OpenURL (url);

		//WWW www = new WWW (url);
		//StartCoroutine (GetAccessToken (www));
	
	}

	public void SteamRequest()
	{


		new LogEventRequest()
			.SetEventKey("steamInitTxn")
			.SetEventAttribute("steamID", steamID.ToString())
			.Send((response) => {
				GSData scriptData = response.ScriptData;
			});

	}

	   string Get_SteamSessionTicket()
    {
        uint ticketLenght = 0;
        byte[] ticketStream = new byte[1024];

        SteamUser.GetAuthSessionTicket(ticketStream, 1024, out ticketLenght);

        string hexEncodedSessionTicket = "";

        for (int i = 0; i < ticketLenght; i++)
        {
            hexEncodedSessionTicket += string.Format("{0:X2}", ticketStream[i]);
        }

        return hexEncodedSessionTicket;
    }


	IEnumerator GetAccessToken(WWW www)
	{
		yield return www;

		if(www.url!="")
		{
			Debug.Log("URL > " + www.url);
		}

	}

	public void OnAccessToken(string accessToken)
	{
		Debug.Log ("Got Access Token: " + accessToken);

	}

	private bool IsInitialized()
	{
		// Only say we are initialized if both the Purchasing references are set.
		return m_StoreController != null && m_StoreExtensionProvider != null;
	}


	public void BuyConsumable()
	{
		// Buy the consumable product using its general identifier. Expect a response either 
		// through ProcessPurchase or OnPurchaseFailed asynchronously.
		BuyProductID(kProductIDConsumable);
	}


	public void BuyNonConsumable()
	{
		// Buy the non-consumable product using its general identifier. Expect a response either 
		// through ProcessPurchase or OnPurchaseFailed asynchronously.
		BuyProductID(kProductIDNonConsumable); 
	}




	void BuyProductID(string productId)
	{
		// If Purchasing has been initialized ...
		if (IsInitialized())
		{
			// ... look up the Product reference with the general product identifier and the Purchasing 
			// system's products collection.
			Product product = m_StoreController.products.WithID(productId);

			// If the look up found a product for this device's store and that product is ready to be sold ... 
			if (product != null && product.availableToPurchase)
			{
				Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
				// ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed 
				// asynchronously.
				m_StoreController.InitiatePurchase(product);
			}
			// Otherwise ...
			else
			{
				// ... report the product look-up failure situation  
				Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
			}
		}
		// Otherwise ...
		else
		{
			// ... report the fact Purchasing has not succeeded initializing yet. Consider waiting longer or 
			// retrying initiailization.
			Debug.Log("BuyProductID FAIL. Not initialized.");
		}
	}


	// Restore purchases previously made by this customer. Some platforms automatically restore purchases, like Google. 
	// Apple currently requires explicit purchase restoration for IAP, conditionally displaying a password prompt.
	/*  public void RestorePurchases()
    {
        // If Purchasing has not yet been set up ...
        if (!IsInitialized())
        {
            // ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
            Debug.Log("RestorePurchases FAIL. Not initialized.");
            return;
        }

        // If we are running on an Apple device ... 
        if (Application.platform == RuntimePlatform.IPhonePlayer ||
            Application.platform == RuntimePlatform.OSXPlayer)
        {
            // ... begin restoring purchases
            Debug.Log("RestorePurchases started ...");

            // Fetch the Apple store-specific subsystem.
            var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
            // Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
            // the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
            apple.RestoreTransactions((result) => {
                // The first phase of restoration. If no more responses are received on ProcessPurchase then 
                // no purchases are available to be restored.
                Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
            });
        }
        // Otherwise ...
        else
        {
            // We are not running on an Apple device. No work is necessary to restore purchases.
            Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
        }
    }*/


	//  
	// --- IStoreListener
	//

	public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
	{
		// Purchasing has succeeded initializing. Collect our Purchasing references.
		Debug.Log("OnInitialized: PASS");

		// Overall Purchasing system, configured with products for this application.
		m_StoreController = controller;
		// Store specific subsystem, for accessing device-specific store features.
		m_StoreExtensionProvider = extensions;
	}


	public void OnInitializeFailed(InitializationFailureReason error)
	{
		// Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
		Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
	}


	public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
	{


		// Return a flag indicating whether this product has completely been received, or if the application needs 
		// to be reminded of this purchase at next app launch. Use PurchaseProcessingResult.Pending when still 
		// saving purchased products to the cloud, and when that save is delayed. 
		Debug.Log(args.purchasedProduct.hasReceipt);
		string receipt = args.purchasedProduct.receipt;
		Debug.Log("receipt " + receipt);

		//validate receipt
		GooglePurchaseData data = new GooglePurchaseData(receipt);
		//send to gs
		new LogEventRequest()
			.SetEventKey("buyIAP")
			.SetEventAttribute("result", receipt)
			.SetEventAttribute("data1",data.inAppDataSignature)
			.SetEventAttribute("data2",data.inAppPurchaseData)
			.Send((response) => {
				GSData scriptData = response.ScriptData;
			});

		/*new GooglePlayBuyGoodsRequest()
			.SetSignature(data.inAppDataSignature)
			.SetSignedData(data.inAppPurchaseData)
			.Send((response) => {
				if (!response.HasErrors)
				{
					Debug.Log("Successful purchase");
				}
				else
				{
					Debug.Log("Purchase error: " + response.Errors.JSON);
				}
			});*/
		return PurchaseProcessingResult.Complete;
	}


	public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
	{
		// A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing 
		// this reason with the user to guide their troubleshooting actions.
		new LogEventRequest()
			.SetEventKey("buyIAP")
			.SetEventAttribute("result", "Fail")
			.Send((response) => {
				GSData scriptData = response.ScriptData;
			});
		Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
	}



}



