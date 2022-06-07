using NiceJson;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace Mopsicus.UBH {

    /// <summary>
    /// Platforms type
    /// </summary>
    public enum PlatformType {
        UNKNOWN = -1,
        GOOGLE = 0,
        IOS = 1,
        HUAWEI = 2,
        WEB = 3
    }

    [InitializeOnLoad]
    public class PlatformValidator : Editor {

#if UNITY_EDITOR
        /// <summary>
        /// Key for save/load last platform
        /// </summary>
        private const string KEY = "last_platform";

        /// <summary>
        /// Config for Huawei services
        /// </summary>
        private const string HUAWEI_CONFIG = "agconnect-services.json";

        /// <summary>
        /// Config for Google services
        /// </summary>
        private const string GOOGLE_CONFIG = "google-services.json";

        /// <summary>
        /// Cached last validated platform
        /// </summary>
        private static PlatformType _lastValidatedPlatform = PlatformType.UNKNOWN;

        /// <summary>
        /// Callback on editor validate
        /// </summary>
        static PlatformValidator() {
#if GOOGLE || HUAWEI
            SetKeyStore();
#endif
            _lastValidatedPlatform = (PlatformType)UBHPrefs.GetInt(KEY, (int)PlatformType.UNKNOWN);
#if HUAWEI
            if (_lastValidatedPlatform != PlatformType.HUAWEI) {
                if (!File.Exists(Path.Combine(Application.dataPath, "StreamingAssets", HUAWEI_CONFIG))) {
                    Debug.LogErrorFormat(string.Format("Huawei config \"{0}\" not found in StreamingAssets!", HUAWEI_CONFIG));
                    return;
                }
                if (!SetKeyStore(true)) {
                    return;
                }
                _lastValidatedPlatform = PlatformType.HUAWEI;
                UBHPrefs.SetInt(KEY, (int)_lastValidatedPlatform);
                Debug.Log("Platform validated for Huawei");
                if (!PlayerSettings.applicationIdentifier.Contains(".huawei")) {
                    PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, string.Format("{0}.huawei", PlayerSettings.applicationIdentifier));
                    Debug.LogFormat("Bundle changed to: {0}", PlayerSettings.applicationIdentifier);
                }
            }
#elif GOOGLE
            if (_lastValidatedPlatform != PlatformType.GOOGLE) {
                if (!File.Exists(Path.Combine(Application.dataPath, "StreamingAssets", GOOGLE_CONFIG))) {
                    Debug.LogErrorFormat("Google config \"{0}\" not found in StreamingAssets!", GOOGLE_CONFIG);
                    return;
                }
                if (!SetKeyStore(true)) {
                    return;
                }
                _lastValidatedPlatform = PlatformType.GOOGLE;
                UBHPrefs.SetInt(KEY, (int)_lastValidatedPlatform);
                Debug.Log("Platform validated for Google");
                if (PlayerSettings.applicationIdentifier.Contains(".huawei")) {
                    PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, PlayerSettings.applicationIdentifier.Replace(".huawei", ""));
                    Debug.LogFormat("Bundle changed to: {0}", PlayerSettings.applicationIdentifier);
                }
            }
#elif UNITY_IOS
            if (_lastValidatedPlatform != PlatformType.IOS) {
                _lastValidatedPlatform = PlatformType.IOS;
                UBHPrefs.SetInt(KEY, (int)_lastValidatedPlatform);
                Debug.Log("Platform validated for iOS");
            }
#endif
        }

        /// <summary>
        /// Set keystore data to settings
        /// </summary>
        /// <param name="isForceUpdate">Refresh data</param>
        public static bool SetKeyStore(bool isForceUpdate = false) {
            if (isForceUpdate) {
                PlayerSettings.Android.keystorePass = "";
            }
            if (!string.IsNullOrEmpty(PlayerSettings.Android.keystorePass)) {
                return true;
            }
#if GOOGLE
            JsonObject keystore = UBHConfig.KeyStoreGoogle;
#elif HUAWEI
            JsonObject keystore = UBHConfig.KeyStoreHuawei;
#elif UNITY_IOS
            JsonObject keystore = new JsonObject();
#endif
            if (string.IsNullOrEmpty(keystore["pass"])) {
                Debug.LogError("KeyStore object in Config not completed");
                return false;
            }
            PlayerSettings.Android.keystoreName = keystore["path"];
            PlayerSettings.Android.keystorePass = keystore["pass"];
            PlayerSettings.Android.keyaliasName = keystore["alias"];
            PlayerSettings.Android.keyaliasPass = keystore["apass"];
            Debug.LogFormat("Keystore file loaded from: {0}", keystore["path"]);
            return true;
        }

#endif

    }

}