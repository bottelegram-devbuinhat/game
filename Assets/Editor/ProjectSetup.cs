using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DudeChaos.EditorTools
{
    public static class ProjectSetup
    {
        public const string RendererPath = "Assets/Settings/Mobile_Renderer.asset";
        public const string PipelinePath = "Assets/Settings/Mobile_URP.asset";

        [MenuItem("DudeChaos/Setup/0 - Import TMP Essentials")]
        public static void ImportTmpEssentials()
        {
            string pkgResources = "Packages/com.unity.ugui/Package Resources/TMP Essential Resources.unitypackage";
            if (File.Exists(pkgResources))
            {
                AssetDatabase.ImportPackage(pkgResources, false);
                Debug.Log("DudeChaos Setup 0: Imported TMP Essential Resources.");
            }
            else
            {
                Debug.LogWarning("DudeChaos Setup 0: TMP Essential Resources package not found at expected path.");
            }
        }

        // §1.9.1 & R1: Treat committed asset as truth when present; assign to GraphicsSettings and all Quality levels.
        [MenuItem("DudeChaos/Setup/1 - Create Or Verify URP Pipeline")]
        public static void CreatePipeline()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Settings"))
                AssetDatabase.CreateFolder("Assets", "Settings");

            UniversalRendererData renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
            if (renderer == null)
            {
                renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(renderer, RendererPath);
            }

            UniversalRenderPipelineAsset pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            if (pipeline == null)
            {
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                pipeline.msaaSampleCount = 1;
                pipeline.supportsHDR = true;
                pipeline.renderScale = 1.0f;
                pipeline.shadowDistance = 50.0f;
                AssetDatabase.CreateAsset(pipeline, PipelinePath);
            }
            else
            {
                Debug.Log($"DudeChaos URP diff check: MSAA={pipeline.msaaSampleCount}, HDR={pipeline.supportsHDR}, shadowDist={pipeline.shadowDistance}, scale={pipeline.renderScale}");
            }

            GraphicsSettings.defaultRenderPipeline = pipeline;
            int currentLevel = QualitySettings.GetQualityLevel();
            string[] names = QualitySettings.names;
            for (int i = 0; i < names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.renderPipeline = pipeline;
            }
            QualitySettings.SetQualityLevel(currentLevel, false);

            AssetDatabase.SaveAssets();
            Debug.Log("Anything pink is now a missing material, not a missing pipeline.");
        }

        [MenuItem("DudeChaos/Setup/2 - Configure Android Target")]
        public static void ConfigureAndroid()
        {
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.dudechaos.wars");
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            Time.fixedDeltaTime = 0.02f;
            Physics.autoSyncTransforms = false;
            AssetDatabase.SaveAssets();
            Debug.Log("DudeChaos Setup 2: Configured Android IL2CPP ARM64 minSdk 25 Linear Landscape.");
        }

        [MenuItem("DudeChaos/Setup/3 - Report Project State")]
        public static void ReportState()
        {
            Debug.Log($"DudeChaos State: Unity={Application.unityVersion}, SRP={GraphicsSettings.defaultRenderPipeline?.name ?? "NONE"}, ColorSpace={PlayerSettings.colorSpace}");
        }
    }
}
