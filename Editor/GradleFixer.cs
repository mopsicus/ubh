using System.IO;
using UnityEditor;
using UnityEditor.Android;
using UnityEngine;

namespace Mopsicus.UBH {

    public class GradleFixer : IPostGenerateGradleAndroidProject {

        /// <summary>
        /// Folder to copy resources
        /// </summary>
        public static string OUTPUT_RESOURCES_FOLDER = "Temp/gradleOut/unityLibrary/src/main/res";

        /// <summary>
        /// Folder with resources
        /// </summary>
        public static string SUPPORT_ANDROID_FOLDER = "SupportFiles/Android";

        /// <summary>
        /// Temp folder for gradle files
        /// </summary>
        public static string SUPPORT_TEMP_FOLDER = "SupportFiles/Temp";

        /// <summary>
        /// Common gradle
        /// </summary>
        public static string COMMON_GRADLE = "CommonMainTemplate.gradle";

        /// <summary>
        /// Main gradle
        /// </summary>
        public static string MAIN_GRADLE = "MainTemplate.gradle";

        /// <summary>
        /// Launcher gradle
        /// </summary>
        public static string LAUNCHER_GRADLE = "LauncherTemplate.gradle";

        /// <summary>
        /// Base project gradle
        /// </summary>
        public static string BASE_GRADLE = "BaseProjectTemplate.gradle";

        /// <summary>
        /// Callback for CLI
        /// </summary>
        public int callbackOrder => 1;

        /// <summary>
        /// Fix gradle
        /// </summary>
        public void OnPostGenerateGradleAndroidProject(string path) {
#if UNITY_ANDROID
#if GOOGLE
            string file = "google-services.json";
#elif HUAWEI
            string file = "agconnect-services.json";
#else
            string file = "";
#endif
            string supportsPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, SUPPORT_TEMP_FOLDER);
            string filePath = Path.Combine(Application.streamingAssetsPath, file);
            string destPath = Path.Combine(Directory.GetParent(path).FullName + Path.DirectorySeparatorChar + "launcher", file);
            string mainTemplatePath = Path.Combine(supportsPath, MAIN_GRADLE);
            FileUtil.CopyFileOrDirectory(mainTemplatePath, Path.GetFullPath(path) + @"/" + MAIN_GRADLE);
            using (StreamWriter writer = File.AppendText(Path.GetFullPath(path) + "/build.gradle")) {
                writer.WriteLine(string.Format("\napply from: '{0}'", MAIN_GRADLE));
            }
            string launcherTemplatePath = Path.Combine(supportsPath, LAUNCHER_GRADLE);
            FileUtil.CopyFileOrDirectory(launcherTemplatePath, Directory.GetParent(path).FullName + @"/launcher/" + LAUNCHER_GRADLE);
            using (StreamWriter writer = File.AppendText(Directory.GetParent(path).FullName + "/launcher/build.gradle")) {
                writer.WriteLine(string.Format("\napply from: '{0}'", LAUNCHER_GRADLE));
            }
            string baseProjectTemplatePath = Path.Combine(supportsPath, BASE_GRADLE);
            FileUtil.CopyFileOrDirectory(baseProjectTemplatePath, Directory.GetParent(path).FullName + @"/" + BASE_GRADLE);
            using (StreamWriter writer = File.AppendText(Directory.GetParent(path).FullName + "/build.gradle")) {
                writer.WriteLine(string.Format("\napply from: '{0}'", BASE_GRADLE));
            }
            string commonTemplatePath = Path.Combine(supportsPath, COMMON_GRADLE);
            FileUtil.CopyFileOrDirectory(commonTemplatePath, Path.GetFullPath(path) + @"/" + COMMON_GRADLE);
            using (StreamWriter writer = File.AppendText(Path.GetFullPath(path) + "/build.gradle")) {
                writer.WriteLine(string.Format("\napply from: '{0}'", COMMON_GRADLE));
            }
            if (File.Exists(destPath)) {
                FileUtil.DeleteFileOrDirectory(destPath);
            }
#if GOOGLE
            bool isGoogleServices = UBHPrefs.GetBool(UnityBuilderHelper.GOOGLE_SERVICES_KEY, false);
            if (!isGoogleServices) {
                return;
            }
#elif HUAWEI
            bool isHuaweiServices = UBHPrefs.GetBool(UnityBuilderHelper.HUAWEI_SERVICES_KEY, false);
            if (!isHuaweiServices) {
                return;
            }
#endif           
            File.Copy(filePath, destPath);
#endif
        }
    }

}