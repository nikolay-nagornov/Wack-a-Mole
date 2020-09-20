using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighscoreTable : MonoBehaviour
{
    public Highscores highscores { get; private set; }

    public static HighscoreTable instance;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(instance);

        //Debug.Log(PlayerPrefs.GetString("highscoreData"));

        string json = PlayerPrefs.GetString("highscoreData");
        highscores = JsonUtility.FromJson<Highscores>(json);

        if (highscores == null || highscores.data == null || highscores.data.Count == 0)
        {
            highscores = new Highscores();
            highscores.data = new List<HighscoreData>()
            {
                new HighscoreData{ score = 900, name = "Player_" + Random.Range(100, 999).ToString() },
                new HighscoreData{ score = 500, name = "Player_" + Random.Range(100, 999).ToString() },
                new HighscoreData{ score = 200, name = "Player_" + Random.Range(100, 999).ToString() }
            };

            SaveData();
            Debug.Log(PlayerPrefs.GetString("highscoreData"));
        }
    }

    public void AddData(int score, string name)
    {
        for (int i = 0; i < highscores.data.Count; i++)
        {
            if (score > highscores.data[i].score)
            {
                //load data
                string json = PlayerPrefs.GetString("highscoreData");
                highscores = JsonUtility.FromJson<Highscores>(json);

                // add data
                highscores.data.Insert(i, new HighscoreData { score = score, name = name });
                highscores.data.RemoveAt(highscores.data.Count - 1);

                SaveData();
                return;
            }
        }
    }

    private void SaveData()
    {
        string json = JsonUtility.ToJson(highscores);
        PlayerPrefs.SetString("highscoreData", json);
        PlayerPrefs.Save();
    }

    public bool CompareScore(int score)
    {
        //load data
        string json = PlayerPrefs.GetString("highscoreData");
        highscores = JsonUtility.FromJson<Highscores>(json);

        for (int i = 0; i < highscores.data.Count; i++)
        {
            if (score > highscores.data[i].score)
                return true;
        }
        return false;
    }

    public class Highscores
    {
        public List<HighscoreData> data;
    }

    [System.Serializable]
    public class HighscoreData
    {
        public int score;
        public string name;
    }

    private class ComparerByScore : IComparer<HighscoreData>
    {
        public int Compare(HighscoreData item1, HighscoreData item2)
        {
            return item1.score.CompareTo(item2.score);
        }
    }
}
