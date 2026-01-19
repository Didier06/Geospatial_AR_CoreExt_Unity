using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Vitesse de rotation en degrés par seconde")]
    public float rotationSpeed = 15f;

    [Tooltip("Axe de rotation (Local). (0,1,0) pour l'axe Y vertical.")]
    public Vector3 rotationAxis = new Vector3(0, 1, 0);

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Space.Self permet une rotation locale (autour des axes de l'objet lui-même)
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime, Space.Self);
    }
}
