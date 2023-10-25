using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;


// responsible for movement
// has its own state
// works with CatInputReceiver
// essentially a brainless zombie
public class CatPawn : MonoBehaviour
{
    [SerializeField] float onGroundSpeed = 5.0f;
    [SerializeField] float inAirSpeed = 2.0f;
    [SerializeField] float decelerateTime = 1.0f;  // time of full in-air deceleration from onGroundSpeed to inAirSpeed
    [SerializeField] float climbSpeed = 3.0f;
    [SerializeField] float jumpStrength = 10.0f;
    [SerializeField] float fallThroughStrength = 2.0f;
    [SerializeField] Vector2 boundsForFallThrough = new Vector2(1f, .1f);
    [SerializeField] Vector2 boundsForGroundCheck = new Vector2(1f, .1f);
    [SerializeField] Vector2 boundsForClimbCheck = new Vector2(1f, 1f);
    [SerializeField] LayerMask groundMask;
    [SerializeField] LayerMask climbableMask;
    [SerializeField] float climbInterval = 0.5f;
    [SerializeField] float coyoteInterval = 0.3f;
    [SerializeField] float anticoyoteInterval = 0.3f;
    [SerializeField] float coyoteRechargeTime = 0.3f;
    [SerializeField] Vector2 climbOffset;  // vector from object's pivot to point that should stay in climbable rectangles
    [SerializeField] Animator anim;
    Rigidbody2D rb;

    bool haveCoyote;
    bool canRechargeCoyote = false;  // coyote can be recharged only if cat was previously in air
    float lastClimbTime;
    float lastGroundTime;
    float lastNoGroundTime;
    ClimbableRectangle climbable;
    Vector2 climbPos;
    MovementMode movementMode = MovementMode.Usual;
    float currentSpeed;

    public enum MovementMode
    {
        Usual = 0,  // running, jumping and all that
        Climb = 1   // climb on pants and socks
    }

    public MovementMode GetMovementMode()
    {
        return movementMode;
    }

    bool IsOnGround()
    {
        var hit = Physics2D.BoxCast(transform.position, boundsForGroundCheck, 0f, Vector2.zero, 0f, groundMask);
        return (bool)hit;
    }

    bool AnticoyoteTime()
    {
        return IsOnGround() && Time.time - lastNoGroundTime > anticoyoteInterval;
    }

    bool CoyoteTime()
    {
        return !IsOnGround() && Time.time - lastGroundTime < coyoteInterval && haveCoyote;
    }

    // Fall through all platforms that have FallThroughPlatform component
    // Return true if fell through a platform
    bool DeactivateFallthroughPlatforms()
    {
        bool result = false;
        List<RaycastHit2D> hits = new List<RaycastHit2D>();

        ContactFilter2D filter = new ContactFilter2D();
        filter.NoFilter();  // to make sure that the filter is clear
        filter.SetLayerMask(groundMask);

        Physics2D.BoxCast(transform.position, boundsForFallThrough, 0f, Vector2.zero, filter, hits, 0f);

        foreach (var hit in hits)
        {
            var candidate = hit.transform.GetComponent<FallThroughPlatform>();
            if (candidate)
            {
                result = true;
                candidate.Trigger();  // disable for a short time
            }
        }

        return result;
    }

    public bool Jump()
    {
        bool canJump = AnticoyoteTime() || CoyoteTime();

        if (canJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpStrength);
            haveCoyote = false;
            canRechargeCoyote = false;
        }

        return canJump;
    }

    public bool Fallthrough()
    {
        bool result = DeactivateFallthroughPlatforms();
        if (result)
        {
            rb.AddForce(Vector2.down * fallThroughStrength, ForceMode2D.Impulse);
        }
        return result;
    }

    public void Run(float direction)  // horizontal
    {
        rb.velocity = new Vector2(direction * currentSpeed, rb.velocity.y);
        anim.SetFloat("HorizontalVelocity", direction);
    }

    // accelerate if on ground (cat runs)
    // decelerate if in air (cat's willpower no longer moves him)
    void UpdateSpeed()
    {
        if (IsOnGround())
        {
            currentSpeed = onGroundSpeed;
        }
        else
        {
            currentSpeed += (inAirSpeed - onGroundSpeed) * Time.deltaTime / decelerateTime;
            currentSpeed = Mathf.Max(currentSpeed, inAirSpeed);
        }
    }

    // Unattach and get an impulse
    public bool JumpOffClimbable(Vector2 direction)
    {
        lastClimbTime = Time.time;
        movementMode = MovementMode.Usual;
        rb.velocity = direction * jumpStrength;
        anim.SetBool("IsClimbing", false);
        return true;
    }

    // move along the current climbable thing
    public void MoveAlongClimbable(Vector2 direction)
    {
        climbPos += direction * Time.deltaTime * climbSpeed;
        var climbBounds = climbable.GetBounds();
        climbPos.x = Mathf.Clamp(climbPos.x, -climbBounds.x, +climbBounds.x);
        climbPos.y = Mathf.Clamp(climbPos.y, -climbBounds.y, +climbBounds.y);
        anim.SetFloat("HorizontalVelocity", direction.x);
    }

    // try to switch to climb mode (but may fail if timer hasn't expired)
    bool TryClimb(ClimbableRectangle climbable)
    {
        if (Time.time - lastClimbTime > climbInterval)
        {
            this.climbable = climbable;
            movementMode = MovementMode.Climb;
            anim.SetBool("IsClimbing", true);
            climbPos = transform.position - climbable.transform.position;
            MoveAlongClimbable(Vector2.zero);  // so that the position resets
            return true;
        }
        return false;
    }

    public bool TryClimbAnything()
    {
        if (this.movementMode == MovementMode.Usual)
        {
            Vector2 boxCastOrigin = new Vector2(transform.position.x, transform.position.y) + climbOffset;
            var hit = Physics2D.BoxCast(boxCastOrigin, boundsForClimbCheck, 0f, Vector2.zero, 0f, climbableMask, -0.1f, +0.1f);
            if (hit)
            {
                ClimbableRectangle climbable = hit.collider.GetComponent<ClimbableRectangle>();
                if (climbable) {
                    return TryClimb(climbable);
                }
            }
        }
        return false;
    }

    void Start()
    {
        lastGroundTime = Time.time;
        lastNoGroundTime = Time.time;
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.Log("No rigidbody!");
        }
        if (anim == null)
        {
            Debug.Log("No animator!");
        }
        currentSpeed = onGroundSpeed;
    }

    void FixedUpdate()
    {
        if (movementMode == MovementMode.Climb)
        {
            Vector2 orig = climbable.transform.position;
            Vector2 newPosition = new Vector2(orig.x, orig.y) + climbPos;
            rb.MovePosition(newPosition);
        }

        if (IsOnGround())
        {
            lastGroundTime = Time.time;
            if (Time.time - lastNoGroundTime > coyoteRechargeTime) {
                if (canRechargeCoyote)
                {
                    canRechargeCoyote = false;
                    haveCoyote = true;
                }
            }
            anim.SetBool("OnGround", true);
        }
        else
        {
            canRechargeCoyote = true;
            lastNoGroundTime = Time.time;
            anim.SetBool("OnGround", false);
        }

        UpdateSpeed();
    }
}
