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
    public int DiamondsOnStart = 40;
    public List<GameObject> DiamondsPrefabs;

    private string nameOfDiamond = "Diamond";
    private string nameOfObstacle = "ToDie";
    private string bestScoreFileName = "/bestScore.txt";
    private int points;
    private float lastDiamondPossition = 0;
    private List<GameObject> createdDiamonds = new List<GameObject>();
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
        
        if (DiamondsPrefabs.Count > 0)
        {
            // Debug.Log("Diamonds: " + DiamondsPrefabs.Count);
            for (int i = 0; i < DiamondsOnStart; i++)
            {
                AddNewDiamond();
            }
        }
    }

    void Update()
    {
        float gameCharacterDistance = gameObject.transform.position.z;
        // Debug.Log (gameCharacterDistance);

        foreach (GameObject diamond in createdDiamonds)
        {
            if (gameCharacterDistance > diamond.transform.position.z + 3.0f)
            {
                RemoveDiamond(diamond);
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
            RemoveDiamond(col.gameObject);
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

    private void AddNewDiamond()
    {
        int newModelIndex = Random.Range(0, DiamondsPrefabs.Count);
        // Debug.Log(newModelIndex);
        float newModelX = possiblePaths.ElementAt(Random.Range(0, possiblePaths.Count)).Value;
        float distanceForNewModel = Random.Range(2.0f, 10.0f);
        lastDiamondPossition += distanceForNewModel;

        GameObject newDiamond = Instantiate(DiamondsPrefabs[newModelIndex],
            new Vector3(newModelX, -3.0f, lastDiamondPossition), Quaternion.identity);

        createdDiamonds.Add(newDiamond);
    }

    private void RemoveDiamond(GameObject diamond)
    {
        createdDiamonds.Remove(diamond);
        Destroy(diamond);

        AddNewDiamond();
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
                // Show message about record
                File.WriteAllText(Application.persistentDataPath + bestScoreFileName, points.ToString());
            }
            else
            {
                // Show message
            }
        }

        Debug.Log("return to menu");
        SceneManager.LoadScene("MainMenu");
    }
}