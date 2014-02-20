// Version 1.2
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using UnityEngine;

namespace Exploder.MeshCutter
{
    /// <summary>
    /// simple implementation of spherical local-sensitive hashing
    /// </summary>
    public class LSHash
    {
//        private readonly float bucketSize;
        private readonly Vector3[] buckets;
        private readonly float bucketSize2;
        private int count;

        public LSHash(float bucketSize, int allocSize)
        {
//            this.bucketSize = bucketSize;
            this.bucketSize2 = bucketSize*bucketSize;
            buckets = new Vector3[allocSize];
            count = 0;
        }

        public int Capacity()
        {
            return buckets.Length;
        }

        public void Clear()
        {
            for (int i = 0; i < buckets.Length; i++)
            {
                buckets[i] = Vector3.zero;
            }
            count = 0;
        }

        public int Hash(Vector3 p)
        {
            for (int i=0; i<count; i++)
            {
                var item = buckets[i];

                float diffX = p.x - item.x;
                float diffY = p.y - item.y;
                float diffZ = p.z - item.z;
                float sqrMag = diffX*diffX + diffY*diffY + diffZ*diffZ;

                if (sqrMag < bucketSize2)
                {
                    return i;
                }
            }

            if (count >= buckets.Length)
            {
                ExploderUtils.Log("Hash out of range: " + count + " "  + buckets.Length);
                return count - 1;
            }

            buckets[count++] = p;
            return count-1;
        }

        public void Hash(Vector3 p0, Vector3 p1, out int hash0, out int hash1)
        {
            float diffX = p0.x - p1.x;
            float diffY = p0.y - p1.y;
            float diffZ = p0.z - p1.z;
            float sqrMag = diffX*diffX + diffY*diffY + diffZ*diffZ;

            if (sqrMag < bucketSize2)
            {
                hash0 = Hash(p0);
                hash1 = hash0;

                return;
            }

            hash0 = Hash(p0);
            hash1 = Hash(p1);
        }
    }
}
