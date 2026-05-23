#if ENABLE_IAP

using UnityEngine.Purchasing;
using JangLib;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public class IAPManager : SingletonWithMono<IAPManager>, IBaseManager
{
    public bool IsInitialized { private set; get; }

    private StoreController _storeController;

    public void Init()
    {
        InitializeIAP();
        IsInitialized = true;
    }

    private async void InitializeIAP()
    {
        _storeController = UnityIAPServices.StoreController();

        _storeController.OnStoreConnected += OnStoreConnected;
        _storeController.OnStoreDisconnected += OnStoreDisconnected;

        _storeController.OnPurchasePending += OnPurchasePending;
        _storeController.OnPurchaseDeferred += OnPurchaseDeferred;
        _storeController.OnPurchaseConfirmed += OnPurchaseConfirmed;
        _storeController.OnPurchaseFailed += OnPurchaseFailed;

        try
        {
            await _storeController.Connect();

            _storeController.OnProductsFetched += OnProductsFetched;
            _storeController.OnProductsFetchFailed += OnProductsFetchFailed;

            _storeController.OnPurchasesFetched += OnPurchasesFetched;
            _storeController.OnPurchasesFetchFailed += OnPurchasesFetchFailed;

            var initialProductsToFetch = new List<ProductDefinition>
            {
                new(IAP_Products.PRODUCT_NO_ADS_ID, ProductType.NonConsumable)
            };
            _storeController.FetchProducts(initialProductsToFetch);

        }
        catch (Exception e)
        {
            EditorLog.LogError($"[IAP] Connection Failed: {e.Message}");
        }
    }

    private void OnStoreConnected()
    {
        EditorLog.Log("[IAP] Store Service Connected!");
    }

    private void OnStoreDisconnected(StoreConnectionFailureDescription desc)
    {
        EditorLog.Log($"[IAP] Store Service Disconnected...\n{desc.message}");
    }

    public void PurchaseNoAds()
    {
        if (!IsInitialized) return;

        EditorLog.Log($"[IAP] Starting purchase: {IAP_Products.PRODUCT_NO_ADS_ID}");

        Product noAdsProduct = _storeController.GetProductById(IAP_Products.PRODUCT_NO_ADS_ID);

        if (noAdsProduct != null)
        {
            _storeController.PurchaseProduct(noAdsProduct);
        }
    }

    #region << Events >>

    private void OnProductsFetched(List<Product> products)
    {
        EditorLog.Log($"[IAP] Fetched {products.Count} products.");

        _storeController.FetchPurchases();
    }

    private void OnProductsFetchFailed(ProductFetchFailed failed)
    {
        EditorLog.Log($"[IAP] Products Fetched Faild...\n{failed.FailureReason}");
    }

    private void OnPurchasesFetched(Orders orders)
    {
        foreach (var order in orders.ConfirmedOrders)
        {
            var product = order.Info.PurchasedProductInfo.FirstOrDefault(x => x.productId == IAP_Products.PRODUCT_NO_ADS_ID);
            if (product != null)
            {
                EditorLog.Log("[IAP] 광고 제거 구매내역 있음");
                EventManager.Instance.Trigger((int)EventType.OnPurchasedNoAds);
            }
        }
    }

    private void OnPurchasesFetchFailed(PurchasesFetchFailureDescription desc)
    {
        EditorLog.Log($"[IAP] Purchases Fetched Faild...\n{desc.message}");
    }

    private void OnPurchasePending(PendingOrder pendingOrder)
    {
        EditorLog.Log($"[IAP] Purchase Pending: {pendingOrder.Info.PurchasedProductInfo[0].productId}");

        foreach (var product in pendingOrder.CartOrdered.Items())
        {
            GrantProduct(product.Product);
        }

        _storeController.ConfirmPurchase(pendingOrder);
    }

    private void OnPurchaseDeferred(DeferredOrder deferredOrder)
    {
        EditorLog.Log($"[IAP] Purchase Deferred: {deferredOrder.Info.PurchasedProductInfo[0].productId}");
    }

    private void OnPurchaseConfirmed(Order order)
    {
        EditorLog.Log($"[IAP] Purchase Confirmed: {order.Info.PurchasedProductInfo[0].productId}. Starting Validation...");

        string rawReceipt = order.Info.Receipt;
        EditorLog.Log($"[IAP] Raw Receipt: {rawReceipt}");

        try
        {
            GooglePurchase purchaseData = GooglePurchase.FromJson(rawReceipt);

            if (purchaseData.Store == "fake")
            {
                EventManager.Instance.Trigger((int)EventType.OnPurchasedNoAds);
                return;
            }

            if (purchaseData.PayloadData != null)
            {
                foreach (var productInfo in order.Info.PurchasedProductInfo)
                {
                    Product product = _storeController.GetProductById(productInfo.productId);
                    ValidateWithPlayFab(product, purchaseData.PayloadData);
                }
            }
        }
        catch (Exception e)
        {
            EditorLog.LogError($"[IAP] Receipt Parsing Failed: {e.Message}");
        }
    }

    private void OnPurchaseFailed(FailedOrder order)
    {
        EditorLog.LogWarning($"[IAP] Purchase Failed: {order}. Reason: {order.FailureReason}");
    }

    #endregion

    private void ValidateWithPlayFab(Product product, PayloadData payload)
    {
        PlayFabManager.Instance.ValidateGooglePurchase(product, payload, (success) =>
        {
            if (success)
            {
                EditorLog.Log("[IAP] Ad-Free Status Updated via Server.");
            }
        });
    }

    // 아이템 적용
    private void GrantProduct(Product product)
    {

    }
}

[Serializable]
public class JsonData
{
    public string orderId;
    public string packageName;
    public string productId;
    public long purchaseTime;
    public int purchaseState;
    public string purchaseToken;
}

[Serializable]
public class PayloadData
{
    public JsonData JsonData;

    public string signature;
    public string json;

    public static PayloadData FromJson(string json)
    {
        var payload = JsonUtility.FromJson<PayloadData>(json);
        payload.JsonData = JsonUtility.FromJson<JsonData>(payload.json);
        return payload;
    }
}

[Serializable]
public class GooglePurchase
{
    public PayloadData PayloadData;

    public string Store;
    public string TransactionID;
    public string Payload;

    public static GooglePurchase FromJson(string json)
    {
        var purchase = JsonUtility.FromJson<GooglePurchase>(json);

        if (purchase.Store == "fake")
        {
            return purchase;
        }

        if (!string.IsNullOrEmpty(purchase.Payload))
        {
            purchase.PayloadData = PayloadData.FromJson(purchase.Payload);
        }
        return purchase;
    }
}

#endif