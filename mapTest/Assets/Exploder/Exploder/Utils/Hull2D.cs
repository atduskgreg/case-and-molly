using System;
using System.Collections.Generic;
using UnityEngine;

namespace Exploder.Utils
{
    /// <summary>
    /// implementation of Andrew's monotone chain convex hull algorithm O(n log n)
    /// reference: http://en.wikibooks.org/wiki/Algorithm_Implementation/Geometry/Convex_hull/Monotone_chain
    /// </summary>
    class Hull2D
    {
        /// <summary>
        /// sort array of points by increasing x and y coordinates
        /// </summary>
        /// <param name="array"></param>
        public static void Sort(Vector2[] array)
        {
            Array.Sort(array, delegate(Vector2 value0, Vector2 value1)
            {
                var comp0 = value0.x.CompareTo(value1.x);

                if (comp0 != 0)
                {
                    return comp0;
                }

                return value0.y.CompareTo(value1.y);
            });
        }

        public static void DumpArray(Vector2[] array)
        {
            foreach (var v in array)
            {
                Debug.Log("V: " + v);
            }
        }

        /// <summary>
        /// computes 2d convex hull of unsorted array of 2d points
        /// </summary>
        /// <param name="Pnts">unsorted array of 2d points</param>
        /// <returns>2d convex hull as a looped path of 2d points</returns>
        public static Vector2[] ChainHull2D(Vector2[] Pnts)
        {
            int n = Pnts.Length, k = 0;

            Sort(Pnts);

            var Hull = new Vector2[2*n];
 
	        // Build lower hull
	        for (int i = 0; i < n; i++)
            {
                while (k >= 2 && Hull2DCross(ref Hull[k - 2], ref Hull[k - 1], ref Pnts[i]) <= 0) k--;
                Hull[k++] = Pnts[i];
	        }
 
	        // Build upper hull
	        for (int i = n-2, t = k+1; i >= 0; i--) {
                while (k >= t && Hull2DCross(ref Hull[k - 2], ref Hull[k - 1], ref Pnts[i]) <= 0) k--;
                Hull[k++] = Pnts[i];
	        }

            var trim = new Vector2[k];

            for (int i = 0; i < k; i++)
            {
                trim[i] = Hull[i];
            }

            return trim;
        }

        static float Hull2DCross(ref Vector2 O, ref Vector2 A, ref Vector2 B)
        {
            return (A.x - O.x) * (B.y - O.y) - (A.y - O.y) * (B.x - O.x);
        }
    }
}
