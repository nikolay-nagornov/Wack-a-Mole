using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class CoinsCollectManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody animatedCoinPrefab;
    [SerializeField] private bool uiTarget;
    [SerializeField] private Camera uiCamera;
    [SerializeField] private Transform target;

    [Header("Available coins : (coins to pool)")]
    [SerializeField] private int maxCoins;
    private Queue<Rigidbody> coinsQueue = new Queue<Rigidbody>();

    [Header("Animation settings")]
    [SerializeField] private float explosionForce = 1f;
    [SerializeField] private float minAnimDuration = 1f;
    [SerializeField] private float maxAnimDuration = 3f;

    [SerializeField] Ease easeType;
    [SerializeField] float spread;

    private Vector3 targetPosition;

    public static CoinsCollectManager instance;
    private LevelManager levelMng;
    private AudioManager audiMng;

    private void Awake()
    {
        if (instance != null)
            Destroy(this);
        else
            instance = this;
        
        if(uiTarget)
        {
            Vector3 pos = target.position;
            pos.z = uiCamera.nearClipPlane + 5f;
            targetPosition = uiCamera.ScreenToWorldPoint(pos);
        }
        else
            targetPosition = target.position;

        //prepare pool
        PrepareCoins();
    }

    private void Start()
    {
        levelMng = LevelManager.instance;
        audiMng = AudioManager.instance;
    }

    private void PrepareCoins()
    {
        Rigidbody coin;
        for (int i = 0; i < maxCoins; i++)
        {
            coin = Instantiate(animatedCoinPrefab, transform);
            coin.gameObject.SetActive(false);
            coinsQueue.Enqueue(coin);
        }
    }

    private void Animate(Vector3 collectedCoinPosition, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            //check if there's coins in the pool
            if (coinsQueue.Count > 0)
            {
                //extract a coin from the pool
                Rigidbody coin = coinsQueue.Dequeue();
                
                coin.isKinematic = false;
                coin.GetComponent<Collider>().isTrigger = false;
                coin.gameObject.SetActive(true);

                //move coin to the collected coin pos
                coin.MovePosition(collectedCoinPosition + (collectedCoinPosition * Random.Range(-spread, spread)));

                // explode coins
                coin.AddForce(Vector3.up * explosionForce, ForceMode.Acceleration);
                coin.DORotate(new Vector3(0, 360f, 0), 1f).SetLoops(-1);


                //animate coin to target position
                float duration = Random.Range(minAnimDuration, maxAnimDuration);

                coin.DOMove(targetPosition, duration)
                .SetEase(easeType)
                .SetUpdate(true)
                .SetDelay(1.1f)
                .OnStart(() =>
                {
                    coin.isKinematic = true;
                    coin.GetComponent<Collider>().isTrigger = true;
                })
                .OnComplete(() => {
                    //executes whenever coin reach target position
                    coin.gameObject.SetActive(false);
                    coinsQueue.Enqueue(coin);

                    levelMng.AddCoins(1);
                });
            }
        }
    }

    public void AddCoins(Vector3 collectedCoinPosition, int amount)
    {
        Animate(collectedCoinPosition, amount);
        audiMng.PlaySound("drop_coin", true);
    }
}