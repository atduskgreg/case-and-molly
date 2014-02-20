using UnityEngine;
using System.Collections;

public class RPGController : MonoBehaviour
{
    /// <summary>
    /// exploder script
    /// </summary>
    public ExploderObject exploder = null;
    public ExploderMouseLook MouseLookCamera = null;
    public Rocket Rocket = null;

    private float nextShotTimeout = 0.0f;

	void Start()
	{
	    Rocket.HitCallback = OnRocketHit;
	}

    /// <summary>
    /// on activate this game object
    /// </summary>
    public void OnActivate()
    {
        Rocket.OnActivate();
    }

    void OnRocketHit(Vector3 position)
    {
        nextShotTimeout = 0.6f;

        // place the exploder object to centroid position
        exploder.transform.position = position;
        exploder.ExplodeSelf = false;

        exploder.Force = 20;

        // fragment pieces
        exploder.TargetFragments = 100;

        // set explosion radius to 10 meters
        exploder.Radius = 10.0f;

        exploder.UseForceVector = false;

        // run explosion
        exploder.Explode();

        // reset rocket position
        Rocket.Reset();
    }

    void Update()
    {
        var targetType = TargetManager.Instance.TargetType;

        if (Input.GetMouseButtonDown(0) && nextShotTimeout < 0 && CursorLocking.IsLocked)
        {
            if (targetType != TargetType.UseObject)
            {
                MouseLookCamera.Kick();

                // run raycast against objects in the scene
                var mouseRay = MouseLookCamera.mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
                UnityEngine.Debug.DrawRay(mouseRay.origin, mouseRay.direction * 10, Color.red, 0);

                Rocket.Launch(mouseRay);

                nextShotTimeout = float.MaxValue;
            }
        }

        nextShotTimeout -= Time.deltaTime;
    }
}
