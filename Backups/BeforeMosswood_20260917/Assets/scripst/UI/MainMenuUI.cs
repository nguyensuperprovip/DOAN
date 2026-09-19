using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    private GameObject container;
    private Text titleText;

    private void Awake()
    {
        BuildUI();
    }

    private void BuildUI()
    {
        container = UIHelper.CreatePanel(transform, "Background", new Color(0.02f, 0.04f, 0.06f, 0.9f));

        // Tiêu đề
        titleText = UIHelper.CreateText(container.transform, "Title", "WARZONE ASSAULT", 72, new ColorUtility().TryParseHtmlString("#33E5FF", out Color cyan) ? cyan : Color.cyan);
        RectTransform titleRt = titleText.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.5f, 0.7f);
        titleRt.anchorMax = new Vector2(0.5f, 0.7f);
        titleText.fontStyle = FontStyle.Bold;

        // Phụ đề
        Text subtitleText = UIHelper.CreateText(container.transform, "Subtitle", "THIRD-PERSON SHOOTER", 24, Color.white);
        RectTransform subRt = subtitleText.GetComponent<RectTransform>();
        subRt.anchorMin = new Vector2(0.5f, 0.6f);
        subRt.anchorMax = new Vector2(0.5f, 0.6f);

        // Nút Play
        Button playBtn = UIHelper.CreateButton(container.transform, "PlayButton", "▶ PLAY", () => {
            GameUIManager.Instance.StartGame();
        });
        playBtn.GetComponent<Image>().color = new Color(0.2f, 0.9f, 1f, 0.8f);
        RectTransform playRt = playBtn.GetComponent<RectTransform>();
        playRt.anchorMin = new Vector2(0.5f, 0.4f);
        playRt.anchorMax = new Vector2(0.5f, 0.4f);
        playRt.anchoredPosition = Vector2.zero;

        // Nút Quit
        Button quitBtn = UIHelper.CreateButton(container.transform, "QuitButton", "✕ QUIT", () => {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        });
        RectTransform quitRt = quitBtn.GetComponent<RectTransform>();
        quitRt.anchorMin = new Vector2(0.5f, 0.3f);
        quitRt.anchorMax = new Vector2(0.5f, 0.3f);
        quitRt.anchoredPosition = Vector2.zero;
    }

    private void Update()
    {
        if (titleText != null && container.activeSelf)
        {
            float scale = 1f + Mathf.Sin(Time.unscaledTime * 2f) * 0.05f;
            titleText.transform.localScale = new Vector3(scale, scale, 1f);
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
