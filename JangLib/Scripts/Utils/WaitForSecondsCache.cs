using System.Collections.Generic;
using UnityEngine;

namespace JangLib
{
    public static class WaitForSecondsCache
    {
        private static Dictionary<float, WaitForSeconds> wfs = new Dictionary<float, WaitForSeconds>();

        public static WaitForSeconds Wait(float sec)
        {
            if (!wfs.ContainsKey(sec))
                wfs[sec] = new WaitForSeconds(sec);

            return wfs[sec];
        }
    }
}
