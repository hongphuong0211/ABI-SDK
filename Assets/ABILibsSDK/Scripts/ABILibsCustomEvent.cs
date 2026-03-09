using Firebase.Analytics;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;
namespace ABILibsSDK
{
    public class ABILibsCustomEvent
    {
        #region  TROAS Ad Event
        private const string TROAS_CACHE_KEY_BANNER = "[ABILibsSDK]troas_cache_banner";
        private const string TROAS_CACHE_KEY = "[ABILibsSDK]troas_cache";
        private const string IS_FIRST_TIME_CACHE_KEY = "[ABILibsSDK]is_first_time_cache_";
        public static void TROASEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            if (ABILibsCustomEventConfig.Instance == null) return;
            double revenue = adInfo.Revenue;
            if (adInfo.AdFormat.ToLower() == "banner" || adInfo.AdFormat.ToLower() == "mrec")
            {
                // With Banner and Mrec, we should track until revenue reach the min threshold
                float troasCacheBanner = PlayerPrefs.GetFloat(TROAS_CACHE_KEY_BANNER, 0);
                float currentTroasCacheBanner = troasCacheBanner + (float)revenue;
                PlayerPrefs.SetFloat(TROAS_CACHE_KEY_BANNER, currentTroasCacheBanner);
                if (currentTroasCacheBanner <= ABILibsCustomEventConfig.Instance.minRevenueThresholdForBannerAndMrec)
                {
                    return;
                }
                else
                {
                    revenue = currentTroasCacheBanner;
                    PlayerPrefs.SetFloat(TROAS_CACHE_KEY_BANNER, 0);
                }
            }

            float troasCache = PlayerPrefs.GetFloat(TROAS_CACHE_KEY, 0);
            float currentTroasCache = troasCache + (float)revenue;
            PlayerPrefs.SetFloat(TROAS_CACHE_KEY, currentTroasCache);
            for (int i = 0; i < ABILibsCustomEventConfig.Instance.troasAdEvents.Length; i++)
            {
                if (currentTroasCache >= ABILibsCustomEventConfig.Instance.troasAdEvents[i])
                {
                    bool isFirstTimeCache = PlayerPrefs.GetInt(IS_FIRST_TIME_CACHE_KEY + i, 0) == 0;
                    float eventVal = (float)adInfo.Revenue;
                    if (isFirstTimeCache)
                    {
                        eventVal = currentTroasCache;
                        PlayerPrefs.SetInt(IS_FIRST_TIME_CACHE_KEY + i, 1);
                    }
                    Parameter[] parameters = new Parameter[2];
                    parameters[0] = new Parameter("value", (double)eventVal);
                    parameters[1] = new Parameter("currency", "USD");
                    FirebaseAnalytics.LogEvent(ABILibsCustomEventConfig.Instance.baseTROASEventName + i.ToString(), parameters);
                }
            }
        }
        private const string TROAS_CACHE_KEY_2 = "[ABILibsSDK]troas_cache_2";
        private const string PREFIX_LAST_SEND_EVENT_CACHE_2 = "[ABILibsSDK]last_send_event_cache_2_";
        public static void TROASEvent2(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            if (ABILibsCustomEventConfig.Instance == null) return;
            double revenue = adInfo.Revenue;
            if (adInfo.AdFormat.ToLower() == "banner" || adInfo.AdFormat.ToLower() == "mrec")
            {
                // With Banner and Mrec, we should track until revenue reach the min threshold
                float troasCacheBanner = PlayerPrefs.GetFloat(TROAS_CACHE_KEY_BANNER, 0);
                float currentTroasCacheBanner = troasCacheBanner + (float)revenue;
                PlayerPrefs.SetFloat(TROAS_CACHE_KEY_BANNER, currentTroasCacheBanner);
                if (currentTroasCacheBanner <= ABILibsCustomEventConfig.Instance.minRevenueThresholdForBannerAndMrec)
                {
                    return;
                }
                else
                {
                    revenue = currentTroasCacheBanner;
                    PlayerPrefs.SetFloat(TROAS_CACHE_KEY_BANNER, 0);
                }
            }

            float troasCache2 = PlayerPrefs.GetFloat(TROAS_CACHE_KEY_2, 0);
            float currentTroasCache2 = troasCache2 + (float)revenue;
            PlayerPrefs.SetFloat(TROAS_CACHE_KEY_2, currentTroasCache2);
            for (int i = 0; i < ABILibsCustomEventConfig.Instance.troasAdEvents2.Length; i++)
            {
                float lastSendEventCache2 = PlayerPrefs.GetFloat(PREFIX_LAST_SEND_EVENT_CACHE_2 + i, 0);
                if (currentTroasCache2 - lastSendEventCache2 >= ABILibsCustomEventConfig.Instance.troasAdEvents2[i])
                {
                    Parameter[] parameters2 = new Parameter[2];
                    parameters2[0] = new Parameter("value", (double)(currentTroasCache2 - lastSendEventCache2));
                    parameters2[1] = new Parameter("currency", "USD");
                    FirebaseAnalytics.LogEvent(ABILibsCustomEventConfig.Instance.baseTROASEventName2 + i.ToString(), parameters2);
                    PlayerPrefs.SetFloat(PREFIX_LAST_SEND_EVENT_CACHE_2 + i, currentTroasCache2);
                    break;
                }
            }
        }
        #endregion

        #region  Bamboo Ad Event
        private const string BAMBOO_COUNT_CACHE_KEY = "[ABILibsSDK]bamboo_count_cache";
        private const string PREFIX_CACHE_REVENUE_BAMBOO = "[ABILibsSDK]cache_revenue_bamboo_";
        private const string PREFIX_LAST_SEND_EVENT_CACHE_BAMBOO = "[ABILibsSDK]last_send_event_cache_bamboo_";
        public static void BambooAdEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            if (ABILibsCustomEventConfig.Instance == null) return;
            int bambooCount = PlayerPrefs.GetInt(BAMBOO_COUNT_CACHE_KEY, 0);
            bambooCount++;
            PlayerPrefs.SetInt(BAMBOO_COUNT_CACHE_KEY, bambooCount);

            for (int i = 0; i < ABILibsCustomEventConfig.Instance.bambooCountAdEvents.Length; i++)
            {
                float cacheRevenue = PlayerPrefs.GetFloat(PREFIX_CACHE_REVENUE_BAMBOO + i, 0);
                cacheRevenue += (float)adInfo.Revenue;
                PlayerPrefs.SetFloat(PREFIX_CACHE_REVENUE_BAMBOO + i, cacheRevenue);
                if (bambooCount >= ABILibsCustomEventConfig.Instance.bambooCountAdEvents[i])
                {
                    float eventVal = (float)adInfo.Revenue;
                    bool isFirstTimeCache = PlayerPrefs.GetInt(PREFIX_LAST_SEND_EVENT_CACHE_BAMBOO + i, 0) == 0;
                    if (isFirstTimeCache)
                    {
                        eventVal = cacheRevenue;
                        PlayerPrefs.SetInt(PREFIX_LAST_SEND_EVENT_CACHE_BAMBOO + i, 1);
                    }
                    Parameter[] parameters = new Parameter[2];
                    parameters[0] = new Parameter("value", (double)eventVal);
                    parameters[1] = new Parameter("currency", "USD");
                    FirebaseAnalytics.LogEvent(ABILibsCustomEventConfig.Instance.baseBambooAdEventName + i.ToString(), parameters);
                    PlayerPrefs.SetFloat(PREFIX_LAST_SEND_EVENT_CACHE_BAMBOO + i, cacheRevenue);
                }
            }
        }
        private const string BAMBOO_COUNT_CACHE_KEY_REWARDED = "[ABILibsSDK]bamboo_count_cache_rewarded";
        private const string PREFIX_CACHE_REVENUE_BAMBOO_REWARDED = "[ABILibsSDK]cache_revenue_bamboo_rewarded_";
        private const string PREFIX_LAST_SEND_EVENT_CACHE_BAMBOO_REWARDED = "[ABILibsSDK]last_send_event_cache_bamboo_rewarded_";
        public static void BambooRewardedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            if (ABILibsCustomEventConfig.Instance == null) return;
            int bambooCount = PlayerPrefs.GetInt(BAMBOO_COUNT_CACHE_KEY_REWARDED, 0);
            bambooCount++;
            PlayerPrefs.SetInt(BAMBOO_COUNT_CACHE_KEY_REWARDED, bambooCount);

            for (int i = 0; i < ABILibsCustomEventConfig.Instance.bambooCountRewardedEvents.Length; i++)
            {
                float cacheRevenue = PlayerPrefs.GetFloat(PREFIX_CACHE_REVENUE_BAMBOO_REWARDED + i, 0);
                cacheRevenue += (float)adInfo.Revenue;
                PlayerPrefs.SetFloat(PREFIX_CACHE_REVENUE_BAMBOO_REWARDED + i, cacheRevenue);
                if (bambooCount >= ABILibsCustomEventConfig.Instance.bambooCountRewardedEvents[i])
                {
                    float eventVal = (float)adInfo.Revenue;
                    bool isFirstTimeCache = PlayerPrefs.GetInt(PREFIX_LAST_SEND_EVENT_CACHE_BAMBOO_REWARDED + i, 0) == 0;
                    if (isFirstTimeCache)
                    {
                        eventVal = cacheRevenue;
                        PlayerPrefs.SetInt(PREFIX_LAST_SEND_EVENT_CACHE_BAMBOO_REWARDED + i, 1);
                    }
                    Parameter[] parameters = new Parameter[2];
                    parameters[0] = new Parameter("value", (double)eventVal);
                    parameters[1] = new Parameter("currency", "USD");
                    FirebaseAnalytics.LogEvent(ABILibsCustomEventConfig.Instance.baseBambooRewardedEventName + i.ToString(), parameters);
                    PlayerPrefs.SetFloat(PREFIX_LAST_SEND_EVENT_CACHE_BAMBOO_REWARDED + i, cacheRevenue);
                }
            }
        }
        #endregion

        #region  Purchase Event
        public static void TROASPurchaseEvent(string productId, float price, string currency)
        {
            if (ABILibsCustomEventConfig.Instance == null) return;
            double priceInUSD = price * GetExchangeRate(currency);
            for (int i = 0; i < ABILibsCustomEventConfig.Instance.troasPurchaseEvents.Length; i++)
            {
                if (priceInUSD >= ABILibsCustomEventConfig.Instance.troasPurchaseEvents[i].minRange && priceInUSD <= ABILibsCustomEventConfig.Instance.troasPurchaseEvents[i].maxRange)
                {
                    int logCount = ABILibsCustomEventConfig.Instance.troasPurchaseEvents[i].logCount;
                    if (logCount > 0)
                    {
                        double eventVal = priceInUSD / logCount;
                        for (int j = 0; j < logCount; j++)
                        {
                            Parameter[] parameters = new Parameter[2];
                            parameters[0] = new Parameter("value", (double)eventVal);
                            parameters[1] = new Parameter("currency", "USD");
                            FirebaseAnalytics.LogEvent(ABILibsCustomEventConfig.Instance.baseTROASPurchaeEventName, parameters);
                        }
                    }
                    break;
                }
            }
        }
        private static double GetExchangeRate(string currency)
        {
            var exchangeRates = ParseExchangeRatesJson(ABILibsCustomEventConfig.Instance.exchangeRates);
            return exchangeRates[currency];
        }

        private static Dictionary<string, double> ParseExchangeRatesJson(string json)
        {
            var result = new Dictionary<string, double>();
            var matches = Regex.Matches(json, @"\""([^""]+)\""\s*:\s*([\d.]+)");
            for (int i = 0; i < matches.Count; i++)
            {
                var m = matches[i];
                result[m.Groups[1].Value] = double.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);
            }
            return result;
        }
        #endregion
        public static void LogEvent(string eventName, Dictionary<string, string> parameters)
        {
            var parametersArray = new Parameter[parameters.Count];
            int i = 0;
            foreach (var parameter in parameters)
            {
                parametersArray[i] = new Parameter(parameter.Key, parameter.Value);
                i++;
            }
            FirebaseAnalytics.LogEvent(eventName, parametersArray);
        }
    }
}