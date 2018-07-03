using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace UnityNetworkDebugger
{
    public class Main
    {
        private static readonly string BetwixtDownloadPath = "https://github.com/thammin/betwixt/releases/download/2.0.0/Betwixt-darwin-x64.zip";

        private static string HomePath { get; } = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        private static string LibraryPath { get; } = $"{HomePath}/Library/UnityNetworkDebugger";

        private static string ZipFileName
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return "Betwixt-darwin-x64.zip";
                }
                return "";
            }
        }

        private static string ZipFilePath { get; } = $"{LibraryPath}/{ZipFileName}";

        private static string ApplicationPath { get; } = $"{LibraryPath}/Betwixt.app";

#if UNITY_EDITOR
        [MenuItem("Tools/Network Debugger")]
#endif
        public static void OpenNetworkDebugger()
        {
            DownloadBetwixt();
            Exec("open", ApplicationPath);
        }

        public static void DownloadBetwixt()
        {
            if (!Directory.Exists(ApplicationPath))
            {
                Exec("wget", $"{BetwixtDownloadPath} -P {LibraryPath}");
                Exec("unzip", $"{ZipFilePath} -d {LibraryPath}");
                Exec("rm", $"-rf {ZipFilePath}");
            }
        }

        private static void Exec(string fileName, string arguments)
        {
            using (var process = new Process
            {
                StartInfo = {
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                }
            })
            {
                process.Start();
                process.StandardOutput.ReadToEnd();
                process.WaitForExit();
            };
        }
    }
}
