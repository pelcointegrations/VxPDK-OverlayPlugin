﻿using System.IO;
using VxSdkNet;

namespace PluginNs.Utilities
{
    static class Const
    {
        /// <summary>
        /// The plugin id is a UUID generated by the plugin developer which should never be changed
        /// </summary>
        public static readonly string PluginId = "";
        /// <summary>
        /// This key is provided by Pelco to allow this plugin to show up in OpsCenter
        /// </summary>
        public static readonly string PluginKey = "";
        /// <summary>
        /// The SdkLicense is provided by Pelco to allow use of the VxSdk
        /// </summary>
        public static readonly string SdkLicense = "";
        /// <summary>
        /// This mock auth token can be acquired by logging into VideoXpert via your browser
        /// then inspecting the cookies that were returned by the server and copying the 'auth_token' value
        /// to this return value. Keep in mind that it can and does expire so you may need to update this multiple
        /// times when developing.
        /// </summary>
        public static readonly string MockAuthToken = "";
        /// <summary>
        /// The base URI for your VideoXpert system in the format of 'https://10.220.232.140:443/'
        /// </summary>
        public static readonly string MockBaseUri = "";



        public static readonly string VxSdkLogFilePath = Directory.GetParent(Utils.I.CurrentAssembly().Location).FullName;
        public static readonly LogLevel.Value VxSdkLogLevel = LogLevel.Value.Warning;
        public static readonly string RegionMainView = "RegionMainView";
        public static readonly string CanvasObject = "CanvasObject";
    }
}
