using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public enum SendMode
{
    SM_BROADCAST,
    SM_ALL_IN_NETWORK,
    SM_COMPUTER,
    SM_PLAYER,
    SM_TO_SERVER_TO_ALL,
    SM_TO_SERVER
}

public enum NetworkState
{
    NET_DISABLED,
    NET_ENABLED,
    NET_CLIENT,
    NET_SERVER
}

public class PlayerInfo
{
    public Color color = new Color(0f, 1f, 0f, 0.5f);
    public int id;
    public IPEndPoint ip;
    public bool isAi;
    public string name = "";
}

public class Computer
{
    public IPEndPoint ip;
    public float offlineTime;
    public int state = 0;
}

public class QueuePack
{
    public IPEndPoint endpoint; //ip nadawcy z portem na który można wysyłać dane
    public QueryPack qp;
}

[Serializable]
public class QueryPack
{
    public string json = "";
    public int port = 11001;
    public SendMode sendMode = SendMode.SM_ALL_IN_NETWORK;
    public int targetPlayerId;
    public string type = "";

    public static string getJson(QueryPack q) {
        return JsonUtility.ToJson(q);
    }

    public static QueryPack getObject(string json) {
        return JsonUtility.FromJson<QueryPack>(json);
    }
}

public class NetworkManager
{
    private const int joinTimeout = 4000;
    private const int receiverTimeout = 100;
    private const int aliveTimeout = 1000;
    private const int kickTimeout = 10;
    public static NetworkManager instance = new NetworkManager(); //instancja

    private static int idCounter;

    public int
        broadcastPort = 11000; //port na którym musi chodzić serwer i na który będą wysyłane wiadomości broadcast.

    public List<Computer> computers;

    public int
        connectionPort =
            11001; //port na którym chodzi klient, serwer pamięta port przez który może się komunikować z klientem.

    private Thread connector;

    private bool disableTrigger;

    //public MainGame GameInstance;
    private Thread joiner;

    private IPEndPoint joinSemaphore;
    private UdpClient listener;
    private int listenerCounter;
    private bool listenerErrorTrigger;
    public bool lockMode; //tryb blokowania wiadomości z poza podłączonyh komputerów (tylko dla serwera)
    private IPEndPoint myIp;

    private NetworkState networkState;

    public List<PlayerInfo> players;
    public int port = 11000;
    private Queue<QueuePack> receiveQueue;
    private Thread receiver;
    private readonly Socket s;

    private Queue<QueuePack> sendQueue;

    private IPEndPoint serverIp;
    private float serverOfflineTime;

    // Use this for initialization
    private NetworkManager() {
        networkState = NetworkState.NET_DISABLED;
        //socket init
        s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        s.EnableBroadcast = true;
        s.MulticastLoopback = false;
        myIp = new IPEndPoint(getMyIp(), port);
    }

    public void kickComputer(IPEndPoint ip) {
        Debug.Log("kickComputer");
        if (networkState == NetworkState.NET_SERVER) {
            if (Equals(myIp, ip)) return;
            for (var i = 0; i < computers.Count; ++i) {
                var access = Equals(computers[i].ip, ip);
                if (access) {
                    computers.RemoveAt(i);
                    for (var j = 0; j < players.Count; ++j) {
                        var access2 = Equals(players[j].ip, ip);
                        if (access2) {
                            var deadPlayerInfo = players[j];
                            players.RemoveAt(j);
                            try {
                                //GameInstance.GetPlayerById(deadPlayerInfo.id);
                                //if (deadPlayer != null) deadPlayer.KillMe();
                            }
                            catch { }

                            --j;
                        }
                    }

                    --i;
                    break;
                }
            }

            var kick = new Q_KICK();
            instance.sendToComputer(kick, ip);
        }
    }

    public void addPlayer(string name, IPEndPoint ip, bool isAi, Color color) {
        var pi = new PlayerInfo();
        pi.color = color;
        pi.name = name;
        pi.id = idCounter++;
        pi.ip = ip;
        pi.isAi = isAi;
        players.Add(pi);
        Debug.Log("addPlayer: " + pi.name + "\t" + pi.id + "\t" + pi.ip + "\t" + pi.isAi);
        foreach (var pl in players) Debug.Log("pl: " + pl.name + "\t" + pl.ip.Address + "\t" + pl.ip.Port);
        foreach (var com in computers) Debug.Log("com: " + com.ip.Address + "\t" + com.ip.Port);
    }

    public void removePlayer(string name, IPEndPoint ip) {
        Debug.Log("removePlayer: " + name);
        var highPriority = Equals(ip, myIp);
        for (var i = 0; i < players.Count; ++i) {
            var access = Equals(players[i].ip, ip);
            if (access || highPriority)
                if (players[i].name == name) {
                    Debug.Log("removed: " + name);
                    players.RemoveAt(i);
                    break;
                }
        }
    }

    //Wysyła obiekt do wszystkich urządzeń w domenie rozgłoszeniowej, nawet do samego siebie
    public void sendBroadcast(object o) {
        var json = JsonUtility.ToJson(o);
        var qp = new QueryPack();
        qp.json = json;
        qp.type = o.GetType().FullName;
        qp.port = port;
        qp.sendMode = SendMode.SM_BROADCAST;
        var queue = new QueuePack();
        queue.qp = qp;
        queue.endpoint = new IPEndPoint(IPAddress.Broadcast, broadcastPort);
        sendQueue.Enqueue(queue);
        //Debug.Log("Broadcast");
    }

    //Pilnuje aby obiekt dotarł do każdego komputera w grze, poza komputerem z którego wysłano obiekt.
    public void sendToAllComputers(object o) {
        if (networkState == NetworkState.NET_DISABLED || networkState == NetworkState.NET_ENABLED) return;
        var json = JsonUtility.ToJson(o);
        var qp = new QueryPack();
        qp.json = json;
        qp.type = o.GetType().FullName;
        qp.port = port;
        qp.sendMode = SendMode.SM_ALL_IN_NETWORK;
        var queue = new QueuePack();
        queue.qp = qp;
        //throw new NotImplementedException();
        switch (networkState) {
            case NetworkState.NET_SERVER:
            {
                foreach (var comp in computers) {
                    var ip = comp.ip;
                    if (Equals(ip, myIp)) continue;
                    var tmp = new QueuePack();
                    tmp.endpoint = ip;
                    tmp.qp = queue.qp;
                    sendQueue.Enqueue(tmp);
                }

                break;
            }
            case NetworkState.NET_CLIENT:
            {
                queue.endpoint = serverIp;
                sendQueue.Enqueue(queue);
                break;
            }
            default:
                sendQueue.Enqueue(queue);
                break;
        }
    }

    //Wysyła obiekt do komputera o podanym ip
    public void sendToComputer(object o, IPEndPoint ip) {
        var json = JsonUtility.ToJson(o);
        var qp = new QueryPack();
        qp.json = json;
        qp.type = o.GetType().FullName;
        qp.port = port;
        qp.sendMode = SendMode.SM_COMPUTER;
        var queue = new QueuePack();
        queue.qp = qp;
        queue.endpoint = ip;
        sendQueue.Enqueue(queue);
    }

    //Wysyła obiekt do komputera na którym gra gracz o danym id, nawet jeżeli to komputer z którego wysłano obiekt.
    public void sendToPlayer(object o, int playerId) {
        throw new NotImplementedException();
        if (networkState == NetworkState.NET_DISABLED || networkState == NetworkState.NET_ENABLED) return;
        var json = JsonUtility.ToJson(o);
        var qp = new QueryPack();
        qp.json = json;
        qp.type = o.GetType().FullName;
        qp.targetPlayerId = playerId;
        qp.port = port;
        qp.sendMode = SendMode.SM_PLAYER;
        var queue = new QueuePack();
        queue.qp = qp;
        switch (networkState) {
            case NetworkState.NET_CLIENT:
                queue.endpoint = serverIp;
                sendQueue.Enqueue(queue);
                break;
            case NetworkState.NET_SERVER:
                foreach (var player in players)
                    if (player.id == playerId) {
                        queue.endpoint = player.ip;
                        sendQueue.Enqueue(queue);
                        break;
                    }

                break;
        }
    }

    //Wysyła obiekt do serwera i serwer wysyła go do wszystkich komputerów łącznie z serwerem. Służy to głównie do traktowania gry jakby była na jakiejś chmurze (czyli model w którym użytkownik nie jest przypisany do stanowiska).
    public void sendToServerToAll(object o) {
        if (networkState == NetworkState.NET_DISABLED || networkState == NetworkState.NET_ENABLED) return;
        var json = JsonUtility.ToJson(o);
        var qp = new QueryPack();
        qp.json = json;
        qp.type = o.GetType().FullName;
        qp.port = port;
        qp.sendMode = SendMode.SM_TO_SERVER_TO_ALL;
        var queue = new QueuePack();
        queue.qp = qp;
        queue.endpoint = serverIp;
        sendQueue.Enqueue(queue);
    }

    //Wysyła obiekt do serwera, jeżeli serwer to wysyła to wyśle sam do siebie.
    public void sendToServer(object o) {
        if (networkState == NetworkState.NET_DISABLED || networkState == NetworkState.NET_ENABLED) return;
        var json = JsonUtility.ToJson(o);
        var qp = new QueryPack();
        qp.json = json;
        qp.type = o.GetType().FullName;
        qp.port = port;
        qp.sendMode = SendMode.SM_TO_SERVER;
        var queue = new QueuePack();
        queue.qp = qp;
        queue.endpoint = serverIp;
        sendQueue.Enqueue(queue);
    }

    public void runSerwer() {
        setStateServer();
    }

    public void connectToSerwer(IPEndPoint ip) {
        //setStateEnabled();
        if (getNetworkState() != NetworkState.NET_DISABLED) {
            joinSemaphore = ip;
            joiner = new Thread(() => JoinThread());
            joiner.Start();
            instance.sendToComputer(new Q_JOIN_REQUEST(), ip);
        }
    }

    private void JoinThread() {
        //Debug.Log("Odliczanie start !!!");
        Thread.Sleep(joinTimeout);
        joinSemaphore = null;
        joiner = null;
        //Debug.Log("Odliczanie null !!!");
        //////////////////////////////
    }

    public IPEndPoint getJoinIp() {
        return joinSemaphore;
    }

    //zmienia stan sieci na kliencki
    public void acceptJoin(IPEndPoint ip) {
        setStateClient(ip);
        if (joiner != null) joiner.Abort();
        joinSemaphore = null;
        joiner = null;

        connector = new Thread(() => AliveThread());
        connector.Start();
    }

    private void AliveThread() {
        while (getNetworkState() == NetworkState.NET_CLIENT) {
            /////////////////////////////////////
            Thread.Sleep(aliveTimeout);
            sendToServer(new Q_IM_ALIVE());
        }
    }

    public void enableNetwork() {
        setStateEnabled();
    }

    public void disableNetwork() {
        setStateDisabled();
    }

    public bool isKnownComputer(IPEndPoint ip) {
        if (networkState == NetworkState.NET_SERVER) {
            var fail = false;
            foreach (var c in computers)
                if (Equals(c.ip, ip))
                    fail = true;
            if (!fail) return true;
        }

        return false;
    }

    public bool addComputer(IPEndPoint ip) {
        if (networkState == NetworkState.NET_SERVER) {
            var fail = false;
            foreach (var c in computers)
                if (Equals(c.ip, ip))
                    fail = true;
            if (!fail) {
                computers.Add(new Computer {ip = ip});
                return true;
            }
        }

        return false;
    }

    ~NetworkManager() {
        if (receiver != null) {
            receiver.Abort();
            if (listener != null) listener.Close();
        }
    }

    //server nie odbiera wiadomości od obcych komputerów (start gry)
    public void setLockMode() {
        lockMode = true;
    }

    public void kill() {
        stopReceiver();
    }

    public NetworkState getNetworkState() {
        return networkState;
    }

    public void setComputerTimeZero(IPEndPoint ip) {
        if (networkState != NetworkState.NET_SERVER) return;
        foreach (var comp in computers)
            if (Equals(comp.ip, ip))
                comp.offlineTime = 0;
    }

    public void setServerTimeZero() {
        if (networkState != NetworkState.NET_CLIENT) return;
        serverOfflineTime = 0;
    }

    public void update() {
        if (networkState == NetworkState.NET_SERVER) {
            foreach (var comp in computers) {
                if (Equals(myIp, comp.ip)) continue;
                var dTime = Time.deltaTime;
                if (dTime > 1) dTime = 1;
                comp.offlineTime += dTime;
            }

            var fakeList = computers;
            for (var i = 0; i < fakeList.Count; ++i) {
                var comp = fakeList[i];
                if (comp.offlineTime > kickTimeout) kickComputer(comp.ip);
            }
        }

        if (networkState == NetworkState.NET_CLIENT) {
            var dTime = Time.deltaTime;
            if (dTime > 1) dTime = 1;
            serverOfflineTime += dTime;
        }

        if (disableTrigger) {
            Debug.Log("Nieoczekiwany błąd. Sieć wyłączona.");
            disableTrigger = false;
            setStateDisabled();
        }

        if (listenerErrorTrigger) {
            listenerErrorTrigger = false;
            Debug.Log("Błąd podczas tworzenia nasłuchiwania na porcie " + port +
                      ". Możliwe, że jest z jakiegoś powodu zajęty. Próbuje naprawić problem.");
            if (networkState == NetworkState.NET_SERVER || networkState == NetworkState.NET_CLIENT) {
                //setStateDisabled();
                setStateEnabled();
                Debug.Log("Nie można naprawić problemu. Port jest blokowany przez inną aplikację.");
            }

            if (networkState == NetworkState.NET_ENABLED) {
                connectionPort++;
                setStateEnabled();
                Debug.Log("Port został zmieniony na " + port);
            }
        }

        sendAllQueriesInQueue();
        executeAllQueriesInQueue();
        if (serverOfflineTime > kickTimeout) {
            Debug.Log("Rozłączono z serwerem - TimeoutKick");
            //GameInstance.pauseMenu.GoToMainMenu();
            setStateDisabled();
        }
    }

    private void sendAllQueriesInQueue() {
        if (networkState != NetworkState.NET_DISABLED && sendQueue != null)
            for (; sendQueue.Count > 0;) {
                var queue = sendQueue.Dequeue();
                var json = QueryPack.getJson(queue.qp);
                sendObject(json, queue.endpoint);
            }
    }

    private void executeAllQueriesInQueue() {
        if (networkState != NetworkState.NET_DISABLED && receiveQueue != null)
            for (; receiveQueue.Count > 0;) {
                var queue = receiveQueue.Dequeue();
                var query = Q_OBJECT.Deserialize(queue.qp.json, queue.qp.type);
                query.executeQuery(queue);
                if (networkState == NetworkState.NET_DISABLED) return;
            }
    }

    public IPAddress getMyIp() {
        var localIPs = Dns.GetHostAddresses(Dns.GetHostName());
        IPAddress ipAddress = null;
        foreach (var a in localIPs)
            if (a.AddressFamily == AddressFamily.InterNetwork)
                ipAddress = a;
        return ipAddress;
    }

    private void sendObject(string json, IPEndPoint ip) {
        var sendbuf = Encoding.UTF8.GetBytes(json);
        s.SendTo(sendbuf, ip);
        //Debug.Log("Message sent: " + json);
    }

    private void setStateServer() {
        setStateDisabled();
        port = broadcastPort;
        myIp = new IPEndPoint(getMyIp(), port);
        runReceiver();
        if (networkState == NetworkState.NET_SERVER) return;
        networkState = NetworkState.NET_SERVER;
        players = new List<PlayerInfo>();
        computers = new List<Computer>();
        addComputer(myIp);
        serverIp = new IPEndPoint(getMyIp(), broadcastPort);
    }

    private void setStateClient(IPEndPoint serverIp) {
        setStateDisabled();
        runReceiver();
        if (networkState == NetworkState.NET_CLIENT) return;
        networkState = NetworkState.NET_CLIENT;
        this.serverIp = serverIp;
    }

    private void setStateEnabled() {
        port = connectionPort;
        setStateDisabled();
        runReceiver();
        if (networkState == NetworkState.NET_ENABLED) return;
        networkState = NetworkState.NET_ENABLED;
    }

    private void setStateDisabled() {
        serverOfflineTime = 0;
        lockMode = false;
        //port = connectionPort;
        myIp = new IPEndPoint(getMyIp(), port);
        if (networkState == NetworkState.NET_DISABLED) return;
        sendQueue = null;
        receiveQueue = null;
        players = null;
        computers = null;
        serverIp = null;
        stopReceiver();
        networkState = NetworkState.NET_DISABLED;
    }

    private void runReceiver() {
        if (receiver == null) {
            sendQueue = new Queue<QueuePack>();
            receiveQueue = new Queue<QueuePack>();
            receiver = new Thread(() => ReceiverThread(Thread.CurrentThread, listenerCounter++));
            receiver.Start();
        }
    }

    private void stopReceiver() {
        if (receiver != null) {
            receiver.Abort();
            if (listener != null) listener.Close();
            receiver = null;
        }
    }

    private void ReceiverThread(Thread main, int id = 0) {
        //Debug.Log("id:"+id+" NetworkManager - ReceiverThread Start");
        try {
            var done = false;

            var groupEP = new IPEndPoint(IPAddress.Any, port);
            listener = new UdpClient(port);
            listener.Client.ReceiveTimeout = receiverTimeout;
            while (!done) {
                //Thread.Sleep(1000);
                if (!main.IsAlive) throw new Exception("NetworkManager - Aplikacja zamknieta");
                try {
                    //Debug.Log("Waiting for broadcast");
                    var bytes = listener.Receive(ref groupEP);

                    //Debug.Log("Odebrano");
                    //Debug.Log(Encoding.UTF8.GetString(bytes, 0, bytes.Length));
                    var json = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                    var queryPack = JsonUtility.FromJson<QueryPack>(json);
                    var queuePack = new QueuePack();
                    queuePack.endpoint = groupEP;
                    queuePack.endpoint.Port = queryPack.port;
                    queuePack.qp = queryPack;
                    processQueueMessage(queuePack);
                    //Console.WriteLine("Received broadcast from {0} :\n {1}\n",groupEP.ToString(),Encoding.ASCII.GetString(bytes, 0, bytes.Length));                
                }
                catch (Exception e) {
                    //Debug.Log("Blad in");
                }
            }

            listener.Close();
        }
        catch (ThreadAbortException e) {
            //Debug.Log("id:" + id + " Abort");        
        }
        catch (SocketException e) {
            Debug.Log("id:" + id + " Port error");
            listenerErrorTrigger = true;
        }
        catch (Exception e) {
            Debug.Log("id:" + id + " Blad");
            disableTrigger = true;
        }
        finally {
            //Debug.Log("id:" + id + " NetworkManager - ReceiverThread Stop");
        }
    }

    private void processQueueMessage(QueuePack queuePack) {
        var wtf = !Equals(queuePack.endpoint, serverIp);
        if (networkState == NetworkState.NET_CLIENT && wtf) return;
        if (networkState == NetworkState.NET_SERVER && lockMode && !isKnownComputer(queuePack.endpoint)) return;
        switch (queuePack.qp.sendMode) {
            case SendMode.SM_BROADCAST:
                if (!Equals(queuePack.endpoint, myIp))
                    receiveQueue.Enqueue(queuePack);
                break;
            case SendMode.SM_ALL_IN_NETWORK:
                receiveQueue.Enqueue(queuePack);
                if (networkState == NetworkState.NET_SERVER) {
                    var source = queuePack.endpoint;
                    foreach (var computer in computers) {
                        if (Equals(source, computer.ip) || Equals(myIp, computer.ip)) continue;
                        var tmp2 = new QueuePack();
                        tmp2.endpoint = computer.ip;
                        tmp2.qp = queuePack.qp;
                        tmp2.qp.port = serverIp.Port;
                        sendQueue.Enqueue(tmp2);
                    }
                }

                break;
            case SendMode.SM_PLAYER:
                if (networkState == NetworkState.NET_SERVER)
                    foreach (var player in players)
                        if (player.id == queuePack.qp.targetPlayerId) {
                            queuePack.endpoint = player.ip;
                            sendQueue.Enqueue(queuePack);
                            break;
                        }
                else
                    receiveQueue.Enqueue(queuePack);

                break;
            case SendMode.SM_TO_SERVER_TO_ALL:
                foreach (var comp in computers) {
                    var ip = comp.ip;
                    var tmp = queuePack.qp;
                    tmp.sendMode = SendMode.SM_COMPUTER;
                    var queue = new QueuePack();
                    queue.qp = tmp;
                    queue.endpoint = ip;
                    sendQueue.Enqueue(queue);
                }

                break;
            case SendMode.SM_TO_SERVER:
                receiveQueue.Enqueue(queuePack);
                break;
            default:
                receiveQueue.Enqueue(queuePack);
                break;
        }
    }

    /*public void SetGameInstance(MainGame mainGame) {
        if (GameInstance == null)
            GameInstance = mainGame;
    }*/
}