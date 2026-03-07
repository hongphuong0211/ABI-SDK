using System;
using UnityEngine;

namespace ABILibsSDK
{
    public class AdsManager : MonoBehaviour
    {
        public static AdsManager Instance { get; private set; }

        public event Action<MaxSdkBase.AdInfo> OnBannerAdRevenuePaid;
        public event Action<MaxSdkBase.AdInfo> OnInterstitialAdRevenuePaid;
        public event Action<MaxSdkBase.AdInfo> OnRewardedAdRevenuePaid;
        public event Action<MaxSdkBase.AdInfo> OnAppOpenAdRevenuePaid;

        private ABILibsSDKConfig _config;
        private float _lastInterstitialTime;
        private bool _isShowingFullScreenAd;

        public bool IsInterstitialReady {
            get{
                if (_config == null || _config.InterstitialAdUnitId == null || _config.InterstitialAdUnitId.Length == 0) return false;

                foreach (var adUnitId in _config.InterstitialAdUnitId)
                {
                    if (string.IsNullOrEmpty(adUnitId)) continue;
                    if (MaxSdk.IsInterstitialReady(adUnitId)) return true;
                }
                return false;
            }
        }

        public bool IsRewardedAdReady {
            get{
                if (_config == null || _config.RewardedAdUnitId == null || _config.RewardedAdUnitId.Length == 0) return false;

                foreach (var adUnitId in _config.RewardedAdUnitId)
                {
                    if (string.IsNullOrEmpty(adUnitId)) continue;
                    if (MaxSdk.IsRewardedAdReady(adUnitId)) return true;
                }
                return false;
            }
        }

        public bool IsAppOpenAdReady {
            get{
                if (_config == null || _config.AppOpenAdUnitId == null || _config.AppOpenAdUnitId.Length == 0) return false;

                foreach (var adUnitId in _config.AppOpenAdUnitId)
                {
                    if (string.IsNullOrEmpty(adUnitId)) continue;
                    if (MaxSdk.IsAppOpenAdReady(adUnitId)) return true;
                }
                return false;
            }
        }

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

            MaxSdkCallbacks.OnSdkInitializedEvent += OnMaxSdkInitialized;
            if (config != null && !config.useMaxTermsAndPrivacyPolicyFlow)
            {
                ConsentManager.ApplyMaxPrivacyFlagsFromSavedState();
            }
            MaxSdk.SetSdkKey(config.maxSdkKey);
            MaxSdk.InitializeSdk();
        }

        private void OnMaxSdkInitialized(MaxSdkBase.SdkConfiguration sdkConfiguration)
        {
            ABILibsSDKConfig.DebugLog("[ABILibsSDK] MAX SDK Initialized");

            InitializeBannerAds();
            InitializeInterstitialAds();
            InitializeRewardedAds();
            InitializeAppOpenAds();

            if (_config != null && _config.autoLoadAds)
            {
                LoadAllAds();
            }
        }

        #region Banner
        private int _currentBannerAdUnitIndex = 0;
        private int _currentBannerRetryAttempt = 0;
        private int _isBannerLoaded = -1;
        private int _isBannerShowing = -1;
        private MaxSdkBase.AdViewConfiguration _bannerAdViewConfiguration;

        private void InitializeBannerAds()
        {
            if (_config == null || _config.BannerAdUnitId == null || _config.BannerAdUnitId.Length == 0) return;

            MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerLoaded;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerLoadFailed;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerRevenuePaid;
        }

        private void OnBannerLoaded(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            ABILibsSDKConfig.DebugLog($"Banner loaded");
            _currentBannerRetryAttempt = 0;
            _isBannerLoaded = _currentBannerAdUnitIndex;
            _currentBannerAdUnitIndex = 0;
        }

        private void OnBannerLoadFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            ABILibsSDKConfig.DebugLog($"Banner load failed: {errorInfo.Message}");
            if (_currentBannerAdUnitIndex < _config.BannerAdUnitId.Length - 1)
            {
                _currentBannerAdUnitIndex++;
                LoadBannerAds(_bannerAdViewConfiguration);
            }
            else if (_currentBannerRetryAttempt < _config.bannerRetryAttempts)
            {
                _currentBannerRetryAttempt++;
                double retryDelay = Math.Pow(2, Math.Min(6, _currentBannerRetryAttempt));
                ABILibsSDKConfig.DebugLog($"Banner load failed, retrying in {retryDelay}s");
                Invoke(nameof(LoadBannerAds), (float)retryDelay);
            }
            else
            {
                ABILibsSDKConfig.DebugLog($"Banner load failed, max retries reached");
            }
        }

        private void OnBannerRevenuePaid(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            OnBannerAdRevenuePaid?.Invoke(adInfo);
        }
        public void LoadBannerAds(MaxSdk.AdViewConfiguration adViewConfiguration = null)
        {
            if (_config == null || _config.BannerAdUnitId == null || _config.BannerAdUnitId.Length == 0) return;
            if (_currentBannerAdUnitIndex < 0 || _currentBannerAdUnitIndex >= _config.BannerAdUnitId.Length) return;
            _bannerAdViewConfiguration = adViewConfiguration == null ? new MaxSdkBase.AdViewConfiguration(MaxSdkBase.AdViewPosition.BottomCenter) : adViewConfiguration;
            MaxSdk.CreateBanner(_config.BannerAdUnitId[_currentBannerAdUnitIndex], adViewConfiguration);
        }

        public void ShowBanner()
        {
            if (_config == null || _config.BannerAdUnitId == null || _config.BannerAdUnitId.Length == 0) return;
            if (_isBannerLoaded < 0 || _isBannerLoaded >= _config.BannerAdUnitId.Length) return;
            MaxSdk.ShowBanner(_config.BannerAdUnitId[_isBannerLoaded]);
            _isBannerShowing = _isBannerLoaded;
            _isBannerLoaded = -1;
        }

        public void HideBanner()
        {
            if (_config == null || _config.BannerAdUnitId == null || _config.BannerAdUnitId.Length == 0) return;
            if (_isBannerShowing < 0 || _isBannerShowing >= _config.BannerAdUnitId.Length) return;
            MaxSdk.HideBanner(_config.BannerAdUnitId[_isBannerShowing]);
            _isBannerShowing = -1;
        }

        #endregion

        #region Interstitial

        private int _currentInterstitialAdUnitIndex = 0;
        private int _currentInterstitialRetryAttempt = 0;
        private Action<bool> _interstitialCallback;
        private bool _isInterstitialLoading = false;
        private string _lastInterstitialPlacement;
        private GameObject _loadingInterstitialUI;

        private void InitializeInterstitialAds()
        {
            if (_config == null || _config.InterstitialAdUnitId == null || _config.InterstitialAdUnitId.Length == 0) return;

            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoaded;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialLoadFailed;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialDisplayed;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialDisplayFailed;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialHidden;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnInterstitialRevenuePaid;
        }

        public void LoadInterstitial()
        {
            ABILibsSDKConfig.DebugLog($"Loading interstitial ad {_currentInterstitialAdUnitIndex}");
            if (_config == null || _config.InterstitialAdUnitId == null || _config.InterstitialAdUnitId.Length == 0) {
                ABILibsSDKConfig.DebugLog($"Interstitial ad unit id is not set");
                return;
            }
            if (_currentInterstitialAdUnitIndex < 0 || _currentInterstitialAdUnitIndex >= _config.InterstitialAdUnitId.Length) {
                ABILibsSDKConfig.DebugLog($"Interstitial ad unit index is out of range");
                return;
            }
            if (_isInterstitialLoading) {
                ABILibsSDKConfig.DebugLog($"Interstitial ad is already loading");
                return;
            }
            _isInterstitialLoading = true;
            MaxSdk.LoadInterstitial(_config.InterstitialAdUnitId[_currentInterstitialAdUnitIndex]);
        }
        /// <summary>
        /// Show interstitial ad
        /// </summary>
        /// <param name="placement">Placement name</param>
        /// <param name="callback">Callback function</param>
        /// <param name="loadingUI">Show loading UI and wait for the ad to load</param>
        /// <returns>True if ad is shown, false otherwise</returns>
        public bool ShowInterstitial(string placement = null, System.Action<bool> callback = null, GameObject loadingUI = null)
        {
            if (_isInterstitialLoading && loadingUI != null) {
                // Show loading UI and wait for the ad to load
                ABILibsSDKConfig.DebugLog($"Showing loading UI");
                _interstitialCallback = callback;
                _loadingInterstitialUI = loadingUI;
                _lastInterstitialPlacement = placement;
                _loadingInterstitialUI.SetActive(true);
                return false;
            }
            if (!CanShowInterstitial())
            {
                callback?.Invoke(false);
                return false;
            }
            _interstitialCallback = callback;
            _isShowingFullScreenAd = true;
            foreach (var adUnitId in _config.InterstitialAdUnitId)
            {
                if (string.IsNullOrEmpty(adUnitId)) continue;
                if (MaxSdk.IsInterstitialReady(adUnitId)) {
                    MaxSdk.ShowInterstitial(adUnitId, placement);
                    _lastInterstitialTime = Time.realtimeSinceStartup;
                    return true;
                }
            }
            return false;
        }

        private bool CanShowInterstitial()
        {
            if (!IsInterstitialReady) return false;
            if (_isShowingFullScreenAd) return false;

            float elapsed = Time.realtimeSinceStartup - _lastInterstitialTime;
            return elapsed >= _config.interstitialInterval || _lastInterstitialTime == 0;
        }

        private void OnInterstitialLoaded(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            ABILibsSDKConfig.DebugLog("Interstitial loaded");
            _currentInterstitialRetryAttempt = 0;
            _isInterstitialLoading = false;
            _currentInterstitialAdUnitIndex = 0;
            if (_loadingInterstitialUI != null && _loadingInterstitialUI.activeSelf)
            {
                _loadingInterstitialUI.SetActive(false);
                ShowInterstitial(_lastInterstitialPlacement, _interstitialCallback, null);
                _loadingInterstitialUI = null;
            }
        }

        private void OnInterstitialLoadFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            ABILibsSDKConfig.DebugLog($"Interstitial load failed: {errorInfo.Message}");
            if (_currentInterstitialAdUnitIndex < _config.InterstitialAdUnitId.Length - 1)
            {
                _currentInterstitialAdUnitIndex++;
                LoadInterstitial();
            }
            else if (_currentInterstitialRetryAttempt < _config.interstitialRetryAttempts)
            {
                _currentInterstitialRetryAttempt++;
                double retryDelay = Math.Pow(2, Math.Min(6, _currentInterstitialRetryAttempt));
                ABILibsSDKConfig.DebugLog($"Interstitial load failed, retrying in {retryDelay}s");
                Invoke(nameof(LoadInterstitial), (float)retryDelay);
            }
            else
            {
                _isInterstitialLoading = false;
                _interstitialCallback?.Invoke(false);
                _interstitialCallback = null;
                if (_loadingInterstitialUI != null) {
                    _loadingInterstitialUI.SetActive(false);
                    _loadingInterstitialUI = null;
                }
                ABILibsSDKConfig.DebugLog($"Interstitial load failed, max retries reached");
            }
        }

        private void OnInterstitialDisplayed(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            ABILibsSDKConfig.DebugLog($"Interstitial displayed");
        }

        private void OnInterstitialDisplayFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            ABILibsSDKConfig.DebugLog($"Interstitial display failed: {errorInfo.Message}");
            _isShowingFullScreenAd = false;
            _interstitialCallback?.Invoke(false);
            _interstitialCallback = null;
            LoadInterstitial();
        }

        private void OnInterstitialHidden(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _isShowingFullScreenAd = false;
            _interstitialCallback?.Invoke(true);
            _interstitialCallback = null;
            LoadInterstitial();
        }

        private void OnInterstitialRevenuePaid(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            ABILibsSDKConfig.DebugLog($"Interstitial revenue paid");
            OnInterstitialAdRevenuePaid?.Invoke(adInfo);
        }

        #endregion

        #region Rewarded

        private int _currentRewardedAdUnitIndex = 0;
        private int _currentRewardedRetryAttempt = 0;
        private Action<bool> _rewardedCallback;
        private GameObject _loadingRewardedUI;
        private string _lastRewardedPlacement;
        private bool _isRewardedLoading = false;
        private bool _rewardedAdGotReward = false;
        private void InitializeRewardedAds()
        {
            if (_config == null || _config.RewardedAdUnitId == null || _config.RewardedAdUnitId.Length == 0) return;

            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedLoaded;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedLoadFailed;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedDisplayed;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedDisplayFailed;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedHidden;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedReceivedReward;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedRevenuePaid;
        }

        public void LoadRewarded()
        {
            ABILibsSDKConfig.DebugLog($"Loading rewarded ad {_currentRewardedAdUnitIndex}");
            if (_config == null || _config.RewardedAdUnitId == null || _config.RewardedAdUnitId.Length == 0) {
                ABILibsSDKConfig.DebugLog($"Rewarded ad unit id is not set");
                return;
            }
            if (_currentRewardedAdUnitIndex < 0 || _currentRewardedAdUnitIndex >= _config.RewardedAdUnitId.Length) {
                ABILibsSDKConfig.DebugLog($"Rewarded ad unit index is out of range");
                return;
            }
            if (_isRewardedLoading) {
                ABILibsSDKConfig.DebugLog($"Rewarded ad is already loading");
                return;
            }
            _isRewardedLoading = true;
            MaxSdk.LoadRewardedAd(_config.RewardedAdUnitId[_currentRewardedAdUnitIndex]);
        }

        public bool ShowRewarded(string placement = null, System.Action<bool> callback = null, GameObject loadingUI = null)
        {
            if (_isRewardedLoading && loadingUI != null) {
                // Show loading UI and wait for the ad to load
                ABILibsSDKConfig.DebugLog($"Showing loading UI");
                _rewardedCallback = callback;
                _loadingRewardedUI = loadingUI;
                _lastRewardedPlacement = placement;
                _loadingRewardedUI.SetActive(true);
                return false;
            }
            if (!IsRewardedAdReady || _isShowingFullScreenAd) {
                ABILibsSDKConfig.DebugLog($"Rewarded ad is not ready or already showing");
                callback?.Invoke(false);
                return false;
            }

            _rewardedCallback = callback;
            _isShowingFullScreenAd = true;
            _rewardedAdGotReward = false;
            foreach (var adUnitId in _config.RewardedAdUnitId)
            {
                if (string.IsNullOrEmpty(adUnitId)) continue;
                if (MaxSdk.IsRewardedAdReady(adUnitId)) {
                    MaxSdk.ShowRewardedAd(adUnitId, placement);
                    return true;
                }
            }
            return false;
        }

        private void OnRewardedLoaded(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            ABILibsSDKConfig.DebugLog("Rewarded loaded");
            _currentRewardedRetryAttempt = 0;
            _isRewardedLoading = false;
            _currentRewardedAdUnitIndex = 0;
            if (_loadingRewardedUI != null && _loadingRewardedUI.activeSelf)
            {
                _loadingRewardedUI.SetActive(false);
                ShowRewarded(_lastRewardedPlacement, _rewardedCallback, null);
                _loadingRewardedUI = null;
            }
        }

        private void OnRewardedLoadFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            ABILibsSDKConfig.DebugLog($"Rewarded load failed: {errorInfo.Message}");
            if (_currentRewardedAdUnitIndex < _config.RewardedAdUnitId.Length - 1)
            {
                _currentRewardedAdUnitIndex++;
                LoadRewarded();
            }
            else if (_currentRewardedRetryAttempt < _config.rewardedRetryAttempts)
            {
                _currentRewardedRetryAttempt++;
                double retryDelay = Math.Pow(2, Math.Min(6, _currentRewardedRetryAttempt));
                ABILibsSDKConfig.DebugLog($"Rewarded load failed, retrying in {retryDelay}s");
                Invoke(nameof(LoadRewarded), (float)retryDelay);
            }
            else
            {
                _isRewardedLoading = false;
                _rewardedCallback?.Invoke(false);
                _rewardedCallback = null;
                if (_loadingRewardedUI != null) {
                    _loadingRewardedUI.SetActive(false);
                    _loadingRewardedUI = null;
                }
                ABILibsSDKConfig.DebugLog($"Rewarded load failed, max retries reached");
            }
        }

        private void OnRewardedDisplayed(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            ABILibsSDKConfig.DebugLog("Rewarded displayed");
        }

        private void OnRewardedDisplayFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            _isShowingFullScreenAd = false;
            _rewardedCallback?.Invoke(false);
            _rewardedCallback = null;
        }

        private void OnRewardedReceivedReward(string adUnitId, MaxSdkBase.Reward reward, MaxSdkBase.AdInfo adInfo)
        {
            ABILibsSDKConfig.DebugLog($"Rewarded ad received reward: {reward.Label} x {reward.Amount}");
            _rewardedAdGotReward = true;
        }

        private void OnRewardedHidden(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _isShowingFullScreenAd = false;
            _rewardedCallback?.Invoke(_rewardedAdGotReward);
            _rewardedCallback = null;
        }

        private void OnRewardedRevenuePaid(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            OnRewardedAdRevenuePaid?.Invoke(adInfo);
        }

        #endregion

        #region App Open

        private int _currentAppOpenAdUnitIndex = 0;
        private int _currentAppOpenRetryAttempt = 0;
        private void InitializeAppOpenAds()
        {
            if (_config == null || _config.AppOpenAdUnitId == null || _config.AppOpenAdUnitId.Length == 0) return;

            MaxSdkCallbacks.AppOpen.OnAdLoadedEvent += OnAppOpenLoaded;
            MaxSdkCallbacks.AppOpen.OnAdLoadFailedEvent += OnAppOpenLoadFailed;
            MaxSdkCallbacks.AppOpen.OnAdDisplayedEvent += OnAppOpenDisplayed;
            MaxSdkCallbacks.AppOpen.OnAdDisplayFailedEvent += OnAppOpenDisplayFailed;
            MaxSdkCallbacks.AppOpen.OnAdHiddenEvent += OnAppOpenHidden;
            MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent += OnAppOpenRevenuePaid;
        }

        public void LoadAppOpenAd()
        {
            if (_config == null || _config.AppOpenAdUnitId == null || _config.AppOpenAdUnitId.Length == 0) return;
            if (_currentAppOpenAdUnitIndex < 0 || _currentAppOpenAdUnitIndex >= _config.AppOpenAdUnitId.Length) return;
            MaxSdk.LoadAppOpenAd(_config.AppOpenAdUnitId[_currentAppOpenAdUnitIndex]);
        }

        public bool ShowAppOpenAd()
        {
            if (!IsAppOpenAdReady || _isShowingFullScreenAd) return false;

            _isShowingFullScreenAd = true;
            foreach (var adUnitId in _config.AppOpenAdUnitId)
            {
                if (string.IsNullOrEmpty(adUnitId)) continue;
                if (MaxSdk.IsAppOpenAdReady(adUnitId)) {
                    MaxSdk.ShowAppOpenAd(adUnitId);
                    return true;
                }
            }
            return true;
        }

        private void OnAppOpenLoaded(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            ABILibsSDKConfig.DebugLog("App Open ad loaded");
            _currentAppOpenRetryAttempt = 0;
            _currentAppOpenAdUnitIndex = 0;
        }

        private void OnAppOpenLoadFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            if (_currentAppOpenAdUnitIndex < _config.AppOpenAdUnitId.Length - 1)
            {
                _currentAppOpenAdUnitIndex++;
                LoadAppOpenAd();
            }
            else if (_currentAppOpenRetryAttempt < _config.appOpenRetryAttempts)
            {
                _currentAppOpenRetryAttempt++;
                double retryDelay = Math.Pow(2, Math.Min(6, _currentAppOpenRetryAttempt));
                ABILibsSDKConfig.DebugLog($"App Open load failed, retrying in {retryDelay}s");
                Invoke(nameof(LoadAppOpenAd), (float)retryDelay);
            }
            else
            {
                ABILibsSDKConfig.DebugLog($"App Open load failed, max retries reached");
            }
        }

        private void OnAppOpenDisplayed(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            ABILibsSDKConfig.DebugLog("App Open displayed");
        }

        private void OnAppOpenDisplayFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            _isShowingFullScreenAd = false;
            LoadAppOpenAd();
        }

        private void OnAppOpenHidden(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _isShowingFullScreenAd = false;
        }

        private void OnAppOpenRevenuePaid(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            OnAppOpenAdRevenuePaid?.Invoke(adInfo);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus && _config != null && _config.showAppOpenOnResume)
            {
                ShowAppOpenAd();
            }
        }

        #endregion

        public void LoadAllAds()
        {
            LoadInterstitial();
            LoadRewarded();
            LoadAppOpenAd();
        }
    }
}
