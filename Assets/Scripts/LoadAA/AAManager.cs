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
        public int allResourceNum;
        public int doneResourceNum;
        
        public void RegisterLoadPercent(LoadPercent loadPercent)
        {
            if (!loadPercents.Contains(loadPercent))
                loadPercents.Add(loadPercent);
        }
        
        private IEnumerator PrintPercent()
        {
            yield return new WaitForSeconds(0.5f);
            
            for (int i = 0; i < loadPercents.Count; i++)
            {
                LoadPercent l = loadPercents[i];
                StartCoroutine(l.Percent());
                loadPercents.Remove(l);
            }
                
            if (doneResourceNum < allResourceNum)
            {
                StartCoroutine(PrintPercent());
            }
        }

        protected override void Awake()
        {
            base.Awake();
            StartCoroutine(PrintPercent());
        }
    }
}