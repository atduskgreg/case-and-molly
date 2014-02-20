using UnityEngine;
using System.Collections;

public class ShotgunController : MonoBehaviour
{
    public AudioClip GunShot = null;
    public AudioClip Reload = null;
    public AudioSource Source = null;
    public ExploderMouseLook MouseLookCamera = null;
    public Light Flash = null;
    public Animation ReloadAnim;
    public AnimationClip HideAnim;
    public GameObject MuzzleFlash;

    private int flashing = 0;
    private float reloadTimeout = float.MaxValue;
    private float nextShotTimeout = 0.0f;
    private TargetType lastTarget;

    /// <summary>
    /// exploder script
    /// </summary>
    public ExploderObject exploder = null;

    /// <summary>
    /// on activate this game object
    /// </summary>
    public void OnActivate()
    {
        ExploderUtils.SetActive(MuzzleFlash, false);
    }

    void Update()
    {
        GameObject hitObject = null;
        var targetType = TargetManager.Instance.TargetType;

        // dont shoot when targeting use object
        if (targetType == TargetType.UseObject)
        {
            if (lastTarget != TargetType.UseObject)
            {
//                animation["shotgunHide"].speed = 1;
//                animation.Play("shotgunHide");
            }

            lastTarget = TargetType.UseObject;
        }

        if (lastTarget == TargetType.UseObject)
        {
//            animation["shotgunHide"].speed = -1;
//
//            if (animation["shotgunHide"].time < 0.01f)
//            {
//                animation["shotgunHide"].time = animation["shotgunHide"].length;
//            }
//
//            animation.Play("shotgunHide", AnimationPlayMode.Mix);
        }

        lastTarget = targetType;

        // run raycast against objects in the scene
        var mouseRay = MouseLookCamera.mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));

        UnityEngine.Debug.DrawRay(mouseRay.origin, mouseRay.direction * 10, Color.red, 0);

        if (targetType == TargetType.DestroyableObject)
        {
            hitObject = TargetManager.Instance.TargetObject;
        }

        if (Input.GetMouseButtonDown(0) && nextShotTimeout < 0 && CursorLocking.IsLocked)
        {
            if (targetType != TargetType.UseObject)
            {
                Source.PlayOneShot(GunShot);
                MouseLookCamera.Kick();

                // play reload sound after this timeout
                reloadTimeout = 0.3f;

                // turn on flash light for 5 frames
                flashing = 5;
            }

            if (hitObject)
            {
                // get centroid of the hitting object
                var centroid = ExploderUtils.GetCentroid(hitObject);

                // place the exploder object to centroid position
                exploder.transform.position = centroid;
                exploder.ExplodeSelf = false;

                // adjust force vector to be in direction from shotgun
                exploder.ForceVector = mouseRay.direction.normalized;
//                Utils.Log("ForceVec: " + exploder.ForceVector);
                exploder.Force = 10;
                exploder.UseForceVector = true;

                // fragment pieces
                exploder.TargetFragments = 30;

                // set explosion radius to 5 meters
                exploder.Radius = 1.0f;

                // run explosion
                exploder.Explode();
            }

            nextShotTimeout = 0.6f;
        }

        nextShotTimeout -= Time.deltaTime;

        if (flashing > 0)
        {
            Flash.intensity = 1.0f;
            ExploderUtils.SetActive(MuzzleFlash, true);
            flashing--;
        }
        else
        {
            Flash.intensity = 0.0f;
            ExploderUtils.SetActive(MuzzleFlash, false);
        }

        reloadTimeout -= Time.deltaTime;

        if (reloadTimeout < 0.0f)
        {
            reloadTimeout = float.MaxValue;

            // play reload sound
            Source.PlayOneShot(Reload);

            // play reload animation
            ReloadAnim.Play();
        }
    }
}
