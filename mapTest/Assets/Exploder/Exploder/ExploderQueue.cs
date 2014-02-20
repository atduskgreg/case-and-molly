// Version 1.2
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using System.Collections.Generic;
using UnityEngine;

namespace Exploder
{
    class ExploderSettings
    {
        public Vector3 Position;
        public Vector3 ForceVector;
        public float Force;
        public float FrameBudget;
        public float Radius;
        public float DeactivateTimeout;
        public int id;
        public int TargetFragments;
        public DeactivateOptions DeactivateOptions;
        public ExploderObject.FragmentOption FragmentOptions;
        public ExploderObject.OnExplosion Callback;
        public bool DontUseTag;
        public bool UseForceVector;
        public bool MeshColliders;
        public bool ExplodeSelf;
        public bool HideSelf;
        public bool DestroyOriginalObject;
        public bool ExplodeFragments;
        public bool SplitMeshIslands;
        public bool processing;
    }

    public class ExploderQueue
    {
        private readonly Queue<ExploderSettings> queue;
        private readonly ExploderObject exploder;

        public ExploderQueue(ExploderObject exploder)
        {
            this.exploder = exploder;
            queue = new Queue<ExploderSettings>();
        }

        public void Explode(ExploderObject.OnExplosion callback)
        {
            var settings = new ExploderSettings
            {
                Position = ExploderUtils.GetCentroid(exploder.gameObject),
                DontUseTag = exploder.DontUseTag,
                Radius = exploder.Radius,
                ForceVector = exploder.ForceVector,
                UseForceVector = exploder.UseForceVector,
                Force = exploder.Force,
                FrameBudget = exploder.FrameBudget,
                TargetFragments = exploder.TargetFragments,
                DeactivateOptions = exploder.DeactivateOptions,
                DeactivateTimeout = exploder.DeactivateTimeout,
                MeshColliders = exploder.MeshColliders,
                ExplodeSelf = exploder.ExplodeSelf,
                HideSelf = exploder.HideSelf,
                DestroyOriginalObject = exploder.DestroyOriginalObject,
                ExplodeFragments = exploder.ExplodeFragments,
                SplitMeshIslands = exploder.SplitMeshIslands,
                FragmentOptions = exploder.FragmentOptions.Clone(),
                Callback = callback,
                processing = false,
            };

            queue.Enqueue(settings);
            ProcessQueue();
        }

        void ProcessQueue()
        {
            if (queue.Count > 0)
            {
                var peek = queue.Peek();

                if (!peek.processing)
                {
                    exploder.DontUseTag = peek.DontUseTag;
                    exploder.Radius = peek.Radius;
                    exploder.ForceVector = peek.ForceVector;
                    exploder.UseForceVector = peek.UseForceVector;
                    exploder.Force = peek.Force;
                    exploder.FrameBudget = peek.FrameBudget;
                    exploder.TargetFragments = peek.TargetFragments;
                    exploder.DeactivateOptions = peek.DeactivateOptions;
                    exploder.DeactivateTimeout = peek.DeactivateTimeout;
                    exploder.MeshColliders = peek.MeshColliders;
                    exploder.ExplodeSelf = peek.ExplodeSelf;
                    exploder.HideSelf = peek.HideSelf;
                    exploder.DestroyOriginalObject = peek.DestroyOriginalObject;
                    exploder.ExplodeFragments = peek.ExplodeFragments;
                    exploder.SplitMeshIslands = peek.SplitMeshIslands;
                    exploder.FragmentOptions = peek.FragmentOptions;

                    peek.id = Random.Range(int.MinValue, int.MaxValue);
                    peek.processing = true;

                    exploder.StartExplosionFromQueue(peek.Position, peek.id, peek.Callback);
                }
            }
        }

        public void OnExplosionFinished(int id)
        {
            var explosion = queue.Dequeue();
            ExploderUtils.Assert(explosion.id == id, "Explosion id mismatch!");
            ProcessQueue();
        }
    }
}
