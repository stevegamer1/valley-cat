using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Window : MonoBehaviour
{
    [SerializeField] SpriteRenderer openedSprite;
    [SerializeField] SpriteRenderer closedSprite;
    [SerializeField] float switchInterval = 1f;

    float nextSwitchTime;

    void Update()
    {
        if (Time.time > nextSwitchTime)
        {
            nextSwitchTime = Time.time + Random.value * switchInterval;
            openedSprite.enabled = !openedSprite.enabled;
            closedSprite.enabled = !closedSprite.enabled;
        }
    }
}
