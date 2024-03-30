using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCarry : MonoBehaviour
{
    [SerializeField] Transform CarryPosition;
    public bool isCarrying = false;
    public CarryObject carryingObject;

    public void carryObject(CarryObject carryObject) 
    {
        carryObject.transform.parent = CarryPosition;
        carryObject.transform.localPosition = Vector3.zero;
        carryingObject = carryObject;
        isCarrying = true;
    }

    public CarryObject dropObject()
    {
        CarryObject temp = carryingObject;
        carryingObject = null;
        temp.transform.parent = null;
        //Give object to other script
        isCarrying=false;
        return temp;
    }
}
