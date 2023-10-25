using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class TrashMovement : MonoBehaviour
{
    [SerializeField] float speed = 10.0f;
    [SerializeField] float jumpForce = 5.0f;
    new Rigidbody2D rigidbody;
    Animator animator;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        if (rigidbody == null )
        {
            Debug.Log("No rigidbody!");
        }
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.Log("No animator!");
        }
    }

    void Update()
    {
        if (rigidbody)
        {
            Vector2 vel = new Vector2(Input.GetAxis("Horizontal") * speed, rigidbody.velocity.y);
            animator.SetBool("IsWalking", math.abs(Input.GetAxis("Horizontal")) > 0);
            if (Input.GetKeyDown(KeyCode.Space))
            {
                vel.y += jumpForce;
            }
            rigidbody.velocity = vel;
        }
    }
}
