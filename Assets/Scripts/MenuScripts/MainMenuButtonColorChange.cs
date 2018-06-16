using UnityEngine;
using UnityEngine.UI;

public class MainMenuButtonColorChange : MonoBehaviour
{
    public Color color;
    private Image img;
    private Vector3 s0;
    private Vector3 s1;

    public float size;
    private Vector3 stmp;
    private Color tmpColor;

    // Use this for initialization
    private void Start() {
        tmpColor = Color.white;
        img = GetComponent<Image>();
        s0 = transform.localScale;
        s1 = transform.localScale * size;
        stmp = s0;
    }

    // Update is called once per frame
    private void Update() {
        img.color = Color.Lerp(img.color, tmpColor, Time.deltaTime * 6);
        transform.localScale = Vector3.Lerp(transform.localScale, stmp, Time.deltaTime * 12);
    }

    public void buttonOn() {
        tmpColor = color;
        stmp = s1;
    }

    public void buttonOff() {
        tmpColor = Color.white;
        stmp = s0;
    }
}