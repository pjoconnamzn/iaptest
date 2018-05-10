using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GooglePurchaseData
{
    // INAPP_PURCHASE_DATA
    public string inAppPurchaseData;
    // INAPP_DATA_SIGNATURE
    public string inAppDataSignature;

    [System.Serializable]
    private struct GooglePurchaseReceipt
    {
        public string Payload;
    }
    [System.Serializable]
    private struct GooglePurchasePayload
    {
        public string json;
        public string signature;
    }
    public GooglePurchaseData(string receipt)
    {
        try
        {
            var purchaseReceipt = JsonUtility.FromJson<GooglePurchaseReceipt>(receipt);
            var purchasePayload = JsonUtility.FromJson<GooglePurchasePayload>(purchaseReceipt.Payload);
            inAppPurchaseData = purchasePayload.json;
            inAppDataSignature = purchasePayload.signature;
        }
        catch
        {
            Debug.Log("Could not parse receipt: " + receipt);
            inAppPurchaseData = "";
            inAppDataSignature = "";
        }
    }

}

