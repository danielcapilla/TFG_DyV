using UnityEngine;

public interface IPowerUp 
{
    void ApplyEffect(GameObject target);
    void RemoveEffect(GameObject target);
}