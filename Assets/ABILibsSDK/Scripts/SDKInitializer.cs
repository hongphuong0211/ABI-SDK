using System;
using System.Collections.Generic;
using UnityEngine;

namespace ABILibsSDK
{
    public class SDKInitializer : MonoBehaviour
    {
        public static SDKInitializer Instance { get; private set; }

        public bool IsFullyInitialized { get; private set; }
        public event Action OnAllSDKsInitialized;

        [SerializeField] private ABILibsSDKConfig config;

        private bool _firebaseReady;
        private bool _adsReady;
        private bool _appsFlyerReady;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (config == null)
            {
                config = ABILibsSDKConfig.Instance;
            }

            if (config == null)
            {
                ABILibsSDKConfig.DebugLog("No config found! Please assign ABILibsSDKConfig.");
                return;
            }

            InitializeSDKs();
        }

        private void InitializeSDKs()
        {
            ABILibsSDKConfig.DebugLog("Starting SDK initialization...");

            if (config.useMaxTermsAndPrivacyPolicyFlow)
            {
                InitializeMaxThenThirdParties();
            }
            else
            {
                ConsentManager.EnsureInstance();
                InitializeFirebase();
                InitializeAppsFlyer();
            }
        }

        private void InitializeMaxThenThirdParties()
        {
            var adsManager = AdsManager.Instance;
            if (adsManager == null)
            {
                ABILibsSDKConfig.DebugLog("AdsManager not found on this GameObject!");
                return;
            }

            SubscribeAdRevenueEvents(adsManager);

            MaxSdkCallbacks.OnSdkInitializedEvent += (_) =>
            {
                _adsReady = true;
                CheckAllInitialized();

                InitializeFirebase();
                InitializeAppsFlyer();
            };

            ABILibsSDKConfig.DebugLog("Initializing MAX SDK (will show Terms/Privacy/CMP flow if enabled)...");
            adsManager.Initialize(config);
        }

        private void InitializeFirebase()
        {
            var firebaseManager = FirebaseManager.Instance;
            if (firebaseManager == null)
            {
                ABILibsSDKConfig.DebugLog("[ABILibsSDK] FirebaseManager not found on this GameObject!");
                return;
            }

            firebaseManager.OnFirebaseInitialized += () =>
            {
                _firebaseReady = true;
                if (!config.useMaxTermsAndPrivacyPolicyFlow)
                {
                    ABILibsSDKConfig.DebugLog("Firebase ready. Waiting for consent before initializing MAX SDK...");

                    ConsentManager.Instance.EnsureConsentResolved((state) =>
                    {
                        Debug.Log($"[ABILibsSDK] Consent resolved. HasUserConsent={state.HasUserConsent}, DoNotSell={state.DoNotSell}. Initializing MAX SDK...");
                        InitializeAds();
                    });
                }

                CheckAllInitialized();
            };

            firebaseManager.Initialize();
        }

        private void InitializeAds()
        {
            var adsManager = AdsManager.Instance;
            if (adsManager == null)
            {
                ABILibsSDKConfig.DebugLog("AdsManager not found on this GameObject!");
                return;
            }

            SubscribeAdRevenueEvents(adsManager);
            adsManager.Initialize(config);
            _adsReady = true;
            CheckAllInitialized();
        }

        private void InitializeAppsFlyer()
        {
            var appsFlyerManager = AppsFlyerManager.Instance;
            if (appsFlyerManager == null)
            {
                ABILibsSDKConfig.DebugLog("AppsFlyerManager not found on this GameObject!");
                return;
            }

            appsFlyerManager.OnConversionDataReceived += (data) =>
            {
                ABILibsSDKConfig.DebugLog("AppsFlyer conversion data received");
            };

            appsFlyerManager.Initialize(config);
            _appsFlyerReady = true;
            CheckAllInitialized();
        }

        private void SubscribeAdRevenueEvents(AdsManager adsManager)
        {
            adsManager.OnBannerAdRevenuePaid += TrackAdRevenue;
            adsManager.OnInterstitialAdRevenuePaid += TrackAdRevenue;
            adsManager.OnRewardedAdRevenuePaid += TrackAdRevenue;
            adsManager.OnAppOpenAdRevenuePaid += TrackAdRevenue;
        }

        private void TrackAdRevenue(MaxSdkBase.AdInfo adInfo)
        {
            if (FirebaseManager.Instance != null)
            {
                FirebaseManager.Instance.LogAdRevenue(adInfo);
            }

            if (AppsFlyerManager.Instance != null)
            {
                AppsFlyerManager.Instance.LogAdRevenue(adInfo);
            }
            ABILibsCustomEvent.TROASEvent(adInfo.AdUnitIdentifier, adInfo);
        }

        private void CheckAllInitialized()
        {
            if (_firebaseReady && _adsReady && _appsFlyerReady && !IsFullyInitialized)
            {
                IsFullyInitialized = true;
                ABILibsSDKConfig.DebugLog("All SDKs initialized successfully!");
                OnAllSDKsInitialized?.Invoke();
            }
        }
    }

    public readonly struct ConsentState
    {
        public readonly bool HasUserConsent;
        public readonly bool DoNotSell;

        public ConsentState(bool hasUserConsent, bool doNotSell)
        {
            HasUserConsent = hasUserConsent;
            DoNotSell = doNotSell;
        }
    }

    public sealed class ConsentManager : MonoBehaviour
    {
        public static ConsentManager Instance { get; private set; }

        private const string KeyConsentResolved = "ABILibsSDK_Consent_Resolved";
        private const string KeyHasUserConsent = "ABILibsSDK_HasUserConsent";
        private const string KeyDoNotSell = "ABILibsSDK_DoNotSell";

        private readonly List<Action<ConsentState>> _pendingCallbacks = new List<Action<ConsentState>>();
        private bool _showDialog;
        private bool _hasUserConsent;
        private bool _doNotSell;

        public static void EnsureInstance()
        {
            if (Instance != null) return;

            var existing = FindObjectOfType<ConsentManager>();
            if (existing != null)
            {
                Instance = existing;
                DontDestroyOnLoad(existing.gameObject);
                return;
            }

            var go = new GameObject(nameof(ConsentManager));
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<ConsentManager>();
        }

        public static bool IsConsentResolved()
        {
            return PlayerPrefs.GetInt(KeyConsentResolved, 0) == 1;
        }

        public static ConsentState GetSavedOrDefault()
        {
            if (!IsConsentResolved())
            {
                return new ConsentState(hasUserConsent: false, doNotSell: false);
            }

            var hasUserConsent = PlayerPrefs.GetInt(KeyHasUserConsent, 0) == 1;
            var doNotSell = PlayerPrefs.GetInt(KeyDoNotSell, 0) == 1;
            return new ConsentState(hasUserConsent, doNotSell);
        }

        public static void ApplyMaxPrivacyFlagsFromSavedState()
        {
            var state = GetSavedOrDefault();
            MaxSdk.SetHasUserConsent(state.HasUserConsent);
            MaxSdk.SetDoNotSell(state.DoNotSell);
        }

        public void EnsureConsentResolved(Action<ConsentState> onResolved)
        {
            if (onResolved == null) return;

            if (IsConsentResolved())
            {
                onResolved(GetSavedOrDefault());
                return;
            }

            _pendingCallbacks.Add(onResolved);

            if (!_showDialog)
            {
                _hasUserConsent = false;
                _doNotSell = false;
                _showDialog = true;
            }
        }

        private void ResolveAndPersist(bool hasUserConsent, bool doNotSell)
        {
            PlayerPrefs.SetInt(KeyConsentResolved, 1);
            PlayerPrefs.SetInt(KeyHasUserConsent, hasUserConsent ? 1 : 0);
            PlayerPrefs.SetInt(KeyDoNotSell, doNotSell ? 1 : 0);
            PlayerPrefs.Save();

            MaxSdk.SetHasUserConsent(hasUserConsent);
            MaxSdk.SetDoNotSell(doNotSell);

            var state = new ConsentState(hasUserConsent, doNotSell);
            for (int i = 0; i < _pendingCallbacks.Count; i++)
            {
                try { _pendingCallbacks[i]?.Invoke(state); }
                catch (Exception e) { Debug.LogException(e); }
            }
            _pendingCallbacks.Clear();
        }

        private void OnGUI()
        {
            if (!_showDialog || IsConsentResolved()) return;

            const float width = 560f;
            const float height = 260f;
            var rect = new Rect(
                (Screen.width - width) * 0.5f,
                (Screen.height - height) * 0.5f,
                width,
                height
            );

            GUI.ModalWindow(934201, rect, DrawConsentWindow, "Quyền riêng tư & Quảng cáo");
        }

        private void DrawConsentWindow(int windowId)
        {
            GUILayout.Space(8);
            GUILayout.Label("Ứng dụng có dùng quảng cáo. Bạn có thể chọn cho phép quảng cáo cá nhân hoá hoặc từ chối.");
            GUILayout.Space(8);

            _hasUserConsent = GUILayout.Toggle(_hasUserConsent, "Tôi đồng ý cho phép thu thập/xử lý dữ liệu để cá nhân hoá quảng cáo (GDPR consent)");
            _doNotSell = GUILayout.Toggle(_doNotSell, "Không bán/chia sẻ dữ liệu cá nhân của tôi (Do Not Sell/Share - một số bang tại Mỹ)");

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Từ chối", GUILayout.Height(34)))
            {
                _showDialog = false;
                ResolveAndPersist(hasUserConsent: false, doNotSell: _doNotSell);
            }

            if (GUILayout.Button("Đồng ý", GUILayout.Height(34)))
            {
                _showDialog = false;
                ResolveAndPersist(hasUserConsent: true, doNotSell: _doNotSell);
            }
            GUILayout.EndHorizontal();
        }
    }
}
