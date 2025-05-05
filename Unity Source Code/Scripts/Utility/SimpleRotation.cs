using System;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class SimpleRotation : MonoBehaviour
{
    [SerializeField] float rotationSpeed;
    [SerializeField] Vector3 rotationVector = Vector3.one;
    [SerializeField] bool doRandomRotationVector;
    [SerializeField] bool doRandomInitialRotation;

    void Start()
    {
        if (doRandomInitialRotation)
        {
            transform.Rotate(Random.Range(-180, 180), Random.Range(-180, 180), Random.Range(-180, 180));
        }

        if (doRandomRotationVector)
        {
            rotationVector = new Vector3(Random.Range(-rotationVector.x, rotationVector.x), Random.Range(-rotationVector.y, rotationVector.y), Random.Range(-rotationVector.z, rotationVector.z));
        }
    }

    void Update()
    {
        transform.Rotate(rotationVector * rotationSpeed * Time.deltaTime);
    }
}
