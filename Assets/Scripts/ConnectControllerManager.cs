using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConnectControllerManager : MonoBehaviour {

    public Text InitialText;
    public Text TryConnectText;
    string textTo = "";
    

    void Start ()
    {
        if (InitialText != null)
        {
            InitialText.text = InitialText.text.Replace("###", NetworkManager.instance.GetMyIp().ToString());
        }
    }
}