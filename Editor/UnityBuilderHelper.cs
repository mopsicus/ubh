using System.Collections.Generic;
using System.IO;
using NiceJson;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace Mopsicus.UBH {

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
        const int HEIGHT = 640;

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
            GUILayout.BeginVertical();
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
                if (string.IsNullOrEmpty(UBHConfig.BotToken)) {
                    if (EditorUtility.DisplayDialog(titleContent.text, "Bot token is empty!", "Close")) {
                        return;
                    }
                }
                if (string.IsNullOrEmpty(UBHConfig.UserID)) {
                    if (EditorUtility.DisplayDialog("Error", "User ID is empty!", "Close")) {
                        return;
                    }
                }
                if (string.IsNullOrEmpty(UBHConfig.GameTitle)) {
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
                string url = string.Format(BOT_COMMAND_URL, UBHConfig.BotToken, UBHConfig.UserID, project, branch, _currentPlatform.ToString().ToLowerInvariant(), defines, BOT_PAYLOAD).Replace("\n", "").Replace("\r", "");
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
                    path = string.Format(ANDROID_BUILD_FILE_MASK, _platformBuilds[(int)_currentPlatform], UBHConfig.GameTitle, _buildVersion, _buildCode, "", postfix, ext);
                    break;
                case PlatformType.HUAWEI:
                    path = string.Format(ANDROID_BUILD_FILE_MASK, _platformBuilds[(int)_currentPlatform], UBHConfig.GameTitle, _buildVersion, _buildCode, "-huawei", postfix, ext);
                    break;
                case PlatformType.IOS:
                case PlatformType.WEB:
                    path = string.Format(IOS_BUILD_MASK, _platformBuilds[(int)_currentPlatform], UBHConfig.GameTitle, _buildVersion, _buildCode, postfix);
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

    }

}