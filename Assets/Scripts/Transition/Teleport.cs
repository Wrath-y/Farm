using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Farm.Transition
{
    public class Teleport : MonoBehaviour
    {
        public string targetSceneName;
        public Vector3 targetPos;

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.CompareTag("Player"))
            {
                Debug.Log(targetSceneName);
                Debug.Log(targetPos);
                EventHandler.CallTransitionEvent(targetSceneName, targetPos);
            }
        }
    }
}
