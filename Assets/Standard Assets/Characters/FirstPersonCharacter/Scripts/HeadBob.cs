using System;
using UnityEngine;
using UnityStandardAssets.Utility;

namespace UnityStandardAssets.Characters.FirstPerson
{
    public class HeadBob : MonoBehaviour
    {
        public Camera Camera;
        public CurveControlledBob motionBob = new CurveControlledBob();
        public LerpControlledBob jumpAndLandingBob = new LerpControlledBob();
        public RigidbodyFirstPersonController rigidbodyFirstPersonController;
        public float StrideInterval;
        [Range(0f, 1f)] public float RunningStrideLengthen;

       // private CameraRefocus m_CameraRefocus;
        private bool m_PreviouslyGrounded;
        private Vector3 m_OriginalCameraposition;


        private void Start()
        {
            motionBob.Setup(Camera, StrideInterval);
            m_OriginalCameraposition = Camera.transform.localPosition;
       //     m_CameraRefocus = new CameraRefocus(Camera, transform.root.transform, Camera.transform.localposition);
        }


        private void Update()
        {
          //  m_CameraRefocus.GetFocusPoint();
            Vector3 newCameraposition;
            if (rigidbodyFirstPersonController.Velocity.magnitude > 0 && rigidbodyFirstPersonController.Grounded)
            {
                Camera.transform.localPosition = motionBob.DoHeadBob(rigidbodyFirstPersonController.Velocity.magnitude*(rigidbodyFirstPersonController.Running ? RunningStrideLengthen : 1f));
                newCameraposition = Camera.transform.localPosition;
                newCameraposition.y = Camera.transform.localPosition.y - jumpAndLandingBob.Offset();
            }
            else
            {
                newCameraposition = Camera.transform.localPosition;
                newCameraposition.y = m_OriginalCameraposition.y - jumpAndLandingBob.Offset();
            }
            Camera.transform.localPosition = newCameraposition;

            if (!m_PreviouslyGrounded && rigidbodyFirstPersonController.Grounded)
            {
                StartCoroutine(jumpAndLandingBob.DoBobCycle());
            }

            m_PreviouslyGrounded = rigidbodyFirstPersonController.Grounded;
          //  m_CameraRefocus.SetFocusPoint();
        }
    }
}
