using System.Collections;
using System.Reflection;
using DudeChaos.Combat;
using DudeChaos.Data;
using DudeChaos.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DudeChaos.Tests.PlayMode
{
    // §4.6 Six baseline Play Mode smoke tests + Explicit slice test; uses WaitForSeconds (never frame counts).
    public sealed class PlayModeSmokeTest
    {
        private sealed class FakeTestInput : MonoBehaviour, IPlayerInput
        {
            public Vector2 Move { get; set; }
            public Vector2 Look { get; set; }
            public bool SprintHeld { get; set; }
            public bool CrouchPressed => false;
            public bool JumpPressed => false;
            public bool FireHeld => false;
            public bool FirePressed => false;
            public bool AimHeld => false;
            public bool ReloadPressed => false;
            public bool MeleePressed => false;
            public bool HeavyMeleePressed => false;
            public bool InteractPressed => false;
            public bool PausePressed => false;
            public bool NitroHeld => false;
            public void SetVirtualMove(Vector2 move) => Move = move;
            public void SetVirtualLook(Vector2 look) => Look = look;
        }

        [UnityTest]
        public IEnumerator Player_Falls_And_Settles_On_Ground()
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            GameObject player = CreateTestPlayer(new Vector3(0f, 3.5f, 0f), out _);

            yield return new WaitForSeconds(1.2f);
            Assert.Less(player.transform.position.y, 1.5f);

            Object.Destroy(player);
            Object.Destroy(floor);
        }

        [UnityTest]
        public IEnumerator Player_Moves_Forward_With_Input()
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            GameObject player = CreateTestPlayer(new Vector3(0f, 0.1f, 0f), out FakeTestInput input);
            input.Move = new Vector2(0f, 1f);

            yield return new WaitForSeconds(0.5f);
            Assert.Greater(player.transform.position.z, 1.0f);

            Object.Destroy(player);
            Object.Destroy(floor);
        }

        [UnityTest]
        public IEnumerator Player_Sprints_Faster_Than_It_Runs()
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            GameObject runner = CreateTestPlayer(new Vector3(-3f, 0.1f, 0f), out FakeTestInput runInput);
            GameObject sprinter = CreateTestPlayer(new Vector3(3f, 0.1f, 0f), out FakeTestInput sprintInput);

            runInput.Move = new Vector2(0f, 1f);
            sprintInput.Move = new Vector2(0f, 1f);
            sprintInput.SprintHeld = true;

            yield return new WaitForSeconds(0.5f);
            PlayerMotor runMotor = runner.GetComponent<PlayerMotor>();
            PlayerMotor sprintMotor = sprinter.GetComponent<PlayerMotor>();
            Assert.AreEqual(6.0f, runMotor.CurrentSpeed, 0.15f);
            Assert.AreEqual(8.8f, sprintMotor.CurrentSpeed, 0.15f);

            Object.Destroy(runner);
            Object.Destroy(sprinter);
            Object.Destroy(floor);
        }

        [UnityTest]
        public IEnumerator Player_Stops_When_Input_Released()
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            GameObject player = CreateTestPlayer(new Vector3(0f, 0.1f, 0f), out FakeTestInput input);
            input.Move = new Vector2(0f, 1f);
            yield return new WaitForSeconds(0.3f);

            input.Move = Vector2.zero;
            yield return new WaitForSeconds(0.2f);
            Assert.Less(player.GetComponent<PlayerMotor>().CurrentSpeed, 0.05f);

            Object.Destroy(player);
            Object.Destroy(floor);
        }

        [UnityTest]
        public IEnumerator HealthSystem_Burns_Armour_Before_Health()
        {
            GameObject go = new GameObject("HealthActor");
            HealthSystem hs = go.AddComponent<HealthSystem>();
            hs.Configure(100f, 100f, 100f, 50f);
            hs.ApplyDamage(new DamageInfo(35f));
            yield return null;

            Assert.AreEqual(15f, hs.CurrentArmour, 0.01f);
            Assert.AreEqual(100f, hs.CurrentHealth, 0.01f);

            hs.ApplyDamage(new DamageInfo(40f));
            Assert.AreEqual(0f, hs.CurrentArmour, 0.01f);
            Assert.AreEqual(75f, hs.CurrentHealth, 0.01f);
            Object.Destroy(go);
        }

        [UnityTest]
        public IEnumerator Built_Test_Scene_Plays()
        {
            SceneManager.LoadScene("Test");
            yield return new WaitForSeconds(0.5f);

            PlayerMotor motor = Object.FindFirstObjectByType<PlayerMotor>();
            Assert.IsNotNull(motor);
            Assert.Less(motor.transform.position.y, 1.5f);
            Assert.IsNotNull(Camera.main);
            Assert.IsNotNull(GameObject.Find("VirtualCamera"));
            Assert.IsNotNull(GameObject.Find("HUD Canvas"));

            Transform[] transforms = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
            int npcCount = 0;
            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i] != null && transforms[i].name.StartsWith("NPC_"))
                    npcCount++;
            }
            Assert.AreEqual(8, npcCount);
        }

        [UnityTest, Explicit("PhysX Simulate investigation tracked under bisect scenes (§4.6 & Landmine #22)")]
        public IEnumerator Built_TestSlice_Scene_Plays()
        {
            SceneManager.LoadScene("TestSlice");
            yield return new WaitForSeconds(0.5f);
            Assert.IsNotNull(Object.FindFirstObjectByType<DamageDummy>());
        }

        private static GameObject CreateTestPlayer(Vector3 pos, out FakeTestInput input)
        {
            GameObject go = new GameObject("SmokeTestPlayer");
            go.transform.position = pos;
            go.AddComponent<CharacterController>();
            input = go.AddComponent<FakeTestInput>();
            PlayerMotor motor = go.AddComponent<PlayerMotor>();
            FieldInfo f = typeof(PlayerMotor).GetField("settings", BindingFlags.Instance | BindingFlags.NonPublic);
            f?.SetValue(motor, ScriptableObject.CreateInstance<MovementSettings>());
            return go;
        }
    }
}
