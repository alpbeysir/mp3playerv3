using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityWorks : MonoBehaviour
{
    private void FixedUpdate()
    {
        transform.Rotate(-Vector3.forward, Time.deltaTime * 100);
    }
}
