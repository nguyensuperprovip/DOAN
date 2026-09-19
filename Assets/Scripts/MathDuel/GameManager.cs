using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace MathDuel
{
    /// <summary>
    /// Central controller for the game scene.
    /// Manages two <see cref="PlayerPanel"/>s, pause/resume, and win detection.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Players")]
        public PlayerPanel player1;
        public PlayerPanel player2;

        [Header("Pause UI")]
        public GameObject pausePanel;
        public Button pauseButton;
        public Button resumeButton;
        public Button restartButton;

        [Header("Win UI")]
        public GameObject winPanel;
        public TMP_Text winText;

        // ────────────────────────────────────────────────────────────────
        private void Start()
        {
            // Ensure time is running
            Time.timeScale = 1f;

            // Hide overlays
            if (pausePanel != null) pausePanel.SetActive(false);
            if (winPanel != null) winPanel.SetActive(false);

            // Wire buttons
            if (pauseButton != null)  pauseButton.onClick.AddListener(Pause);
            if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
            if (restartButton != null) restartButton.onClick.AddListener(Restart);

            // Subscribe to scoring
            if (player1 != null) player1.OnPlayerScored += OnPlayerScored;
            if (player2 != null) player2.OnPlayerScored += OnPlayerScored;

            // Start the duel
            StartGame();
        }

        private void OnDestroy()
        {
            if (player1 != null) player1.OnPlayerScored -= OnPlayerScored;
            if (player2 != null) player2.OnPlayerScored -= OnPlayerScored;
        }

        // ────────────────────────────────────────────────────────────────

        private void StartGame()
        {
            player1.ResetScore();
            player2.ResetScore();
            player1.LoadNewQuestion();
            player2.LoadNewQuestion();
        }

        private void OnPlayerScored(PlayerPanel player)
        {
            if (player.Score >= GameSettings.TargetScore)
            {
                string winner = (player == player1) ? "Người chơi 1" : "Người chơi 2";
                ShowWin(winner);
            }
        }

        // ── Pause / Resume / Restart ───────────────────────────────────

        private void Pause()
        {
            Time.timeScale = 0f;
            if (pausePanel != null) pausePanel.SetActive(true);
        }

        private void Resume()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            Time.timeScale = 1f;
        }

        private void Restart()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        // ── Win ────────────────────────────────────────────────────────

        private void ShowWin(string winnerName)
        {
            Time.timeScale = 0f;
            if (winPanel != null) winPanel.SetActive(true);
            if (winText != null) winText.text = $"{winnerName}\nTHẮNG!";
        }
    }
}
