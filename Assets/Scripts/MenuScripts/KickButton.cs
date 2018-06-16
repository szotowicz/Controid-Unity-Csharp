using UnityEngine;
using UnityEngine.UI;

public class KickButton : MonoBehaviour
{
    private ComputerPanel sbm;

    // Use this for initialization
    private void Start() {
        sbm = GetComponentInParent<ComputerPanel>();
        var btn = GetComponent<Button>();
        btn.onClick.AddListener(TaskOnClick);
    }

    // Update is called once per frame
    private void Update() { }

    public void TaskOnClick() {
        sbm.kick();
    }
}