using System.Collections.Generic;
using NiceJson;

namespace Mopsicus.UBH {

    public class BaseUBHConfig {

        /// <summary>
        /// Telegram user id
        /// </summary>
        public virtual string UserID { get; } = "";

        /// <summary>
        /// Telegram bot token
        /// </summary>
        public virtual string BotToken { get; } = "";

        /// <summary>
        /// Game title
        /// </summary>
        public virtual string GameTitle { get; } = "";

        /// <summary>
        /// Locales for iOS
        /// </summary>
        public virtual string[] Locales { get; } = new string[] { "ru", "en" };

        /// <summary>
        /// Keystore config for Google
        /// </summary>
        public virtual JsonObject KeyStoreGoogle { 
            get {
                JsonObject config = new JsonObject();
                config["path"] = "";
                config["pass"] = "";
                config["alias"] = "";
                config["apass"] = "";
                return config;
            }            
        }

        /// <summary>
        /// Keystore config for Huawei
        /// </summary>
        public virtual JsonObject KeyStoreHuawei { 
            get {
                JsonObject config = new JsonObject();
                config["path"] = "";
                config["pass"] = "";
                config["alias"] = "";
                config["apass"] = "";
                return config;
            }            
        }        

        /// <summary>
        /// Frameworks list for iOS
        /// </summary>
        public virtual string[] Frameworks { get; } = new string[] { "AppTrackingTransparency", "UserNotifications", "AuthenticationServices", "StoreKit", "MessageUI", "Webkit" };

        /// <summary>
        /// Files add to build
        /// </summary>
        public virtual string[] SupportFilesForiOS { get; } = new string[] { "splash-iphone.png", "splash-ipad.png" };

        /// <summary>
        /// Dictionary with plist data for iOS
        /// </summary>
        public virtual Dictionary<string, string> PListForiOS { get; } = new Dictionary<string, string>() { { "NSUserTrackingUsageDescription", "$(PRODUCT_NAME) need to access the IDFA in order to deliver personalized advertising and to help us improve the game." } };

        /// <summary>
        /// Is iOS push notifications enabled
        /// </summary>
        public virtual bool IsPushEnabled { get; } = true;

        /// <summary>
        /// Is iOS purchases enabled
        /// </summary>
        public virtual bool IsPurchaseEnabled { get; } = true;

        /// <summary>
        /// Is iOS sign in with Apple enabled
        /// </summary>
        public virtual bool IsSignInEnabled { get; } = true;
    }

}