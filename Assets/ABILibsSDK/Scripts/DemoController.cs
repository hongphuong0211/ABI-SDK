using UnityEngine;
using UnityEngine.UI;

namespace ABILibsSDK
{
    public class DemoController : MonoBehaviour
    {
        [Header("ABILibsSDKConfig")]
        [SerializeField] private Transform configUI;
        [Header("UI References")]
        [SerializeField] private Button btnLoadBanner;
        [SerializeField] private Button btnShowBanner;
        [SerializeField] private Button btnHideBanner;
        [SerializeField] private Button btnLoadInterstitial;
        [SerializeField] private Button btnShowInterstitial;
        [SerializeField] private Button btnLoadRewarded;
        [SerializeField] private Button btnShowRewarded;
        [SerializeField] private Button btnLoadAppOpen;
        [SerializeField] private Button btnShowAppOpen;
        [SerializeField] private Button btnFetchRemoteConfig;
        [SerializeField] private Button btnLogAppsFlyerEvent;
        [SerializeField] private Text txtStatus;
        [SerializeField] private GameObject loadingUI;

        private void Start()
        {
            // Create UI/UX to config ABILibsSDKConfig
            SetupConfigUI();
            SetupButtons();

            if (SDKInitializer.Instance != null)
            {
                SDKInitializer.Instance.OnAllSDKsInitialized += OnSDKsReady;
            }

            Log("Waiting for SDK initialization...");
        }

        private void SetupConfigUI()
        {
            var config = ABILibsSDKConfig.Instance;
            if (config == null || configUI == null)
            {
                Debug.LogError("ABILibsSDKConfig.Instance is null hoặc configUI chưa được gán trong Inspector");
                if (configUI != null) configUI.gameObject.SetActive(false);
                return;
            }

            configUI.gameObject.SetActive(true);

            // Xoá toàn bộ con cũ trong panel để tạo lại
            ClearChildren(configUI);

            // Tiêu đề
            CreateTitle(configUI, "ABILibs SDK Config");

            // Các field đơn
            CreateInputRow(configUI, "MAX SDK Key", config.maxSdkKey, value => config.maxSdkKey = value);
            CreateToggleRow(configUI, "Use MAX Terms & Privacy Flow", config.useMaxTermsAndPrivacyPolicyFlow,
                value => config.useMaxTermsAndPrivacyPolicyFlow = value);
            CreateToggleRow(configUI, "Auto Load Ads", config.autoLoadAds,
                value => config.autoLoadAds = value);
            CreateToggleRow(configUI, "Show AppOpen On Resume", config.showAppOpenOnResume,
                value => config.showAppOpenOnResume = value);

            // Ad Unit IDs - Android
            CreateSectionTitle(configUI, "Android Ad Unit IDs");
            CreateAdUnitList(configUI, "Banner", config.androidBannerAdUnitId,
                (index, value) => config.androidBannerAdUnitId[index] = value);
            CreateAdUnitList(configUI, "Interstitial", config.androidInterstitialAdUnitId,
                (index, value) => config.androidInterstitialAdUnitId[index] = value);
            CreateAdUnitList(configUI, "Rewarded", config.androidRewardedAdUnitId,
                (index, value) => config.androidRewardedAdUnitId[index] = value);
            CreateAdUnitList(configUI, "App Open", config.androidAppOpenAdUnitId,
                (index, value) => config.androidAppOpenAdUnitId[index] = value);

            // Ad Unit IDs - iOS
            CreateSectionTitle(configUI, "iOS Ad Unit IDs");
            CreateAdUnitList(configUI, "Banner", config.iosBannerAdUnitId,
                (index, value) => config.iosBannerAdUnitId[index] = value);
            CreateAdUnitList(configUI, "Interstitial", config.iosInterstitialAdUnitId,
                (index, value) => config.iosInterstitialAdUnitId[index] = value);
            CreateAdUnitList(configUI, "Rewarded", config.iosRewardedAdUnitId,
                (index, value) => config.iosRewardedAdUnitId[index] = value);
            CreateAdUnitList(configUI, "App Open", config.iosAppOpenAdUnitId,
                (index, value) => config.iosAppOpenAdUnitId[index] = value);
            LayoutRebuilder.ForceRebuildLayoutImmediate(configUI.GetComponent<RectTransform>());
        }

        #region Helpers for dynamic UI

        private void ClearChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Destroy(parent.GetChild(i).gameObject);
            }
        }

        private Font GetDefaultFont()
        {
            // Trên Unity các phiên bản mới, Arial.ttf không còn là built-in font hợp lệ.
            // LegacyRuntime.ttf là font built-in mới được Unity khuyến nghị.
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private void CreateTitle(Transform parent, string text)
        {
            var go = new GameObject("Title", typeof(RectTransform), typeof(Text));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);

            var txt = go.GetComponent<Text>();
            txt.font = GetDefaultFont();
            txt.text = text;
            txt.fontSize = 22;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
        }

        private void CreateSectionTitle(Transform parent, string text)
        {
            var go = new GameObject("Section_" + text, typeof(RectTransform), typeof(Text));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);

            var txt = go.GetComponent<Text>();
            txt.font = GetDefaultFont();
            txt.text = text;
            txt.fontSize = 18;
            txt.alignment = TextAnchor.MiddleLeft;
            txt.color = Color.yellow;
        }

        private void CreateInputRow(Transform parent, string labelText, string defaultValue, System.Action<string> onValueChanged)
        {
            var row = new GameObject("Row_" + labelText, typeof(RectTransform));
            var rowRect = row.GetComponent<RectTransform>();
            rowRect.SetParent(parent, false);

            var layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = false;
            layout.spacing = 8f;

            // Label
            var labelGO = new GameObject("Label", typeof(RectTransform), typeof(Text));
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.SetParent(row.transform, false);
            var labelTextComp = labelGO.GetComponent<Text>();
            labelTextComp.font = GetDefaultFont();
            labelTextComp.text = labelText;
            labelTextComp.fontSize = 14;
            labelTextComp.alignment = TextAnchor.MiddleLeft;
            labelTextComp.color = Color.white;

            // Input
            CreateInputField(row.transform, defaultValue, onValueChanged);
        }

        private void CreateToggleRow(Transform parent, string labelText, bool defaultValue, System.Action<bool> onValueChanged)
        {
            var row = new GameObject("Row_" + labelText, typeof(RectTransform));
            var rowRect = row.GetComponent<RectTransform>();
            rowRect.SetParent(parent, false);

            var layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = false;
            layout.spacing = 8f;

            // Toggle
            var toggleGO = new GameObject("Toggle", typeof(RectTransform), typeof(Image), typeof(Toggle));
            var toggleRect = toggleGO.GetComponent<RectTransform>();
            toggleRect.SetParent(row.transform, false);
            toggleRect.sizeDelta = new Vector2(20, 20);

            var bgImage = toggleGO.GetComponent<Image>();
            bgImage.color = Color.white;

            var checkmarkGO = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
            var checkmarkRect = checkmarkGO.GetComponent<RectTransform>();
            checkmarkRect.SetParent(toggleGO.transform, false);
            checkmarkRect.anchorMin = new Vector2(0.2f, 0.2f);
            checkmarkRect.anchorMax = new Vector2(0.8f, 0.8f);
            checkmarkRect.offsetMin = Vector2.zero;
            checkmarkRect.offsetMax = Vector2.zero;

            var checkmarkImage = checkmarkGO.GetComponent<Image>();
            checkmarkImage.color = Color.green;

            var toggle = toggleGO.GetComponent<Toggle>();
            toggle.targetGraphic = bgImage;
            toggle.graphic = checkmarkImage;
            toggle.isOn = defaultValue;

            if (onValueChanged != null)
            {
                toggle.onValueChanged.AddListener(value => onValueChanged(value));
            }

            // Label
            var labelGO = new GameObject("Label", typeof(RectTransform), typeof(Text));
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.SetParent(row.transform, false);
            var labelTextComp = labelGO.GetComponent<Text>();
            labelTextComp.font = GetDefaultFont();
            labelTextComp.text = labelText;
            labelTextComp.fontSize = 14;
            labelTextComp.alignment = TextAnchor.MiddleLeft;
            labelTextComp.color = Color.white;
        }

        private void CreateAdUnitList(Transform parent, string typeName, string[] ids, System.Action<int, string> onValueChanged)
        {
            if (ids == null) return;

            for (int i = 0; i < ids.Length; i++)
            {
                string label = $"{typeName} #{i + 1}";
                int index = i;
                CreateInputRow(parent, label, ids[i], value =>
                {
                    onValueChanged?.Invoke(index, value);
                });
            }
        }

        private void CreateInputField(Transform parent, string defaultValue, System.Action<string> onValueChanged)
        {
            var inputGO = new GameObject("InputField", typeof(RectTransform), typeof(Image), typeof(InputField));
            var inputRect = inputGO.GetComponent<RectTransform>();
            inputRect.SetParent(parent, false);
            inputRect.sizeDelta = new Vector2(0, 30);

            var bgImage = inputGO.GetComponent<Image>();
            bgImage.color = Color.white;

            var input = inputGO.GetComponent<InputField>();

            // Text thực
            var textGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
            var textRect = textGO.GetComponent<RectTransform>();
            textRect.SetParent(inputGO.transform, false);
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.offsetMin = new Vector2(10, 6);
            textRect.offsetMax = new Vector2(-10, -6);

            var textComp = textGO.GetComponent<Text>();
            textComp.font = GetDefaultFont();
            textComp.text = defaultValue;
            textComp.fontSize = 14;
            textComp.alignment = TextAnchor.MiddleLeft;
            textComp.color = Color.black;

            // Placeholder
            var placeholderGO = new GameObject("Placeholder", typeof(RectTransform), typeof(Text));
            var placeholderRect = placeholderGO.GetComponent<RectTransform>();
            placeholderRect.SetParent(inputGO.transform, false);
            placeholderRect.anchorMin = new Vector2(0, 0);
            placeholderRect.anchorMax = new Vector2(1, 1);
            placeholderRect.offsetMin = new Vector2(10, 6);
            placeholderRect.offsetMax = new Vector2(-10, -6);

            var placeholderComp = placeholderGO.GetComponent<Text>();
            placeholderComp.font = GetDefaultFont();
            placeholderComp.text = string.IsNullOrEmpty(defaultValue) ? "Enter value..." : "";
            placeholderComp.fontSize = 14;
            placeholderComp.alignment = TextAnchor.MiddleLeft;
            placeholderComp.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);

            input.textComponent = textComp;
            input.placeholder = placeholderComp;
            input.text = defaultValue ?? string.Empty;

            if (onValueChanged != null)
            {
                input.onEndEdit.AddListener(value => onValueChanged(value));
            }
        }

        #endregion

        private void SetupButtons()
        {
            if (btnLoadBanner != null)
                btnLoadBanner.onClick.AddListener(OnLoadBanner);
            if (btnShowBanner != null)
                btnShowBanner.onClick.AddListener(OnShowBanner);
            if (btnHideBanner != null)
                btnHideBanner.onClick.AddListener(OnHideBanner);
            if (btnLoadInterstitial != null)
                btnLoadInterstitial.onClick.AddListener(OnLoadInterstitial);
            if (btnShowInterstitial != null)
                btnShowInterstitial.onClick.AddListener(OnShowInterstitial);
            if (btnLoadRewarded != null)
                btnLoadRewarded.onClick.AddListener(OnLoadRewarded);
            if (btnShowRewarded != null)
                btnShowRewarded.onClick.AddListener(OnShowRewarded);
            if (btnLoadAppOpen != null)
                btnLoadAppOpen.onClick.AddListener(OnLoadAppOpen);
            if (btnShowAppOpen != null)
                btnShowAppOpen.onClick.AddListener(OnShowAppOpen);
            if (btnFetchRemoteConfig != null)
                btnFetchRemoteConfig.onClick.AddListener(OnFetchRemoteConfig);
            if (btnLogAppsFlyerEvent != null)
                btnLogAppsFlyerEvent.onClick.AddListener(OnLogAppsFlyerEvent);
        }

        private void OnSDKsReady()
        {
            Log("All SDKs initialized!");

            if (AppsFlyerManager.Instance != null)
            {
                AppsFlyerManager.Instance.OnConversionDataReceived += (data) =>
                {
                    string source = AppsFlyerManager.Instance.IsOrganic ? "Organic" : AppsFlyerManager.Instance.MediaSource;
                    Log($"Attribution: {source}, Campaign: {AppsFlyerManager.Instance.Campaign}");
                };
            }
        }

        private void OnLoadBanner()
        {
            if (AdsManager.Instance == null) return;
            AdsManager.Instance.LoadBannerAds(new MaxSdkBase.AdViewConfiguration(MaxSdkBase.AdViewPosition.BottomCenter));
            Log("Banner loaded");
        }

        private void OnShowBanner()
        {
            if (AdsManager.Instance == null) return;
            AdsManager.Instance.ShowBanner();
            Log("Banner shown");
        }

        private void OnHideBanner()
        {
            if (AdsManager.Instance == null) return;
            AdsManager.Instance.HideBanner();
            Log("Banner hidden");
        }

        private void OnLoadInterstitial()
        {
            if (AdsManager.Instance == null) return;
            AdsManager.Instance.LoadInterstitial();
            Log("Interstitial loaded");
        }

        private void OnShowInterstitial()
        {
            if (AdsManager.Instance == null) return;

            bool shown = AdsManager.Instance.ShowInterstitial("demo_placement", null, loadingUI);
            Log(shown ? "Interstitial shown" : "Interstitial not ready or interval not met");
        }

        private void OnLoadRewarded()
        {
            if (AdsManager.Instance == null) return;
            AdsManager.Instance.LoadRewarded();
            Log("Rewarded loaded");
        }

        private void OnShowRewarded()
        {
            if (AdsManager.Instance == null) return;

            bool shown = AdsManager.Instance.ShowRewarded("demo_reward", (rewarded) =>
            {
                Log(rewarded ? "Rewarded: User got reward!" : "Rewarded: User skipped");
            }, loadingUI);

            if (!shown)
                Log("Rewarded not ready");
        }

        private void OnLoadAppOpen()
        {
            if (AdsManager.Instance == null) return;
            AdsManager.Instance.LoadAppOpenAd();
            Log("App Open loaded");
        }

        private void OnShowAppOpen()
        {
            if (AdsManager.Instance == null) return;

            bool shown = AdsManager.Instance.ShowAppOpenAd();
            Log(shown ? "App Open shown" : "App Open not ready");
        }

        private void OnFetchRemoteConfig()
        {
            if (FirebaseManager.Instance == null) return;

            FirebaseManager.Instance.FetchRemoteConfig((success) =>
            {
                if (success)
                {
                    long interval = FirebaseManager.Instance.GetRemoteConfigLong("inter_ad_interval");
                    bool showBanner = FirebaseManager.Instance.GetRemoteConfigBool("show_banner");
                    Log($"Remote Config: interval={interval}, banner={showBanner}");
                }
                else
                {
                    Log("Remote Config fetch failed");
                }
            });
        }

        private void OnLogAppsFlyerEvent()
        {
            if (AppsFlyerManager.Instance == null) return;

            AppsFlyerManager.Instance.LogLevelComplete("demo_level_1", "100");
            Log("AppsFlyer event logged: level_complete");
        }

        private void Log(string message)
        {
            ABILibsSDKConfig.DebugLog($"[Demo] {message}");
            if (txtStatus != null)
                txtStatus.text = message;
        }
    }
}
