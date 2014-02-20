// Version 1.2
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

#if !(UNITY_2_6	|| UNITY_2_6_1 || UNITY_3_0	|| UNITY_3_0_0 || UNITY_3_1	|| UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
#define PHYSICS_2D
#endif

using Exploder.MeshCutter;
using UnityEngine;

namespace Exploder
{
    /// <summary>
    /// options for deactivating the fragment
    /// </summary>
    public enum DeactivateOptions
    {
        /// <summary>
        /// fragments remain active until they are needed for next explosion
        /// </summary>
        None,

        /// <summary>
        /// fragment will be deactivated if it is not visible by main camera
        /// </summary>
        DeactivateOutsideOfCamera,

        /// <summary>
        /// fragment will be deactivated after timeout
        /// </summary>
        DeactivateTimeout,
    }

    /// <summary>
    /// options for fadeout fragments
    /// </summary>
    public enum FadeoutOptions
    {
        /// <summary>
        /// fragments will be fully visible until deactivation
        /// </summary>
        None,

        /// <summary>
        /// fragments will fadeout on deactivateTimeout
        /// </summary>
        Fadeout,

        /// <summary>
        /// fragments will scale down to zero on deactivateTimeout
        /// </summary>
        ScaleDown,
    }

    /// <summary>
    /// script component for fragment game object
    /// the only logic here is visibility test against main camera and timeout sleeping for rigidbody
    /// </summary>
    public class Fragment : MonoBehaviour
    {
        /// <summary>
        /// is this fragment explodable
        /// </summary>
        public bool explodable;

        /// <summary>
        /// options for deactivating the fragment after explosion
        /// </summary>
        public DeactivateOptions deactivateOptions;

        /// <summary>
        /// deactivate timeout, valid only if DeactivateOptions == DeactivateTimeout
        /// </summary>
        public float deactivateTimeout = 10.0f;

        /// <summary>
        /// options for fading out fragments after explosion
        /// </summary>
        public FadeoutOptions fadeoutOptions = FadeoutOptions.None;

        /// <summary>
        /// flag if this fragment is visible from main camera
        /// </summary>
        public bool visible;

        /// <summary>
        /// is this fragment active
        /// </summary>
        public bool activeObj;

        /// <summary>
        /// minimum size of fragment bounding box to be explodable (if explodable flag is true)
        /// </summary>
        public float minSizeToExplode = 0.5f;

        /// <summary>
        /// mesh filter component for faster access
        /// </summary>
        public MeshFilter meshFilter;

        /// <summary>
        /// mesh renderer component for faster access
        /// </summary>
        public MeshRenderer meshRenderer;

        /// <summary>
        /// mesh collider component for faster access
        /// </summary>
        public MeshCollider meshCollider;

        /// <summary>
        /// box collider component for faster access
        /// </summary>
        public BoxCollider boxCollider;

#if PHYSICS_2D
        public PolygonCollider2D polygonCollider2D;

        public Rigidbody2D rigid2D;
#endif

        public bool IsSleeping()
        {
#if PHYSICS_2D
            if (rigid2D)
            {
                return rigid2D.IsSleeping();
            }
            return rigidBody.IsSleeping();
#else
            return rigidBody.IsSleeping();
#endif
        }

        public void Sleep()
        {
#if PHYSICS_2D
            if (rigid2D)
            {
                rigid2D.Sleep();
            }
            else
            {
                rigidBody.Sleep();
            }
#else
            rigidBody.Sleep();
#endif
        }

        public void WakeUp()
        {
#if PHYSICS_2D
            if (rigid2D)
            {
                rigid2D.WakeUp();
            }
            else
            {
                rigidBody.WakeUp();
            }
#else
            rigidBody.WakeUp();
#endif
        }

        public void SetConstraints(RigidbodyConstraints constraints)
        {
#if PHYSICS_2D
            if (rigidbody)
            {
                rigidBody.constraints = constraints;
            }
#else
            rigidBody.constraints = constraints;
#endif
        }

        /// <summary>
        /// apply physical explosion to fragment piece
        /// </summary>
        public void ApplyExplosion(Transform meshTransform, Vector3 centroid, Vector3 mainCentroid, ExploderObject.FragmentOption fragmentOption, 
                                    bool useForceVector, Vector3 ForceVector, float force, GameObject original, int targetFragments)
        {
#if PHYSICS_2D
            if (rigid2D)
            {
                ApplyExplosion2D(meshTransform, centroid, mainCentroid, fragmentOption, useForceVector, ForceVector, force, original, targetFragments);
                return;
            }
#endif

            var rigid = rigidBody;

            // apply fragment mass and velocity properties
            var parentVelocity = Vector3.zero;
            var parentAngularVelocity = Vector3.zero;
            var mass = fragmentOption.Mass;

            // inherit velocity and mass from original object
            if (fragmentOption.InheritParentPhysicsProperty)
            {
                if (original && original.rigidbody)
                {
                    var parentRigid = original.rigidbody;

                    parentVelocity = parentRigid.velocity;
                    parentAngularVelocity = parentRigid.angularVelocity;
                    mass = parentRigid.mass / targetFragments;
                }
            }

            var forceVector = (meshTransform.TransformPoint(centroid) - mainCentroid).normalized;
            var angularVelocity = fragmentOption.AngularVelocity * (fragmentOption.RandomAngularVelocityVector ? Random.onUnitSphere : fragmentOption.AngularVelocityVector);

            if (useForceVector)
            {
                forceVector = ForceVector;
            }

            rigid.velocity = forceVector * force + parentVelocity;
            rigid.angularVelocity = angularVelocity + parentAngularVelocity;
            rigid.mass = mass;
        }

#if PHYSICS_2D
        /// <summary>
        /// apply physical explosion to fragment piece (2D case)
        /// </summary>
        void ApplyExplosion2D(Transform meshTransform, Vector3 centroid, Vector3 mainCentroid,
                              ExploderObject.FragmentOption fragmentOption,
                              bool useForceVector, Vector2 ForceVector, float force, GameObject original,
                              int targetFragments)
        {
            var rigid = rigid2D;

            // apply fragment mass and velocity properties
            var parentVelocity = Vector2.zero;
            var parentAngularVelocity = 0.0f;
            var mass = fragmentOption.Mass;

            // inherit velocity and mass from original object
            if (fragmentOption.InheritParentPhysicsProperty)
            {
                if (original && original.rigidbody2D)
                {
                    var parentRigid = original.rigidbody2D;

                    parentVelocity = parentRigid.velocity;
                    parentAngularVelocity = parentRigid.angularVelocity;
                    mass = parentRigid.mass / targetFragments;
                }
            }

            Vector2 forceVector = (meshTransform.TransformPoint(centroid) - mainCentroid).normalized;
            float angularVelocity = fragmentOption.AngularVelocity * (fragmentOption.RandomAngularVelocityVector ? Random.insideUnitCircle.x : fragmentOption.AngularVelocityVector.y);

            if (useForceVector)
            {
                forceVector = ForceVector;
            }

            rigid.velocity = forceVector * force + parentVelocity;
            rigid.angularVelocity = angularVelocity + parentAngularVelocity;
            rigid.mass = mass;
        }
#endif

        /// <summary>
        /// options component for faster access
        /// </summary>
        public ExploderOption options;

        /// <summary>
        /// rigidbody component for faster access
        /// </summary>
        public Rigidbody rigidBody;

        /// <summary>
        /// refresh local members components objects
        /// </summary>
        public void RefreshComponentsCache()
        {
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            meshCollider = GetComponent<MeshCollider>();
            boxCollider = GetComponent<BoxCollider>();
            options = GetComponent<ExploderOption>();
            rigidBody = GetComponent<Rigidbody>();

#if PHYSICS_2D
            rigid2D = GetComponent<Rigidbody2D>();
            polygonCollider2D = GetComponent<PolygonCollider2D>();
#endif
        }

        /// <summary>
        /// this is called from exploder class to start the explosion
        /// </summary>
        public void Explode()
        {
            activeObj = true;
            ExploderUtils.SetActiveRecursively(gameObject, true);
            visibilityCheckTimer = 0.1f;
            visible = true;
            deactivateTimer = deactivateTimeout;
            originalScale = transform.localScale;

            if (explodable)
            {
                tag = ExploderObject.Tag;
            }
        }

        /// <summary>
        /// deactivate this fragment piece
        /// </summary>
        public void Deactivate()
        {
            ExploderUtils.SetActive(gameObject, false);
            visible = false;
            activeObj = false;
        }

        private Vector3 originalScale;
        private float visibilityCheckTimer;
        private float deactivateTimer;

        void Start()
        {
            visibilityCheckTimer = 1.0f;
            RefreshComponentsCache();
            visible = false;
        }

        void Update()
        {
            if (activeObj)
            {
                if (deactivateOptions == DeactivateOptions.DeactivateTimeout)
                {
                    deactivateTimer -= Time.deltaTime;

                    if (deactivateTimer < 0.0f)
                    {
                        Sleep();
                        activeObj = false;
                        ExploderUtils.SetActiveRecursively(gameObject, false);

                        // return fragment to previous fadout state
                        switch (fadeoutOptions)
                        {
                            case FadeoutOptions.Fadeout:
                                break;
                        }
                    }
                    else
                    {
                        var t = deactivateTimer/deactivateTimeout;

                        switch (fadeoutOptions)
                        {
                            case FadeoutOptions.Fadeout:
                                if (meshRenderer.material && meshRenderer.material.HasProperty("_Color"))
                                {
                                    var color = meshRenderer.material.color;
                                    color.a = t;
                                    meshRenderer.material.color = color;
                                }
                                break;

                            case FadeoutOptions.ScaleDown:
                                gameObject.transform.localScale = originalScale*t;
                                break;
                        }
                    }
                }

                visibilityCheckTimer -= Time.deltaTime;

                if (visibilityCheckTimer < 0.0f)
                {
                    var viewportPoint = UnityEngine.Camera.main.WorldToViewportPoint(transform.position);

                    if (viewportPoint.z < 0 || viewportPoint.x < 0 || viewportPoint.y < 0 ||
                        viewportPoint.x > 1 || viewportPoint.y > 1)
                    {
                        if (deactivateOptions == DeactivateOptions.DeactivateOutsideOfCamera)
                        {
                            Sleep();
                            activeObj = false;
                            ExploderUtils.SetActiveRecursively(gameObject, false);
                        }

                        visible = false;
                    }
                    else
                    {
                        visible = true;
                    }

                    visibilityCheckTimer = Random.Range(0.1f, 0.3f);

                    if (explodable)
                    {
                        var size = collider.bounds.size;

                        if (Mathf.Max(size.x, size.y, size.z) < minSizeToExplode)
                        {
                            tag = string.Empty;
                        }
                    }
                }
            }
        }
    }
}
