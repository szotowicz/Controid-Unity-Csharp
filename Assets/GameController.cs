using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

   // public UnityStandardAssets.Characters.ThirdPerson.ThirdPersonCharacter character;

	// Use this for initialization
	void Start () {
        NetworkManager.instance.SetCurrentGame(this);
	}

    public void TurnLeftCharacter()
    {
        UnityStandardAssets.Characters.ThirdPerson.ThirdPersonUserControl thirdPerson = gameObject.GetComponent<UnityStandardAssets.Characters.ThirdPerson.ThirdPersonUserControl>();
        if (thirdPerson != null)
        {
            Debug.Log("lewooo");
            thirdPerson.TurnLeft();
        }
    }

    public void TurnRightCharacter()
    {
        UnityStandardAssets.Characters.ThirdPerson.ThirdPersonUserControl thirdPerson = gameObject.GetComponent<UnityStandardAssets.Characters.ThirdPerson.ThirdPersonUserControl>();
        if (thirdPerson != null)
        {
            Debug.Log("prawooo");
            thirdPerson.TurnRight();
        }
    }

    public void JumpCharacter()
    {
        UnityStandardAssets.Characters.ThirdPerson.ThirdPersonUserControl thirdPerson = gameObject.GetComponent<UnityStandardAssets.Characters.ThirdPerson.ThirdPersonUserControl>();
        if (thirdPerson != null)
        {
            Debug.Log("skokkk");
            thirdPerson.JumpCharacter();
        }
    }
}