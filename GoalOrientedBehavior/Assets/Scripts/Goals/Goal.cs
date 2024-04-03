using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Goal
{
    public float value;
    protected float change;

    public virtual float getDiscontentment(float newValue) { return newValue * newValue; }
    public virtual float getChange() { return change; }
}