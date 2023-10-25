using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMoving : MonoBehaviour
{
    [SerializeField] float maxOffset = 5.0f;
    [SerializeField] float minSwitchTime = 0.1f;
    [SerializeField] float maxSwitchTime = 3.0f;
    [SerializeField] float maxSpeed = 5f;
    bool isMoving = false;
    float nextSwitchTime;
    float currentSpeed;
    Vector3 initialPosition;

    private void Start()
    {
        nextSwitchTime = Time.time;
        initialPosition = transform.position;
    }

    void FixedUpdate()
    {
        if (isMoving)
        {
            transform.position += Vector3.right * currentSpeed * Time.deltaTime;
            float newX = Mathf.Clamp(transform.position.x, initialPosition.x - maxOffset, initialPosition.x + maxOffset);
            transform.position = new Vector3 (newX, transform.position.y, transform.position.z);
        }

        if (Time.time > nextSwitchTime)
        {
            isMoving = !isMoving;
            var delay = Random.value * (maxSwitchTime - minSwitchTime) + minSwitchTime;
            currentSpeed = 2 * Random.value * maxSpeed - maxSpeed;
            nextSwitchTime = Time.time + delay;
        }
    }
}
