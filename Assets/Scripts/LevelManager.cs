using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [Header("Spawning")]
    [SerializeField] private int startHolesCount = 9;
    [SerializeField] private float spawningDist = 2f;
    [SerializeField] private Transform[] spawningPoints;
    private int curSpawningPointIndex = 0;
    [SerializeField] private Transform holePrefab;
    [SerializeField] private Transform holesParent;
    private List<Enemy> enemies = new List<Enemy>();
    private int enemiesCount;
    private int spawningRound = 0;
    private Vector3 lastSpawningPos = Vector3.zero;
    private Vector3 spawningDir = Vector3.forward;

    /* Player stats */
    private int health = 3;
    public int Health
    {
        get
        {
            return health;
        }
        set
        {
            if (health < 0)
                health = 0;
            else
                health = value;
        }
    }
    private int score = 0;
    public int Score
    {
        get
        {
            return score;
        }
        private set
        {
            if (value < 0)
                score = 0;
            else score = value;
        }
    }

    /* Level state */
    public bool IsPaused { get; private set; }

    private int difficulty = 1;
    [SerializeField] private float moleMovingDurationDecreaseValue = 0.1f;
    [SerializeField] private float moleDisplayTimeDesreaseValue = 0.05f;
    [SerializeField] private float moleStartDelayDecreaseValue = 0.1f;

    [Header("Effects")]
    [SerializeField] private ParticleSystem hurtEffect;

    private AudioManager audioMng;

    public delegate void OnChangedStats();
    public OnChangedStats onChangedStatsCallback;
    public static LevelManager instance;

    private void Awake()
    {
        if (instance != null)
            Destroy(this);
        else
            instance = this;

        // move the camera to next spawning point and save current index
        curSpawningPointIndex = PlayerPrefs.GetInt("SpawningPointIndex");
        curSpawningPointIndex++;
        if (curSpawningPointIndex == spawningPoints.Length)
            curSpawningPointIndex = 0;
        Vector3 camOffset = Camera.main.transform.position - spawningPoints[0].position;
        Camera.main.transform.position = spawningPoints[curSpawningPointIndex].position + camOffset;
        PlayerPrefs.SetInt("SpawningPointIndex", curSpawningPointIndex);

        Application.targetFrameRate = 60;
    }

    void Start()
    {
        audioMng = AudioManager.instance;

        SpawnHole(startHolesCount);
        StartCoroutine(DifficultyUp());
    }

    public void AddCoins(int count)
    {
        Score += count;
        onChangedStatsCallback.Invoke();
    }

    public void RemoveCoins(int count)
    {
        Score -= count;
        onChangedStatsCallback.Invoke();
    }

    public void Hurt(Vector3 pos)
    {
        Health--;

        hurtEffect.transform.position = pos;
        hurtEffect.Play();
        audioMng.PlaySound("player_hurt", true);
        Handheld.Vibrate();

        onChangedStatsCallback.Invoke();

        if (Health == 0)
            GameOver();
    }

    private IEnumerator DifficultyUp()
    {
        float timer = 0;

        while (true)
        {
            if (difficulty >= 20)
                yield break;

            while (timer < 20f)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            difficulty++;

            if (difficulty > 4 && difficulty % 4 == 0)
                SpawnHole(1);

            for (int i = 0; i < enemiesCount; i++)
            {
                if (difficulty > 2 && difficulty % 2 == 0)
                    enemies[i].ChanceArmorActivate--;

                enemies[i].MovingDuration -= moleMovingDurationDecreaseValue;
                enemies[i].DisplayTime -= moleDisplayTimeDesreaseValue;
                enemies[i].StartDelay -= moleStartDelayDecreaseValue;
                yield return null;
            }

            timer = 0;
        }
    }

    private void GameOver()
    {
        SetPause(true);
    }

    public void RestartLevel()
    {
        //SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        for (int i = 0; i < enemies.Count; i++)
            enemies[i].Restart();

        Health = 3;
        Score = 0;
        onChangedStatsCallback.Invoke();
        SetPause(false);
    }

    public void SaveData()
    {
        PlayerPrefs.SetString("Highscore", "json");
    }

    public void SetPause(bool pause)
    {
        if (pause)
            Time.timeScale = 0;
        else
            Time.timeScale = 1f;
    }

    private void SpawnHole(int count)
    {
        int i = 0;
        Transform curPrefab;
        Vector3 minPos = new Vector3(spawningRound * -spawningDist, 0, spawningRound * -spawningDist);
        Vector3 maxPos = new Vector3(spawningRound * spawningDist, 0, spawningRound * spawningDist);

        while (i < count)
        {
            i++;
            enemies.Add(Instantiate(holePrefab, spawningPoints[curSpawningPointIndex].position + lastSpawningPos, Quaternion.identity, holesParent).GetComponentInChildren<Enemy>());

            if (lastSpawningPos == minPos)
            {
                spawningRound++;
                minPos = new Vector3(spawningRound * -spawningDist, 0, spawningRound * -spawningDist);
                maxPos = new Vector3(spawningRound * spawningDist, 0, spawningRound * spawningDist);
                lastSpawningPos = minPos;
            }

            if (spawningDir.x < 0)
            {
                if (lastSpawningPos.x != minPos.x)
                    lastSpawningPos.x -= spawningDist;
                else
                {
                    if (lastSpawningPos.z != minPos.z)
                    {
                        spawningDir = -Vector3.forward;
                        lastSpawningPos.z -= spawningDist;
                    }
                    else
                    {
                        spawningDir = Vector3.forward;
                        lastSpawningPos.z += spawningDist;
                    }
                }
                continue;
            }
            if (spawningDir.x > 0)
            {
                if (lastSpawningPos.x != maxPos.x)
                    lastSpawningPos.x += spawningDist;
                else
                {
                    if (lastSpawningPos.z != minPos.z)
                    {
                        spawningDir = -Vector3.forward;
                        lastSpawningPos.z -= spawningDist;
                    }
                    else
                    {
                        spawningDir = Vector3.forward;
                        lastSpawningPos.z += spawningDist;
                    }
                }
                continue;
            }

            if (spawningDir.z < 0)
            {
                if (lastSpawningPos.z != minPos.z)
                    lastSpawningPos.z -= spawningDist;
                else
                {
                    if (lastSpawningPos.x != minPos.x)
                    {
                        spawningDir = -Vector3.right;
                        lastSpawningPos.x -= spawningDist;
                    }
                    else
                    {
                        spawningDir = Vector3.right;
                        lastSpawningPos.x += spawningDist;
                    }
                }
                continue;
            }
            if (spawningDir.z > 0)
            {
                if (lastSpawningPos.z != maxPos.z)
                    lastSpawningPos.z += spawningDist;
                else
                {
                    if (lastSpawningPos.x != minPos.x)
                    {
                        spawningDir = -Vector3.right;
                        lastSpawningPos.x -= spawningDist;
                    }
                    else
                    {
                        spawningDir = Vector3.right;
                        lastSpawningPos.x += spawningDist;
                    }
                }
                continue;
            }
        }
        enemiesCount = enemies.Count;
    }
}
