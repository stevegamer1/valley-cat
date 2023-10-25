using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// responsible for cooldowns of actions and all that
// has its own state
// works in pair with CatPawn
// essentially a parasitic mind-controlling worm
public class CatInputReceiver : MonoBehaviour
{
    [SerializeField] float jumpCooldownTime = 0.5f;
    [SerializeField] float fallthroughCooldownTime = 0.5f;
    [SerializeField] float climbCooldownTime = 0.5f;
    Cooldown jumpCooldown;
    Cooldown fallthroughCooldown;
    Cooldown climbCooldown;
    CatPawn catPawn;

    void Start()
    {
        jumpCooldown = new Cooldown(jumpCooldownTime);
        fallthroughCooldown = new Cooldown(fallthroughCooldownTime);
        climbCooldown = new Cooldown(climbCooldownTime);

        catPawn = GetComponent<CatPawn>();
        if (catPawn == null )
        {
            Debug.Log("No cat pawn to control!");
        }
    }

    private void FixedUpdate()
    {
        if (catPawn.GetMovementMode() == CatPawn.MovementMode.Usual)
        {
            ControlPawnUsualMode();
        }
        else if (catPawn.GetMovementMode() == CatPawn.MovementMode.Climb)
        {
            ControlPawnClimbMode();
        }
    }

    void ControlPawnUsualMode()
    {
        // jump if good cooldown
        if (Input.GetAxis("Vertical") > 0.01)
        {
            bool didJump = jumpCooldown.DoBoolAction(catPawn.Jump);
            if (!didJump)
            {
                if (catPawn.TryClimbAnything())
                {
                    climbCooldown.ManualReset();
                }
            }
        }
        // fall through platforms
        if (Input.GetAxis("Vertical") < -0.01)
        {
            fallthroughCooldown.DoBoolAction(catPawn.Fallthrough);
        }
        // run
        catPawn.Run(Input.GetAxis("Horizontal"));
    }

    void ControlPawnClimbMode()
    {
        if (Mathf.Abs(Input.GetAxis("Vertical")) > 0.01)
        {
            Vector2 jumpDirection = Vector2.up * Mathf.Max(0f, Input.GetAxis("Vertical"));
            jumpDirection.Normalize();
            climbCooldown.DoBoolAction(() => { return catPawn.JumpOffClimbable(jumpDirection); });
        }
        else
        {
            catPawn.MoveAlongClimbable(Vector2.right * Input.GetAxis("Horizontal"));
        }
    }
}
