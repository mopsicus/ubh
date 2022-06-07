using System.Collections.Generic;
using NiceJson;

namespace Mopsicus.UBH {

    public class UBHConfig {

        /// <summary>
        /// Telegram user id
        /// </summary>
        public static string UserID = "";

        /// <summary>
        /// Telegram bot token
        /// </summary>
        public static string BotToken = "";

        /// <summary>
        /// Game title
        /// </summary>
        public static string GameTitle = "";

        /// <summary>
        /// Locales for iOS
        /// </summary>
        public static string[] Locales = new string[] { "ru", "en", "es", "it" };

        /// <summary>
        /// Keystore config for Google
        /// </summary>
        public static JsonObject KeyStoreGoogle = new JsonObject();

        /// <summary>
        /// Keystore config for Huawei
        /// </summary>
        public static JsonObject KeyStoreHuawei = new JsonObject();

        /// <summary>
        /// Frameworks list for iOS
        /// </summary>
        public static string[] Frameworks = new string[] { "AppTrackingTransparency", "UserNotifications", "AuthenticationServices", "StoreKit", "MessageUI", "Webkit" };

        /// <summary>
        /// Files add to build
        /// </summary>
        public static string[] SupportFilesForiOS = new string[] { "splash-iphone.png", "splash-ipad.png" };

        /// <summary>
        /// Dictionary with plist data for iOS
        /// </summary>
        public static Dictionary<string, string> PListForiOS = new Dictionary<string, string>() { { "NSUserTrackingUsageDescription", "$(PRODUCT_NAME) need to access the IDFA in order to deliver personalized advertising and to help us improve the game." } };

        /// <summary>
        /// Is iOS push notifications enabled
        /// </summary>
        public static bool IsPushEnabled = true;

        /// <summary>
        /// Is iOS purchases enabled
        /// </summary>
        public static bool IsPurchaseEnabled = true;

        /// <summary>
        /// Is iOS sign in with Apple enabled
        /// </summary>
        public static bool IsSignInEnabled = true;
    }

}