using System.Collections.Generic;
using System.IO;
using System.Text;
using NiceJson;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace Mopsicus.UBH {

    /// <summary>
    /// Struct for fill PList
    /// </summary>
    public struct PListItem {
        public string Key;
        public string Value;
    }

    public class UnityBuilderHelper : EditorWindow {

        /// <summary>
        /// Types of loggers
        /// </summary>
        enum LoggerType {
            CLIENT = 0,
            GAME = 1,
            PING = 2
        }

        /// <summary>
        /// Default width
        /// </summary>
        const int WIDTH = 576;

        /// <summary>
        /// Default height
        /// </summary>
        const int HEIGHT = 600;

        /// <summary>
        /// Horizontal
        /// </summary>
        const int OFFSET = 150;

        /// <summary>
        /// Height for select buttons
        /// </summary>
        const int BUTTON_HEIGHT = 64;

        /// <summary>
        /// Height for action buttons
        /// </summary>
        const int ACTION_HEIGHT = 48;

        /// <summary>
        /// Max bundle count
        /// </summary>
        const int MAX_BUNDLE = 9999;

        /// <summary>
        /// Window title
        /// </summary>
        const string TITLE = "Unity Builder Helper";

        /// <summary>
        /// Color for enabled button
        /// </summary>
        private Color ON_BUTTON = Color.green;

        /// <summary>
        /// Color for disabled button
        /// </summary>
        private Color OFF_BUTTON = Color.gray;

        /// <summary>
        /// Key for save/load position
        /// </summary>
        const string POSITION_KEY = "ubh_position";

        /// <summary>
        /// Key for save/load platform
        /// </summary>
        const string PLATFORM_KEY = "ubh_platform";

        /// <summary>
        /// Key for save/load logger
        /// </summary>
        const string LOGGER_KEY = "ubh_logger";

        /// <summary>
        /// Key for save/load build path
        /// </summary>
        const string BUILD_KEY = "ubh_build";

        /// <summary>
        /// Browse button width
        /// </summary>
        private const int BROWSE_WIDTH = 32;

        /// <summary>
        /// Spacing for vertical elements
        /// </summary>
        private const float VERTICAL_SPACING = 10f;

        /// <summary>
        /// Android build file mask
        /// </summary>
        private const string ANDROID_BUILD_FILE_MASK = "{0}/{1}.{2}.{3}{4}{5}.{6}";

        /// <summary>
        /// iOS build mask
        /// </summary>
        private const string IOS_BUILD_MASK = "{0}/{1}.{2}.{3}{4}";

        /// <summary>
        /// Bot command payload
        /// </summary>
        private const string BOT_PAYLOAD = "&parse_mode=html&reply_markup={\"inline_keyboard\": [[{\"text\": \"🛠️ Run build\", \"callback_data\": \"build\"}]]}";

        /// <summary>
        /// Bot url mask command
        /// </summary>
        private const string BOT_COMMAND_URL = "https://api.telegram.org/bot{0}/sendMessage?chat_id={1}&text=<code>/build {2} {3} {4} {5}</code>{6}";

        /// <summary>
        /// Remote key fold
        /// </summary>
        private const string REMOTE_KEY = "ubh_remote";

        /// <summary>
        /// Android key fold
        /// </summary>
        private const string ANDROID_KEY = "ubh_android";

        /// <summary>
        /// iOS key fold
        /// </summary>
        private const string IOS_KEY = "ubh_ios";

        /// <summary>
        /// Token key
        /// </summary>
        public const string BOT_TOKEN_KEY = "ubh_token";

        /// <summary>
        /// Telegram user key
        /// </summary>
        public const string USER_ID_KEY = "ubh_user";

        /// <summary>
        /// Game title key
        /// </summary>
        public const string GAME_TITLE_KEY = "ubh_title";

        /// <summary>
        /// Google keystore path
        /// </summary>
        public const string GOOGLE_PATH_KEY = "ubh_g_path";

        /// <summary>
        /// Google keystore password
        /// </summary>
        public const string GOOGLE_PASSWORD_KEY = "ubh_g_pass";

        /// <summary>
        /// Google keystore alias
        /// </summary>
        public const string GOOGLE_ALIAS_KEY = "ubh_g_alias";

        /// <summary>
        /// Google keystore alias pass
        /// </summary>
        public const string GOOGLE_APASS_KEY = "ubh_g_apass";

        /// <summary>
        /// Huawei keystore path
        /// </summary>
        public const string HUAWEI_PATH_KEY = "ubh_h_path";

        /// <summary>
        /// Huawei keystore password
        /// </summary>
        public const string HUAWEI_PASSWORD_KEY = "ubh_h_pass";

        /// <summary>
        /// Huawei keystore alias
        /// </summary>
        public const string HUAWEI_ALIAS_KEY = "ubh_h_alias";

        /// <summary>
        /// Huawei keystore alias pass
        /// </summary>
        public const string HUAWEI_APASS_KEY = "ubh_h_apass";

        /// <summary>
        /// Huawei dependencies
        /// </summary>
        public const string HUAWEI_DEPS_KEY = "ubh_h_deps";

        /// <summary>
        /// iOS locales
        /// </summary>
        public const string LOCALES_KEY = "ubh_locales";

        /// <summary>
        /// iOS frameworks
        /// </summary>
        public const string FRAMEWORKS_KEY = "ubh_frameworks";

        /// <summary>
        /// iOS support files
        /// </summary>
        public const string SUPPORT_FILES_KEY = "ubh_support";

        /// <summary>
        /// iOS push enabled
        /// </summary>
        public const string PUSH_KEY = "ubh_push";

        /// <summary>
        /// iOS purchase enabled
        /// </summary>
        public const string PURCHASE_KEY = "ubh_purchase";

        /// <summary>
        /// iOS sign in with apple enabled
        /// </summary>
        public const string SIGN_KEY = "ubh_sign";

        /// <summary>
        /// iOS plist
        /// </summary>
        public const string PLIST_KEY = "ubh_plist";

        /// <summary>
        /// Current platform
        /// </summary>
        private PlatformType _currentPlatform = PlatformType.IOS;

        /// <summary>
        /// Color for platform buttons
        /// </summary>
        private Color[] _platformsColors = new Color[4];

        /// <summary>
        /// Icons for buttons
        /// </summary>
        private Texture2D[] _platformIcons = new Texture2D[4];

        /// <summary>
        /// Paths for builds
        /// </summary>
        private string[] _platformBuilds = new string[4];

        /// <summary>
        /// Color for loggers buttons
        /// </summary>
        private Color[] _loggersColors = new Color[3];

        /// <summary>
        /// Icons for loggers
        /// </summary>
        private Texture2D[] _loggersIcons = new Texture2D[3];

        /// <summary>
        /// Active loggers
        /// </summary>
        private List<LoggerType> _loggers = new List<LoggerType>(3);

        /// <summary>
        /// Cached last action message
        /// </summary>
        private string _actionMessage = null;

        /// <summary>
        /// Cached app version
        /// </summary>
        private string _buildVersion = "";

        /// <summary>
        /// Cached app bundle code
        /// </summary>
        private string _buildCode = "";

        /// <summary>
        /// Flag to apply bundle build for Android
        /// </summary>
        private bool _isBundleBuild = false;

        /// <summary>
        /// Flog to show settings
        /// </summary>
        private bool _isSettingsMode = false;

        /// <summary>
        /// Title for builds
        /// </summary>
        private string _gameTitle = null;

        /// <summary>
        /// Telegram bot token
        /// </summary>
        private string _botToken = null;

        /// <summary>
        /// Telegram user id
        /// </summary>
        private string _userID = null;

        /// <summary>
        /// Locales list, separated by comma
        /// </summary>
        private string _locales = null;

        /// <summary>
        /// Framework names list, separated by comma
        /// </summary>
        private string _frameworks = null;

        /// <summary>
        /// Support files for iOS, separated by comma
        /// </summary>
        private string _supportFiles = null;

        /// <summary>
        /// Enable push notification in xcode
        /// </summary>
        private bool _isPushes = false;

        /// <summary>
        /// Enable purchases in xcode
        /// </summary>
        private bool _isPurchases = false;

        /// <summary>
        /// Enable sign in with Apple in xcode
        /// </summary>
        private bool _isSignIn = false;

        /// <summary>
        /// Path to keystore file
        /// </summary>
        private string _googlePath = "";

        /// <summary>
        /// Password for keystore
        /// </summary>        
        private string _googlePassword = "";

        /// <summary>
        /// Alias in keystore
        /// </summary>        
        private string _googleAlias = "";

        /// <summary>
        /// Password for alias
        /// </summary>        
        private string _googleAliasPassword = "";

        /// <summary>
        /// Path to keystore file
        /// </summary>
        private string _huaweiPath = "";

        /// <summary>
        /// Password for keystore
        /// </summary>
        private string _huaweiPassword = "";

        /// <summary>
        /// Alias in keystore
        /// </summary>
        private string _huaweiAlias = "";

        /// <summary>
        /// Password for alias
        /// </summary>
        private string _huaweiAliasPassword = "";

        /// <summary>
        /// Dependencies separated by comma
        /// </summary>
        private string _huaweiDependencies = "";

        /// <summary>
        /// Position for scroll view
        /// </summary>
        Vector2 _scrollPosition = Vector2.zero;

        /// <summary>
        /// Visible status for remote
        /// </summary>
        private bool _remoteStatus = false;

        /// <summary>
        /// Visible status for Android
        /// </summary>
        private bool _androidStatus = false;

        /// <summary>
        /// Visible status for iOS
        /// </summary>
        private bool _iosStatus = false;

        /// <summary>
        /// List of plist items
        /// </summary>
        private List<PListItem> _plist = new List<PListItem>();

        /// <summary>
        /// Init
        /// </summary>
        [MenuItem("Tools/Unity Builder Helper %g")]
        static void Init() {
            UnityBuilderHelper window = GetWindow<UnityBuilderHelper>(true);
            window.LoadPosition();
            window.Load();
            window.Show();
        }

        /// <summary>
        /// 
        /// </summary>
        void OnEnable() {
            titleContent.text = TITLE;
        }

        /// <summary>
        /// Action on close
        /// </summary>
        void OnDisable() {
            SavePosition();
        }

        /// <summary>
        /// Load window position and size
        /// </summary>
        void LoadPosition() {
            UnityBuilderHelper window = GetWindow<UnityBuilderHelper>(true);
            string data = UBHPrefs.GetString(POSITION_KEY);
            if (string.IsNullOrEmpty(data)) {
                window.position = new Rect(WIDTH, HEIGHT / 2, WIDTH, HEIGHT);
            } else {
                JsonObject json = (JsonObject)JsonNode.ParseJsonString(data);
                window.position = new Rect(json["x"], json["y"], json["w"], json["h"]);
            }
        }

        /// <summary>
        /// Save window position and size
        /// </summary>
        void SavePosition() {
            JsonObject json = new JsonObject();
            UnityBuilderHelper window = GetWindow<UnityBuilderHelper>(true);
            json["x"] = window.position.x;
            json["y"] = window.position.y;
            json["w"] = window.position.width;
            json["h"] = window.position.height;
            SaveData(POSITION_KEY, json.ToJsonString());
        }

        /// <summary>
        /// Check data before save
        /// </summary>
        private void SaveData(string key, string value) {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value)) {
                Debug.LogErrorFormat("Error on save data: key = {0}, value = {1}", key, value);
                return;
            }
            UBHPrefs.SetString(key, value);
        }


        /// <summary>
        /// Init window
        /// </summary>
        void Load() {
            InitColors();
            InitPlatformIcons();
            InitLoggerIcons();
            LoadSettings();
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            PlatformType platform = (PlatformType)UBHPrefs.GetInt(PLATFORM_KEY, -1);
            SwitchPlatform(platform);
            string list = UBHPrefs.GetString(LOGGER_KEY);
            if (!string.IsNullOrEmpty(list)) {
                JsonArray json = (JsonArray)JsonNode.ParseJsonString(list);
                foreach (int logger in json) {
                    SwitchLogger((LoggerType)logger);
                }
            }
            list = UBHPrefs.GetString(BUILD_KEY);
            if (!string.IsNullOrEmpty(list)) {
                JsonArray json = (JsonArray)JsonNode.ParseJsonString(list);
                for (int i = 0; i < json.Count; i++) {
                    _platformBuilds[i] = json[i];
                }
            }
            _isBundleBuild = EditorUserBuildSettings.buildAppBundle;
#if UNITY_ANDROID
            _buildCode = PlayerSettings.Android.bundleVersionCode.ToString();
#elif UNITY_IOS
            _buildCode = PlayerSettings.iOS.buildNumber;
#endif
            _buildVersion = PlayerSettings.bundleVersion;
        }

        /// <summary>
        /// Init platform icons
        /// </summary>
        void InitPlatformIcons() {
            _platformIcons[(int)PlatformType.IOS] = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("ubh-apple-icon")[0]));
            _platformIcons[(int)PlatformType.GOOGLE] = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("ubh-google-icon")[0]));
            _platformIcons[(int)PlatformType.HUAWEI] = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("ubh-huawei-icon")[0]));
            _platformIcons[(int)PlatformType.WEB] = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("ubh-web-icon")[0]));
        }

        /// <summary>
        /// Init loggers icons
        /// </summary>
        void InitLoggerIcons() {
            _loggersIcons[(int)LoggerType.CLIENT] = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("ubh-client-icon")[0]));
            _loggersIcons[(int)LoggerType.GAME] = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("ubh-game-icon")[0]));
            _loggersIcons[(int)LoggerType.PING] = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("ubh-ping-icon")[0]));
        }

        /// <summary>
        /// Show current loggers states
        /// </summary>
        /// <param name="logger">Logger type</param>
        void SwitchLogger(LoggerType logger) {
            if (_loggers.Contains(logger)) {
                _loggersColors[(int)logger] = OFF_BUTTON;
                _loggers.Remove(logger);
                ShowActionMessage(string.Format("Logger {0} disabled", logger.ToString()));
            } else {
                _loggersColors[(int)logger] = ON_BUTTON;
                _loggers.Add(logger);
                ShowActionMessage(string.Format("Logger {0} enabled", logger.ToString()));
            }
        }

        /// <summary>
        /// Show current platform state
        /// </summary>
        /// <param name="platform">Platform type</param>
        void SwitchPlatform(PlatformType platform) {
            for (int i = 0; i < _platformsColors.Length; i++) {
                _platformsColors[i] = OFF_BUTTON;
            }
            if (platform != PlatformType.UNKNOWN) {
                _platformsColors[(int)platform] = ON_BUTTON;
                _currentPlatform = platform;
                ShowActionMessage(string.Format("Platform {0} selected", _currentPlatform.ToString()));
            } else {
                ShowActionMessage(string.Format("Platform unknown: {0}", platform.ToString()));
            }
        }

        /// <summary>
        /// Draw window items
        /// </summary>
        void OnGUI() {
            if (_isSettingsMode) {
                ShowSettings();
            } else {
                ShowPanel();
            }
        }

        /// <summary>
        /// Show settings
        /// </summary>
        private void ShowSettings() {
            GUILayout.BeginVertical();
            GUILayout.Space(VERTICAL_SPACING);
            GUI.backgroundColor = Color.gray;
            _remoteStatus = EditorGUILayout.Foldout(_remoteStatus, "Remote settings");
            if (_remoteStatus) {
                _botToken = EditorGUILayout.TextField("Bot token:", _botToken);
                _userID = EditorGUILayout.TextField("User ID:", _userID);
                _gameTitle = EditorGUILayout.TextField("Game title:", _gameTitle);
            }
            GUILayout.Space(VERTICAL_SPACING);
            _androidStatus = EditorGUILayout.Foldout(_androidStatus, "Android settings");
            if (_androidStatus) {
                GUILayout.Label("KeyStore for Google", EditorStyles.label);
                GUILayout.BeginHorizontal();
                _googlePath = EditorGUILayout.TextField("Path:", _googlePath);
                if (GUILayout.Button("...", GUILayout.MaxWidth(BROWSE_WIDTH))) {
                    string path = EditorUtility.OpenFilePanel("Select keystore", _googlePath, "");
                    if (!string.IsNullOrEmpty(path)) {
                        _googlePath = path;
                    }
                }
                GUILayout.EndHorizontal();
                _googlePassword = EditorGUILayout.TextField("Password:", _googlePassword);
                _googleAlias = EditorGUILayout.TextField("Alias:", _googleAlias);
                _googleAliasPassword = EditorGUILayout.TextField("Alias password:", _googleAliasPassword);
                GUILayout.Space(VERTICAL_SPACING);
                GUILayout.Label("KeyStore for Huawei", EditorStyles.label);
                GUILayout.BeginHorizontal();
                _huaweiPath = EditorGUILayout.TextField("Path:", _huaweiPath);
                if (GUILayout.Button("...", GUILayout.MaxWidth(BROWSE_WIDTH))) {
                    string path = EditorUtility.OpenFilePanel("Select keystore", _huaweiPath, "");
                    if (!string.IsNullOrEmpty(path)) {
                        _huaweiPath = path;
                    }
                }
                GUILayout.EndHorizontal();
                _huaweiPassword = EditorGUILayout.TextField("Password:", _huaweiPassword);
                _huaweiAlias = EditorGUILayout.TextField("Alias:", _huaweiAlias);
                _huaweiAliasPassword = EditorGUILayout.TextField("Alias password:", _huaweiAliasPassword);
                GUILayout.Space(VERTICAL_SPACING);
                _huaweiDependencies = EditorGUILayout.TextField("Huawei dependencies:", _huaweiDependencies);
            }
            GUILayout.Space(VERTICAL_SPACING);
            _iosStatus = EditorGUILayout.Foldout(_iosStatus, "iOS settings");
            if (_iosStatus) {
                _locales = EditorGUILayout.TextField("Locales:", _locales);
                _frameworks = EditorGUILayout.TextField("Frameworks:", _frameworks);
                _supportFiles = EditorGUILayout.TextField("Support files:", _supportFiles);
                _isPushes = EditorGUILayout.Toggle("Pushes:", _isPushes);
                _isPurchases = EditorGUILayout.Toggle("Purchases:", _isPurchases);
                _isSignIn = EditorGUILayout.Toggle("Sign in with Apple:", _isSignIn);
                GUILayout.Space(VERTICAL_SPACING);
                GUILayout.Label("Plist items", EditorStyles.label);
                GUILayout.BeginHorizontal();
                GUILayout.Space(OFFSET);
                GUILayout.BeginVertical();
                _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);
                if (_plist.Count > 0) {
                    for (int i = 0; i < _plist.Count; i++) {
                        PListItem item = _plist[i];
                        GUILayout.BeginVertical();
                        item.Key = EditorGUILayout.TextField("Key:", item.Key);
                        item.Value = EditorGUILayout.TextField("Value:", item.Value);
                        _plist[i] = item;
                        if (GUILayout.Button("Remove")) {
                            _plist.Remove(item);
                        }
                        GUILayout.EndVertical();
                    }
                }
                if (GUILayout.Button("Add")) {
                    PListItem item = new PListItem();
                    _plist.Add(item);
                }
                GUILayout.EndScrollView();
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
            GUILayout.Space(VERTICAL_SPACING);
            if (GUILayout.Button("Save", GUILayout.Height(ACTION_HEIGHT))) {
                SaveSettings();
                _isSettingsMode = false;
            }
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Load all settings
        /// </summary>
        private void LoadSettings() {
            _remoteStatus = UBHPrefs.GetBool(REMOTE_KEY, true);
            _androidStatus = UBHPrefs.GetBool(ANDROID_KEY, false);
            _iosStatus = UBHPrefs.GetBool(IOS_KEY, true);
            _botToken = UBHPrefs.GetString(BOT_TOKEN_KEY);
            _userID = UBHPrefs.GetString(USER_ID_KEY);
            _gameTitle = UBHPrefs.GetString(GAME_TITLE_KEY);
            _googlePath = UBHPrefs.GetString(GOOGLE_PATH_KEY);
            _googlePassword = UBHPrefs.GetString(GOOGLE_PASSWORD_KEY);
            _googleAlias = UBHPrefs.GetString(GOOGLE_ALIAS_KEY);
            _googleAliasPassword = UBHPrefs.GetString(GOOGLE_APASS_KEY);
            _huaweiPath = UBHPrefs.GetString(HUAWEI_PATH_KEY);
            _huaweiPassword = UBHPrefs.GetString(HUAWEI_PASSWORD_KEY);
            _huaweiAlias = UBHPrefs.GetString(HUAWEI_ALIAS_KEY);
            _huaweiAliasPassword = UBHPrefs.GetString(HUAWEI_APASS_KEY);
            _huaweiDependencies = UBHPrefs.GetString(HUAWEI_DEPS_KEY, "com.huawei.hms:base:6.3.0.303, com.huawei.hms:hwid:6.4.0.300, com.huawei.agconnect:agconnect-auth:1.6.4.300, com.huawei.agconnect:agconnect-auth-huawei:1.6.4.300, com.huawei.hms:iap:6.3.0.300, com.huawei.hms:push:6.3.0.302, com.huawei.hms:ads:3.4.52.302, com.huawei.hms:ads-identifier:3.4.39.302, com.huawei.hms:game:5.0.4.303");
            _locales = UBHPrefs.GetString(LOCALES_KEY, "en, ru");
            _frameworks = UBHPrefs.GetString(FRAMEWORKS_KEY, "AppTrackingTransparency, UserNotifications, AuthenticationServices, StoreKit, MessageUI, Webkit");
            _supportFiles = UBHPrefs.GetString(SUPPORT_FILES_KEY, "splash-iphone.png, splash-ipad.png");
            _isPushes = UBHPrefs.GetBool(PUSH_KEY, true);
            _isPurchases = UBHPrefs.GetBool(PURCHASE_KEY, true);
            _isSignIn = UBHPrefs.GetBool(SIGN_KEY, true);
            string data = UBHPrefs.GetString(PLIST_KEY);
            if (!string.IsNullOrEmpty(data)) {
                _plist.Clear();
                JsonArray list = (JsonArray)JsonNode.ParseJsonString(data);
                foreach (JsonObject json in list) {
                    PListItem item = new PListItem();
                    item.Key = json["key"];
                    item.Value = json["value"];
                    _plist.Add(item);
                }
            } else {
                PListItem item = new PListItem();
                item.Key = "NSUserTrackingUsageDescription";
                item.Value = "$(PRODUCT_NAME) need to access the IDFA in order to deliver personalized advertising and to help us improve the game.";
                _plist.Add(item);
            }
        }

        /// <summary>
        /// Save all settings
        /// </summary>
        private void SaveSettings() {
            UBHPrefs.SetBool(REMOTE_KEY, _remoteStatus);
            UBHPrefs.SetBool(ANDROID_KEY, _androidStatus);
            UBHPrefs.SetBool(IOS_KEY, _iosStatus);
            UBHPrefs.SetString(BOT_TOKEN_KEY, !string.IsNullOrEmpty(_botToken) ? _botToken.Trim() : "");
            UBHPrefs.SetString(USER_ID_KEY, !string.IsNullOrEmpty(_userID) ? _userID.Trim() : "");
            UBHPrefs.SetString(GAME_TITLE_KEY, !string.IsNullOrEmpty(_gameTitle) ? _gameTitle.Trim() : "");
            UBHPrefs.SetString(GOOGLE_PATH_KEY, _googlePath);
            UBHPrefs.SetString(GOOGLE_PASSWORD_KEY, _googlePassword);
            UBHPrefs.SetString(GOOGLE_ALIAS_KEY, _googleAlias);
            UBHPrefs.SetString(GOOGLE_APASS_KEY, _googleAliasPassword);
            UBHPrefs.SetString(HUAWEI_PATH_KEY, _huaweiPath);
            UBHPrefs.SetString(HUAWEI_PASSWORD_KEY, _huaweiPassword);
            UBHPrefs.SetString(HUAWEI_ALIAS_KEY, _huaweiAlias);
            UBHPrefs.SetString(HUAWEI_APASS_KEY, _huaweiAliasPassword);
            UBHPrefs.SetString(HUAWEI_DEPS_KEY, !string.IsNullOrEmpty(_huaweiDependencies) ? _huaweiDependencies.Trim() : "");
            UBHPrefs.SetString(LOCALES_KEY, !string.IsNullOrEmpty(_locales) ? _locales.Trim() : "");
            UBHPrefs.SetString(FRAMEWORKS_KEY, !string.IsNullOrEmpty(_frameworks) ? _frameworks.Trim() : "");
            UBHPrefs.SetString(SUPPORT_FILES_KEY, !string.IsNullOrEmpty(_supportFiles) ? _supportFiles.Trim() : "");
            UBHPrefs.SetBool(PUSH_KEY, _isPushes);
            UBHPrefs.SetBool(PURCHASE_KEY, _isPurchases);
            UBHPrefs.SetBool(SIGN_KEY, _isSignIn);
            JsonArray plist = new JsonArray();
            for (int i = 0; i < _plist.Count; i++) {
                JsonObject item = new JsonObject();
                item["key"] = _plist[i].Key;
                item["value"] = _plist[i].Value;
                plist.Add(item);
            }
            UBHPrefs.SetString(PLIST_KEY, plist.ToJsonString());
        }

        /// <summary>
        /// Show UBB panel
        /// </summary>
        private void ShowPanel() {
            GUILayout.BeginVertical();
            GUILayout.Space(VERTICAL_SPACING);
            if (GUILayout.Button("Settings", GUILayout.Height(ACTION_HEIGHT))) {
                _isSettingsMode = true;
            }
            GUILayout.Space(VERTICAL_SPACING);
            GUILayout.Label("Platform:", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            GUI.backgroundColor = _platformsColors[(int)PlatformType.IOS];
            if (GUILayout.Button(_platformIcons[(int)PlatformType.IOS], GUILayout.Height(BUTTON_HEIGHT))) {
                SwitchPlatform(PlatformType.IOS);
            }
            GUI.backgroundColor = _platformsColors[(int)PlatformType.GOOGLE];
            if (GUILayout.Button(_platformIcons[(int)PlatformType.GOOGLE], GUILayout.Height(BUTTON_HEIGHT))) {
                SwitchPlatform(PlatformType.GOOGLE);
            }
            GUI.backgroundColor = _platformsColors[(int)PlatformType.HUAWEI];
            if (GUILayout.Button(_platformIcons[(int)PlatformType.HUAWEI], GUILayout.Height(BUTTON_HEIGHT))) {
                SwitchPlatform(PlatformType.HUAWEI);
            }
            GUI.backgroundColor = _platformsColors[(int)PlatformType.WEB];
            if (GUILayout.Button(_platformIcons[(int)PlatformType.WEB], GUILayout.Height(BUTTON_HEIGHT))) {
                SwitchPlatform(PlatformType.WEB);
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(VERTICAL_SPACING);
            GUILayout.Label("Logging:", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            GUI.backgroundColor = _loggersColors[(int)LoggerType.CLIENT];
            if (GUILayout.Button(_loggersIcons[(int)LoggerType.CLIENT], GUILayout.Height(BUTTON_HEIGHT))) {
                SwitchLogger(LoggerType.CLIENT);
            }
            GUI.backgroundColor = _loggersColors[(int)LoggerType.GAME];
            if (GUILayout.Button(_loggersIcons[(int)LoggerType.GAME], GUILayout.Height(BUTTON_HEIGHT))) {
                SwitchLogger(LoggerType.GAME);
            }
            GUI.backgroundColor = _loggersColors[(int)LoggerType.PING];
            if (GUILayout.Button(_loggersIcons[(int)LoggerType.PING], GUILayout.Height(BUTTON_HEIGHT))) {
                SwitchLogger(LoggerType.PING);
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(VERTICAL_SPACING);
            GUI.backgroundColor = Color.gray;
            GUILayout.Label("Build settings:", EditorStyles.boldLabel);
            _buildVersion = EditorGUILayout.TextField("Version:", _buildVersion);
            _buildCode = EditorGUILayout.TextField("Code:", _buildCode);
            GUILayout.BeginHorizontal();
            _platformBuilds[(int)_currentPlatform] = EditorGUILayout.TextField("Build directory:", _platformBuilds[(int)_currentPlatform]);
            if (GUILayout.Button("...", GUILayout.MaxWidth(BROWSE_WIDTH))) {
                string path = EditorUtility.OpenFolderPanel("Select build directory", _platformBuilds[(int)_currentPlatform], "");
                if (!string.IsNullOrEmpty(path)) {
                    _platformBuilds[(int)_currentPlatform] = path;
                }
            }
            GUILayout.EndHorizontal();
            if (_currentPlatform == PlatformType.GOOGLE || _currentPlatform == PlatformType.HUAWEI) {
                GUI.backgroundColor = Color.white;
                bool isBundleBuild = EditorGUILayout.Toggle("App bundle:", _isBundleBuild);
                if (_isBundleBuild != isBundleBuild) {
                    _isBundleBuild = isBundleBuild;
                    EditorUserBuildSettings.buildAppBundle = _isBundleBuild;
                }
            }
            GUILayout.Space(VERTICAL_SPACING);
            GUI.backgroundColor = Color.yellow;
            if (GUILayout.Button("Apply settings", GUILayout.Height(ACTION_HEIGHT))) {
                ApplyPlatform();
                ApplyVersion();
                ApplyLoggers();
                ApplyPath();
                EditorUtility.DisplayDialog(titleContent.text, "Apply settings completed", "Close");
            }
            GUILayout.EndVertical();
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Local build", GUILayout.Height(ACTION_HEIGHT))) {
                Close();
                BuildResult result = Build();
                string message = (result == BuildResult.Succeeded) ? "Build succeeded!" : "Build failed. See console logs :(";
                if (EditorUtility.DisplayDialog(titleContent.text, message, "Close")) {
                    if (result == BuildResult.Succeeded) {
                        EditorUtility.RevealInFinder(_platformBuilds[(int)_currentPlatform]);
                    }
                }
            }
            GUI.backgroundColor = Color.gray;
            if (!string.IsNullOrEmpty(_actionMessage)) {
                EditorGUILayout.HelpBox(_actionMessage, MessageType.None, true);
            }
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = Color.magenta;
            if (GUILayout.Button("Remote build", GUILayout.Height(ACTION_HEIGHT))) {
                if (string.IsNullOrEmpty(_botToken)) {
                    if (EditorUtility.DisplayDialog("Error", "Bot token is empty!", "Close")) {
                        return;
                    }
                }
                if (string.IsNullOrEmpty(_userID)) {
                    if (EditorUtility.DisplayDialog("Error", "User ID is empty!", "Close")) {
                        return;
                    }
                }
                if (string.IsNullOrEmpty(_gameTitle)) {
                    if (EditorUtility.DisplayDialog("Error", "Game title is empty!", "Close")) {
                        return;
                    }
                }
                string error = "";
                string branch = Execute("git", "branch --show-current", out error);
                if (!string.IsNullOrEmpty(error)) {
                    if (EditorUtility.DisplayDialog("Error", error, "Close")) {
                        return;
                    }
                }
                string path = Execute("git", "rev-parse --show-toplevel", out error);
                if (!string.IsNullOrEmpty(error)) {
                    if (EditorUtility.DisplayDialog("Error", error, "Close")) {
                        return;
                    }
                }
                string project = new DirectoryInfo(path).Name;
                string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
                string url = string.Format(BOT_COMMAND_URL, _botToken, _userID, project, branch, _currentPlatform.ToString().ToLowerInvariant(), defines, BOT_PAYLOAD).Replace("\n", "").Replace("\r", "");
                UnityWebRequest request = UnityWebRequest.Get(url);
                request.SendWebRequest();
            }
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape) {
                Close();
            }
        }

        /// <summary>
        /// Get active scene list
        /// </summary>
        static string[] GetScenes() {
            List<string> scenes = new List<string>();
            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++) {
                if (EditorBuildSettings.scenes[i].enabled) {
                    scenes.Add(EditorBuildSettings.scenes[i].path);
                }
            }
            return scenes.ToArray();
        }

        /// <summary>
        /// Run build process
        /// </summary>
        private BuildResult Build() {
            string postfix = "";
            string path = "";
            string[] scenes = GetScenes();
            string ext = (_isBundleBuild) ? "aab" : "apk";
#if CLIENT_DEBUG
            EditorUserBuildSettings.development = true;
            postfix = "-dev";
#endif      
            switch (_currentPlatform) {
                case PlatformType.GOOGLE:
                    path = string.Format(ANDROID_BUILD_FILE_MASK, _platformBuilds[(int)_currentPlatform], _gameTitle, _buildVersion, _buildCode, "", postfix, ext);
                    break;
                case PlatformType.HUAWEI:
                    path = string.Format(ANDROID_BUILD_FILE_MASK, _platformBuilds[(int)_currentPlatform], _gameTitle, _buildVersion, _buildCode, "-huawei", postfix, ext);
                    break;
                case PlatformType.IOS:
                case PlatformType.WEB:
                    path = string.Format(IOS_BUILD_MASK, _platformBuilds[(int)_currentPlatform], _gameTitle, _buildVersion, _buildCode, postfix);
                    if (!Directory.Exists(path)) {
                        Directory.CreateDirectory(path);
                    }
                    break;
                default:
                    return BuildResult.Unknown;
            }
            Debug.LogFormat("App path: {0}", path);
            BuildReport report = BuildPipeline.BuildPlayer(scenes, path, EditorUserBuildSettings.activeBuildTarget, BuildOptions.None);
            return report.summary.result;
        }

        /// <summary>
        /// Apply logger in settings
        /// </summary>
        void ApplyLoggers() {
            BuildTargetGroup group = BuildTargetGroup.Unknown;
            switch (_currentPlatform) {
                case PlatformType.IOS:
                    group = BuildTargetGroup.iOS;
                    break;
                case PlatformType.GOOGLE:
                case PlatformType.HUAWEI:
                    group = BuildTargetGroup.Android;
                    break;
                case PlatformType.WEB:
                    group = BuildTargetGroup.WebGL;
                    break;
                default:
                    break;
            }
            string[] defines = new string[3] { "CLIENT_DEBUG", "GAME_DEBUG", "SHOW_PING" };
            DefineSymbols.Remove(defines, group);
            SaveData(LOGGER_KEY, "[]");
            foreach (LoggerType logger in _loggers) {
                SetLoggerDefine(logger, group);
            }
        }

        /// <summary>
        /// Switch on logger
        /// </summary>
        /// <param name="logger">Logger type</param>
        /// <param name="group">Target group</param>
        void SetLoggerDefine(LoggerType logger, BuildTargetGroup group) {
            Debug.LogFormat("Enable logger: {0}", logger.ToString());
            string[] define = new string[1];
            switch (logger) {
                case LoggerType.CLIENT:
                    define[0] = "CLIENT_DEBUG";
                    break;
                case LoggerType.GAME:
                    define[0] = "GAME_DEBUG";
                    break;
                case LoggerType.PING:
                    define[0] = "SHOW_PING";
                    break;
                default:
                    break;
            }
            DefineSymbols.Add(define, group);
            JsonArray list = new JsonArray();
            foreach (LoggerType log in _loggers) {
                list.Add((int)log);
            }
            SaveData(LOGGER_KEY, list.ToJsonString());
        }

        /// <summary>
        /// Apply version and build code
        /// </summary>
        private void ApplyVersion() {
            PlayerSettings.bundleVersion = _buildVersion;
#if UNITY_ANDROID
            int bundle = MAX_BUNDLE;
            int.TryParse(_buildCode, out bundle);
            if (bundle == MAX_BUNDLE) {
                Debug.LogErrorFormat("Can't apply new bundle code. It must contains only digits: {0}", _buildCode);
            } else {
                PlayerSettings.Android.bundleVersionCode = bundle;
            }
#elif UNITY_IOS
            PlayerSettings.iOS.buildNumber = _buildCode;
#endif
        }

        /// <summary>
        /// Apply platform in settings
        /// </summary>
        void ApplyPlatform() {
            switch (_currentPlatform) {
                case PlatformType.IOS:
                    DefineSymbols.Remove(new string[] { "HUAWEI", "GOOGLE" }, BuildTargetGroup.iOS);
                    EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.iOS, BuildTarget.iOS);
                    break;
                case PlatformType.GOOGLE:
                    DefineSymbols.Remove(new string[] { "HUAWEI" }, BuildTargetGroup.Android);
                    DefineSymbols.Add(new string[] { "GOOGLE" }, BuildTargetGroup.Android);
                    EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.Android, BuildTarget.Android);
                    break;
                case PlatformType.HUAWEI:
                    DefineSymbols.Remove(new string[] { "GOOGLE" }, BuildTargetGroup.Android);
                    DefineSymbols.Add(new string[] { "HUAWEI" }, BuildTargetGroup.Android);
                    EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.Android, BuildTarget.Android);
                    break;
                case PlatformType.WEB:
                    DefineSymbols.Remove(new string[] { "HUAWEI", "GOOGLE" }, BuildTargetGroup.WebGL);
                    EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.WebGL, BuildTarget.WebGL);
                    break;
                default:
                    break;
            }
            Debug.LogFormat("Enable platform: {0}", _currentPlatform.ToString());
            UBHPrefs.SetInt(PLATFORM_KEY, (int)_currentPlatform);
        }

        /// <summary>
        /// Run external app
        /// </summary>
        /// <param name="cmd">Command name</param>
        /// <param name="args">Arguments</param>
        /// <param name="error">Error</param>
        private string Execute(string cmd, string args, out string error) {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = cmd;
            process.StartInfo.Arguments = args;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            return result;
        }

        /// <summary>
        /// Save build paths
        /// </summary>
        private void ApplyPath() {
            JsonArray list = new JsonArray();
            foreach (string item in _platformBuilds) {
                list.Add(item);
            }
            SaveData(BUILD_KEY, list.ToJsonString());
        }

        /// <summary>
        /// Show action message in log
        /// </summary>
        /// <param name="message">Log message</param>
        void ShowActionMessage(string message) {
            _actionMessage = message;
            HideMessage();
        }

        /// <summary>
        /// Hide message after delay
        /// </summary>
        async static void HideMessage() {
            UnityBuilderHelper window = GetWindow<UnityBuilderHelper>(true);
            await System.Threading.Tasks.Task.Delay(1500);
            window._actionMessage = null;
            window.Repaint();
        }

        /// <summary>
        /// Init buttons colors
        /// </summary>
        void InitColors() {
            for (int i = 0; i < _platformsColors.Length; i++) {
                _platformsColors[i] = OFF_BUTTON;
            }
            for (int i = 0; i < _loggersColors.Length; i++) {
                _loggersColors[i] = OFF_BUTTON;
            }
        }

        /// <summary>
        /// Assmble project by command line
        /// </summary>
        static void Assemble() {
            string[] args = System.Environment.GetCommandLineArgs();
            Dictionary<string, string> param = new Dictionary<string, string>();
            for (int i = 1; i < args.Length - 1; i++) {
                string key = (args[i][0].Equals('-')) ? args[i].Substring(1) : "";
                if (!string.IsNullOrEmpty(key) && !args[i + 1][0].Equals('-')) {
                    param.Add(key, args[i + 1]);
                }
            }
            JsonObject info = new JsonObject();
            PlatformType platform = PlatformType.UNKNOWN;
            switch (param["platform"]) {
                case "ios":
                    platform = PlatformType.IOS;
                    break;
                case "google":
                    platform = PlatformType.GOOGLE;
                    break;
                case "huawei":
                    platform = PlatformType.HUAWEI;
                    break;
                case "webgl":
                    platform = PlatformType.WEB;
                    break;
                default:
                    break;
            }
            string list = UBHPrefs.GetString(LOGGER_KEY);
            string defines = param["defines"];
            string path = param["output"];
            string postfix = "";
            BuildTargetGroup group = BuildTargetGroup.Unknown;
            BuildTarget target = BuildTarget.iOS;
            switch (platform) {
                case PlatformType.IOS:
                    group = BuildTargetGroup.iOS;
                    target = BuildTarget.iOS;
                    info["code"] = PlayerSettings.iOS.buildNumber;
                    path = string.Format(IOS_BUILD_MASK, path, UBHPrefs.GetString(GAME_TITLE_KEY), PlayerSettings.bundleVersion, PlayerSettings.iOS.buildNumber, "-dev");
                    if (!Directory.Exists(path)) {
                        Directory.CreateDirectory(path);
                    }
                    break;
                case PlatformType.GOOGLE:
                    group = BuildTargetGroup.Android;
                    target = BuildTarget.Android;
                    info["code"] = PlayerSettings.Android.bundleVersionCode;
                    path = string.Format(ANDROID_BUILD_FILE_MASK, path, UBHPrefs.GetString(GAME_TITLE_KEY), PlayerSettings.bundleVersion, PlayerSettings.Android.bundleVersionCode, "", "-dev", "apk");
                    break;
                case PlatformType.HUAWEI:
                    group = BuildTargetGroup.Android;
                    target = BuildTarget.Android;
                    postfix = "-huawei";
                    info["code"] = PlayerSettings.Android.bundleVersionCode;
                    path = string.Format(ANDROID_BUILD_FILE_MASK, path, UBHPrefs.GetString(GAME_TITLE_KEY), PlayerSettings.bundleVersion, PlayerSettings.Android.bundleVersionCode, "-huawei", "-dev", "apk");
                    break;
                default:
                    break;
            }
            info["name"] = UBHPrefs.GetString(GAME_TITLE_KEY);
            info["bundle"] = PlayerSettings.applicationIdentifier;
            info["company"] = PlayerSettings.companyName;
            info["version"] = PlayerSettings.bundleVersion;
            info["source"] = string.Format("{0}.{1}.{2}{3}-dev", UBHPrefs.GetString(GAME_TITLE_KEY), info["version"], info["code"], postfix);
            byte[] data = Encoding.UTF8.GetBytes(info.ToJsonString());
            File.WriteAllText(Path.Combine(param["output"], string.Format("{0}.{1}.build.json", param["project"], param["platform"])), info.ToJsonString());
            PlayerSettings.SetScriptingBackend(group, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, defines);
            EditorUserBuildSettings.SwitchActiveBuildTarget(group, target);
            EditorUserBuildSettings.development = true;
            if (platform == PlatformType.GOOGLE) {
                PlayerSettings.Android.keystoreName = UBHPrefs.GetString(GOOGLE_PATH_KEY);
                PlayerSettings.Android.keystorePass = UBHPrefs.GetString(GOOGLE_PASSWORD_KEY);
                PlayerSettings.Android.keyaliasName = UBHPrefs.GetString(GOOGLE_ALIAS_KEY);
                PlayerSettings.Android.keyaliasPass = UBHPrefs.GetString(GOOGLE_APASS_KEY);
            } else if (platform == PlatformType.HUAWEI) {
                PlayerSettings.Android.keystoreName = UBHPrefs.GetString(HUAWEI_PATH_KEY);
                PlayerSettings.Android.keystorePass = UBHPrefs.GetString(HUAWEI_PASSWORD_KEY);
                PlayerSettings.Android.keyaliasName = UBHPrefs.GetString(HUAWEI_ALIAS_KEY);
                PlayerSettings.Android.keyaliasPass = UBHPrefs.GetString(HUAWEI_APASS_KEY);
            }
            BuildReport report = BuildPipeline.BuildPlayer(GetScenes(), path, target, BuildOptions.None);
            int code = (report.summary.result == BuildResult.Succeeded) ? 0 : 1;
            EditorApplication.Exit(code);
        }

    }

}