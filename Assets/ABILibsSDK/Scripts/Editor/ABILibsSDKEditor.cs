using UnityEngine;
using UnityEditor;

namespace ABILibsSDK.Editor
{
    public static class ABILibsSDKEditor
    {
        [MenuItem("ABILibsSDK/Create SDK Config")]
        public static void CreateConfig()
        {
            var existing = Resources.Load<ABILibsSDKConfig>("ABILibsSDKConfig");
            if (existing != null)
            {
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = existing;
                ABILibsSDKConfig.DebugLog("Config already exists. Selecting it.");
                return;
            }

            var config = ScriptableObject.CreateInstance<ABILibsSDKConfig>();

            const string path = "Assets/ABILibsSDK/Resources";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder("Assets/ABILibsSDK", "Resources");
            }

            AssetDatabase.CreateAsset(config, $"{path}/ABILibsSDKConfig.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = config;

            ABILibsSDKConfig.DebugLog("Config created at " + path);
        }

        [MenuItem("ABILibsSDK/Create ABILibsSDK Prefab")]
        public static void CreateSDKPrefab()
        {
            const string prefabPath = "Assets/ABILibsSDK/Prefabs/ABILibsSDK.prefab";
            var existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existingPrefab != null)
            {
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = existingPrefab;
                ABILibsSDKConfig.DebugLog("Prefab already exists. Selecting it.");
                return;
            }

            var go = new GameObject("ABILibsSDK");
            go.AddComponent<SDKInitializer>();
            go.AddComponent<AdsManager>();
            go.AddComponent<FirebaseManager>();
            go.AddComponent<AppsFlyerManager>();
            go.AddComponent<MainThreadDispatcher>();

            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = prefab;

            ABILibsSDKConfig.DebugLog("SDK Prefab created at " + prefabPath);
        }
        [MenuItem("ABILibsSDK/Create Custom Event Config")]
        public static void CreateCustomEventConfig()
        {
            var existing = Resources.Load<ABILibsCustomEventConfig>("ABILibsCustomEventConfig");
            if (existing != null)
            {
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = existing;
                ABILibsSDKConfig.DebugLog("Custom Event Config already exists. Selecting it.");
                return;
            }
            var config = ScriptableObject.CreateInstance<ABILibsCustomEventConfig>();
            const string path = "Assets/ABILibsSDK/Resources";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder("Assets/ABILibsSDK", "Resources");
            }
            AssetDatabase.CreateAsset(config, $"{path}/ABILibsCustomEventConfig.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = config;
            ABILibsSDKConfig.DebugLog("Custom Event Config created at " + path);
        }

        [MenuItem("ABILibsSDK/Setup (Create All)")]
        public static void SetupAll()
        {
            CreateConfig();
            CreateSDKPrefab();
            CreateCustomEventConfig();
            ABILibsSDKConfig.DebugLog("Setup complete! Fill in your SDK keys in the config and custom event config.");
        }
    }
}
