using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Action
{
    protected float duration;

    public virtual float getGoalChange(Goal goal) { return goal.getChange() * getDuration(); }
    public virtual float getDuration() { return duration; }
}

public class TimeAveragedAction : Action
{
    public float recencyWeight = 0.5f;
    protected float recencyAvg;

    public override float getGoalChange(Goal goal)
    {
        recencyAvg = (1 - recencyWeight) * recencyAvg + goal.getChange() / getDuration();
        return recencyAvg * getDuration();
    }
}