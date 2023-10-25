using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cooldown
{
    public delegate bool CooldownDelegate();
    float duration;
    float lastTime;

    public Cooldown(float duration) {
        this.duration = duration;
        lastTime = Time.time;
    }

    public bool DoBoolAction(CooldownDelegate action)
    {
        if (Time.time - lastTime > duration) {
            if (action())
            {
                lastTime = Time.time;
                return true;
            }
        }
        return false;
    }

    public void ManualReset()
    {
        lastTime = Time.time;
    }
}
