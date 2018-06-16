using UnityEngine;
using UnityEngine.UI;

public class RemovePlayerButton : MonoBehaviour
{
    private PlayerPanel pp;

    // Use this for initialization
    private void Start() {
        pp = GetComponentInParent<PlayerPanel>();
        var btn = GetComponent<Button>();
        btn.onClick.AddListener(TaskOnClick);
        gameObject.SetActive(pp.removeEnabled);
    }

    // Update is called once per frame
    private void Update() { }

    private void TaskOnClick() {
        pp.removePlayer();
    }
}