#if UNITY_EDITOR

using System;
using System.Globalization;
using System.IO;
using NiceJson;
using UnityEngine;

namespace Mopsicus.UBH {

    /// <summary>
    /// UBH editor prefs manager
    /// </summary>
    public static class UBHPrefs {

        /// <summary>
        /// Path mask to save prefs file
        /// </summary>
        const string STORAGE_PATH = "{0}/../ProjectSettings/UBHPrefs.txt";

        /// <summary>
        /// Path to storage
        /// </summary>
        static string _storageFile = null;

        /// <summary>
        /// Data cache
        /// </summary>
        static JsonObject _data = null;

        /// <summary>
        /// Constructor
        /// </summary>
        static UBHPrefs() { }

        /// <summary>
        /// Load data
        /// </summary>
        static void Load() {
            if (string.IsNullOrEmpty(_storageFile)) {
                _storageFile = string.Format(STORAGE_PATH, Application.dataPath);
            }
            if (_data != null) {
                return;
            }
            try {
                string content = File.ReadAllText(_storageFile);
                _data = (JsonObject)JsonNode.ParseJsonString(content);
            } catch {
                _data = new JsonObject();
            }
        }

        /// <summary>
        /// Save data
        /// </summary>
        static void Save() {
            try {
                File.WriteAllText(_storageFile, _data.ToJsonString());
            } catch (Exception e) {
                Debug.LogErrorFormat("UBHPrefs save: {0}", e);
            }
        }

        /// <summary>
        /// Is store contains key
        /// </summary>
        /// <param name="key">Key</param>
        public static bool Has(string key) {
            if (string.IsNullOrEmpty(key)) {
                throw new UnityException("Key is missing");
            }
            Load();
            return _data.ContainsKey(key);
        }

        /// <summary>
        /// Clear all
        /// </summary>
        public static void Clear() {
            if (string.IsNullOrEmpty(_storageFile)) {
                _storageFile = string.Format(STORAGE_PATH, Application.dataPath);
            }
            _data.Clear();
            Save();
        }

        /// <summary>
        /// Delete key
        /// </summary>
        /// <param name="key">Key</param>
        public static void Delete(string key) {
            Load();
            if (Has(key)) {
                _data.Remove(key);
            }
            Save();
        }

        /// <summary>
        /// Set string value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="data">Data</param>
        public static void SetString(string key, string data) {
            Has(key);
            _data[key] = data;
            Save();
        }

        /// <summary>
        /// Get string value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="defaultValue">Default value</param>
        public static string GetString(string key, string defaultValue = "") {
            return Has(key) ? (string)_data[key] : defaultValue;
        }

        /// <summary>
        /// Set float value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="data">Data</param>
        public static void SetFloat(string key, float data) {
            SetString(key, data.ToString(NumberFormatInfo.InvariantInfo));
        }

        /// <summary>
        /// Get float value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="defaultValue">Default value</param>
        public static float GetFloat(string key, float defaultValue = 0f) {
            if (Has(key)) {
                float value = 0f;
                if (float.TryParse(_data[key], NumberStyles.Float, NumberFormatInfo.InvariantInfo, out value)) {
                    return value;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// Set int value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="data">Data</param>
        public static void SetInt(string key, int data) {
            SetString(key, data.ToString(NumberFormatInfo.InvariantInfo));
        }

        /// <summary>
        /// Get int value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="defaultValue">Default value</param>
        public static int GetInt(string key, int defaultValue = 0) {
            if (Has(key)) {
                int value = 0;
                if (int.TryParse(_data[key], out value)) {
                    return value;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// Get bool value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="defaultValue">Default value</param>
        public static bool GetBool(string key, bool defaultValue = false) {
            return GetInt(key, defaultValue ? 1 : 0) != 0;
        }

        /// <summary>
        /// Set bool value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="data">Data</param>
        public static void SetBool(string key, bool data) {
            SetInt(key, data ? 1 : 0);
        }
    }
}

#endif