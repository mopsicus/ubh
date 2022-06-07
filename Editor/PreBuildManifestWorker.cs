using System;
using System.IO;
using System.Text;
using System.Xml;
using UnityEditor.Android;
using UnityEngine;

namespace Mopsicus.UBH {

    public class PreBuildManifestWorker : IPostGenerateGradleAndroidProject {

        /// <summary>
        /// Huawei config JSON
        /// </summary>
        const string HUAWEI_CONFIG = "agconnect-services.json";

        /// <summary>
        /// Method runs after gradle generated, before build
        /// </summary>
        public void OnPostGenerateGradleAndroidProject(string basePath) {
            AndroidManifest manifest = new AndroidManifest(GetManifestPath(basePath));
            manifest.SetHardwareAcceleration();
#if HUAWEI
            manifest.RemoveUsesPermission("bluetooth");
            manifest.RemoveUsesPermission("camera");
            manifest.RemoveUsesPermission("read_calendar");
            manifest.RemoveUsesPermission("write_calendar");
            manifest.RemoveUsesPermission("access_wifi_state");
            manifest.RemoveUsesPermission("query_all_packages");
            string data = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, HUAWEI_CONFIG));
            try {
                JsonObject json = (JsonObject)JsonNode.ParseJsonString(data);
                manifest.AddMetaData("com.huawei.hms.client.appid", string.Format("appid={0}", json["client"]["app_id"]));
                manifest.AddMetaData("com.huawei.hms.client.cpid", string.Format("cpid={0}", json["client"]["cp_id"]));
            } catch (Exception e) {
                Debug.LogErrorFormat("Can't get Huawei config data for manifest: {0}", e.Message);
            }
#endif
            manifest.Save();
        }

        /// <summary>
        /// Callback (need for CLI)
        /// </summary>
        public int callbackOrder {
            get {
                return 1;
            }
        }

        /// <summary>
        /// Cached manifest path
        /// </summary>
        private string _manifestFilePath = null;

        /// <summary>
        /// Get manifest file
        /// </summary>
        private string GetManifestPath(string basePath) {
            if (string.IsNullOrEmpty(_manifestFilePath)) {
                StringBuilder pathBuilder = new StringBuilder(basePath);
                pathBuilder.Append(Path.DirectorySeparatorChar).Append("src");
                pathBuilder.Append(Path.DirectorySeparatorChar).Append("main");
                pathBuilder.Append(Path.DirectorySeparatorChar).Append("AndroidManifest.xml");
                _manifestFilePath = pathBuilder.ToString();
            }
            return _manifestFilePath;
        }
    }

    internal class AndroidXmlDocument : XmlDocument {

        /// <summary>
        /// Cached path
        /// </summary>
        private string _path = null;

        /// <summary>
        /// Namespace manager
        /// </summary>
        protected XmlNamespaceManager _namespaceManager = null;

        /// <summary>
        /// XML namespace
        /// </summary>
        public readonly string AndroidXmlNamespace = "http://schemas.android.com/apk/res/android";

        /// <summary>
        /// Tools namespace
        /// </summary>
        public readonly string ToolsXmlNamespace = "http://schemas.android.com/tools";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path">Path for document</param>
        public AndroidXmlDocument(string path) {
            _path = path;
            using (var reader = new XmlTextReader(_path)) {
                reader.Read();
                Load(reader);
            }
            _namespaceManager = new XmlNamespaceManager(NameTable);
            _namespaceManager.AddNamespace("android", AndroidXmlNamespace);
        }

        /// <summary>
        /// Save file
        /// </summary>
        public string Save() {
            return SaveAs(_path);
        }

        /// <summary>
        /// Save file as
        /// </summary>
        /// <param name="path">Path to save</param>
        public string SaveAs(string path) {
            using (XmlTextWriter writer = new XmlTextWriter(path, new UTF8Encoding(false))) {
                writer.Formatting = Formatting.Indented;
                Save(writer);
            }
            return path;
        }
    }

    internal class AndroidManifest : AndroidXmlDocument {

        /// <summary>
        /// Application element
        /// </summary>
        private readonly XmlElement _application = null;

        /// <summary>
        /// Root manifest element
        /// </summary>
        private readonly XmlElement _root = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path">Path to manifest</param>
        public AndroidManifest(string path) : base(path) {
            _root = SelectSingleNode("/manifest") as XmlElement;
            _application = SelectSingleNode("/manifest/application") as XmlElement;
        }

        /// <summary>
        /// Create new attribute to manifest
        /// </summary>
        private XmlAttribute CreateAndroidAttribute(string key, string value) {
            XmlAttribute attr = CreateAttribute("android", key, AndroidXmlNamespace);
            attr.Value = value;
            return attr;
        }

        /// <summary>
        /// Get main activity
        /// </summary>
        internal XmlNode GetActivityWithLaunchIntent() {
            return SelectSingleNode("/manifest/application/activity[intent-filter/action/@android:name='android.intent.action.MAIN' and intent-filter/category/@android:name='android.intent.category.LAUNCHER']", _namespaceManager);
        }

        /// <summary>
        /// Change app theme
        /// </summary>
        internal void SetApplicationTheme(string theme) {
            Debug.LogFormat("App theme set to {0}", theme);
            _application.Attributes.Append(CreateAndroidAttribute("theme", theme));
        }

        /// <summary>
        /// Set starting activity
        /// </summary>
        internal void SetStartingActivityName(string name) {
            Debug.LogFormat("Start activity set to {0}", name);
            GetActivityWithLaunchIntent().Attributes.Append(CreateAndroidAttribute("name", name));
        }

        /// <summary>
        /// Enable hardware acceleration
        /// </summary>
        internal void SetHardwareAcceleration() {
            Debug.Log("Set hardware acceleration done");
            GetActivityWithLaunchIntent().Attributes.Append(CreateAndroidAttribute("hardwareAccelerated", "true"));
        }

        /// <summary>
        /// Add meta data to activity
        /// </summary>
        internal void AddMetaData(string name, string value) {
            Debug.LogFormat("Add meta data to manifest: name = {0}, value = {1}", name, value);
            XmlNode node = CreateNode(XmlNodeType.Element, "meta-data", null);
            node.Attributes.Append(CreateAndroidAttribute("name", name));
            node.Attributes.Append(CreateAndroidAttribute("value", value));
            _application.AppendChild(node);
        }

        /// <summary>
        /// Set authorities for file provider
        /// </summary>
        /// <param name="name">App name</param>
        internal void SetProviderAuthorities(string name) {
            XmlNode node = SelectSingleNode("/manifest/application/provider[meta-data/@android:name='android.support.FILE_PROVIDER_PATHS']", _namespaceManager);
            if (node == null) {
                Debug.LogError("Can't find <provider> in manifest");
            } else {
                Debug.LogFormat("Set provider authorities: {0}", name);
                node.Attributes.Append(CreateAndroidAttribute("authorities", string.Format("{0}.fileprovider", name)));
            }
        }

        /// <summary>
        /// Remove permission from manifest on merge
        /// </summary>
        /// <param name="name">Permission name</param>
        internal void RemoveUsesPermission(string name) {
            XmlNode node = SelectSingleNode(string.Format("/manifest[uses-permission/@android:name='android.permission.{0}']", name.ToUpperInvariant()), _namespaceManager);
            if (node == null) {
                Debug.LogFormat("Add uses-permission permission to manifest: name = {0}", name);
                node = CreateNode(XmlNodeType.Element, "uses-permission", null);
                node.Attributes.Append(CreateAndroidAttribute("name", string.Format("android.permission.{0}", name.ToUpperInvariant())));
                _root.AppendChild(node);
            }
            Debug.LogFormat("Add tools remove permission action to manifest: name = {0}", name);
            XmlAttribute remover = CreateAttribute("tools", "node", ToolsXmlNamespace);
            remover.Value = "remove";
            node.Attributes.Append(remover);
        }

    }

}