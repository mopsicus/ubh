using System.IO;
using NiceJson;
using UnityEditor;
using UnityEngine;
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
                bool isHuaweiServices = UBHPrefs.GetBool(UnityBuilderHelper.HUAWEI_SERVICES_KEY, false);
                if (isHuaweiServices && !File.Exists(Path.Combine(Application.dataPath, "StreamingAssets", HUAWEI_CONFIG))) {
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
                bool isGoogleServices = UBHPrefs.GetBool(UnityBuilderHelper.GOOGLE_SERVICES_KEY, false);
                if (isGoogleServices && !File.Exists(Path.Combine(Application.dataPath, "StreamingAssets", GOOGLE_CONFIG))) {
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
            bool isGoogleKeystore = UBHPrefs.GetBool(UnityBuilderHelper.GOOGLE_KEYSTORE_KEY, false);
            if (!isGoogleKeystore) {
                return false;
            }
            if (string.IsNullOrEmpty(UBHPrefs.GetString(UnityBuilderHelper.GOOGLE_PASSWORD_KEY))) {
                Debug.LogError("KeyStore object in Config not completed");
                return false;
            }
            PlayerSettings.Android.keystoreName = UBHPrefs.GetString(UnityBuilderHelper.GOOGLE_PATH_KEY);
            PlayerSettings.Android.keystorePass = UBHPrefs.GetString(UnityBuilderHelper.GOOGLE_PASSWORD_KEY);
            PlayerSettings.Android.keyaliasName = UBHPrefs.GetString(UnityBuilderHelper.GOOGLE_ALIAS_KEY);
            PlayerSettings.Android.keyaliasPass = UBHPrefs.GetString(UnityBuilderHelper.GOOGLE_APASS_KEY);
            Debug.LogFormat("Keystore file loaded from: {0}", UBHPrefs.GetString(UnityBuilderHelper.GOOGLE_PATH_KEY));
#elif HUAWEI
            bool isHuaweiKeystore = UBHPrefs.GetBool(UnityBuilderHelper.HUAWEI_KEYSTORE_KEY, false);
            if (!isHuaweiKeystore) {
                return false;
            }
            if (string.IsNullOrEmpty(UBHPrefs.GetString(UnityBuilderHelper.HUAWEI_PASSWORD_KEY))) {
                Debug.LogError("KeyStore object in Config not completed");
                return false;
            }
            PlayerSettings.Android.keystoreName = UBHPrefs.GetString(UnityBuilderHelper.HUAWEI_PATH_KEY);
            PlayerSettings.Android.keystorePass = UBHPrefs.GetString(UnityBuilderHelper.HUAWEI_PASSWORD_KEY);
            PlayerSettings.Android.keyaliasName = UBHPrefs.GetString(UnityBuilderHelper.HUAWEI_ALIAS_KEY);
            PlayerSettings.Android.keyaliasPass = UBHPrefs.GetString(UnityBuilderHelper.HUAWEI_APASS_KEY);
            Debug.LogFormat("Keystore file loaded from: {0}", UBHPrefs.GetString(UnityBuilderHelper.HUAWEI_PATH_KEY));
#endif            
            return true;
        }

#endif

    }

}