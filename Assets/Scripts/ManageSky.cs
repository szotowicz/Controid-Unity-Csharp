using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManageSky : MonoBehaviour
{
    public float RotationSpeed = 1;

    void Update()
    {
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * RotationSpeed);
    }
}
