using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

//Z tego co ludzie pisali w internecie, nie można serializować tablic za pomocą JsonUtility, więc trzeba uważać.

[Serializable]
public abstract class Q_OBJECT : object
{
    public abstract void executeQuery(QueuePack queuePack);

    public static Q_OBJECT Deserialize(string json, string type) {
        switch (type) {
            case "Q_SERVER_INFO_REQUEST":
                return JsonUtility.FromJson<Q_SERVER_INFO_REQUEST>(json);
            case "Q_SERVER_INFO":
                return JsonUtility.FromJson<Q_SERVER_INFO>(json);
            case "Q_HELLO":
                return JsonUtility.FromJson<Q_HELLO>(json);
            case "Q_JOIN_REQUEST":
                return JsonUtility.FromJson<Q_JOIN_REQUEST>(json);
            case "Q_JOIN_OK":
                return JsonUtility.FromJson<Q_JOIN_OK>(json);
            case "Q_IM_ALIVE":
                return JsonUtility.FromJson<Q_IM_ALIVE>(json);
            case "Q_IM_ALIVE_RESPONSE":
                return JsonUtility.FromJson<Q_IM_ALIVE_RESPONSE>(json);
            case "Q_ADD_PLAYER":
                return JsonUtility.FromJson<Q_ADD_PLAYER>(json);
            case "Q_REMOVE_PLAYER":
                return JsonUtility.FromJson<Q_REMOVE_PLAYER>(json);
            case "Q_PLAYERS_LIST_ELEMENT":
                return JsonUtility.FromJson<Q_PLAYERS_LIST_ELEMENT>(json);
            case "Q_PLAYERS_LIST_RESET":
                return JsonUtility.FromJson<Q_PLAYERS_LIST_RESET>(json);
            case "Q_START_GAME":
                return JsonUtility.FromJson<Q_START_GAME>(json);
            case "Q_KICK":
                return JsonUtility.FromJson<Q_KICK>(json);
            case "Q_JUMP":
                return JsonUtility.FromJson<Q_JUMP>(json);
            case "Q_CROUCH":
                return JsonUtility.FromJson<Q_CROUCH>(json);
            case "Q_LEFT":
                return JsonUtility.FromJson<Q_LEFT>(json);
            case "Q_RIGHT":
                return JsonUtility.FromJson<Q_RIGHT>(json);
            default:
                //na wypadek błędu
                Debug.Log("Q_OBJECT ERROR, Nieznany typ " + type);
                Debug.Log("Zapomniałeś dopisać tą linię kodu w Q_OBJECT");
                throw new Exception("Q_OBJECT ERROR, Nieznany typ " + type);
        }
    }
}

[Serializable]
public class Q_SERVER_INFO_REQUEST : Q_OBJECT //obiekt oznaczający, że ktoś chce się dowiedzieć coś o serwerze
{
    public override void executeQuery(QueuePack queuePack) {
        Debug.Log("Q_SERVER_INFO_REQUEST execute.");
        if (NetworkManager.instance.getNetworkState() == NetworkState.NET_SERVER)
            try {
                var obj = GameObject.Find("MenuManager");
                var mm = obj.GetComponent<MenuManager>();
                var info = new Q_SERVER_INFO();
                info.serverName = mm.serverName;
                info.color = mm.serverColor;
                NetworkManager.instance.sendToComputer(info, queuePack.endpoint);
            }
            catch { }
    }
}

[Serializable]
public class Q_SERVER_INFO : Q_OBJECT //obiekt zawierający dane o serwerze
{
    public Color color;
    public string serverName;

    public override void executeQuery(QueuePack queuePack) {
        /*Debug.Log("Q_SERVER_INFO: "+ serverName+"\t"+ numberOfPlayers);
        GameObject gameObject = GameObject.Find("GameObject");
        Test test = gameObject.GetComponent<Test>();
        if (test != null)
            test.ip = queuePack.endpoint;*/

        //cos co doda kafelke w menu       
        try {
            var obj = GameObject.Find("ServersContainer");
            var sbm = obj.GetComponent<ServerButtonManager>();
            sbm.addData(serverName, queuePack.endpoint.Address.ToString(), color, queuePack.endpoint.Port);
        }
        catch { }
    }
}

[Serializable]
public class Q_HELLO : Q_OBJECT //obiekt do testowania
{
    public string text;

    public override void executeQuery(QueuePack queuePack) {
        Debug.Log("HELLO " + text);
    }
}

[Serializable]
public class Q_JOIN_REQUEST : Q_OBJECT //obiekt oznaczający chęć dołączenia do gry
{
    public override void executeQuery(QueuePack queuePack) {
        Debug.Log("Q_JOIN_REQUEST execute.");
        if (NetworkManager.instance.getNetworkState() == NetworkState.NET_SERVER) {
            if (NetworkManager.instance.isKnownComputer(queuePack.endpoint)
            ) //może dołączyć nawet w trakcie gry, jeżeli na chwilę go wywali
            {
                NetworkManager.instance.addComputer(queuePack.endpoint);
                NetworkManager.instance.sendToComputer(new Q_JOIN_OK(), queuePack.endpoint);
            }

            if (true /*jakis warunek typu, jak gra jest w toku false*/) {
                NetworkManager.instance.addComputer(queuePack.endpoint);
                NetworkManager.instance.sendToComputer(new Q_JOIN_OK(), queuePack.endpoint);
                SceneManager.LoadScene("MainMenu");
                Debug.Log("Q_JOIN_REQUEST done." + queuePack.endpoint.Address + "\t" + queuePack.endpoint.Port);
            }
            else {
                Debug.Log("Q_JOIN_REQUEST fail.");
            }
        }
    }
}

[Serializable]
public class Q_JOIN_OK : Q_OBJECT //obiekt oznaczający fakt dołączenia do gry
{
    public override void executeQuery(QueuePack queuePack) {
        Debug.Log("Q_JOIN_OK execute.");
        var tmp = NetworkManager.instance.getJoinIp();
        var res = Equals(tmp, queuePack.endpoint);
        if (res) {
            var ip = queuePack.endpoint;
            NetworkManager.instance.acceptJoin(ip);
            MenuManager.instance.setClientMenu();
            Debug.Log("Q_JOIN_OK done.");
            //zmiana menu
        }
    }
}

[Serializable]
public class Q_IM_ALIVE : Q_OBJECT //obiekt oznaczający że komputer nie umarł
{
    public override void executeQuery(QueuePack queuePack) {
        Debug.Log("Q_IM_ALIVE " + queuePack.endpoint.Address);
        NetworkManager.instance.setComputerTimeZero(queuePack.endpoint);
        NetworkManager.instance.sendToComputer(new Q_IM_ALIVE_RESPONSE(), queuePack.endpoint);
    }
}

[Serializable]
public class Q_IM_ALIVE_RESPONSE : Q_OBJECT //obiekt oznaczający że komputer nie umarł
{
    public override void executeQuery(QueuePack queuePack) {
        Debug.Log("Q_IM_ALIVE_RESPONSE " + queuePack.endpoint.Address);
        NetworkManager.instance.setServerTimeZero();
    }
}

[Serializable]
public class Q_ADD_PLAYER : Q_OBJECT
{
    public Color color = new Color(0.5f, 1f, 1f, 0.5f);
    public bool isAi;
    public string name;

    public override void executeQuery(QueuePack queuePack) {
        Debug.Log("Q_ADD_PLAYER lockmode:" + NetworkManager.instance.lockMode);
        if (NetworkManager.instance.getNetworkState() == NetworkState.NET_SERVER &&
            NetworkManager.instance.lockMode == false) {
            NetworkManager.instance.addPlayer(name, queuePack.endpoint, isAi, color);
            Debug.Log("Q_ADD_PLAYER done");
        }
    }
}

[Serializable]
public class Q_REMOVE_PLAYER : Q_OBJECT
{
    public string name;

    public override void executeQuery(QueuePack queuePack) {
        Debug.Log("Q_REMOVE_PLAYER lockmode:" + NetworkManager.instance.lockMode);
        if (NetworkManager.instance.getNetworkState() == NetworkState.NET_SERVER &&
            NetworkManager.instance.lockMode == false) {
            NetworkManager.instance.removePlayer(name, queuePack.endpoint);
            Debug.Log("Q_REMOVE_PLAYER done");
        }
    }
}

[Serializable]
public class Q_PLAYERS_LIST_RESET : Q_OBJECT
{
    public override void executeQuery(QueuePack queuePack) {
        //Debug.Log("Q_PLAYERS_LIST_RESET");
        if (NetworkManager.instance.getNetworkState() == NetworkState.NET_CLIENT)
            try {
                var obj = GameObject.Find("ClientPlayersList");
                var sbm = obj.GetComponent<ComputersListManagerClient>();
                sbm.killElementsWithLogic();
            }
            catch { }
    }
}

[Serializable]
public class Q_PLAYERS_LIST_ELEMENT : Q_OBJECT
{
    public Color color = new Color(1f, 0f, 0f, 0.5f);

    //public IPEndPoint ip;
    public int id = 0;
    public string ip;
    public bool isAi;
    public string name;
    public int port;

    public override void executeQuery(QueuePack queuePack) {
        //Debug.Log("Q_PLAYERS_LIST_ELEMENT");
        if (NetworkManager.instance.getNetworkState() == NetworkState.NET_CLIENT)
            try {
                var obj = GameObject.Find("ClientPlayersList");
                var sbm = obj.GetComponent<ComputersListManagerClient>();
                sbm.addElement(new IPEndPoint(IPAddress.Parse(ip), port), name, isAi, color, id);
            }
            catch { }
    }
}

[Serializable]
public class Q_START_GAME : Q_OBJECT
{

    public override void executeQuery(QueuePack queuePack) {
        Debug.Log("Q_START_GAME");
        if (NetworkManager.instance.getNetworkState() == NetworkState.NET_CLIENT)
            try {
                MenuManager.instance.startGameClient();
            }
            catch { }
    }
}

[Serializable]
public class Q_KICK : Q_OBJECT
{
    public override void executeQuery(QueuePack queuePack) {
        Debug.Log("Q_KICK");
        if (NetworkManager.instance.getNetworkState() == NetworkState.NET_CLIENT)
            try {
                MenuManager.instance.setMainMenu();
            }
            catch { }
    }
}

//ruchy graczy
[Serializable]
public class Q_JUMP : Q_OBJECT
{
    public override void executeQuery(QueuePack queuePack) {
        Debug.Log("JUMP");
    }
}

[Serializable]
public class Q_CROUCH : Q_OBJECT
{
    public override void executeQuery(QueuePack queuePack) {
        Debug.Log("CROUCH");
    }
}

[Serializable]
public class Q_LEFT : Q_OBJECT
{
    public override void executeQuery(QueuePack queuePack) {
        Debug.Log("LEFT");
    }
}

[Serializable]
public class Q_RIGHT : Q_OBJECT
{
    public override void executeQuery(QueuePack queuePack) {
        Debug.Log("RIGHT");
    }
}