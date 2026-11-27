using DudeChaos.Core;
using DudeChaos.Save;
using UnityEngine;

namespace DudeChaos.UI
{
    public sealed class PauseMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject pausePanel;
        private SaveSystem _saveSystem;

        public void BindSaveSystem(SaveSystem saveSystem) => _saveSystem = saveSystem;

        public void TogglePause()
        {
            bool next = GameManager.Instance == null || !GameManager.Instance.IsPaused;
            GameManager.Instance?.SetPaused(next);
            if (pausePanel != null) pausePanel.SetActive(next);
        }

        public void Resume()
        {
            GameManager.Instance?.SetPaused(false);
            if (pausePanel != null) pausePanel.SetActive(false);
        }

        public void QuickSaveSlotZero(SaveData snapshot)
        {
            _saveSystem?.Save(0, snapshot);
        }
    }
}
