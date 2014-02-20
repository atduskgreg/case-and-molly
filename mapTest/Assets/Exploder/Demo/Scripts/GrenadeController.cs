using Exploder.MeshCutter;
using UnityEngine;

public class GrenadeController : MonoBehaviour
{
    public AudioClip Throw = null;
    public AudioSource Source = null;
    public GrenadeObject Grenade = null;
    public Camera MainCamera = null;

    private float explodeTimer;
    private float throwTimer;
    private bool exploding;

    void Start()
	{
	    throwTimer = float.MaxValue;
	    explodeTimer = float.MaxValue;
	    exploding = false;
	}

	void Update()
    {
	    if (Input.GetKeyDown(KeyCode.G) && !exploding)
	    {
	        throwTimer = 0.4f;

	        Source.PlayOneShot(Throw);

	        explodeTimer = 2.0f;

	        exploding = true;

            Grenade.Throw();

	        ExploderUtils.SetVisible(gameObject, false);
	    }

	    throwTimer -= Time.deltaTime;

        if (throwTimer < 0.0f)
        {
            throwTimer = float.MaxValue;

    	    ExploderUtils.SetVisible(Grenade.gameObject, true);
            ExploderUtils.SetActive(Grenade.gameObject, true);

            Grenade.transform.position = gameObject.transform.position;

            Grenade.rigidbody.velocity = MainCamera.transform.forward*20;
        }

	    explodeTimer -= Time.deltaTime;

	    if (explodeTimer < 0.0f)
	    {
	        Grenade.Explode();
            explodeTimer = float.MaxValue;
	    }

        if (Grenade.ExplodeFinished)
        {
            exploding = false;
            ExploderUtils.SetVisible(gameObject, true);
        }
    }
}
