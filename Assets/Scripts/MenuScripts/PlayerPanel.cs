using UnityEngine;
using UnityEngine.UI;

public class PlayerPanel : MonoBehaviour
{
    private ComputersListManager clm;
    public Color color = new Color(1, 1, 1);
    public bool isAi = false;
    public string name;
    public bool removeEnabled = false;

    private Text text;

    // Use this for initialization
    private void Start() {
        clm = GetComponentInParent<ComputersListManager>();
        var n = transform.Find("Back").Find("name").gameObject;
        var back = transform.Find("Back").gameObject;
        var img = back.GetComponent<Image>();
        if (isAi) img.color = new Color(0.7f, 0.7f, 1.0f);
        img.color = color;
        //img.color += new Color(0.3f, 0.3f, 0.3f);
        text = n.GetComponent<Text>();
        text.text = name;
    }

    // Update is called once per frame
    private void Update() { }

    public void removePlayer() {
        var obj = new Q_REMOVE_PLAYER();
        obj.name = name;
        NetworkManager.instance.SendToServer(obj);
    }
}