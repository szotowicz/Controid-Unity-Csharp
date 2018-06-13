using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.IO;
using UnityEngine.SceneManagement;

public class DiamondsCollisionAndGenerate : MonoBehaviour
{
    public Text PointsMonitor;
    public Text BestScoreMonitor;
    public int DiamondsOnStart = 15;
    public List<GameObject> DiamondsPrefabs;
    public int ObstaclesOnStart = 15;
    public List<GameObject> ObstaclePrefabs;
    public List<GameObject> BackgroundObjPrefabs;

    private string nameOfDiamond = "Diamond";
    private string nameOfObstacle = "ToDie";
    private string nameOfBackgroundObj = "BackgroundObj";
    private string bestScoreFileName = "/bestScore.txt";
    private int points;
    private float lastCreatedObjectPosstion = 0;
    private float lastCreatedBackgroundObjectPosstion = 0;
    private List<GameObject> createdObject = new List<GameObject>();
    private Dictionary<string, float> possiblePaths = new Dictionary<string, float>()
    {
        {"left", -2.0f },
        {"middle", 0.0f },
        {"right", 2.0f }
    };

    void Start()
    {
        points = 0;
        ShowPoints();

        if (!File.Exists(Application.persistentDataPath + bestScoreFileName))
        {
            CreateNewBestScoreFile();
        }

        ShowBestScore();
        while (DiamondsOnStart > 0 && ObstaclesOnStart > 0)
        {
            int objType = Random.Range(0, 2);

            if (objType == 0)
            {
                if (ObstaclePrefabs.Count > 0)
                {// TODO: cos nie dziala czasem, za nisko pozycja ?
                    CreateNewObject(ObstaclePrefabs);
                    ObstaclesOnStart--;
                }
            }
            else if (objType == 1)
            {
                if (DiamondsPrefabs.Count > 0)
                {
                    CreateNewObject(DiamondsPrefabs);
                    DiamondsOnStart--;
                }
            }
        }

        for (int i = 0; i < 15; i++)
        {
            CreateNewBackgroundObj();
        }
    }

    void Update()
    {
        float gameCharacterDistance = gameObject.transform.position.z;
  
        foreach (GameObject obj in createdObject)
        {
            if (gameCharacterDistance > obj.transform.position.z + 10.0f)
            {
                RemoveCreatedObject(obj);
                break;
            }            
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.name.Contains(nameOfDiamond))
        {
            points++;
            ShowPoints();
            RemoveCreatedObject(col.gameObject);
        }
        else if (col.gameObject.name.Contains(nameOfObstacle))
        {
            GameOver();
        }
    }

    private void ShowPoints()
    {
        PointsMonitor.text = points.ToString();
    }

    private void CreateNewBestScoreFile()
    {
        File.WriteAllText(Application.persistentDataPath + bestScoreFileName, "0");
    }

    private void ShowBestScore()
    {
        if (File.Exists(Application.persistentDataPath + bestScoreFileName))
        {
            string bestScore = File.ReadAllText(Application.persistentDataPath + bestScoreFileName);
            BestScoreMonitor.text = bestScore;
        }
    }
    
    private void CreateNewObject(List<GameObject> prefabsList)
    {
        int newModelIndex = Random.Range(0, prefabsList.Count);
        float newModelX = possiblePaths.ElementAt(Random.Range(0, possiblePaths.Count)).Value;
        float distanceForNewModel = Random.Range(5.0f, 12.0f);
        lastCreatedObjectPosstion += distanceForNewModel;

        GameObject newObjectModel = Instantiate(prefabsList[newModelIndex],
            new Vector3(newModelX, 0.0f, lastCreatedObjectPosstion), Quaternion.identity);

        createdObject.Add(newObjectModel);
    }

    private void CreateNewBackgroundObj()
    {
        int newModelIndex = Random.Range(0, BackgroundObjPrefabs.Count);

        GameObject newObjectModel = Instantiate(BackgroundObjPrefabs[newModelIndex],
            new Vector3(3.5f + Random.Range(0.0f, 2.0f), 0.0f, lastCreatedBackgroundObjectPosstion + Random.Range(2.0f, 5.0f)), Quaternion.identity);
        createdObject.Add(newObjectModel);

        newObjectModel = Instantiate(BackgroundObjPrefabs[newModelIndex],
            new Vector3(-3.5f - Random.Range(0.0f, 2.0f), 0.0f, lastCreatedBackgroundObjectPosstion + Random.Range(2.0f, 5.0f)), Quaternion.identity);
        createdObject.Add(newObjectModel);

        newObjectModel = Instantiate(BackgroundObjPrefabs[newModelIndex],
            new Vector3(6.0f + Random.Range(0.0f, 2.0f), 0.0f, lastCreatedBackgroundObjectPosstion + Random.Range(1.5f, 5.0f)), Quaternion.identity);
        createdObject.Add(newObjectModel);

        newObjectModel = Instantiate(BackgroundObjPrefabs[newModelIndex],
            new Vector3(-6.0f - Random.Range(0.0f, 2.0f), 0.0f, lastCreatedBackgroundObjectPosstion + Random.Range(1.5f, 5.0f)), Quaternion.identity);
        createdObject.Add(newObjectModel);

        lastCreatedBackgroundObjectPosstion += 5.0f;
    }

    private void RemoveCreatedObject(GameObject objToRemove)
    {
        createdObject.Remove(objToRemove);
        Destroy(objToRemove);

        if (objToRemove.name.Contains(nameOfDiamond))
        {
            CreateNewObject(DiamondsPrefabs);
        }
        else if (objToRemove.name.Contains(nameOfObstacle))
        {
            CreateNewObject(ObstaclePrefabs);
        }
        else if (objToRemove.name.Contains(nameOfBackgroundObj))
        {
            CreateNewBackgroundObj();
        }
        //Debug.Log(objToRemove.name);
    }

    private void GameOver()
    {
        Debug.Log("You are dead");

        // Update best score
        if (File.Exists(Application.persistentDataPath + bestScoreFileName))
        {
            string currentBestScore = File.ReadAllText(Application.persistentDataPath + bestScoreFileName);

            if (int.Parse(currentBestScore) < points)
            {
                File.WriteAllText(Application.persistentDataPath + bestScoreFileName, points.ToString());
                // TODO: Show message about record
            }
            else
            {
                // TODO: Show message
            }
        }

        //Debug.Log("return to menu");
        SceneManager.LoadScene("MainMenu");
    }
}