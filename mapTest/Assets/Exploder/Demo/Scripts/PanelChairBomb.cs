using System.Collections.Generic;
using UnityEngine;

public class PanelChairBomb : UseObject
{
    public ExploderObject Exploder = null;
    public GameObject ChairBomb = null;
    public AudioSource SourceExplosion = null;
    public AudioClip ExplosionSound = null;
    public Light Flash = null;

    private int flashing = 0;

    public override void Use()
    {
        base.Use();

        Exploder.transform.position = ChairBomb.transform.position;

        // dont destroy exploder game object
        Exploder.ExplodeSelf = false;

        // dont use force vector, default is explosion in every direction
        Exploder.UseForceVector = false;

        // set explosion radius to 5 meters
        Exploder.Radius = 10.0f;

        // fragment pieces
        Exploder.TargetFragments = 300;

        // adjust force
        Exploder.Force = 30;

        // run explosion
        Exploder.Explode(OnExplode);
    }

    private void OnExplode(float timeMS, ExploderObject.ExplosionState state)
    {
        if (state == ExploderObject.ExplosionState.ExplosionStarted)
        {
            // play explosion sound
            SourceExplosion.PlayOneShot(ExplosionSound);

            Flash.gameObject.transform.position = ChairBomb.transform.position;
            Flash.gameObject.transform.position += Vector3.up;

            // turn on flash light for 10 frames
            flashing = 10;
        }
    }

    private void Update()
    {
        if (flashing > 0)
        {
            Flash.intensity = 5.0f;
            flashing--;
        }
        else
        {
            Flash.intensity = 0.0f;
        }
    }
}
