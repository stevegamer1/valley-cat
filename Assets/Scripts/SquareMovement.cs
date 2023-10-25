using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareMovement : MonoBehaviour
{
    [SerializeField] float speed = 10.0f;
    [SerializeField] float jumpStrength = 10.0f;
    [SerializeField] float jumpInterval = 0.5f;
    [SerializeField] float fallThroughStrength = 2.0f;
    [SerializeField] Vector2 boundsForFallThrough = new Vector2(1f, .1f);
    [SerializeField] Vector2 boundsForGroundCheck = new Vector2(1f, .1f);
    [SerializeField] LayerMask groundMask;
    [SerializeField] float climbInterval = 0.5f;
    [SerializeField] string climbTag = "Climbable";
    Rigidbody2D rb;
    float lastJumpTime;
    float lastClimbTime;
    Transform climbPoint;  // transform because it can move
    MovementMode movementMode = MovementMode.Usual;

    enum MovementMode
    {
        Usual = 0,
        Climb = 1
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if ( rb == null )
        {
            Debug.Log("No rigidbody!");
        }
    }

    bool IsOnGround()
    {
        var hit = Physics2D.BoxCast(transform.position, boundsForGroundCheck, 0f, Vector2.zero, 0f, groundMask);
        return (bool)hit;
    }

    void FallThrought()
    {
        List<RaycastHit2D> hits = new List<RaycastHit2D>();

        ContactFilter2D filter = new ContactFilter2D();
        filter.NoFilter();  // to make sure that the filter is clear
        filter.SetLayerMask(groundMask);

        Physics2D.BoxCast(transform.position, boundsForFallThrough, 0f, Vector2.zero, filter, hits, 0f);

        foreach (var hit in hits)
        {
            var candidate = hit.transform.GetComponent<FallThroughPlatform>();
            if ( candidate )
            {
                candidate.Trigger();  // disable for a short time
            }
        }
    }

    // Can the character jump right now according to rules?
    // Coyote time and double jump included.
    bool CanJump()
    {
        bool EnoughTimeSinceLastJump = (Time.time - lastJumpTime) > jumpInterval;

        return (IsOnGround() /*|| CanDoubleJump() || CoyoteTime()*/) && EnoughTimeSinceLastJump;
    }

    // Mode of movement when the cat just runs and jumps around
    void UsualModeMove()
    {
        var new_velocity = new Vector2(Input.GetAxis("Horizontal") * speed, 0);

        if (Input.GetAxis("Vertical") > 0.01 && CanJump())
        {
            new_velocity.y = jumpStrength;
            lastJumpTime = Time.time;
        }
        else if (Input.GetAxis("Vertical") < -0.01)
        {
            FallThrought();
            new_velocity.y = rb.velocity.y - fallThroughStrength;
        }
        else
        {
            new_velocity.y = rb.velocity.y;
        }

        rb.velocity = new_velocity;
    }

    void ClimbModeMove()
    {
        rb.MovePosition(climbPoint.position);

        movementMode = MovementMode.Usual;
        if (Input.GetAxis("Horizontal") > 0.01)
        {
            rb.velocity = new Vector2(1.0f, 2.0f);
        }
        else if (Input.GetAxis("Horizontal") < -0.01)
        {
            rb.velocity = new Vector2(-1.0f, 2.0f);
        }
        else if (Input.GetAxis("Vertical") > 0.01)
        {
            rb.velocity = new Vector2(0.0f, 5.0f);
        }
        else
        {
            movementMode = MovementMode.Climb;
            lastClimbTime = Time.time;
        }
    }

    void FixedUpdate()
    {
        if (movementMode == MovementMode.Usual)
        {
            UsualModeMove();
        } else if (movementMode == MovementMode.Climb)
        {
            ClimbModeMove();
        }
    }

    // try to switch to climb mode (but may fail if timer hasn't expired)
    void TryClimb(Transform pointTransform)
    {
        if (Time.time - lastClimbTime > climbInterval)
        {
            climbPoint = pointTransform;
            movementMode = MovementMode.Climb;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if ( collision.transform.tag == climbTag )
        {
            TryClimb(collision.transform);
        }
    }
}
