using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Farm.Transition
{
    public class Teleport : MonoBehaviour
    {
        [SceneName]
        public string targetSceneName;
        
        public Vector3 targetPos;

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.CompareTag("Player"))
            {
                EventHandler.CallTransitionEvent(targetSceneName, targetPos);
            }
        }
    }
}
