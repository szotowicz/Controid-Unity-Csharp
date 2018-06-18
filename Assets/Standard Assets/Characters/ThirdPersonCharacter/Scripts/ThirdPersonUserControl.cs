using System;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof(ThirdPersonCharacter))]
    public class ThirdPersonUserControl : MonoBehaviour
    {
        private ThirdPersonCharacter m_Character; // A reference to the ThirdPersonCharacter on the object
        private Transform m_Cam;                  // A reference to the main camera in the scenes transform
        private Vector3 m_CamForward;             // The current forward direction of the camera
        private Vector3 m_Move;
        private bool m_Jump;                      // the world-relative desired move direction, calculated from the camForward and user input.

        public bool IsAutoRunner;
        private const float velocityOfChangingPosition = 5;
        private float currentVelocityOfChangingPosition = 0;
        private float direction = 0;
        private bool isMoving = false;
        private Dictionary<string, float> possiblePaths = new Dictionary<string, float>()
        {
            {"left", -2.0f },
            {"middle", 0.0f },
            {"right", 2.0f }
        };
        
        private void Start()
        {
            // get the transform of the main camera
            if (Camera.main != null)
            {
                m_Cam = Camera.main.transform;
            }
            else
            {
                Debug.LogWarning(
                    "Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.", gameObject);
                // we use self-relative controls in this case, which probably isn't what the user wants, but hey, we warned them!
            }

            // get the third person character ( this should never be null due to require component )
            m_Character = GetComponent<ThirdPersonCharacter>();
        }

        private void Update()
        {
            if (!m_Jump)
            {
                m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
            }
        }

        public void JumpCharacter()
        {
            m_Jump = true;
        }

        public bool TurnRight()
        {
            if (direction == possiblePaths["middle"])
            {
                currentVelocityOfChangingPosition = velocityOfChangingPosition;
                direction = possiblePaths["right"];
                isMoving = true;
                return true;
            }
            else if (direction < possiblePaths["middle"])
            {
                currentVelocityOfChangingPosition = velocityOfChangingPosition;
                direction = possiblePaths["middle"];
                isMoving = true;
                return true;
            }
            return false;
        }

        public bool TurnLeft()
        {
            if (direction == possiblePaths["middle"])
            {
                currentVelocityOfChangingPosition = velocityOfChangingPosition * -1;
                direction = possiblePaths["left"];
                isMoving = true;
                return true;
            }
            else if (direction > possiblePaths["middle"])
            {
                currentVelocityOfChangingPosition = velocityOfChangingPosition * -1;
                direction = possiblePaths["middle"];
                isMoving = true;
                return true;
            }
            return false;
        }

        // Fixed update is called in sync with physics
        private void FixedUpdate()
        {
            // read inputs
            float h = CrossPlatformInputManager.GetAxis("Horizontal");
            float v = CrossPlatformInputManager.GetAxis("Vertical");
            bool crouch = Input.GetKey(KeyCode.C);

            // calculate move direction to pass to character
            if (m_Cam != null)
            {
                // calculate camera relative direction to move:
                m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;

                if (IsAutoRunner)
                {
                    // To right
                    if (h > 0 && !isMoving)
                    {
                        if (!TurnRight())
                        {
                            h = 0;
                        }
                    }
                    // To left
                    if (h < 0 && !isMoving)
                    {
                        if (!TurnLeft())
                        {
                            h = 0;
                        }
                    }

                    // Character constantly run
                    if (isMoving == true)
                    {
                        h = 0;
                    }
                    m_Move = 1 * m_CamForward + h * m_Cam.right;
                    GetComponent<Rigidbody>().velocity = new Vector3(currentVelocityOfChangingPosition, GetComponent<Rigidbody>().velocity.y, GetComponent<Rigidbody>().velocity.z);

                    if (direction == possiblePaths["right"])
                    {
                        if (GetComponent<Rigidbody>().position.x >= possiblePaths["right"])
                        {
                            currentVelocityOfChangingPosition = 0;
                            isMoving = false;
                        }
                    }
                    if (direction == possiblePaths["left"])
                    {
                        if (GetComponent<Rigidbody>().position.x <= possiblePaths["left"])
                        {
                            currentVelocityOfChangingPosition = 0;
                            isMoving = false;
                        }
                    }
                    if (direction == possiblePaths["middle"])
                    {
                        // Approximately value of middle
                        if (GetComponent<Rigidbody>().position.x >= possiblePaths["middle"] - 0.1
                            && GetComponent<Rigidbody>().position.x <= possiblePaths["middle"] + 0.1)
                        {
                            currentVelocityOfChangingPosition = 0;
                            isMoving = false;
                        }
                    }
                }
                else
                {
                    m_Move = v * m_CamForward + h * m_Cam.right;
                }
            }
            else
            {
                // we use world-relative directions in the case of no main camera
                m_Move = v * Vector3.forward + h * Vector3.right;
            }

            // walk speed multiplier
            //if (Input.GetKey(KeyCode.LeftShift)) m_Move *= 0.5f;

            // pass all parameters to the character control script
            m_Character.Move(m_Move, crouch, m_Jump);
            m_Jump = false;
        }
    }
}