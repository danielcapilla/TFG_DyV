using Unity.Netcode;
using UnityEngine;

public abstract class PowerUp : NetworkBehaviour, IPowerUp
{
    private float speed = 2f;
    private float amplitudeY = 0.15f;
    // Parecido al mutex, evitar multiples regodidas
    protected NetworkVariable<bool> isPickedUp = new NetworkVariable<bool>(false);
    [SerializeField] protected float duration = 10f;
    private Vector3 startLocalPos;
    [SerializeField] private AudioSource pickupSound;

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
    protected void PlayPickUpSound()
    {
        pickupSound.Play();
    }
}
