using System.Net;
using UnityEngine;

public class ComputersListManagerClient : ComputersListManager
{
    public void addElement(IPEndPoint ip, string name, bool isAi, Color color, int id) {
        foreach (var a in comps)
            if (Equals(a.ip, ip)) {
                a.players.Add(new playerElement {name = name, isAi = isAi, color = color, id = id});
                return;
            }

        var ele = new ComputersListElement();
        ele.ip = ip;
        ele.players.Add(new playerElement {name = name, isAi = isAi, color = color, id = id});
        comps.Add(ele);
    }
}