using UnityEngine;
using UnityEngine.UI;

public class ServerButton : MonoBehaviour
{
    public Color color = Color.white;
    public string ip = "null";
    public string name = "null";
    public int port;

    private ServerButtonManager sbm;

    // Use this for initialization
    private void Start() {
        var obj = gameObject.transform.Find("ip").gameObject;
        obj.GetComponent<Text>().text = ip;
        obj = gameObject.transform.Find("name").gameObject;
        obj.GetComponent<Text>().text = name;
        var btn = GetComponent<Button>();
        btn.onClick.AddListener(TaskOnClick);
        sbm = GetComponentInParent<ServerButtonManager>();
        setColor(color);
    }

    private void setColor(Color c) {
        var btn = GetComponent<Button>();
        var cb = btn.colors;
        c.a = 0.3f;
        cb.normalColor = c;
        c.a = 0.6f;
        cb.highlightedColor = c;

        btn.colors = cb;
    }

    private void TaskOnClick() {
        sbm.tryToJoin(ip);
        MenuManager.instance.setClientColorAndTitle(name, color);
    }
}