using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text scoreText;

    [Header("Popup windows")]
    [SerializeField] private Button closeBtn;
    [SerializeField] private RectTransform resultPanel;
    [SerializeField] private RectTransform highscoresPanel;
    [SerializeField] private RectTransform newRecordPanel;
    private RectTransform curOpenedPanel;
    [SerializeField] private TMP_Text scoreResultText;
    [SerializeField] private Image darkBG;

    [Header("Highscores")]
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private RectTransform highscoreItemPrefab;
    [SerializeField] private RectTransform highscoreItemsParent;

    [Header("Collecting animation")]
    [SerializeField] private Vector3 animScaleVector;
    [SerializeField] private float animDuration = 1f;
    [SerializeField] private int animVibrato = 10;
    [SerializeField] private float animElasticity = 1f;
    [SerializeField] private Ease easyType;
    [SerializeField] private RectTransform healthPanel;
    [SerializeField] private RectTransform scorePanel;

    private LevelManager levelMng;
    private HighscoreTable highscoreTable;
    private AudioManager audioMng;

    void Start()
    {
        levelMng = LevelManager.instance;
        highscoreTable = HighscoreTable.instance;
        audioMng = AudioManager.instance;

        levelMng.onChangedStatsCallback += UpdateStatsUI;
        UpdateStatsUI();
        UpdateHighscoreUI();
    }

    void UpdateStatsUI()
    {
        if(healthText.text != levelMng.Health.ToString())
        {
            healthText.text = levelMng.Health.ToString();
            healthPanel.DORewind();
            healthPanel.DOPunchScale(animScaleVector, animDuration, animVibrato, animElasticity)
                .SetEase(easyType);
        }
        if(scoreText.text != levelMng.Score.ToString())
        {
            scoreText.text = levelMng.Score.ToString();
            scorePanel.DORewind();
            scorePanel.DOPunchScale(animScaleVector, animDuration, animVibrato, animElasticity).
                SetEase(easyType);

            audioMng.PlaySound("add_coin", true);
        }
        
        if(levelMng.Health == 0)
        {
            if (highscoreTable.CompareScore(levelMng.Score))
                DisplayNewRecordPanel();
            else
                DisplayResultPanel();
        }
    }

    public void DisplayResultPanel()
    {
        CloseCurrentPanel();
        levelMng.SetPause(true);
        darkBG.enabled = true;
        scoreResultText.text = levelMng.Score.ToString();
        resultPanel.gameObject.SetActive(true);
        closeBtn.gameObject.SetActive(true);
        closeBtn.onClick.RemoveAllListeners();
        if (levelMng.Health == 0)
            closeBtn.onClick.AddListener(() => levelMng.RestartLevel());
        else
            closeBtn.onClick.AddListener(() => CloseCurrentPanel());

        curOpenedPanel = resultPanel;
    }

    public void DisplayHighscoresPanel()
    {
        CloseCurrentPanel();
        levelMng.SetPause(true);
        darkBG.enabled = true;
        highscoresPanel.gameObject.SetActive(true);
        closeBtn.gameObject.SetActive(true);
        closeBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.AddListener(() => DisplayResultPanel());
        curOpenedPanel = highscoresPanel;
    }

    public void DisplayNewRecordPanel()
    {
        darkBG.enabled = true;
        newRecordPanel.gameObject.SetActive(true);
        curOpenedPanel = newRecordPanel;
        audioMng.PlaySound("new_record");
    }

    public void CloseCurrentPanel()
    {
        if (curOpenedPanel == null)
            return;
        curOpenedPanel.gameObject.SetActive(false);
        closeBtn.gameObject.SetActive(false);
        darkBG.enabled = false;
        curOpenedPanel = null;
        levelMng.SetPause(false);
    }

    public void RestartLevel()
    {
        CloseCurrentPanel();
        levelMng.RestartLevel();
    }

    public void AddHighscore()
    {
        highscoreTable.AddData(levelMng.Score, nameInput.text);
        UpdateHighscoreUI();
        DisplayHighscoresPanel();
    }

    public void UpdateHighscoreUI()
    {
        RectTransform newItem;

        for (int i = 0; i < highscoreItemsParent.childCount; i++)
            Destroy(highscoreItemsParent.GetChild(i).gameObject);

        for (int i = 0; i < highscoreTable.highscores.data.Count; i++)
        {
            newItem = Instantiate(highscoreItemPrefab, highscoreItemsParent);
            newItem.GetChild(0).GetComponentInChildren<TMP_Text>().text = (i + 1).ToString();
            newItem.GetChild(1).GetComponent<TMP_Text>().text = highscoreTable.highscores.data[i].score.ToString();
            newItem.GetChild(2).GetComponent<TMP_Text>().text = highscoreTable.highscores.data[i].name;
        }
    }
}
