using System;
using UnityEngine;

namespace UnityStandardAssets.Utility
{
    public class AutoMoveAndRotate : MonoBehaviour
    {
        public bool ignoreTimescale;
        private float m_LastRealTime;
        public Vector3andSpace moveUnitsPerSecond;
        public Vector3andSpace rotateDegreesPerSecond;


        private void Start() {
            m_LastRealTime = Time.realtimeSinceStartup;
        }


        // Update is called once per frame
        private void Update() {
            var deltaTime = Time.deltaTime;
            if (ignoreTimescale) {
                deltaTime = Time.realtimeSinceStartup - m_LastRealTime;
                m_LastRealTime = Time.realtimeSinceStartup;
            }

            transform.Translate(moveUnitsPerSecond.value * deltaTime, moveUnitsPerSecond.space);
            transform.Rotate(rotateDegreesPerSecond.value * deltaTime, moveUnitsPerSecond.space);
        }


        [Serializable]
        public class Vector3andSpace
        {
            public Space space = Space.Self;
            public Vector3 value;
        }
    }
}