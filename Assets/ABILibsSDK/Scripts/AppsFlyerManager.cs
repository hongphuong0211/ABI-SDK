using System;
using System.Collections.Generic;
using UnityEngine;
using AppsFlyerSDK;

namespace ABILibsSDK
{
    public class AppsFlyerManager : MonoBehaviour, IAppsFlyerConversionData
    {
        public static AppsFlyerManager Instance { get; private set; }

        public bool IsInitialized { get; private set; }

        public event Action<Dictionary<string, object>> OnConversionDataReceived;
        public event Action<string> OnConversionDataFailed;
        public event Action<Dictionary<string, object>> OnAttributionDataReceived;

        private ABILibsSDKConfig _config;
        private Dictionary<string, object> _conversionData;
        private string _mediaSource;
        private string _campaign;
        private string _adGroup;
        private string _adSet;
        private bool _isOrganic;

        public Dictionary<string, object> ConversionData => _conversionData;
        public string MediaSource => _mediaSource;
        public string Campaign => _campaign;
        public string AdGroup => _adGroup;
        public string AdSet => _adSet;
        public bool IsOrganic => _isOrganic;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void Initialize(ABILibsSDKConfig config)
        {
            _config = config;

            AppsFlyer.setIsDebug(config.appsFlyerDebug);
            AppsFlyer.initSDK(config.appsFlyerDevKey, config.appsFlyerAppIdIOS, this);

#if UNITY_IOS && !UNITY_EDITOR
            AppsFlyer.waitForATTUserAuthorizationWithTimeoutInterval(60);
#endif

            AppsFlyer.startSDK();
            IsInitialized = true;

            ABILibsSDKConfig.DebugLog("AppsFlyer initialized");
        }

        #region Ad Revenue Tracking

        public void LogAdRevenue(MaxSdkBase.AdInfo adInfo)
        {
            if (!IsInitialized) return;

            double revenue = adInfo.Revenue;
            if (revenue <= 0) return;

            var adRevenueData = new AFAdRevenueData(
                adInfo.NetworkName,
                MediationNetwork.ApplovinMax,
                "USD",
                revenue
            );

            var additionalParams = new Dictionary<string, string>
            {
                { AdRevenueScheme.AD_UNIT, adInfo.AdUnitIdentifier },
                { AdRevenueScheme.AD_TYPE, adInfo.AdFormat },
                { AdRevenueScheme.PLACEMENT, adInfo.Placement ?? "" }
            };

            AppsFlyer.logAdRevenue(adRevenueData, additionalParams);

            Debug.Log($"[ABILibsSDK] AppsFlyer ad revenue logged: ${revenue:F6} from {adInfo.NetworkName}");
        }

        #endregion

        #region In-App Events

        public void LogEvent(string eventName, Dictionary<string, string> eventValues = null)
        {
            if (!IsInitialized) return;
            AppsFlyer.sendEvent(eventName, eventValues ?? new Dictionary<string, string>());
        }

        public void LogPurchase(string currency, string revenue, string contentId = "", string contentType = "")
        {
            if (!IsInitialized) return;

            var eventValues = new Dictionary<string, string>
            {
                { AFInAppEvents.CURRENCY, currency },
                { AFInAppEvents.REVENUE, revenue }
            };

            if (!string.IsNullOrEmpty(contentId))
                eventValues[AFInAppEvents.CONTENT_ID] = contentId;
            if (!string.IsNullOrEmpty(contentType))
                eventValues[AFInAppEvents.CONTENT_TYPE] = contentType;

            AppsFlyer.sendEvent(AFInAppEvents.PURCHASE, eventValues);
        }

        public void LogLevelComplete(string level, string score = "")
        {
            if (!IsInitialized) return;

            var eventValues = new Dictionary<string, string>
            {
                { AFInAppEvents.LEVEL, level }
            };

            if (!string.IsNullOrEmpty(score))
                eventValues[AFInAppEvents.SCORE] = score;

            AppsFlyer.sendEvent(AFInAppEvents.LEVEL_ACHIEVED, eventValues);
        }

        #endregion

        #region Attribution / Conversion Data

        public void onConversionDataSuccess(string conversionData)
        {
            Debug.Log($"[ABILibsSDK] AppsFlyer conversion data: {conversionData}");

            _conversionData = AppsFlyer.CallbackStringToDictionary(conversionData);

            ExtractAttributionData(_conversionData);

            OnConversionDataReceived?.Invoke(_conversionData);
        }

        public void onConversionDataFail(string error)
        {
            Debug.LogWarning($"[ABILibsSDK] AppsFlyer conversion data error: {error}");
            _isOrganic = true;
            OnConversionDataFailed?.Invoke(error);
        }

        public void onAppOpenAttribution(string attributionData)
        {
            Debug.Log($"[ABILibsSDK] AppsFlyer app open attribution: {attributionData}");

            var data = AppsFlyer.CallbackStringToDictionary(attributionData);
            OnAttributionDataReceived?.Invoke(data);
        }

        public void onAppOpenAttributionFailure(string error)
        {
            Debug.LogWarning($"[ABILibsSDK] AppsFlyer app open attribution error: {error}");
        }

        private void ExtractAttributionData(Dictionary<string, object> data)
        {
            if (data == null) return;

            _mediaSource = GetStringValue(data, "media_source");
            _campaign = GetStringValue(data, "campaign");
            _adGroup = GetStringValue(data, "adgroup");
            _adSet = GetStringValue(data, "adset");

            var status = GetStringValue(data, "af_status");
            _isOrganic = string.IsNullOrEmpty(status) || status.Equals("Organic", StringComparison.OrdinalIgnoreCase);

            Debug.Log($"[ABILibsSDK] Attribution - Organic: {_isOrganic}, Source: {_mediaSource}, Campaign: {_campaign}");

            if (FirebaseManager.Instance != null && FirebaseManager.Instance.IsInitialized)
            {
                FirebaseManager.Instance.SetUserProperty("media_source", _isOrganic ? "organic" : _mediaSource);
                FirebaseManager.Instance.SetUserProperty("campaign", _campaign ?? "");
                FirebaseManager.Instance.SetUserProperty("is_organic", _isOrganic.ToString().ToLower());
            }
        }

        public string GetAttributionValue(string key)
        {
            if (_conversionData == null) return "";
            return GetStringValue(_conversionData, key);
        }

        private static string GetStringValue(Dictionary<string, object> dict, string key)
        {
            if (dict != null && dict.TryGetValue(key, out var value) && value != null)
            {
                return value.ToString();
            }
            return "";
        }

        #endregion

        #region User Identity

        public void SetCustomerUserId(string userId)
        {
            if (!IsInitialized) return;
            AppsFlyer.setCustomerUserId(userId);
        }

        public string GetAppsFlyerId()
        {
            return AppsFlyer.getAppsFlyerId();
        }

        #endregion
    }
}
