using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    private Text titleText;
    private Text statsText;

    private void Awake()
    {
        GameObject container = UIHelper.CreatePanel(transform, "Background", new Color(0.02f, 0.02f, 0.02f, 0.98f));

        titleText = UIHelper.CreateText(container.transform, "Title", "MISSION FAILED", 70, Color.red);
        titleText.fontStyle = FontStyle.Bold;
        RectTransform titleRt = titleText.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.5f, 0.75f);
        titleRt.anchorMax = new Vector2(0.5f, 0.75f);

        statsText = UIHelper.CreateText(container.transform, "Stats", "KILLS: 0\nTIME: 00:00", 30, Color.white);
        RectTransform statsRt = statsText.GetComponent<RectTransform>();
        statsRt.anchorMin = new Vector2(0.5f, 0.6f);
        statsRt.anchorMax = new Vector2(0.5f, 0.6f);

        Button playAgainBtn = UIHelper.CreateButton(container.transform, "PlayAgainBtn", "↻ PLAY AGAIN", () => {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        });
        RectTransform paRt = playAgainBtn.GetComponent<RectTransform>();
        paRt.anchorMin = new Vector2(0.5f, 0.4f); paRt.anchorMax = new Vector2(0.5f, 0.4f);
        paRt.anchoredPosition = Vector2.zero;

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

    public void Show(bool won, int kills, float timeSurvived)
    {
        gameObject.SetActive(true);
        
        if (won)
        {
            titleText.text = "MISSION COMPLETE";
            titleText.color = new ColorUtility().TryParseHtmlString("#33E5FF", out Color cyan) ? cyan : Color.cyan;
        }
        else
        {
            titleText.text = "MISSION FAILED";
            titleText.color = Color.red;
        }

        int mins = Mathf.FloorToInt(timeSurvived / 60F);
        int secs = Mathf.FloorToInt(timeSurvived - mins * 60);
        string timeStr = string.Format("{0:00}:{1:00}", mins, secs);

        statsText.text = $"KILLS: {kills}\nTIME SURVIVED: {timeStr}";
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
