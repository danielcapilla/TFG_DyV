using Unity.Netcode;
using UnityEngine;

public abstract class PowerUp : NetworkBehaviour, IPowerUp
{
    private float speed = 2f;
    private float amplitudeY = 0.15f;
    private Vector3 startLocalPos;
    [SerializeField] private AudioSource popPickup;

    private void Start()
    {
        startLocalPos = transform.localPosition;
    }
    private void Update()
    {
        transform.localPosition = startLocalPos + Vector3.up * Mathf.Sin(Time.time * speed ) * amplitudeY;
    }

    public abstract void ApplyEffect(GameObject target);
    public abstract void RemoveEffect(GameObject target);
    protected void PlayPopPickUpSound()
    {
        popPickup.Play();
    }
}
