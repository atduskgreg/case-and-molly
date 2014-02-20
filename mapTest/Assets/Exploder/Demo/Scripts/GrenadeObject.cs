using Exploder.MeshCutter;
using UnityEngine;

public class GrenadeObject : MonoBehaviour
{
    public AudioClip ExplosionSound = null;
    public AudioSource SourceExplosion = null;
    public ParticleEmitter ExplosionEffect = null;
    public Light Flash = null;
    public bool ExplodeFinished;
    public bool Impact;

    private bool throwing;
    private float explodeTimeoutMax;
    private bool explosionInProgress;

    /// <summary>
    /// exploder script
    /// </summary>
    public ExploderObject exploder = null;

    private int flashing = 0;

    public void Throw()
    {
        Impact = false;
        throwing = true;
        explodeTimeoutMax = 5.0f;
        ExplodeFinished = false;
        flashing = -1;
    }

    public void Explode()
    {
        if (explosionInProgress)
        {
            return;
        }

        explosionInProgress = true;
        throwing = false;

        if (!Impact)
        {
            // grenade is still in the air
            explodeTimeoutMax = 5.0f;
        }
        else
        {
            exploder.transform.position = transform.position;

            // dont destroy exploder game object
            exploder.ExplodeSelf = false;

            // dont use force vector, default is explosion in every direction
            exploder.UseForceVector = false;

            // set explosion radius to 5 meters
            exploder.Radius = 5.0f;

            // fragment pieces
            exploder.TargetFragments = 200;

            // adjust force
            exploder.Force = 20;

            // run explosion
            exploder.Explode(OnExplode);

            ExploderUtils.Log("Explode(OnExplode)");

            ExplodeFinished = false;
        }
    }

    private void OnExplode(float timeMS, ExploderObject.ExplosionState state)
    {
        if (state == ExploderObject.ExplosionState.ExplosionStarted)
        {
            // deactivate the grenade game object
            ExploderUtils.SetVisible(gameObject, false);

            // play explosion sound
            SourceExplosion.PlayOneShot(ExplosionSound);

            Flash.gameObject.transform.position = gameObject.transform.position;
            Flash.gameObject.transform.position += Vector3.up;

            // turn on flash light for 5 frames
            flashing = 10;

            ExplosionEffect.gameObject.transform.position = gameObject.transform.position;
            ExplosionEffect.Emit(1);

            ExploderUtils.Log("OnExplode started");
        }

        if (state == ExploderObject.ExplosionState.ExplosionFinished)
        {
            ExplodeFinished = true;
            explosionInProgress = false;

            ExploderUtils.Log("OnExplode finished");
        }
    }

    void OnCollisionEnter(Collision other)
    {
        Impact = true;

        if (!throwing && !ExplodeFinished)
        {
            Explode();
        }
    }

    private void Update()
    {
        if (flashing >= 0)
        {
            if (flashing > 0)
            {
                Flash.intensity = 5.0f;
                flashing--;
            }
            else
            {
                Flash.intensity = 0.0f;
                flashing = -1;
            }
        }

        explodeTimeoutMax -= Time.deltaTime;

        if (!ExplodeFinished && explodeTimeoutMax < 0.0f)
        {
            Impact = true;
            Explode();
        }
    }
}
