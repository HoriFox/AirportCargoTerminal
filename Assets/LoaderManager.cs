using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoaderManager : MonoBehaviour
{
    public Transform parkingPlace;
    public Transform currentPlace;
    public float currentWeight;

    private void Awake()
    {
        currentPlace = parkingPlace;
    }
}
