namespace DOAN.LegacyCombat {
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameHUD : MonoBehaviour
{
    private PlayerHealth playerHealth;
    private PlayerGun playerGun;
    private PlayerController playerController;

    private Image hpFill;
    private Text hpText;
    private Image damageIndicator;
    private float damageFlashTimer;

    private Text ammoText;
    private Image reloadBar;
    private Image ammoBg;

    private Text killCounterText;
    private RectTransform crosshairContainer;
    private RectTransform[] crosshairLines;
    private Image hitmarker;
    private float hitmarkerTimer;

    private RectTransform killFeedContainer;
    private class KillMessage { public Text text; public float timeRemaining; }
    private List<KillMessage> killMessages = new List<KillMessage>();

    private float currentHpVelocity;
    private float targetHpFill;
    
    private Color cyanColor = new Color(0.2f, 0.9f, 1f, 1f);
    private Color darkBg = new Color(0.02f, 0.04f, 0.06f, 0.8f);

    private void Awake()
    {
        BuildUI();
    }

    private void BuildUI()
    {
        // 1. HP BAR
        GameObject hpPanel = UIHelper.CreatePanel(transform, "HPPanel", darkBg);
        RectTransform hpRt = hpPanel.GetComponent<RectTransform>();
        hpRt.anchorMin = new Vector2(0, 0);
        hpRt.anchorMax = new Vector2(0, 0);
        hpRt.sizeDelta = new Vector2(300, 60);
        hpRt.anchoredPosition = new Vector2(170, 50);

        hpText = UIHelper.CreateText(hpPanel.transform, "HPText", "100/100", 20, Color.white);
        hpText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        hpText.fontStyle = FontStyle.Bold;

        GameObject hpBg = UIHelper.CreatePanel(hpPanel.transform, "HPBg", new Color(0.1f, 0.1f, 0.1f));
        RectTransform hBgrt = hpBg.GetComponent<RectTransform>();
        hBgrt.anchorMin = new Vector2(0, 0); hBgrt.anchorMax = new Vector2(1, 0);
        hBgrt.sizeDelta = new Vector2(0, 10);
        hBgrt.anchoredPosition = new Vector2(0, -25);

        GameObject hpFillGo = UIHelper.CreatePanel(hpBg.transform, "HPFill", Color.green);
        hpFill = hpFillGo.GetComponent<Image>();
        hpFill.type = Image.Type.Filled;
        hpFill.fillMethod = Image.FillMethod.Horizontal;
        RectTransform fillRt = hpFill.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = Vector2.one;

        // 2. AMMO COUNTER
        GameObject ammoPanel = UIHelper.CreatePanel(transform, "AmmoPanel", darkBg);
        ammoBg = ammoPanel.GetComponent<Image>();
        RectTransform ammoRt = ammoPanel.GetComponent<RectTransform>();
        ammoRt.anchorMin = new Vector2(1, 0); ammoRt.anchorMax = new Vector2(1, 0);
        ammoRt.sizeDelta = new Vector2(200, 80);
        ammoRt.anchoredPosition = new Vector2(-120, 60);

        ammoText = UIHelper.CreateText(ammoPanel.transform, "AmmoText", "30 / 90", 32, cyanColor);
        ammoText.fontStyle = FontStyle.Bold;

        GameObject reloadBg = UIHelper.CreatePanel(ammoPanel.transform, "ReloadBarBg", new Color(0.1f, 0.1f, 0.1f));
        RectTransform rbBgRt = reloadBg.GetComponent<RectTransform>();
        rbBgRt.anchorMin = new Vector2(0, 0); rbBgRt.anchorMax = new Vector2(1, 0);
        rbBgRt.sizeDelta = new Vector2(0, 5); rbBgRt.anchoredPosition = new Vector2(0, -35);

        GameObject reloadFill = UIHelper.CreatePanel(reloadBg.transform, "ReloadBar", cyanColor);
        reloadBar = reloadFill.GetComponent<Image>();
        reloadBar.type = Image.Type.Filled;
        reloadBar.fillMethod = Image.FillMethod.Horizontal;
        reloadBar.fillAmount = 0;

        // 3. CROSSHAIR
        GameObject chContainerGo = new GameObject("Crosshair");
        chContainerGo.transform.SetParent(transform, false);
        crosshairContainer = chContainerGo.AddComponent<RectTransform>();
        crosshairContainer.anchorMin = new Vector2(0.5f, 0.5f);
        crosshairContainer.anchorMax = new Vector2(0.5f, 0.5f);

        crosshairLines = new RectTransform[4];
        for (int i = 0; i < 4; i++)
        {
            GameObject line = UIHelper.CreatePanel(crosshairContainer, "Line" + i, new Color(1f, 1f, 1f, 0.8f));
            RectTransform lrt = line.GetComponent<RectTransform>();
            lrt.sizeDelta = new Vector2(12, 2);
            crosshairLines[i] = lrt;
        }
        crosshairLines[0].localRotation = Quaternion.Euler(0, 0, 0);   // Right
        crosshairLines[1].localRotation = Quaternion.Euler(0, 0, 90);  // Top
        crosshairLines[2].localRotation = Quaternion.Euler(0, 0, 180); // Left
        crosshairLines[3].localRotation = Quaternion.Euler(0, 0, 270); // Bottom

        GameObject centerDot = UIHelper.CreatePanel(crosshairContainer, "Dot", cyanColor);
        centerDot.GetComponent<RectTransform>().sizeDelta = new Vector2(4, 4);

        GameObject hmGo = UIHelper.CreatePanel(crosshairContainer, "Hitmarker", Color.clear);
        hitmarker = hmGo.GetComponent<Image>();
        hitmarker.GetComponent<RectTransform>().sizeDelta = new Vector2(20, 20);
        hitmarker.sprite = null; // Ideally a cross sprite, but we just use color
        hitmarker.color = new Color(1, 0, 0, 0);

        // 4. KILL FEED
        GameObject kfGo = new GameObject("KillFeed");
        kfGo.transform.SetParent(transform, false);
        killFeedContainer = kfGo.AddComponent<RectTransform>();
        killFeedContainer.anchorMin = new Vector2(1, 1);
        killFeedContainer.anchorMax = new Vector2(1, 1);
        killFeedContainer.sizeDelta = new Vector2(300, 200);
        killFeedContainer.anchoredPosition = new Vector2(-160, -110);

        // 5. KILL COUNTER
        killCounterText = UIHelper.CreateText(transform, "KillCounter", "KILLS: 0", 28, Color.white);
        RectTransform kcRt = killCounterText.GetComponent<RectTransform>();
        kcRt.anchorMin = new Vector2(0.5f, 1f); kcRt.anchorMax = new Vector2(0.5f, 1f);
        kcRt.anchoredPosition = new Vector2(0, -40);

        // 6. DAMAGE INDICATOR
        GameObject dmgGo = UIHelper.CreatePanel(transform, "DamageIndicator", new Color(1, 0, 0, 0));
        damageIndicator = dmgGo.GetComponent<Image>();
        damageIndicator.raycastTarget = false;
    }

    private void Start()
    {
        playerHealth = FindObjectOfType<PlayerHealth>();
        playerGun = FindObjectOfType<PlayerGun>();
        playerController = FindObjectOfType<PlayerController>();

        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += HandleHealthChanged;
            playerHealth.OnPlayerDeath += HandleDeath;
        }
        if (playerGun != null)
        {
            playerGun.OnAmmoChanged += HandleAmmoChanged;
            playerGun.OnReloadStart += () => { if (reloadBar) reloadBar.fillAmount = 0; };
            playerGun.OnReloadEnd += () => { if (reloadBar) reloadBar.fillAmount = 0; };
            playerGun.OnFire += () => SetCrosshairSpread(1f);
        }
    }

    private void Update()
    {
        // Smooth HP lerp
        if (hpFill != null)
        {
            hpFill.fillAmount = Mathf.SmoothDamp(hpFill.fillAmount, targetHpFill, ref currentHpVelocity, 0.1f);
            hpFill.color = Color.Lerp(Color.red, Color.green, hpFill.fillAmount);
        }

        // Damage Flash
        if (damageFlashTimer > 0)
        {
            damageFlashTimer -= Time.deltaTime;
            damageIndicator.color = new Color(1, 0, 0, (damageFlashTimer / 0.5f) * 0.3f);
        }

        // Hitmarker Flash
        if (hitmarkerTimer > 0)
        {
            hitmarkerTimer -= Time.deltaTime;
            hitmarker.color = new Color(1, 0, 0, hitmarkerTimer / 0.2f);
        }

        // Crosshair Spread
        if (playerController != null && crosshairLines != null)
        {
            float spread = playerController.IsSprinting ? 40f : (playerController.IsMoving ? 20f : 5f);
            SetCrosshairSpread(Mathf.Lerp(crosshairLines[0].anchoredPosition.x, spread, Time.deltaTime * 10f) / 40f);
        }

        // Reload Bar logic (approximate since we don't have reload time in contract)
        if (playerGun != null && playerGun.IsReloading && reloadBar != null)
        {
            reloadBar.fillAmount = Mathf.Repeat(Time.time * 2f, 1f); // Fake progress
        }
        else if (reloadBar != null && reloadBar.fillAmount > 0)
        {
            reloadBar.fillAmount = 0;
        }

        // Kill Feed Update
        for (int i = killMessages.Count - 1; i >= 0; i--)
        {
            killMessages[i].timeRemaining -= Time.deltaTime;
            if (killMessages[i].timeRemaining <= 0)
            {
                Destroy(killMessages[i].text.gameObject);
                killMessages.RemoveAt(i);
            }
            else
            {
                Color c = killMessages[i].text.color;
                c.a = Mathf.Min(1f, killMessages[i].timeRemaining);
                killMessages[i].text.color = c;
            }
        }

        if (killCounterText != null && GameUIManager.Instance != null)
        {
            killCounterText.text = "KILLS: " + GameUIManager.Instance.KillCount;
        }
    }

    private void HandleHealthChanged(int current, int max)
    {
        hpText.text = current + "/" + max;
        targetHpFill = (float)current / max;
    }

    private void HandleDeath()
    {
        GameUIManager.Instance.ShowGameOver(false);
    }

    private void HandleAmmoChanged(int current, int total)
    {
        ammoText.text = current + " / " + total;
        if (current <= 5)
            ammoBg.color = new Color(0.3f, 0, 0, 0.8f);
        else
            ammoBg.color = darkBg;
    }

    public void ShowDamageIndicator()
    {
        damageFlashTimer = 0.5f;
    }

    public void ShowHitmarker()
    {
        hitmarkerTimer = 0.2f;
    }

    public void AddKillMessage(string msg)
    {
        if (killMessages.Count >= 4)
        {
            Destroy(killMessages[0].text.gameObject);
            killMessages.RemoveAt(0);
        }
        
        Text msgText = UIHelper.CreateText(killFeedContainer, "Msg", msg, 18, Color.white, TextAnchor.MiddleRight);
        msgText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -killMessages.Count * 25);
        killMessages.Add(new KillMessage { text = msgText, timeRemaining = 3f });
    }

    public void SetCrosshairSpread(float t)
    {
        float offset = Mathf.Lerp(5f, 40f, t);
        if (crosshairLines != null && crosshairLines.Length == 4)
        {
            crosshairLines[0].anchoredPosition = new Vector2(offset, 0);
            crosshairLines[1].anchoredPosition = new Vector2(0, offset);
            crosshairLines[2].anchoredPosition = new Vector2(-offset, 0);
            crosshairLines[3].anchoredPosition = new Vector2(0, -offset);
        }
    }

    public void Show() { gameObject.SetActive(true); }
    public void Hide() { gameObject.SetActive(false); }
}

}