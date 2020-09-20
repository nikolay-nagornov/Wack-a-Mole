using System.Collections;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Rigidbody))]
public class Enemy : MonoBehaviour
{
    [SerializeField] private int defaultRewardAmount = 1;
    private int rewardAmount;

    private int chanceArmorActivate = 8;
    public int ChanceArmorActivate
    {
        get
        {
            return chanceArmorActivate;
        }
        set
        {
            if (value < 2)
                chanceArmorActivate = 2;
            else
                chanceArmorActivate = value;
        }
    }
    private float movingDuration = 1f;
    public float MovingDuration
    {
        get { return movingDuration; }
        set
        {
            if (value < 0.1f)
                movingDuration = 0.1f;
            else
                movingDuration = value;
        }
    }
    private float displayTime = 5f;
    public float DisplayTime
    {
        get { return displayTime; }
        set
        {
            if (value < 0.25f)
                displayTime = 0.25f;
            else
                displayTime = value;
        }
    }

    private float startDelay = 5f;
    public float StartDelay
    {
        get { return startDelay; }
        set
        {
            if (value < 0)
                startDelay = 0;
            else
                startDelay = value;
        }
    }
    public bool OnDisplay { get; private set; }

    [SerializeField] private ParticleSystem defeatEffect;
    [SerializeField] private EnemiesArmor[] armors = new EnemiesArmor[3];

    private Vector3 startPos;
    private int armorsCount;

    private Rigidbody rb;
    private Collider col;
    private LevelManager levelMng;
    private CoinsCollectManager collectMng;
    private AudioManager audioMng;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        levelMng = LevelManager.instance;
        collectMng = CoinsCollectManager.instance;
        audioMng = AudioManager.instance;

        startPos = transform.position;
        armorsCount = armors.Length;

        float startDelay = Random.Range(3f, 10f);
        Invoke("MovingUp", startDelay);
    }

    public void MovingUp()
    {
        OnDisplay = true;
        col.enabled = true;
        rewardAmount = defaultRewardAmount;

        ActivateRandomArmors();
            
        rb.DOMoveY(0, MovingDuration).OnComplete(() =>
        {
            if (col.enabled)
                MovingDown();
        });
    }

    public void MovingDown()
    {
        rb.DOMoveY(-1.55f, MovingDuration).OnComplete(() =>
        {
            OnDisplay = false;
            if (col.enabled)
            {
                levelMng.Hurt(rb.position + new Vector3(0, 2f, 0));
                col.enabled = false;
            }
            for (int i = 0; i < armorsCount; i++)
            {
                if (armors[i].IsActive)
                    armors[i].Deactivate();
            }
            float delay = Random.Range(StartDelay - 0.25f, StartDelay + 1f);
            Invoke("MovingUp", delay);
        }).SetDelay(DisplayTime);
    }

    public void Defeate()
    {
        defeatEffect.Play();
        audioMng.PlaySound("enemy_defeat", true);

        col.enabled = false;
        collectMng.AddCoins(rb.position + new Vector3(0, 2f, 0), rewardAmount);

        float delay = Random.Range(StartDelay - 0.25f, StartDelay + 1f);
        rb.DOKill();
        rb.DOMoveY(-1.55f, MovingDuration).OnComplete(() =>
        {
            OnDisplay = false;
            Invoke("MovingUp", delay);
        });
    }

    private void ActivateRandomArmors()
    {
        for (int i = 0; i < armorsCount; i++)
        {
            if (!armors[i].IsActive && Random.Range(0, ChanceArmorActivate) == 0)
                    armors[i].Init();
        }
    }

    public void Restart()
    {
        rb.DOKill();
        col.enabled = false;
        OnDisplay = false;
        rb.MovePosition(startPos);
        for (int i = 0; i < armors.Length; i++)
            armors[i].Defeate();

        float startDelay = Random.Range(3f, 10f);
        Invoke("MovingUp", startDelay);
    }
}
