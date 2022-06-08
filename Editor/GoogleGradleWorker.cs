using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Mopsicus.UBH {

    public class GoogleGradleWorker : IPreprocessBuildWithReport {

        /// <summary>
        /// Path supports folder
        /// </summary>
        private string _supportsPath = null;

        /// <summary>
        /// Callback for CLI
        /// </summary>
        public int callbackOrder => 0;

        /// <summary>
        /// Create/update gradle files
        /// </summary>
        /// <param name="configs">Array of configs</param>
        private void CreateGradleFiles(string[] configs) {
            CreateMainGradleFile(configs);
            CreateLauncherGradleFile(configs);
            CreateBaseProjectGradleFile();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Create main gradle file and add dependencies
        /// </summary>
        /// <param name="configs">Array of configs</param>
        private void CreateMainGradleFile(string[] configs) {
            using (StreamWriter file = File.CreateText(Path.Combine(_supportsPath, GradleFixer.MAIN_GRADLE))) {
                file.Write("dependencies {\n");
                for (int i = 0; i < configs.Length; i++) {
                    file.Write(AddDependency(configs[i]));
                }
                file.Write("}\n");
            }
        }

        /// <summary>
        /// Create launcher gradle file and add dependencies
        /// </summary>
        /// <param name="configs">Array of configs</param>
        private void CreateLauncherGradleFile(string[] configs) {
            using (StreamWriter file = File.CreateText(Path.Combine(_supportsPath, GradleFixer.LAUNCHER_GRADLE))) {
                bool isGoogleServices = UBHPrefs.GetBool(UnityBuilderHelper.GOOGLE_SERVICES_KEY, false);
                if (isGoogleServices) {
                    file.Write("apply plugin: 'com.google.gms.google-services'\n\n");
                }
                file.Write("dependencies {\n");
                for (int i = 0; i < configs.Length; i++) {
                    file.Write(AddDependency(configs[i]));
                }
                file.Write("}\n");
            }
        }

        /// <summary>
        /// Create base gradle file
        /// </summary>
        private void CreateBaseProjectGradleFile() {
            using (StreamWriter file = File.CreateText(Path.Combine(_supportsPath, GradleFixer.BASE_GRADLE))) {
                bool isGoogleServices = UBHPrefs.GetBool(UnityBuilderHelper.GOOGLE_SERVICES_KEY, false);
                file.Write("allprojects {\n");
                file.Write("\tbuildscript {\n");
                file.Write("\t\tdependencies {\n");
                if (isGoogleServices) {
                    file.Write(AddClasspath("com.google.gms:google-services:4.3.10"));
                }
                file.Write("\t\t}\n\t}\n}");
            }
        }

        /// <summary>
        /// Add dependency class to implementation section
        /// </summary>
        /// <param name="name">Class name</param>
        private string AddDependency(string name) {
            return string.Format("\timplementation '{0}'\n", name);
        }

        /// <summary>
        /// Add classpath to gradle
        /// </summary>
        /// <param name="name">Class name</param>
        private string AddClasspath(string name) {
            return string.Format("\t\t\tclasspath '{0}'\n", name);
        }

        /// <summary>
        /// Action before build
        /// Only for Google
        /// </summary>
        public void OnPreprocessBuild(BuildReport report) {
#if GOOGLE
            Application.logMessageReceived += OnBuildError;
            _supportsPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, GradleFixer.SUPPORT_TEMP_FOLDER);
            if (!Directory.Exists(_supportsPath)) {
                Directory.CreateDirectory(_supportsPath);
            }
            string[] list = new string[0];
            CreateGradleFiles(list);
#endif
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
