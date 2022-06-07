#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Mopsicus.UBH {

    public static class DefineSymbols {

        /// <summary>
        /// Delimiter for store symbols
        /// </summary>
        private const char DEFINE_SEPARATOR = ';';

        /// <summary>
        /// List for all defines
        /// </summary>
        private static readonly List<string> _list = new List<string>(32);

        /// <summary>
        /// Add defines to editor settings
        /// </summary>
        /// <param name="defines">Array of defines</param>
        /// <param name="group">Target group</param>
        public static void Add(string[] defines, BuildTargetGroup group = BuildTargetGroup.Unknown) {
            if (group == BuildTargetGroup.Unknown) {
                group = EditorUserBuildSettings.selectedBuildTargetGroup;
            }
            _list.Clear();
            _list.AddRange(GetDefines(group));
            _list.AddRange(defines.Except(_list));
            UpdateDefines(_list, group);
        }

        /// <summary>
        /// Remove from editor settings
        /// </summary>
        /// <param name="defines">Array of defines</param>
        /// <param name="group">Target group</param>
        public static void Remove(string[] defines, BuildTargetGroup group = BuildTargetGroup.Unknown) {
            if (group == BuildTargetGroup.Unknown) {
                group = EditorUserBuildSettings.selectedBuildTargetGroup;
            }
            _list.Clear();
            _list.AddRange(GetDefines(group).Except(defines));
            UpdateDefines(_list, group);
        }

        /// <summary>
        /// Clear all defines
        /// </summary>
        /// <param name="group">Target group</param>
        public static void Clear(BuildTargetGroup group = BuildTargetGroup.Unknown) {
            if (group == BuildTargetGroup.Unknown) {
                group = EditorUserBuildSettings.selectedBuildTargetGroup;
            }
            _list.Clear();
            UpdateDefines(_list, group);
        }

        /// <summary>
        /// Get defines list
        /// </summary>
        /// <param name="group">Target group</param>
        private static IEnumerable<string> GetDefines(BuildTargetGroup group) {
            return PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(DEFINE_SEPARATOR).ToList();
        }

        /// <summary>
        /// Update defines list
        /// </summary>
        /// <param name="list">List to update</param>
        /// <param name="group">Target group</param>
        private static void UpdateDefines(List<string> list, BuildTargetGroup group) {
            string defines = string.Join(DEFINE_SEPARATOR.ToString(), list.ToArray());
            Debug.LogFormat("Update defines: {0}", defines);
            try {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, defines);
            } catch (Exception e) {
                Debug.LogErrorFormat("Can't update define symbols for {0}. Error: {1}", group, e.Message);
            }
        }
    }

}

#endif