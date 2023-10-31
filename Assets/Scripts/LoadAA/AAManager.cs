using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LoadAA
{
    public class AAManager : Singleton<AAManager>
    {
        private List<LoadPercent> loadPercents = new List<LoadPercent>();
        
        public void RegisterLoadPercent(LoadPercent loadPercent)
        {
            if (!loadPercents.Contains(loadPercent))
                loadPercents.Add(loadPercent);
        }
        
        public void PrintPercent()
        {
            foreach (var loadPercent in loadPercents)
            {
                StartCoroutine(loadPercent.Percent());
            }
        }

        protected void Start()
        {
            PrintPercent();
        }
    }
}