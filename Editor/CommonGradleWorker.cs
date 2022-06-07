using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Mopsicus.UBH {

    public class CommonGradleWorker : IPreprocessBuildWithReport {

        /// <summary>
        /// Path supports folder
        /// </summary>
        private string _supportsPath = null;

        /// <summary>
        /// Callback for CLI
        /// </summary>
        public int callbackOrder => 0;

        /// <summary>
        /// Create main gradle file
        /// With task to copy files
        /// </summary>
        /// <param name="from">Folder with project support resources</param>
        /// <param name="to">Output folder</param>
        private void CreateMainGradleFile(string from, string to) {
            using (StreamWriter file = File.CreateText(Path.Combine(_supportsPath, GradleFixer.COMMON_GRADLE))) {
                file.Write("task copyFiles(type: Copy) {\n");
                file.Write(string.Format("\tfrom '{0}'\n", from));
                file.Write(string.Format("\tinto '{0}'\n", to));
                file.Write("}\n\n");
                file.Write("preBuild.dependsOn(copyFiles)\n");
            }
        }

        /// <summary>
        /// Action before build
        /// </summary>
        public void OnPreprocessBuild(BuildReport report) {
            Application.logMessageReceived += OnBuildError;
            _supportsPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, GradleFixer.SUPPORT_TEMP_FOLDER);
            if (!Directory.Exists(_supportsPath)) {
                Directory.CreateDirectory(_supportsPath);
            }
            string from = Path.Combine(Directory.GetParent(Application.dataPath).FullName, GradleFixer.SUPPORT_ANDROID_FOLDER);
            string to = Path.Combine(Directory.GetParent(Application.dataPath).FullName, GradleFixer.OUTPUT_RESOURCES_FOLDER);
            CreateMainGradleFile(from, to);
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Callback if error occurs
        /// </summary>
        private void OnBuildError(string condition, string stackTrace, LogType type) {
            if (type == LogType.Error) {
                Application.logMessageReceived -= OnBuildError;
            }
        }
    }

}