using UnityEngine;

public class Collisions : MonoBehaviour
{
    private void OnCollisionStay(Collision collision)
    {

        
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Debug.Log($"Collision with obstacle detected! {collision.gameObject.name}");
            // Handle collision with obstacle
        }
    }
}
