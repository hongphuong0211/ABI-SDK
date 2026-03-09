## ABILibs SDK cho Unity

ABILibs SDK là một bộ wrapper/tiện ích cho Unity giúp bạn:

- **Tích hợp nhanh quảng cáo với AppLovin MAX Mediation**
- **Theo dõi doanh thu quảng cáo với Firebase Analytics & AppsFlyer**
- **Tự động gửi các custom event nâng cao (TROAS, Bamboo, v.v.)** để tối ưu UA/monetization

SDK đã được đóng gói sẵn trong thư mục `Assets/ABILibsSDK`.

---

## 1. Yêu cầu

- **Unity**: khuyến nghị 2020.3+ (URP/Built-in đều được).
- **Firebase**:
  - Firebase SDK for Unity (tối thiểu Firebase Analytics).
  - File cấu hình `google-services.json` (Android) / `GoogleService-Info.plist` (iOS) đã import đúng cách.
- **AppsFlyer**:
  - AppsFlyer Unity SDK.
  - Đã cấu hình `Dev Key` và `App ID iOS` hợp lệ.
- **AppLovin MAX Mediation**:
  - AppLovin MAX Unity SDK.
  - Đã cấu hình các ad network adapter cần thiết.
- **External Dependency Manager (EDM4U)**: nên được bật để tự động xử lý dependency của Firebase/AppsFlyer/MAX.

> **Lưu ý**: Dự án mẫu này đã tích hợp sẵn EDM4U và một số cấu hình cơ bản cho Firebase/MAX. Khi đưa vào dự án khác, hãy chắc chắn bạn đã import đầy đủ các SDK ở trên trước khi dùng ABILibs SDK.

---

## 2. Cấu trúc chính của ABILibs SDK

- **`ABILibsSDKConfig`**: ScriptableObject chứa:
  - MAX SDK Key.
  - Ad Unit IDs cho Banner / Interstitial / Rewarded / App Open (Android & iOS).
  - AppsFlyer DevKey, AppId iOS.
  - Các setting cho retry, interval, auto load ads, App Open on resume,...
- **`SDKInitializer`**:
  - Quản lý vòng đời khởi tạo: MAX → Firebase → AppsFlyer (tùy theo cấu hình consent).
  - Subcribe các event doanh thu quảng cáo và forward sang Firebase, AppsFlyer, Custom Event.
  - Expose event `OnAllSDKsInitialized` để game có thể chờ SDK sẵn sàng rồi mới continue flow.
- **`FirebaseManager`**, **`AppsFlyerManager`**, **`AdsManager`**:
  - Đóng gói logic khởi tạo & log event/doanh thu tương ứng cho từng SDK.
- **`ABILibsCustomEvent` + `ABILibsCustomEventConfig`**:
  - Quản lý các custom event: TROAS Ad Event, TROAS Ad Event 2, Bamboo Ad Event,...
  - Cho phép cấu hình threshold doanh thu / số lần xem quảng cáo → log custom event sang Firebase Analytics.

---

## 3. Tích hợp **FULL SDK** (Ads + Firebase + AppsFlyer + Custom Event)

Trường hợp này bạn muốn dùng toàn bộ flow của ABILibs SDK: MAX Mediation, Firebase, AppsFlyer và custom event.

### 3.1. Import thư viện

1. Import hoặc copy thư mục `Assets/ABILibsSDK` vào project của bạn.
2. Đảm bảo đã import các SDK:
   - Firebase SDK for Unity (có Firebase Analytics).
   - AppsFlyer Unity SDK.
   - AppLovin MAX Unity SDK + các adapter cần thiết.
3. Chạy `Assets > External Dependency Manager > Android Resolver / iOS Resolver` nếu cần để pull đầy đủ dependency.

### 3.2. Tạo config `ABILibsSDKConfig`

1. Trong Unity, chọn **`Assets > Create > ABILibsSDK > ABILibsSDKConfig`** để tạo asset config.
2. Đặt tên (ví dụ `ABILibsSDKConfig.asset`) và **đặt asset này vào một thư mục `Resources`** (ví dụ `Assets/Resources/ABILibsSDKConfig.asset`).
3. Điền cấu hình:
   - **MAX SDK Key**.
   - **Ad Unit IDs Android**: Banner / Interstitial / Rewarded / App Open.
   - **Ad Unit IDs iOS**: Banner / Interstitial / Rewarded / App Open.
   - **AppsFlyer**: `appsFlyerDevKey`, `appsFlyerAppIdIOS`, bật/tắt `appsFlyerDebug` tùy môi trường.
   - Tùy chỉnh thêm các tham số retry, interval, auto load ads,… phù hợp game của bạn.

> Nếu không gán trực tiếp trong `SDKInitializer`, class sẽ tự load `ABILibsSDKConfig` từ `Resources/ABILibsSDKConfig`.

### 3.3. Tạo config `ABILibsCustomEventConfig` (khuyến nghị)

1. Chọn **`Assets > Create > ABILibsSDK > CustomEventConfig`** để tạo `ABILibsCustomEventConfig`.
2. Đặt tên asset và cũng **đặt trong thư mục `Resources`** (ví dụ `Assets/Resources/ABILibsCustomEventConfig.asset`).
3. Tùy chỉnh:
   - `baseTROASPurchaeEventName`, `troasPurchaseEvents`.
   - `baseTROASEventName`, `troasAdEvents`.
   - `baseTROASEventName2`, `troasAdEvents2`.
   - `baseBambooAdEventName`, `bambooCountAdEvents`.
   - `baseBambooRewardedEventName`, `bambooCountRewardedEvents`.
   - `minRevenueThresholdForBannerAndMrec`.

SDK sẽ dùng config này khi log các event trong `ABILibsCustomEvent`.

### 3.4. Thêm `SDKInitializer` vào Scene

1. Tạo một `GameObject` trong **Scene khởi đầu** (ví dụ `SDKRoot`).
2. Add component:
   - `SDKInitializer`
   - `FirebaseManager`
   - `AppsFlyerManager`
   - `AdsManager`
   - (Nếu có sẵn prefab mẫu trong thư mục `ABILibsSDK`, bạn có thể kéo prefab đó vào scene thay cho việc add tay.)
3. Gán reference:
   - Trong `SDKInitializer`, nếu muốn override config mặc định, gán trực tiếp `ABILibsSDKConfig` asset vào field `config`. Nếu để trống, nó sẽ tự tìm trong `Resources`.
4. Đảm bảo `SDKInitializer` và các Manager sử dụng `DontDestroyOnLoad` (đã cài trong code) để sống xuyên suốt app.

Khi chạy game:

- `SDKInitializer` sẽ:
  - Khởi tạo MAX (và flow consent nếu `useMaxTermsAndPrivacyPolicyFlow` bật).
  - Khởi tạo Firebase, AppsFlyer.
  - Subscribe event doanh thu quảng cáo từ `AdsManager`.
  - Gọi `ABILibsCustomEvent.TROASEvent(...)` khi có ad revenue.
- Khi tất cả SDK sẵn sàng, event `OnAllSDKsInitialized` được gọi → bạn có thể lắng nghe event này để bắt đầu các flow quan trọng (home, remote config, v.v.).

---

## 4. Chỉ tích hợp **Custom Event** (không dùng full SDK)

Trường hợp này bạn đã tự tích hợp Firebase / AppsFlyer / MAX trong project, và chỉ muốn dùng phần **custom event TROAS/Bamboo** của ABILibs.

### 4.1. Import phần cần thiết

1. Import thư mục `Assets/ABILibsSDK` (hoặc ít nhất:
   - `ABILibsCustomEvent.cs`
   - `ABILibsCustomEventConfig.cs`
   - Các file liên quan tới namespace `ABILibsSDK` mà `ABILibsCustomEvent` cần).
2. Đảm bảo bạn đã có:
   - Firebase Analytics (`Firebase.Analytics`).
   - MAX SDK (`MaxSdkBase.AdInfo`).

### 4.2. Tạo `ABILibsCustomEventConfig`

1. Tạo asset: **`Assets > Create > ABILibsSDK > CustomEventConfig`**.
2. Đặt asset trong thư mục `Resources` với tên mặc định `ABILibsCustomEventConfig` (hoặc tương đương).
3. Cấu hình các threshold / tên event như ở mục 3.3.

### 4.3. Gọi custom event từ code của bạn

Ở nơi bạn đã nhận callback doanh thu/quảng cáo từ MAX (banner/interstitial/rewarded/app open), gọi:

- **TROAS Ad Event (kiểu 1)**:

```csharp
using ABILibsSDK;

void OnAdRevenuePaid(string adUnitId, MaxSdkBase.AdInfo adInfo)
{
    ABILibsCustomEvent.TROASEvent(adUnitId, adInfo);
}
```

- **TROAS Ad Event 2 (reset cache sau mỗi lần đạt ngưỡng)**:

```csharp
using ABILibsSDK;

void OnAdRevenuePaid(string adUnitId, MaxSdkBase.AdInfo adInfo)
{
    ABILibsCustomEvent.TROASEvent2(adUnitId, adInfo);
}
```

- **Bamboo Ad Event (tính theo số lần xem quảng cáo)**:

```csharp
using ABILibsSDK;

void OnInterstitialRevenuePaid(string adUnitId, MaxSdkBase.AdInfo adInfo)
{
    ABILibsCustomEvent.BambooAdEvent(adUnitId, adInfo);
}

void OnRewardedRevenuePaid(string adUnitId, MaxSdkBase.AdInfo adInfo)
{
    ABILibsCustomEvent.BambooRewardedEvent(adUnitId, adInfo);
}
```

> **Lưu ý**:
> - Hãy chắc chắn `ABILibsCustomEventConfig.Instance` load thành công (asset nằm trong `Resources`).
> - Firebase Analytics phải được init trước khi gọi các hàm trên, nếu không event sẽ không được log.

---

## 5. Demo Scene

Trong thư mục `Assets/ABILibsSDK` có sẵn **scene demo** với `DemoController`:

- Hỗ trợ UI để cấu hình nhanh `ABILibsSDKConfig` trực tiếp trong runtime (MAX SDK Key, ad unit IDs, v.v.).
- Các nút test: load/show/hide Banner, Interstitial, Rewarded, App Open, fetch Remote Config, log AppsFlyer event,...

Bạn có thể mở scene demo này để tham khảo cách setup `SDKInitializer`, `AdsManager`, `FirebaseManager`, `AppsFlyerManager` và cách lắng nghe `OnAllSDKsInitialized`.

---

## 6. Gợi ý best practices

- **Phân tách config Prod/Staging**:
  - Tạo nhiều `ABILibsSDKConfig` (ví dụ: `ABILibsSDKConfig_Prod`, `ABILibsSDKConfig_Staging`) và chọn asset tương ứng theo build variant.
- **Không hard-code key trong code**, luôn dùng ScriptableObject (`ABILibsSDKConfig`, `ABILibsCustomEventConfig`).
- **Test kỹ flow consent (GDPR/CCPA)**:
  - Nếu bạn để `useMaxTermsAndPrivacyPolicyFlow = true`, MAX sẽ điều khiển luồng Terms & Privacy/CMP, sau đó mới tiếp tục init các SDK khác.
  - Nếu `false`, hãy tự xử lý consent rồi gọi init ads sau khi có consent hợp lệ.

Nếu bạn cần thêm hướng dẫn chi tiết cho dự án cụ thể, hãy mở issue hoặc cập nhật phần này cho phù hợp workflow nội bộ.

