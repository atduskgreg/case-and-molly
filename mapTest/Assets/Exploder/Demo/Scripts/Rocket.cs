using UnityEngine;

public class Rocket : MonoBehaviour
{
    public AudioClip GunShot = null;
    public AudioClip Explosion = null;
    public AudioSource Source = null;
    public ParticleEmitter SmokeTrail = null;
    public ParticleEmitter ExplosionSmoke = null;
    public ParticleEmitter ExplosionEffect = null;
    public GameObject RocketStatic = null;
    public Light RocketLight = null;

    public float RocketVelocity = 1.0f;

    public delegate void OnHit(Vector3 pos);
    public OnHit HitCallback;

    private Ray direction;
    private bool launch;
    private float launchTimeout;
    private Transform parent;
    private Vector3 shotPos;
    private float targetDistance;

    void Start()
    {
        parent = transform.parent;

        launchTimeout = float.MaxValue;

        SmokeTrail.emit = false;
        ExplosionEffect.emit = false;
        ExplosionSmoke.emit = false;

        ExploderUtils.SetActive(SmokeTrail.gameObject, true);
        ExploderUtils.SetActive(ExplosionEffect.gameObject, true);
        ExploderUtils.SetActive(ExplosionSmoke.gameObject, true);
        ExploderUtils.SetActive(RocketStatic.gameObject, false);
    }

    /// <summary>
    /// on activate this game object
    /// </summary>
    public void OnActivate()
    {
        ExploderUtils.SetActive(RocketStatic.gameObject, true);

        if (parent)
        {
            ExploderUtils.SetVisible(gameObject, false);
        }
    }

    public void Reset()
    {
        ExploderUtils.SetActive(RocketStatic.gameObject, true);
    }

    public void Launch(Ray ray)
    {
        direction = ray;
        Source.PlayOneShot(GunShot);
        launchTimeout = 0.3f;
        launch = false;

        ExploderUtils.SetActive(RocketStatic.gameObject, false);
        ExploderUtils.SetVisible(gameObject, true);
        gameObject.transform.parent = parent;
        gameObject.transform.localPosition = RocketStatic.gameObject.transform.localPosition;
        gameObject.transform.localRotation = RocketStatic.gameObject.transform.localRotation;
        gameObject.transform.localScale = RocketStatic.gameObject.transform.localScale;
    }

    void Update()
    {
        if (launchTimeout < 0.0f)
        {
            if (!launch)
            {
                launch = true;
                transform.parent = null;

                SmokeTrail.emit = true;

                RocketLight.intensity =2;

                RaycastHit hit;
                direction.origin = direction.origin + direction.direction*2.0f;
                if (Physics.Raycast(direction, out hit, Mathf.Infinity))
                {
                    shotPos = gameObject.transform.position;
                    targetDistance = (gameObject.transform.position - hit.point).sqrMagnitude;
                }
                else
                {
                    targetDistance = 100*100;
                }
            }

            gameObject.transform.position += direction.direction * RocketVelocity * Time.timeScale;

            RocketLight.transform.position = gameObject.transform.position;

            if ((shotPos - gameObject.transform.position).sqrMagnitude > targetDistance)
            {
                Source.PlayOneShot(Explosion);

                HitCallback(gameObject.transform.position);

                launchTimeout = float.MaxValue;
                launch = false;

                SmokeTrail.emit = false;
                ExplosionEffect.gameObject.transform.position = gameObject.transform.position;
                ExplosionSmoke.gameObject.transform.position = gameObject.transform.position;
                ExplosionEffect.Emit(1);
                ExplosionSmoke.Emit(1);

                ExploderUtils.SetVisible(gameObject, false);

                RocketLight.intensity = 0;
            }
        }

        launchTimeout -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.H))
        {
            HitCallback(gameObject.transform.position);
        }
    }
}
