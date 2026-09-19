namespace DOAN.LegacyCombat {
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BotSpawner : MonoBehaviour
{
    public int maxBots = 4;
    public float respawnDelay = 5f;
    public float difficultyIncreaseInterval = 60f;
    public int maxBotsLimit = 8;

    [SerializeField] private GameObject botPrefab;
    private List<Transform> spawnPoints = new List<Transform>();
    private List<GameObject> activeBots = new List<GameObject>();

    private void Awake()
    {
        GameObject[] sps = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (var sp in sps)
        {
            if (sp.name.StartsWith("SpawnPoint_"))
            {
                spawnPoints.Add(sp.transform);
            }
        }

        if (spawnPoints.Count == 0)
        {
            Vector3 basePos = new Vector3(490f, 50f, 890f);
            for (int i = 0; i < 4; i++)
            {
                GameObject sp = new GameObject($"SpawnPoint_{i}");
                float angle = i * Mathf.PI / 2f;
                sp.transform.position = basePos + new Vector3(Mathf.Cos(angle) * 30f, 0, Mathf.Sin(angle) * 30f);
                spawnPoints.Add(sp.transform);
            }
        }

        if (botPrefab == null)
        {
            GameObject existingBot = GameObject.Find("Bot_01");
            if (existingBot != null)
            {
                botPrefab = existingBot;
                botPrefab.SetActive(false); // keep it as template
            }
            else
            {
                // Fallback basic bot
                botPrefab = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                botPrefab.name = "Bot_Template";
                botPrefab.AddComponent<CharacterController>();
                botPrefab.SetActive(false);
            }
        }
    }

    private IEnumerator Start()
    {
        while (GameUIManager.Instance == null || GameUIManager.Instance.CurrentState != GameState.Playing)
        {
            yield return null;
        }

        for (int i = 0; i < maxBots; i++)
        {
            SpawnBot();
        }

        InvokeRepeating(nameof(IncreaseDifficulty), difficultyIncreaseInterval, difficultyIncreaseInterval);
    }

    private void SpawnBot()
    {
        Transform sp = spawnPoints[Random.Range(0, spawnPoints.Count)];
        GameObject bot = Instantiate(botPrefab, sp.position, sp.rotation);
        bot.SetActive(true);
        bot.name = $"Bot_{activeBots.Count + 1}";

        EnemyHealth hp = bot.GetComponent<EnemyHealth>();
        if (hp == null) hp = bot.AddComponent<EnemyHealth>();
        
        BotAI ai = bot.GetComponent<BotAI>();
        if (ai == null) ai = bot.AddComponent<BotAI>();

        hp.OnEnemyDeath += () => OnBotDeath(bot);
        activeBots.Add(bot);
    }

    private void OnBotDeath(GameObject bot)
    {
        StartCoroutine(RespawnRoutine(bot));
    }

    private IEnumerator RespawnRoutine(GameObject bot)
    {
        yield return new WaitForSeconds(respawnDelay);

        Transform sp = spawnPoints[Random.Range(0, spawnPoints.Count)];
        
        CharacterController cc = bot.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        
        bot.transform.position = sp.position;
        bot.transform.rotation = sp.rotation;

        Renderer[] renderers = bot.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers) r.enabled = true;
        
        Collider[] colliders = bot.GetComponentsInChildren<Collider>();
        foreach (var c in colliders) c.enabled = true;

        EnemyHealth hp = bot.GetComponent<EnemyHealth>();
        if (hp != null) hp.ResetHealth();
        
        BotAI ai = bot.GetComponent<BotAI>();
        if (ai != null) ai.CurrentState = BotState.Patrol;

        if (cc != null) cc.enabled = true;
    }

    private void IncreaseDifficulty()
    {
        if (maxBots < maxBotsLimit)
        {
            maxBots++;
            SpawnBot();
        }
    }
}

}