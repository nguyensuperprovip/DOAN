namespace DOAN.LegacyCombat {
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Lớp hỗ trợ tạo UI nhanh bằng code (không dùng prefab)
public static class UIHelper
{
    public static Font GetDefaultFont()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return font;
    }

    public static GameObject CreatePanel(Transform parent, string name, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        go.AddComponent<CanvasRenderer>();
        Image img = go.AddComponent<Image>();
        img.color = color;
        return go;
    }

    public static Text CreateText(Transform parent, string name, string textContent, int fontSize, Color color, TextAnchor alignment = TextAnchor.MiddleCenter)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        go.AddComponent<CanvasRenderer>();
        Text txt = go.AddComponent<Text>();
        txt.font = GetDefaultFont();
        txt.text = textContent;
        txt.fontSize = fontSize;
        txt.color = color;
        txt.alignment = alignment;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        txt.verticalOverflow = VerticalWrapMode.Overflow;
        return txt;
    }

    public static Button CreateButton(Transform parent, string name, string textContent, UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(250, 60);
        go.AddComponent<CanvasRenderer>();
        Image img = go.AddComponent<Image>();
        img.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        
        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);

        Text txt = CreateText(go.transform, "Text", textContent, 24, Color.white);
        return btn;
    }
}

public enum GameState { MainMenu, Playing, Paused, GameOver }

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }

    public GameState CurrentState { get; private set; }
    public int KillCount { get; private set; }

    private MainMenuUI mainMenuUI;
    private GameHUD gameHUD;
    private PauseMenuUI pauseMenuUI;
    private GameOverUI gameOverUI;

    private float gameStartTime;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupUI();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SetupUI()
    {
        // Khởi tạo Canvas
        GameObject canvasGo = new GameObject("GameCanvas");
        canvasGo.transform.SetParent(transform);
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasGo.AddComponent<GraphicRaycaster>();

        // Khởi tạo EventSystem
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.transform.SetParent(transform);
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<StandaloneInputModule>();
        }

        // Tạo các màn hình UI
        mainMenuUI = CreateUIComponent<MainMenuUI>(canvasGo.transform, "MainMenuUI");
        gameHUD = CreateUIComponent<GameHUD>(canvasGo.transform, "GameHUD");
        pauseMenuUI = CreateUIComponent<PauseMenuUI>(canvasGo.transform, "PauseMenuUI");
        gameOverUI = CreateUIComponent<GameOverUI>(canvasGo.transform, "GameOverUI");

        ShowMainMenu();
    }

    private T CreateUIComponent<T>(Transform parent, string name) where T : MonoBehaviour
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return go.AddComponent<T>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (CurrentState == GameState.Playing) PauseGame();
            else if (CurrentState == GameState.Paused) ResumeGame();
        }
    }

    public void ShowMainMenu()
    {
        CurrentState = GameState.MainMenu;
        Time.timeScale = 0f;
        SetCursorState(true);
        
        mainMenuUI.Show();
        gameHUD.Hide();
        pauseMenuUI.Hide();
        gameOverUI.Hide();
    }

    public void StartGame()
    {
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;
        KillCount = 0;
        gameStartTime = Time.time;
        SetCursorState(false);

        mainMenuUI.Hide();
        gameHUD.Show();
        pauseMenuUI.Hide();
        gameOverUI.Hide();
    }

    public void PauseGame()
    {
        if (CurrentState != GameState.Playing) return;
        CurrentState = GameState.Paused;
        Time.timeScale = 0f;
        SetCursorState(true);

        pauseMenuUI.Show();
    }

    public void ResumeGame()
    {
        if (CurrentState != GameState.Paused) return;
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;
        SetCursorState(false);

        pauseMenuUI.Hide();
    }

    public void ShowGameOver(bool won)
    {
        CurrentState = GameState.GameOver;
        Time.timeScale = 0f;
        SetCursorState(true);
        
        float timeSurvived = Time.time - gameStartTime;

        gameHUD.Hide();
        pauseMenuUI.Hide();
        gameOverUI.Show(won, KillCount, timeSurvived);
    }

    public void AddKill(string victimName)
    {
        KillCount++;
        if (gameHUD != null && gameHUD.gameObject.activeSelf)
        {
            gameHUD.AddKillMessage($"✦ Eliminated {victimName}");
        }
    }

    private void SetCursorState(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }
}

}