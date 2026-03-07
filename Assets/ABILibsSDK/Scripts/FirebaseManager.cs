using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Analytics;
using Firebase.RemoteConfig;

namespace ABILibsSDK
{
    public class FirebaseManager : MonoBehaviour
    {
        public static FirebaseManager Instance { get; private set; }

        public bool IsInitialized { get; private set; }
        public event Action OnFirebaseInitialized;

        private FirebaseApp _app;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void Initialize()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available)
                {
                    _app = FirebaseApp.DefaultInstance;
                    IsInitialized = true;

                    SetDefaultRemoteConfigValues();

                    ABILibsSDKConfig.DebugLog("Firebase initialized successfully");

                    MainThreadDispatcher.Enqueue(() => OnFirebaseInitialized?.Invoke());
                }
                else
                {
                    Debug.LogError($"[ABILibsSDK] Firebase dependency error: {dependencyStatus}");
                }
            });
        }

        #region Analytics

        public void LogEvent(string eventName)
        {
            if (!IsInitialized) return;
            FirebaseAnalytics.LogEvent(eventName);
        }

        public void LogEvent(string eventName, string paramName, string paramValue)
        {
            if (!IsInitialized) return;
            FirebaseAnalytics.LogEvent(eventName, paramName, paramValue);
        }

        public void LogEvent(string eventName, string paramName, long paramValue)
        {
            if (!IsInitialized) return;
            FirebaseAnalytics.LogEvent(eventName, paramName, paramValue);
        }

        public void LogEvent(string eventName, string paramName, double paramValue)
        {
            if (!IsInitialized) return;
            FirebaseAnalytics.LogEvent(eventName, paramName, paramValue);
        }

        public void LogEvent(string eventName, params Parameter[] parameters)
        {
            if (!IsInitialized) return;
            FirebaseAnalytics.LogEvent(eventName, parameters);
        }

        public void LogAdRevenue(MaxSdkBase.AdInfo adInfo)
        {
            if (!IsInitialized) return;

            double revenue = adInfo.Revenue;
            if (revenue <= 0) return;

            var impressionParameters = new[]
            {
                new Parameter("ad_platform", "AppLovin"),
                new Parameter("ad_source", adInfo.NetworkName),
                new Parameter("ad_unit_name", adInfo.AdUnitIdentifier),
                new Parameter("ad_format", adInfo.AdFormat),
                new Parameter("value", revenue),
                new Parameter("currency", "USD")
            };

            FirebaseAnalytics.LogEvent("ad_impression", impressionParameters);
        }

        public void SetUserId(string userId)
        {
            if (!IsInitialized) return;
            FirebaseAnalytics.SetUserId(userId);
        }

        public void SetUserProperty(string name, string value)
        {
            if (!IsInitialized) return;
            FirebaseAnalytics.SetUserProperty(name, value);
        }

        #endregion

        #region Remote Config

        private void SetDefaultRemoteConfigValues()
        {
            var defaults = new Dictionary<string, object>
            {
                { "inter_ad_interval", 30 },
                { "show_banner", true },
                { "show_app_open", true },
            };

            FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults).ContinueWith(task =>
            {
                ABILibsSDKConfig.DebugLog("Remote Config defaults set");
                FetchRemoteConfig();
            });
        }

        public void FetchRemoteConfig(Action<bool> callback = null)
        {
            if (!IsInitialized)
            {
                callback?.Invoke(false);
                return;
            }

            var fetchTask = FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.FromHours(1));
            fetchTask.ContinueWith(task =>
            {
                if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
                {
                    FirebaseRemoteConfig.DefaultInstance.ActivateAsync().ContinueWith(activateTask =>
                    {
                        ABILibsSDKConfig.DebugLog("Remote Config fetched and activated");
                        MainThreadDispatcher.Enqueue(() => callback?.Invoke(true));
                    });
                }
                else
                {
                    ABILibsSDKConfig.DebugLog("Remote Config fetch failed");
                    MainThreadDispatcher.Enqueue(() => callback?.Invoke(false));
                }
            });
        }

        public string GetRemoteConfigString(string key, string defaultValue = "")
        {
            if (!IsInitialized) return defaultValue;
            return FirebaseRemoteConfig.DefaultInstance.GetValue(key).StringValue;
        }

        public long GetRemoteConfigLong(string key, long defaultValue = 0)
        {
            if (!IsInitialized) return defaultValue;
            return FirebaseRemoteConfig.DefaultInstance.GetValue(key).LongValue;
        }

        public double GetRemoteConfigDouble(string key, double defaultValue = 0)
        {
            if (!IsInitialized) return defaultValue;
            return FirebaseRemoteConfig.DefaultInstance.GetValue(key).DoubleValue;
        }

        public bool GetRemoteConfigBool(string key, bool defaultValue = false)
        {
            if (!IsInitialized) return defaultValue;
            return FirebaseRemoteConfig.DefaultInstance.GetValue(key).BooleanValue;
        }

        #endregion
    }
}
