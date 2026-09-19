namespace DOAN.LegacyCombat {
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    private void Awake()
    {
        GameObject container = UIHelper.CreatePanel(transform, "Background", new Color(0.02f, 0.04f, 0.06f, 0.95f));

        Text titleText = UIHelper.CreateText(container.transform, "Title", "PAUSED", 60, Color.white);
        titleText.fontStyle = FontStyle.Bold;
        RectTransform titleRt = titleText.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.5f, 0.7f);
        titleRt.anchorMax = new Vector2(0.5f, 0.7f);

        Button resumeBtn = UIHelper.CreateButton(container.transform, "ResumeButton", "▶ RESUME", () => {
            GameUIManager.Instance.ResumeGame();
        });
        RectTransform resRt = resumeBtn.GetComponent<RectTransform>();
        resRt.anchorMin = new Vector2(0.5f, 0.5f); resRt.anchorMax = new Vector2(0.5f, 0.5f);
        resRt.anchoredPosition = Vector2.zero;

        Button restartBtn = UIHelper.CreateButton(container.transform, "RestartButton", "↻ RESTART", () => {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        });
        RectTransform restRt = restartBtn.GetComponent<RectTransform>();
        restRt.anchorMin = new Vector2(0.5f, 0.4f); restRt.anchorMax = new Vector2(0.5f, 0.4f);
        restRt.anchoredPosition = Vector2.zero;

        Button quitBtn = UIHelper.CreateButton(container.transform, "QuitButton", "✕ QUIT", () => {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        });
        RectTransform quitRt = quitBtn.GetComponent<RectTransform>();
        quitRt.anchorMin = new Vector2(0.5f, 0.3f); quitRt.anchorMax = new Vector2(0.5f, 0.3f);
        quitRt.anchoredPosition = Vector2.zero;
    }

    public void Show() { gameObject.SetActive(true); }
    public void Hide() { gameObject.SetActive(false); }
}

}