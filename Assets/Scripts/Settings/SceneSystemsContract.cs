using System.Collections.Generic;

namespace DudeChaos.Settings
{
    // Plain C# scene systems contract validator (§1.9.2 & §4.2).
    // Rule: >=1 scene checked; exactly one SettingsStore, one QualityApplier, one SettingsApplier,
    // all on one GameObject named "Systems", and applier's store wired to the scene's SettingsStore.
    public struct SceneComponentDescriptor
    {
        public string objectName;
        public int objectInstanceId;
        public bool applierStoreMatchesSceneStore;
    }

    public struct SceneSystemsSnapshot
    {
        public string sceneName;
        public SceneComponentDescriptor[] stores;
        public SceneComponentDescriptor[] qualityAppliers;
        public SceneComponentDescriptor[] settingsAppliers;
    }

    public static class SceneSystemsContract
    {
        public const string RequiredObjectName = "Systems";

        public static List<string> ValidateScene(SceneSystemsSnapshot snapshot)
        {
            List<string> errors = new List<string>();
            string sceneLabel = string.IsNullOrEmpty(snapshot.sceneName) ? "<UnnamedScene>" : snapshot.sceneName;

            int storeCount = snapshot.stores != null ? snapshot.stores.Length : 0;
            int qaCount = snapshot.qualityAppliers != null ? snapshot.qualityAppliers.Length : 0;
            int saCount = snapshot.settingsAppliers != null ? snapshot.settingsAppliers.Length : 0;

            if (storeCount == 0)
                errors.Add($"{sceneLabel}: missing SettingsStore.");
            else if (storeCount > 1)
                errors.Add($"{sceneLabel}: duplicated SettingsStore ({storeCount} found).");

            if (qaCount == 0)
                errors.Add($"{sceneLabel}: missing QualityApplier.");
            else if (qaCount > 1)
                errors.Add($"{sceneLabel}: duplicated QualityApplier ({qaCount} found).");

            if (saCount == 0)
                errors.Add($"{sceneLabel}: missing SettingsApplier.");
            else if (saCount > 1)
                errors.Add($"{sceneLabel}: duplicated SettingsApplier ({saCount} found).");

            if (storeCount == 1)
            {
                if (snapshot.stores[0].objectName != RequiredObjectName)
                    errors.Add($"{sceneLabel}: SettingsStore must be on '{RequiredObjectName}', found on '{snapshot.stores[0].objectName}'.");
            }

            if (qaCount == 1)
            {
                if (snapshot.qualityAppliers[0].objectName != RequiredObjectName)
                    errors.Add($"{sceneLabel}: QualityApplier must be on '{RequiredObjectName}', found on '{snapshot.qualityAppliers[0].objectName}'.");
            }

            if (saCount == 1)
            {
                if (snapshot.settingsAppliers[0].objectName != RequiredObjectName)
                    errors.Add($"{sceneLabel}: SettingsApplier must be on '{RequiredObjectName}', found on '{snapshot.settingsAppliers[0].objectName}'.");
                if (!snapshot.settingsAppliers[0].applierStoreMatchesSceneStore)
                    errors.Add($"{sceneLabel}: SettingsApplier.store is not wired to the scene's SettingsStore.");
            }

            if (storeCount == 1 && qaCount == 1 && saCount == 1)
            {
                int storeObj = snapshot.stores[0].objectInstanceId;
                if (snapshot.qualityAppliers[0].objectInstanceId != storeObj ||
                    snapshot.settingsAppliers[0].objectInstanceId != storeObj)
                {
                    errors.Add($"{sceneLabel}: SettingsStore, QualityApplier and SettingsApplier are split across multiple GameObjects.");
                }
            }

            return errors;
        }

        public static List<string> ValidateBuiltScenes(IReadOnlyList<SceneSystemsSnapshot> builtScenes)
        {
            List<string> allErrors = new List<string>();
            if (builtScenes == null || builtScenes.Count == 0)
            {
                allErrors.Add("No built scenes found in EditorBuildSettings.");
                return allErrors;
            }

            for (int i = 0; i < builtScenes.Count; i++)
            {
                allErrors.AddRange(ValidateScene(builtScenes[i]));
            }
            return allErrors;
        }
    }
}
