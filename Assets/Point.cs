using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Point : MonoBehaviour
{
    public Color32 colorPoint = new Color32(255, 120, 20, 255);

    private void OnDrawGizmos()
    {
        Gizmos.color = colorPoint;
        Gizmos.DrawWireSphere(transform.position, 0.4f);
    }
}
