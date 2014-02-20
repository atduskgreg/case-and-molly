using UnityEngine;

public abstract class UseObject : MonoBehaviour
{
    public float UseRadius = 5.0f;
    public string HelperText = string.Empty;

    public AudioClip UseClip = null;

    public virtual void Use()
    {
        var source = GetComponent<AudioSource>();

        if (source && UseClip)
        {
            source.PlayOneShot(UseClip);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, UseRadius);
    }
}
