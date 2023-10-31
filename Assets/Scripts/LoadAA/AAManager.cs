using System.Collections.Generic;

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
    }
}