using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallThroughPlatform : MonoBehaviour
{
    [SerializeField] float reactivationTime = .2f;
    new Collider2D collider;
    // When to activate the collider again after the player fell through it:
    float PlannedReactivationTime;

    void Start()
    {
        collider = GetComponent<Collider2D>();
        if (collider == null )
        {
            Debug.Log("No collider!");
        }
    }

    // Deactivate the collider for a short time
    public void Trigger()
    {
        if (collider)
        {
            collider.enabled = false;
            PlannedReactivationTime = Time.time + reactivationTime;
        }
    }

    void Update()  // It can be done with coroutines, but that's an overkill.
    {
        if (collider && PlannedReactivationTime < Time.time)
        {
            collider.enabled = true;
            PlannedReactivationTime = float.MaxValue;  // so that it doesn't do anything
        }
    }
}
