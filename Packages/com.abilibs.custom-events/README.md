# ABI Libs Custom Events (UPM)

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

- AppLovin MAX Unity SDK (`MaxSdk.Scripts` assembly)
- Firebase Unity SDK (Analytics; Remote Config optional for `FIREBASE_REMOTE_CONFIG`)

## After install

Ensure `ABILibsCustomEventConfig` exists under a `Resources` folder (the package ships a default asset under `Runtime/Resources`). Referencing assemblies should reference **`ABILibsSDK.CustomEvents`** in their `.asmdef` if they call these APIs directly.
