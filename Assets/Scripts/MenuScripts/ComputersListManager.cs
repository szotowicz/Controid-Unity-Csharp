using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public class playerElement
{
    public Color color = new Color(0.5f, 0f, 1f, 0.5f);
    public int id;
    public bool isAi;
    public string name = "";
}

public class ComputersListElement
{
    public IPEndPoint ip;
    public List<playerElement> players = new List<playerElement>();
}

public class ComputersListManager : MonoBehaviour
{
    public ComputerPanel compPrefab;
    public List<ComputersListElement> comps = new List<ComputersListElement>();

    private float cooldown;
    public bool debugMode;
    public InputField inputName;
    public PlayerPanel playerPrefab;

    private GameObject portErrorMsg;
    private Random rnd = new Random();
    public bool serverMode;
    public Toggle toggle;

    // Use this for initialization
    private void Start() {
        portErrorMsg = GameObject.Find("PortErrorMsg");
        if (portErrorMsg == null) return;
        portErrorMsg.SetActive(true);
    }

    // Update is called once per frame
    private void Update() {
        cooldown += Time.deltaTime;
        if (cooldown > 1) {
            refreshLogic();
            refreshList();
            updateClients();
            cooldown = 0;
        }

        if (portErrorMsg == null) return;
        if (NetworkManager.instance.getNetworkState() == NetworkState.NET_SERVER)
            portErrorMsg.SetActive(false);
        else
            portErrorMsg.SetActive(true);
    }

    private void updateClients() {
        if (NetworkManager.instance.getNetworkState() == NetworkState.NET_SERVER) {
            var obj0 = new Q_PLAYERS_LIST_RESET();
            NetworkManager.instance.sendToAllComputers(obj0);
            foreach (var a in comps)
            foreach (var b in a.players) {
                var obj = new Q_PLAYERS_LIST_ELEMENT();
                obj.name = b.name;
                obj.ip = a.ip.Address.ToString();
                obj.port = a.ip.Port;
                obj.isAi = b.isAi;
                obj.color = b.color;
                obj.id = b.id;
                NetworkManager.instance.sendToAllComputers(obj);
            }
        }
    }

    public void killElements() {
        foreach (Transform child in transform)
            if (child != transform)
                Destroy(child.gameObject);
    }

    public void killElementsWithLogic() {
        comps = new List<ComputersListElement>();
    }

    public void refreshLogic() {
        if (NetworkManager.instance.getNetworkState() == NetworkState.NET_SERVER) {
            comps = new List<ComputersListElement>();
            foreach (var c in NetworkManager.instance.computers) {
                var l = new ComputersListElement();
                l.ip = c.ip;
                foreach (var p in NetworkManager.instance.players)
                    if (Equals(p.ip, l.ip))
                        l.players.Add(new playerElement {name = p.name, isAi = p.isAi, color = p.color, id = p.id});
                comps.Add(l);
            }
        }
    }

    public void addPlayer() {
        var r = UnityEngine.Random.Range(0.3f, 1f);
        var g = UnityEngine.Random.Range(0.3f, 1f);
        var b = UnityEngine.Random.Range(0.3f, 1f);
        var ktory = UnityEngine.Random.Range(0, 3);
        switch (ktory) {
            case 0:
                r = UnityEngine.Random.Range(0.0f, 0.3f);
                break;
            case 1:
                g = UnityEngine.Random.Range(0.0f, 0.3f);
                break;
            case 2:
                b = UnityEngine.Random.Range(0.0f, 0.3f);
                break;
        }

        var losowyKolor = new Color(r, g, b);
        var playerNametmp = inputName.text;
        if (NetworkManager.instance.getNetworkState() == NetworkState.NET_SERVER) {
            NetworkManager.instance.addPlayer(playerNametmp,
                new IPEndPoint(NetworkManager.instance.getMyIp(), NetworkManager.instance.port), toggle.isOn,
                losowyKolor);
        }
        else {
            if (NetworkManager.instance.getNetworkState() == NetworkState.NET_CLIENT) {
                var obj = new Q_ADD_PLAYER();
                obj.name = playerNametmp;
                obj.isAi = toggle.isOn;
                //ustalenie koloru !!!
                obj.color = losowyKolor;
                NetworkManager.instance.sendToServer(obj);
            }
        }

        inputName.text = "";
    }

    public void refreshList() {
        killElements();
        foreach (var element in comps) {
            var sb = Instantiate(compPrefab);
            sb.ip = element.ip.Address.ToString();
            if (NetworkManager.instance.getMyIp().ToString() == element.ip.Address.ToString() &&
                element.ip.Port == NetworkManager.instance.port && serverMode)
                sb.kickEnabled = false;
            else
                sb.kickEnabled = true;
            if (serverMode == false) sb.kickEnabled = false;
            if (debugMode) sb.kickEnabled = true;
            sb.port = element.ip.Port;
            sb.transform.SetParent(transform, false);

            foreach (var pl in element.players) {
                var pp = Instantiate(playerPrefab);
                pp.name = pl.name;
                pp.isAi = pl.isAi;
                pp.color = pl.color;
                if (serverMode) {
                    pp.removeEnabled = true;
                }
                else {
                    pp.removeEnabled = false;
                    if (NetworkManager.instance.getMyIp().ToString() == element.ip.Address.ToString() &&
                        element.ip.Port == NetworkManager.instance.port) pp.removeEnabled = true;
                }

                if (debugMode) pp.removeEnabled = true;
                pp.transform.SetParent(transform, false);
            }
        }
    }

    public void clearData() {
        killElements();
        comps.Clear();
    }
}