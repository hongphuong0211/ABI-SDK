# ABI Libs Custom Events (UPM)

TROAS- and Bamboo-style **Firebase Analytics** helpers for Unity: AppLovin MAX ad callbacks, **main-thread dispatch**, `PlayerPrefs` caching, optional **HTTP config portal** and **Firebase Remote Config** overrides on `ABILibsCustomEventConfig`.

Install from this repository using Unity Package Manager and a **Git URL with `?path=`**, because the package lives under `Packages/com.abilibs.custom-events`, not at the repo root.

## Add to `Packages/manifest.json`

**Latest from `main`:**

```json
"com.abilibs.custom-events": "https://github.com/hongphuong0211/ABI-SDK.git?path=Packages/com.abilibs.custom-events#main"
```

**Pinned to a tag or commit (recommended for production):**

```json
"com.abilibs.custom-events": "https://github.com/hongphuong0211/ABI-SDK.git?path=Packages/com.abilibs.custom-events#v1.0.0"
```

Replace `#v1.0.0` with a real tag you create on this repo, or use a full commit hash after `#`.

## Requirements in the host project

- **AppLovin MAX** Unity SDK (`MaxSdk.Scripts` — referenced by this package’s assembly).
- **Firebase Unity SDK**: **Analytics** (required). **Remote Config** is optional: if the `Firebase.RemoteConfig` package is present, the assembly defines `FIREBASE_REMOTE_CONFIG` and `FetchAndApplyRemoteConfigAsync` becomes functional; otherwise that API no-ops.

## Assembly reference

Projects that call these APIs from their own code should add a reference to **`ABILibsSDK.CustomEvents`** in their `.asmdef`.

## Configuration

1. **ScriptableObject**: Ensure an asset named **`ABILibsCustomEventConfig`** exists and is loadable via `Resources.Load` from path **`ABILibsCustomEventConfig`** (no extension). The package ships a default under `Runtime/Resources/`.
2. **Create / move**: `Assets → Create → ABILibsSDK → CustomEventConfig`, then place the asset in any folder named **`Resources`** (including inside your project or a package).
3. **Runtime**: `ABILibsCustomEventConfig.Instance` loads the asset on first access and applies activated remote config (see below). If the asset is missing, APIs log an error and **`ABILibsCustomEvent` methods no-op** when they check `Instance`.

Key serialized fields (Inspector):

| Area | Fields |
|------|--------|
| TROAS purchase | `baseTROASPurchaeEventName`, `troasPurchaseEvents`, `exchangeRates` (JSON map of currency → rate vs USD) |
| TROAS ads (cumulative tiers) | `baseTROASEventName`, `troasAdEvents` |
| TROAS ads 2 (delta / repeat tiers) | `baseTROASEventName2`, `troasAdEvents2` |
| Bamboo (interstitial / non-rewarded flow) | `baseBambooAdEventName`, `bambooCountAdEvents` |
| Bamboo rewarded | `baseBambooRewardedEventName`, `bambooCountRewardedEvents` |
| Banner / MREC | `minRevenueThresholdForBannerAndMrec` |

HTTP and Firebase Remote Config keys / URLs are defined in code on `ABILibsCustomEventConfig` (`UseHttpRemoteConfig`, `HttpConfigBaseUrl`, `KeyTroasAdEvents`, etc.). Call `FetchAndApplyHttpAsync` and/or `FetchAndApplyRemoteConfigAsync` at startup if you use those sources.

## Public API (`ABILibsCustomEvent`)

All public methods **snapshot** any needed data, then run logic on the **Unity main thread** via `MainThreadDispatcher`. Safe to call from **MAX revenue callbacks** or background threads.

| Method | Purpose |
|--------|---------|
| `TROASEvent(adUnitId, adInfo)` | Cumulative ad revenue; banner/MREC batched until `minRevenueThresholdForBannerAndMrec`; fires `baseTROASEventName` + index when thresholds in `troasAdEvents` are crossed. Params: `value` (USD), `currency` = `"USD"`. |
| `TROASEvent2(adUnitId, adInfo)` | Same banner/MREC rule; fires when revenue **since last log** for a tier reaches `troasAdEvents2[i]`; `value` is that **delta**. Event: `baseTROASEventName2` + index. |
| `BambooAdEvent(adUnitId, adInfo)` | Increments shared impression count; at `bambooCountAdEvents` thresholds logs `baseBambooAdEventName` + index with revenue semantics (see XML docs in source). |
| `BambooRewardedEvent(adUnitId, adInfo)` | Same pattern for rewarded using `bambooCountRewardedEvents` / `baseBambooRewardedEventName`. |
| `TROASPurchaseEvent(productId, price, currency)` | Converts `price` to USD with `exchangeRates`, finds matching `troasPurchaseEvents` range, logs `baseTROASPurchaeEventName` (possibly multiple times per `logCount`). |
| `LogEvent(eventName, parameters)` | Generic `FirebaseAnalytics.LogEvent` with string parameters; `parameters` copied immediately; `null` → no parameters. |

`adUnitId` is reserved for API symmetry with MAX; current implementations **do not** use it in analytics payloads.

### Example: wire MAX callbacks

```csharp
using ABILibsSDK;

// Interstitial (example — adjust to your ad types)
MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += (adUnitId, adInfo) =>
{
    ABILibsCustomEvent.TROASEvent(adUnitId, adInfo);
    ABILibsCustomEvent.TROASEvent2(adUnitId, adInfo);
    ABILibsCustomEvent.BambooAdEvent(adUnitId, adInfo);
};

MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += (adUnitId, adInfo) =>
{
    ABILibsCustomEvent.TROASEvent(adUnitId, adInfo);
    ABILibsCustomEvent.TROASEvent2(adUnitId, adInfo);
    ABILibsCustomEvent.BambooRewardedEvent(adUnitId, adInfo);
};

// After a successful IAP (price in local currency)
ABILibsCustomEvent.TROASPurchaseEvent(productId, price, currencyCode);
```

### Example: generic event

```csharp
ABILibsCustomEvent.LogEvent("my_event", new Dictionary<string, string>
{
    { "level", "10" }
});
```

## Further documentation

- XML comments on `ABILibsCustomEvent` and `ABILibsCustomEventConfig` in the Runtime sources (IntelliSense / IDE tooltips).

## After install

Ensure `ABILibsCustomEventConfig` exists under a `Resources` folder (the package ships a default asset under `Runtime/Resources`). Referencing assemblies should reference **`ABILibsSDK.CustomEvents`** in their `.asmdef` if they call these APIs directly.
