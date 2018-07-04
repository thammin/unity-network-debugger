using System;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace UnityNetworkDebugger
{
    public class Main
    {
        private static string BetwixtVersion = "1.6.1";
        private static string OSString
        {
            get
            {
                string arch = Environment.Is64BitOperatingSystem ? "x64" : "ia32";
                string os = "";
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) os = "darwin";
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) os = "win32";
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) os = "linux";

                return $"{os}-{arch}";
            }
        }

        private static string BetwixtDownloadPath = $"https://github.com/kdzwinel/betwixt/releases/download/{BetwixtVersion}/Betwixt-{OSString}.zip";

        private static string HomePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        private static string LibraryPath = $"{HomePath}/Library/UnityNetworkDebugger";

        private static string ZipFilePath = $"{LibraryPath}/Betwixt-{OSString}.zip";

        private static string ApplicationPath
        {
            get
            {
                string ext = "";
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) ext = ".app";
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) ext = ".exe";
                return $"{LibraryPath}/Betwixt-{OSString}/Betwixt{ext}";
            }
        }

        [MenuItem("Tools/Network Debugger")]
        public static async void OpenNetworkDebugger()
        {
            await DownloadBetwixt();
            Exec("open", ApplicationPath);
        }

        private static Task<bool> DownloadBetwixt()
        {
            TaskCompletionSource<bool> promise = new TaskCompletionSource<bool>();
            if (!hasApplication())
            {
                using (WebClient wc = new WebClient())
                {
                    wc.DownloadProgressChanged += (sender, e) =>
                    {
                        EditorUtility.DisplayCancelableProgressBar(
                            "Network Debugger",
                            "Downloading... This will take few minutes.",
                            (float)e.ProgressPercentage / 100
                        );
                    };
                    wc.DownloadFileCompleted += (sender, e) =>
                    {
                        Exec("unzip", $"{ZipFilePath} -d {LibraryPath}");
                        Exec("rm", $"-rf {ZipFilePath}");
                        EditorUtility.ClearProgressBar();
                        promise.SetResult(true);
                    };
                    wc.DownloadFileAsync(new System.Uri(BetwixtDownloadPath), ZipFilePath);
                }
            }
            else
            {
                promise.SetResult(true);
            }
            return promise.Task;
        }

        private static bool hasApplication()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return Directory.Exists(ApplicationPath);
            }
            return File.Exists(ApplicationPath);
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
