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
    public int ObstaclesOnStart = 20;
    public List<GameObject> ObstaclePrefabs;
    public List<GameObject> BackgroundObjPrefabs;
    public GameObject MessageBox;

    private string nameOfDiamond = "Diamond";
    private string nameOfObstacle = "ToDie";
    private string nameOfBackgroundObj = "BackgroundObj";
    private string bestScoreFileName = "/bestScore.txt";
    private int collectedPoints;
    private int uncollectedPoints;
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
        collectedPoints = 0;
        uncollectedPoints = 0;
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
                {
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
                if (obj.name.Contains(nameOfDiamond))
                {
                    uncollectedPoints++;
                    //Debug.Log("c: " + collectedPoints + " u: " + uncollectedPoints);
                }
                RemoveCreatedObject(obj);
                break;
            }            
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.name.Contains(nameOfDiamond))
        {
            collectedPoints++;
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
        PointsMonitor.text = collectedPoints.ToString();
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

    /* policy: 
     * start: 5.0f - 10.0f
     * >75% collected = obstacle 4.0f-8.0f
     * >90% collected = obstacle 3.0f-6.0f
     * <50% collected = diamonds 4.0f-8.0f
     * <25% collected = diamonds 3.0f-6.0f
    */
    private void CreateNewObject(List<GameObject> prefabsList)
    {
        float defaultDistanceStart = 5.0f;
        float defaultDistanceEnd = 10.0f;

        if (prefabsList.Equals(ObstaclePrefabs))
        {
            if (uncollectedPoints > 0 && collectedPoints / uncollectedPoints > 0.75)
            {
                defaultDistanceStart = 4.0f;
                defaultDistanceEnd = 8.0f;
                //Debug.Log(">75%");
            }
            if ((uncollectedPoints == 0 && collectedPoints > 0) || (uncollectedPoints > 0 && collectedPoints / uncollectedPoints > 0.9))
            {
                defaultDistanceStart = 3.0f;
                defaultDistanceEnd = 6.0f;
                //Debug.Log(">90%");
            }
        }
        if (prefabsList.Equals(DiamondsPrefabs))
        {
            if (uncollectedPoints > 0 && collectedPoints / uncollectedPoints < 0.5)
            {
                defaultDistanceStart = 4.0f;
                defaultDistanceEnd = 8.0f;
                //Debug.Log("<50%");
            }
            if (uncollectedPoints > 0 && collectedPoints / uncollectedPoints < 0.25)
            {
                defaultDistanceStart = 3.0f;
                defaultDistanceEnd = 6.0f;
                //Debug.Log("<25%");
            }
        }

        int newModelIndex = Random.Range(0, prefabsList.Count);
        float newModelX = possiblePaths.ElementAt(Random.Range(0, possiblePaths.Count)).Value;
        float distanceForNewModel = Random.Range(defaultDistanceStart, defaultDistanceEnd);
        lastCreatedObjectPosstion += distanceForNewModel;

        GameObject newObjectModel = Instantiate(prefabsList[newModelIndex],
            new Vector3(newModelX, 0.0f, lastCreatedObjectPosstion), Quaternion.identity);

        createdObject.Add(newObjectModel);
    }

    private void CreateNewBackgroundObj()
    {
        int newModelIndex = Random.Range(0, BackgroundObjPrefabs.Count);

        GameObject newObjectModel = Instantiate(BackgroundObjPrefabs[newModelIndex],
            new Vector3(3.6f + Random.Range(0.0f, 2.0f), 0.0f, lastCreatedBackgroundObjectPosstion + Random.Range(2.0f, 5.0f)), Quaternion.identity);
        createdObject.Add(newObjectModel);

        newObjectModel = Instantiate(BackgroundObjPrefabs[newModelIndex],
            new Vector3(-3.6f - Random.Range(0.0f, 2.0f), 0.0f, lastCreatedBackgroundObjectPosstion + Random.Range(2.0f, 5.0f)), Quaternion.identity);
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

            Text textLabel = null;
            UnityStandardAssets.Characters.ThirdPerson.ThirdPersonCharacter thirdPerson = gameObject.GetComponent<UnityStandardAssets.Characters.ThirdPerson.ThirdPersonCharacter>();
            if (thirdPerson != null)
            {
                thirdPerson.m_MoveSpeedMultiplier = 0;
                thirdPerson.m_AnimSpeedMultiplier = 0;
                Transform child = MessageBox.transform.Find("InfoTextLabel");
                textLabel = child.GetComponent<Text>();
                
            }

            if (int.Parse(currentBestScore) < collectedPoints)
            {
                File.WriteAllText(Application.persistentDataPath + bestScoreFileName, collectedPoints.ToString());
                if (textLabel != null)
                {
                    textLabel.text = "Brawo! Właśnie pobiłeś rekord gry! Spróbuj swoich sił jeszcze raz :)";
                    MessageBox.SetActive(true);
                }
            }
            else
            {
                if (textLabel != null)
                {
                    textLabel.text = "Koniec gry! Niestety nie udało Ci się pobić rekordu. Spróbuj swoich sił jeszcze raz :)";
                    MessageBox.SetActive(true);
                }
            }
        }

        //Debug.Log("return to menu");
       // SceneManager.LoadScene("MainMenu");
    }
}