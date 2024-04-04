using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal
{
    public string name;
    public float value;

    public float getDiscontentment(float newValue) {
        return newValue * newValue;
    }
}

public class Action
{
    public string name;
    public List<Goal> targetGoals;

    public float getGoalChange(Goal goal) {
        foreach (Goal target in targetGoals)
        {
            if (target.name == goal.name)
                return target.value;
        }
        return 0f;
    }
}