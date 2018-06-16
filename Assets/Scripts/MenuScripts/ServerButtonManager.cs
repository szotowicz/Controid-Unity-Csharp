using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class serverButtonData
{
    public Color color;
    public string ip;
    public string name;
    public int port;
}

public class ServerButtonManager : MonoBehaviour
{
    public ServerButton buttonPrefab;
    public InputField dirIp;

    private List<serverButtonData> kafelki;

    // Use this for initialization
    private void Awake() {
        kafelki = new List<serverButtonData>();
        //addButton("test name", "123.123.123.123", Color.red);
    }

    // Update is called once per frame
    private void Update() {
        /*if(Input.GetKeyDown(KeyCode.A)) {
            addButton("test name", "123.123.123.123", Color.red, 0);
        }
        if (Input.GetKeyDown(KeyCode.D)) {
            killButtons();
        }*/
    }

    public void killButtons() {
        var kafelkiDelete = new List<serverButtonData>();
        foreach (Transform child in transform)
            if (child != transform) {
                var serbut = child.gameObject.GetComponent<ServerButton>();
                var wystepuje = false;
                foreach (var ele2 in kafelki)
                    if (serbut.ip == ele2.ip && serbut.port == ele2.port) {
                        wystepuje = true;
                        break;
                    }

                if (!wystepuje) Destroy(child.gameObject);
            }

        foreach (var ele2 in kafelki) {
            var wystepuje = false;
            foreach (Transform child in transform)
                if (child != transform) {
                    var serbut = child.gameObject.GetComponent<ServerButton>();
                    if (serbut.ip == ele2.ip && serbut.port == ele2.port) {
                        wystepuje = true;
                        break;
                    }
                }

            if (!wystepuje) {
                var sb = Instantiate(buttonPrefab);
                sb.ip = ele2.ip;
                sb.port = ele2.port;
                sb.name = ele2.name;
                sb.color = ele2.color;
                sb.transform.SetParent(transform, false);
            }
        }

        kafelki.Clear();
    }

    public void addData(string name, string ip, Color color, int port) {
        var czyDodac = true;
        foreach (var dat in kafelki)
            if (dat.ip == ip)
                czyDodac = false;
        if (!czyDodac) return;
        kafelki.Add(new serverButtonData {name = name, ip = ip, color = color, port = port});
    }

    public void tryToJoin(string ip) {
        MenuManager.instance.Connect(ip);
        //Debug.Log(ip);
    }

    public void tryToDirectJoin() {
        var ip = dirIp.text;
        MenuManager.instance.Connect(ip);
        //Debug.Log(ip);
    }

    public void clearData() {
        foreach (Transform child in transform)
            if (child != transform)
                Destroy(child.gameObject);
        kafelki.Clear();
    }
}