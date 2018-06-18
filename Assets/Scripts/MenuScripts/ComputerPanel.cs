using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class ComputerPanel : MonoBehaviour
{
    private Button but;

    private ComputersListManager clm;
    public string ip;

    private Text ipText;
    public bool kickEnabled = true;

    public int port;

    // Use this for initialization
    private void Start() {
        var obj = transform.Find("Back").Find("ip").gameObject;
        ipText = obj.GetComponent<Text>();

        var obj2 = transform.Find("KickButton").gameObject;
        obj2.SetActive(kickEnabled);
        but = obj2.GetComponent<Button>();

        ipText.text = ip;
        //but.enabled = kickEnabled;

        clm = GetComponentInParent<ComputersListManager>();
    }

    // Update is called once per frame
    private void Update() { }

    public void kick() {
        var ipep = new IPEndPoint(IPAddress.Parse(ip), port);
        NetworkManager.instance.KickComputer(ipep);
    }
}