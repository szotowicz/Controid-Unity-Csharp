using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public static MenuManager instance;

    public ComputersListManager clm;

    public float refreshRate = 1f;
    private float refreshCoolDown = 0;
    // Use this for initialization

    public void Awake() {
        instance = this;
        DontDestroyOnLoad(transform.gameObject);
    }

    public void Start() {
        setMainMenu();
        Application.runInBackground = true;
        NetworkManager.instance.runSerwer();
    }

    // Update is called once per frame
    void Update() {
        NetworkManager.instance.update();
        refreshCoolDown += Time.deltaTime;
        if (refreshCoolDown > refreshRate) {
            OnRefreshButton();
            refreshCoolDown = 0;
        }
    }

    public void setMainMenu() {
        NetworkManager.instance.disableNetwork();
    }

    public Color serverColor;
    public string serverName;

    private void randomServerParams() {
        int r = UnityEngine.Random.Range(0, 8);
        switch (r) {
            case 0:
                serverColor = Color.cyan;
                serverName = "Cyan";
                break;
            case 1:
                serverColor = Color.green;
                serverName = "Green";
                break;
            case 2:
                serverColor = Color.blue;
                serverName = "Blue";
                break;
            case 3:
                serverColor = Color.magenta;
                serverName = "Magenta";
                break;
            case 4:
                serverColor = Color.white;
                serverName = "White";
                break;
            case 5:
                serverColor = Color.yellow;
                serverName = "Yellow";
                break;
            case 6:
                serverColor = Color.red;
                serverName = "Red";
                break;
            case 7:
                serverColor = Color.grey;
                serverName = "Grey";
                break;
            default:
                serverColor = Color.grey;
                serverName = "Grey";
                break;
        }
        //serverNameText.text = serverName;
        Color tmp = serverColor;
        tmp.a = 0.5f;
        //serverColorPlane.color = tmp;
    }

    public void setServerMenu() {
        //clm.clearData();
        randomServerParams();
        NetworkManager.instance.runSerwer();
        OnRefreshButton();
        ConnectController();
    }

    public void setSettingsMenu() {
        Options();
    }

    public void setClientMenu() {
        //middlePlain.SetBool("isBig", true);        
        //randomServerParams();
        //NetworkManager.instance.runSerwer();
        //OnRefreshButton();
    }

    public void OnRefreshButton() {
        return;
    }

    public void updatePlayersList(List<ComputersListElement> l) {

    }

    public void Connect(string ip) {
        try {
            IPEndPoint iped = new IPEndPoint(IPAddress.Parse(ip), NetworkManager.instance.broadcastPort);
            NetworkManager.instance.connectToSerwer(iped);
        }
        catch {

        }
    }

    public void setClientColorAndTitle(string title, Color color) {
        color.a = 0.5f;
    }

    public void debugStartGameServer() {
        NetworkManager.instance.runSerwer();
        long seed = UnityEngine.Random.Range(0, int.MaxValue);
        Debug.Log("Debug Start Game Server");

        var menuData = new DaneZMenuGlownego();
        List<ComputersListElement> ele = new List<ComputersListElement>();
        var cs = new ComputersListElement();
        cs.ip = new IPEndPoint(NetworkManager.instance.getMyIp(), NetworkManager.instance.port);
        cs.players.Add(new playerElement() { isAi = true, name = "Bot", id = 0 });
        cs.players.Add(new playerElement() { isAi = false, name = "gracz 1", id = 1 });
        cs.players.Add(new playerElement() { isAi = false, name = "gracz 2", id = 2 });
        cs.players.Add(new playerElement() { isAi = false, name = "gracz 3", id = 3 });
        cs.players.Add(new playerElement() { isAi = false, name = "gracz 4", id = 4 });
        ele.Add(cs);
        menuData.komputeryIGracze = ele;
        PlayGame();//todo menudata
    }

    public void startGameServer() {
        long seed = UnityEngine.Random.Range(0, int.MaxValue);
        Debug.Log("Start Game Server");
        var obj = GameObject.Find("PlayersListServer");
        var com = obj.GetComponent<ComputersListManager>();
        var komputeryIGracze = com.comps;

        int minimalnaIloscGraczy = 2;
        int iloscGraczy = 0;
        if (komputeryIGracze.Count == 0) return;
        foreach (var ele in komputeryIGracze) {
            foreach (var grac in ele.players) {
                iloscGraczy++;
            }
        }

        if (iloscGraczy < minimalnaIloscGraczy) return;

        var start = new Q_START_GAME();

        NetworkManager.instance.sendToAllComputers(start);
        var menuData = new DaneZMenuGlownego();
        menuData.komputeryIGracze = komputeryIGracze;
        PlayGame();//todo menudata

    }

    public void startGameClient() {
        Debug.Log("Start Game Client");
        var obj = GameObject.Find("ClientPlayersList");
        var com = obj.GetComponent<ComputersListManagerClient>();
        var komputeryIGracze = com.comps;
        var menuData = new DaneZMenuGlownego();
        menuData.komputeryIGracze = komputeryIGracze;
        PlayGame();//todo menudata
    }

    public static DaneZMenuGlownego StaticData;


    private void PlayGame() {
        Debug.Log("New game");
        SceneManager.LoadScene("NewGame");
    }

    private void ConnectController() {
        Debug.Log("ConnectController");
        SceneManager.LoadScene("ConnectController");
    }

    private void Options() {
        Debug.Log("options");
        SceneManager.LoadScene("Options");
    }

    public void AboutGame() {
        Debug.Log(" about game");
        SceneManager.LoadScene("AboutGame");
    }

    public void Exit() {
        Debug.Log("exit");
        Application.Quit();
    }

    private void ReturnToMenu() {
        Debug.Log("return to menu");
        SceneManager.LoadScene("MainMenu");
    }
}