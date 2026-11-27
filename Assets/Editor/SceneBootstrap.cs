using System;
using System.Collections.Generic;
using DudeChaos.CameraRig;
using DudeChaos.Combat;
using DudeChaos.Data;
using DudeChaos.Npc;
using DudeChaos.Player;
using DudeChaos.Player.Input;
using DudeChaos.Settings;
using DudeChaos.UI;
using DudeChaos.Vehicles;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DudeChaos.EditorTools
{
    public static class SceneBootstrap
    {
        [MenuItem("DudeChaos/Build All Test Scenes")]
        public static void BuildAllTestScenes()
        {
            BuildScene("Assets/Scenes/Test.unity", includeCar: false, includeTargets: false, includeWeapon: false);
            BuildScene("Assets/Scenes/TestSlice.unity", includeCar: true, includeTargets: true, includeWeapon: true);
            BuildScene("Assets/Scenes/TestSlice_CarOnly.unity", includeCar: true, includeTargets: false, includeWeapon: false);
            BuildScene("Assets/Scenes/TestSlice_TargetsOnly.unity", includeCar: false, includeTargets: true, includeWeapon: false);
            BuildScene("Assets/Scenes/TestSlice_WeaponOnly.unity", includeCar: false, includeTargets: false, includeWeapon: true);
            AssetDatabase.SaveAssets();
        }

        // Landmine #10: Create scene FIRST, then load settings assets, then wire.
        private static void BuildScene(string scenePath, bool includeCar, bool includeTargets, bool includeWeapon)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientMode = AmbientMode.Flat;

            MovementSettings moveSettings = AssetDatabase.LoadAssetAtPath<MovementSettings>("Assets/Settings/MovementSettings.asset");
            RagdollSettings ragdollSettings = AssetDatabase.LoadAssetAtPath<RagdollSettings>("Assets/Settings/RagdollSettings.asset");
            CombatSettings combatSettings = AssetDatabase.LoadAssetAtPath<CombatSettings>("Assets/Settings/CombatSettings.asset");
            VehicleSettings vehicleSettings = AssetDatabase.LoadAssetAtPath<VehicleSettings>("Assets/Settings/VehicleSettings.asset");

            GameObject systems = CreateSystems();
            CreateSunAndGround();
            GameObject player = CreatePlayer(moveSettings, ragdollSettings, combatSettings, includeWeapon);
            Camera mainCam = CreateCameraRig(player, moveSettings);
            (HudController hud, MobileJoystick joy) = CreateHud(player);
            SettingsApplier applier = systems.GetComponent<SettingsApplier>();
            if (applier != null)
            {
                WireSerializedField(applier, "targetCamera", mainCam);
                WireSerializedField(applier, "hud", hud);
                WireSerializedField(applier, "joystick", joy);
            }
            CreateBaselineNpcs(8);

            if (includeCar)
                CreateSliceCar(vehicleSettings);
            if (includeTargets)
                CreateSliceTargets();

            List<string> errors = CheckSystems(scene);
            for (int i = 0; i < errors.Count; i++)
                Debug.LogError(errors[i]);

            EditorSceneManager.SaveScene(scene, scenePath);
        }

        public static GameObject CreateSystems()
        {
            GameObject systems = new GameObject(SceneSystemsContract.RequiredObjectName);
            QualityApplier qa = systems.AddComponent<QualityApplier>();
            SettingsStore store = systems.AddComponent<SettingsStore>();
            SettingsApplier applier = systems.AddComponent<SettingsApplier>();

            WireSerializedField(store, "qualityApplier", qa);
            WireSerializedField(applier, "store", store);
            WireSerializedField(applier, "qualityApplier", qa);
            return systems;
        }

        private static void CreateSunAndGround()
        {
            GameObject sunGo = new GameObject("Directional Sun");
            sunGo.transform.rotation = Quaternion.Euler(50f, -35f, 0f);
            Light sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.1f;
            sun.shadows = LightShadows.Soft;

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground // ART REPLACE";
            ground.transform.localScale = new Vector3(25f, 1f, 25f);
        }

        private static GameObject CreatePlayer(
            MovementSettings moveSettings,
            RagdollSettings ragdollSettings,
            CombatSettings combatSettings,
            bool includeWeapon)
        {
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player // ART REPLACE";
            player.transform.position = new Vector3(0f, 1.05f, 0f);

            UnityEngine.Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());
            CharacterController cc = player.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.35f;
            cc.center = new Vector3(0f, 0.9f, 0f);

            player.AddComponent<InputSystemPlayerInput>();
            player.AddComponent<HealthSystem>();
            RagdollController ragdoll = player.AddComponent<RagdollController>();
            PlayerMotor motor = player.AddComponent<PlayerMotor>();
            player.AddComponent<VehicleInteraction>();

            if (moveSettings != null) WireSerializedField(motor, "settings", moveSettings);
            if (ragdollSettings != null) WireSerializedField(ragdoll, "settings", ragdollSettings);

            if (includeWeapon)
            {
                WeaponController wc = player.AddComponent<WeaponController>();
                PlayerWeaponInput wi = player.AddComponent<PlayerWeaponInput>();
                if (combatSettings != null) WireSerializedField(wc, "settings", combatSettings);
                WireSerializedField(wi, "weaponController", wc);
            }

            return player;
        }

        private static Camera CreateCameraRig(GameObject player, MovementSettings moveSettings)
        {
            GameObject mainCamGo = new GameObject("Main Camera");
            mainCamGo.tag = "MainCamera";
            Camera cam = mainCamGo.AddComponent<Camera>();
            cam.fieldOfView = 60f;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 500f;
            mainCamGo.AddComponent<AudioListener>();
            mainCamGo.AddComponent<CinemachineBrain>();

            GameObject yawPivot = new GameObject("CameraYawPivot");
            yawPivot.transform.SetParent(player.transform, false);
            yawPivot.transform.localPosition = new Vector3(0f, 1.5f, 0f);

            GameObject pitchPivot = new GameObject("CameraPitchPivot");
            pitchPivot.transform.SetParent(yawPivot.transform, false);

            GameObject vcamGo = new GameObject("VirtualCamera");
            vcamGo.transform.SetParent(pitchPivot.transform, false);
            vcamGo.transform.localPosition = new Vector3(0f, 0f, -4.5f);
            CinemachineCamera vcam = vcamGo.AddComponent<CinemachineCamera>();

            PlayerMotor motor = player.GetComponent<PlayerMotor>();
            ThirdPersonCameraRig rig = player.AddComponent<ThirdPersonCameraRig>();
            PlayerLookController look = player.AddComponent<PlayerLookController>();
            WireSerializedField(rig, "yawPivot", yawPivot.transform);
            WireSerializedField(rig, "pitchPivot", pitchPivot.transform);
            WireSerializedField(rig, "virtualCamera", vcam);
            WireSerializedField(rig, "playerMotor", motor);
            if (motor != null) WireSerializedField(motor, "cameraYawReference", yawPivot.transform);
            if (moveSettings != null) WireSerializedField(rig, "movementSettings", moveSettings);
            WireSerializedField(look, "cameraRig", rig);
            return cam;
        }

        private static (HudController hud, MobileJoystick joy) CreateHud(GameObject player)
        {
            GameObject canvasGo = new GameObject("HUD Canvas");
            canvasGo.AddComponent<Canvas>();
            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasGo.AddComponent<GraphicRaycaster>();
            canvasGo.AddComponent<SafeAreaFitter>();
            HudController hud = canvasGo.AddComponent<HudController>();

            GameObject joyGo = new GameObject("MobileJoystick");
            joyGo.transform.SetParent(canvasGo.transform, false);
            joyGo.AddComponent<RectTransform>();
            MobileJoystick joy = joyGo.AddComponent<MobileJoystick>();
            WireSerializedField(joy, "playerInputBehaviour", player.GetComponent<InputSystemPlayerInput>());
            return (hud, joy);
        }

        private static void CreateBaselineNpcs(int count)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject npc = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                npc.name = $"NPC_{i:D2} // ART REPLACE";
                float angle = (i / (float)count) * Mathf.PI * 2f;
                npc.transform.position = new Vector3(Mathf.Cos(angle) * 14f, 1f, Mathf.Sin(angle) * 14f);
                npc.AddComponent<NpcAgent>();
            }
        }

        private static void CreateSliceCar(VehicleSettings vehicleSettings)
        {
            GameObject car = GameObject.CreatePrimitive(PrimitiveType.Cube);
            car.name = "SliceSedan // ART REPLACE";
            car.transform.position = new Vector3(6f, 0.75f, 4f);
            car.transform.localScale = new Vector3(1.8f, 1.4f, 4.6f);
            Rigidbody rb = car.AddComponent<Rigidbody>();
            rb.mass = 1200f;
            VehicleController vc = car.AddComponent<VehicleController>();
            car.AddComponent<VehicleHealth>();
            if (vehicleSettings != null) WireSerializedField(vc, "settings", vehicleSettings);
        }

        private static void CreateSliceTargets()
        {
            for (int i = 0; i < 3; i++)
            {
                GameObject dummy = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                dummy.name = $"DamageDummy_{i} // ART REPLACE";
                dummy.transform.position = new Vector3(-4f + i * 2.5f, 1f, 8f);
                DamageDummy dd = dummy.AddComponent<DamageDummy>();
                BodyPartTag tag = dummy.AddComponent<BodyPartTag>();
                WireSerializedField(tag, "damageTarget", dd);
            }
        }

        // P6 requirement: Must log "<Type> has no serialized field '<name>'" instead of silently failing.
        public static void WireSerializedField(UnityEngine.Object target, string fieldName, UnityEngine.Object value)
        {
            if (target == null) return;
            SerializedObject so = new SerializedObject(target);
            SerializedProperty prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogError($"{target.GetType().Name} has no serialized field '{fieldName}'");
                return;
            }
            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static List<string> CheckSystems(Scene scene)
        {
            var stores = UnityEngine.Object.FindObjectsByType<SettingsStore>(FindObjectsSortMode.None);
            var qas = UnityEngine.Object.FindObjectsByType<QualityApplier>(FindObjectsSortMode.None);
            var sas = UnityEngine.Object.FindObjectsByType<SettingsApplier>(FindObjectsSortMode.None);

            SettingsStore singleStore = stores.Length == 1 ? stores[0] : null;

            SceneComponentDescriptor[] storeDescs = new SceneComponentDescriptor[stores.Length];
            for (int i = 0; i < stores.Length; i++)
            {
                storeDescs[i] = new SceneComponentDescriptor
                {
                    objectName = stores[i].gameObject.name,
                    objectInstanceId = stores[i].gameObject.GetInstanceID()
                };
            }

            SceneComponentDescriptor[] qaDescs = new SceneComponentDescriptor[qas.Length];
            for (int i = 0; i < qas.Length; i++)
            {
                qaDescs[i] = new SceneComponentDescriptor
                {
                    objectName = qas[i].gameObject.name,
                    objectInstanceId = qas[i].gameObject.GetInstanceID()
                };
            }

            SceneComponentDescriptor[] saDescs = new SceneComponentDescriptor[sas.Length];
            for (int i = 0; i < sas.Length; i++)
            {
                saDescs[i] = new SceneComponentDescriptor
                {
                    objectName = sas[i].gameObject.name,
                    objectInstanceId = sas[i].gameObject.GetInstanceID(),
                    applierStoreMatchesSceneStore = singleStore != null && ReferenceEquals(sas[i].Store, singleStore)
                };
            }

            return SceneSystemsContract.ValidateScene(new SceneSystemsSnapshot
            {
                sceneName = scene.name,
                stores = storeDescs,
                qualityAppliers = qaDescs,
                settingsAppliers = saDescs
            });
        }

        [MenuItem("DudeChaos/Verify Systems In Built Scenes")]
        public static bool VerifyBuiltScenes()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            if (scenes == null || scenes.Length == 0)
            {
                Debug.LogError("No built scenes found in EditorBuildSettings.");
                return false;
            }

            bool allOk = true;
            for (int i = 0; i < scenes.Length; i++)
            {
                if (!scenes[i].enabled) continue;
                Scene opened = EditorSceneManager.OpenScene(scenes[i].path, OpenSceneMode.Single);
                List<string> errs = CheckSystems(opened);
                for (int e = 0; e < errs.Count; e++)
                {
                    Debug.LogError(errs[e]);
                    allOk = false;
                }
            }
            return allOk;
        }

        public static void VerifyBuiltScenesBatch()
        {
            bool ok = VerifyBuiltScenes();
            EditorApplication.Exit(ok ? 0 : 1);
        }
    }
}
