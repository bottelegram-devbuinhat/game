using DudeChaos.City;
using DudeChaos.Player;
using DudeChaos.Progression;
using DudeChaos.Wanted;
using UnityEngine;

namespace DudeChaos.GameFlow
{
    public enum StoryEnding
    {
        None = 0,
        Clean = 1,
        King = 2,
        Ash = 3
    }

    // §2.7 Three endings chosen by what the player actually did:
    // - Ash: 20 missions done, destroyed >= 20 civilian vehicles
    // - King: 20 missions done, owns >= 3 armoured vehicles
    // - Clean: 20 missions done, final wanted level < 3
    public sealed class GameFlowController : MonoBehaviour
    {
        [SerializeField] private Transform defaultSpawnPoint;
        [SerializeField] private HotelService hotelService;
        [SerializeField] private HealthSystem playerHealth;
        [SerializeField] private WantedSystem wantedSystem;

        private int _destroyedCivilianVehicles;

        public void NotifyCivilianVehicleDestroyed() => _destroyedCivilianVehicles++;

        public static StoryEnding ResolveEnding(
            int completedMissions,
            int finalWantedLevel,
            int ownedArmouredVehicles,
            int destroyedCivilianVehicles)
        {
            if (completedMissions < 20)
                return StoryEnding.None;
            if (destroyedCivilianVehicles >= 20)
                return StoryEnding.Ash;
            if (ownedArmouredVehicles >= 3)
                return StoryEnding.King;
            if (finalWantedLevel < 3)
                return StoryEnding.Clean;
            return StoryEnding.Ash;
        }

        public Vector3 GetRespawnPosition()
        {
            if (hotelService != null && hotelService.HasActiveRental)
                return hotelService.BedRespawnPosition;
            return defaultSpawnPoint != null ? defaultSpawnPoint.position : Vector3.zero;
        }

        public void RespawnPlayer(Transform playerTransform)
        {
            if (playerTransform == null) return;
            CharacterController cc = playerTransform.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            playerTransform.position = GetRespawnPosition();
            if (cc != null) cc.enabled = true;
            playerHealth?.Revive(1.0f, 0f);
            wantedSystem?.Clear();
        }
    }
}
