using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace DudeChaos.EditorTools
{
    // §4.7 & P11: Builds APK; release signing comes ONLY from DUDECHAOS_KEYSTORE* env vars, failing fast if absent.
    public static class BuildScript
    {
        [MenuItem("DudeChaos/Build/Android Development APK")]
        public static void BuildAndroidDev() => RunAndroidBuild(release: false);

        [MenuItem("DudeChaos/Build/Android Release APK")]
        public static void BuildAndroidRelease() => RunAndroidBuild(release: true);

        private static void RunAndroidBuild(bool release)
        {
            if (release)
            {
                string ksPath = Environment.GetEnvironmentVariable("DUDECHAOS_KEYSTORE_PATH");
                string ksPass = Environment.GetEnvironmentVariable("DUDECHAOS_KEYSTORE_PASS");
                string aliasName = Environment.GetEnvironmentVariable("DUDECHAOS_KEYALIAS_NAME");
                string aliasPass = Environment.GetEnvironmentVariable("DUDECHAOS_KEYALIAS_PASS");

                if (string.IsNullOrEmpty(ksPath) || string.IsNullOrEmpty(ksPass) ||
                    string.IsNullOrEmpty(aliasName) || string.IsNullOrEmpty(aliasPass))
                {
                    throw new InvalidOperationException("Release APK build refused: DUDECHAOS_KEYSTORE_* env vars are not set.");
                }

                PlayerSettings.Android.useCustomKeystore = true;
                PlayerSettings.Android.keystoreName = ksPath;
                PlayerSettings.Android.keystorePass = ksPass;
                PlayerSettings.Android.keyaliasName = aliasName;
                PlayerSettings.Android.keyaliasPass = aliasPass;
            }

            List<string> enabledScenes = new List<string>();
            foreach (var s in EditorBuildSettings.scenes)
            {
                if (s.enabled) enabledScenes.Add(s.path);
            }

            BuildPlayerOptions opts = new BuildPlayerOptions
            {
                scenes = enabledScenes.ToArray(),
                locationPathName = release ? "Builds/DudeChaosWars-release.apk" : "Builds/DudeChaosWars-dev.apk",
                target = BuildTarget.Android,
                options = release ? BuildOptions.None : BuildOptions.Development
            };

            BuildReport report = BuildPipeline.BuildPlayer(opts);
            if (report.summary.result != BuildResult.Succeeded)
                throw new Exception($"Android build failed: {report.summary.result}");
        }
    }
}
