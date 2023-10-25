using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbableRectangle : MonoBehaviour
{
    [SerializeField] bool useYBound = false;
    Vector2 climbBounds;

    void Start()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider)
        {
            climbBounds = Vector2.right * collider.bounds.extents.x;
            if (useYBound)
            {
                climbBounds.y = collider.bounds.extents.y;
            }
        }
        else
        {
            Debug.Log("No collider!");
            climbBounds = Vector2.up + Vector2.right;
        }
    }

    public Vector2 GetBounds()
    {
        return climbBounds;
    }
}
