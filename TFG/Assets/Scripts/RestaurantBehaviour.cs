using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestaurantBehaviour : MonoBehaviour
{
    public Transform[] spawnPositions;

    private GameObject restaurant;
    [SerializeField]
    private Transform bucketPosition;
    private int occupiedPositions = 0;
    private void Start()
    {
        spawnPositions = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            spawnPositions[i] = transform.GetChild(i);
        }
        restaurant = this.transform.parent.gameObject;
    }

    public void AddPosition(Transform transform)
    {
        transform.position = spawnPositions[occupiedPositions%spawnPositions.Length].position;
        occupiedPositions++;
    }
    public void RemovePosition(Transform transform)
    {
        transform.position = bucketPosition.position;
        occupiedPositions--;
    }
}
