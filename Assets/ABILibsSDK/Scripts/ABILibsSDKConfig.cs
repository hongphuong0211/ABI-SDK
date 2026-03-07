using UnityEngine;

namespace ABILibsSDK
{
    [CreateAssetMenu(fileName = "ABILibsSDKConfig", menuName = "ABILibsSDK/Config")]
    public class ABILibsSDKConfig : ScriptableObject
    {
        private const string RESOURCE_PATH = "ABILibsSDKConfig";
        private const string LOG_PREFIX = "[ABILibsSDK]";

        [Header("MAX SDK")]
        public string maxSdkKey = "";

        [Header("Privacy / Consent")]
        [Tooltip("Bật luồng Terms & Privacy Policy / CMP của MAX (Google UMP automation nếu bạn bật trong Integration Manager). Nếu bật, hãy init Firebase/AppsFlyer sau callback OnSdkInitializedEvent.")]
        public bool useMaxTermsAndPrivacyPolicyFlow = true;

        [Header("MAX Ad Unit IDs - Android")]
        public string[] androidBannerAdUnitId = new string[0];
        public string[] androidInterstitialAdUnitId = new string[0];
        public string[] androidRewardedAdUnitId = new string[0];
        public string[] androidAppOpenAdUnitId = new string[0];

        [Header("MAX Ad Unit IDs - iOS")]
        public string[] iosBannerAdUnitId = new string[0];
        public string[] iosInterstitialAdUnitId = new string[0];
        public string[] iosRewardedAdUnitId = new string[0];
        public string[] iosAppOpenAdUnitId = new string[0];

        [Header("AppsFlyer")]
        public string appsFlyerDevKey = "";
        public string appsFlyerAppIdIOS = "";
        public bool appsFlyerDebug = false;

        [Header("Ads Settings")]
        [Tooltip("Max retry attempts for banner ads")]
        public int bannerRetryAttempts = 3;
        [Tooltip("Max retry attempts for interstitial ads")]
        public int interstitialRetryAttempts = 3;
        [Tooltip("Max retry attempts for rewarded ads")]
        public int rewardedRetryAttempts = 3;
        [Tooltip("Max retry attempts for app open ads")]
        public int appOpenRetryAttempts = 3;
        [Tooltip("Minimum interval (seconds) between interstitial ads")]
        public float interstitialInterval = 30f;
        [Tooltip("Auto-load ads after initialization")]
        public bool autoLoadAds = true;
        [Tooltip("Show App Open ad on app resume")]
        public bool showAppOpenOnResume = true;
        [Tooltip("Cooldown time to load next ad when ad is failed to load")]
        public float adLoadCooldownTime = 2f;

        public string[] BannerAdUnitId
        {
            get
            {
#if UNITY_ANDROID
                return androidBannerAdUnitId;
#elif UNITY_IOS
                return iosBannerAdUnitId;
#else
                return androidBannerAdUnitId;
#endif
            }
        }

        public string[] InterstitialAdUnitId
        {
            get
            {
#if UNITY_ANDROID
                return androidInterstitialAdUnitId;
#elif UNITY_IOS
                return iosInterstitialAdUnitId;
#else
                return androidInterstitialAdUnitId;
#endif
            }
        }

        public string[] RewardedAdUnitId
        {
            get
            {
#if UNITY_ANDROID
                return androidRewardedAdUnitId;
#elif UNITY_IOS
                return iosRewardedAdUnitId;
#else
                return androidRewardedAdUnitId;
#endif
            }
        }

        public string[] AppOpenAdUnitId
        {
            get
            {
#if UNITY_ANDROID
                return androidAppOpenAdUnitId;
#elif UNITY_IOS
                return iosAppOpenAdUnitId;
#else
                return androidAppOpenAdUnitId;
#endif
            }
        }

        private static ABILibsSDKConfig _instance;
        public static ABILibsSDKConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<ABILibsSDKConfig>(RESOURCE_PATH);
                    if (_instance == null)
                    {
                        Debug.LogError($"[ABILibsSDK] Config not found at Resources/{RESOURCE_PATH}. " +
                                       "Create one via Assets > Create > ABILibsSDK > Config and place it in a Resources folder.");
                    }
                }
                return _instance;
            }
        }
        public static void DebugLog(string message, System.Exception exception = null)
        {
            if (exception != null)
            {
                message = $"{message} {exception.Message}";
                message += $"\n{exception.StackTrace}";
            }
            Debug.Log($"{LOG_PREFIX} {message}");
        }
    }
}
