using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal
{
    public string name;
    public float value;

    public virtual float GetDiscontentment(float newValue) {
        return newValue * newValue;
    }
}
public class TimedGoal : Goal
{
    public float changeOverTime;
}
public class GoalCriteria
{
    public List<Goal> minimums;
    public List<Goal> maximums;

    public virtual bool Check(List<Goal> goals)
    {
        foreach (Goal minimum in minimums)
        {
            if (goals.Find(x => x.name == minimum.name).value < minimum.value)
                return false;
        }
        foreach (Goal maximum in maximums)
        {
            if (goals.Find(x => x.name == maximum.name).value > maximum.value)
                return false;
        }
        return true;
    }
}

public class Action
{
    public string name;
    public List<Goal> targetGoals;

    public virtual float GetGoalChange(Goal goal) {
        foreach (Goal target in targetGoals)
        {
            if (target.name == goal.name)
                return target.value;
        }
        return 0f;
    }
}
public class TimedAction : Action
{
    public float duration = 0f;
}
public class ActionSequence
{
    public string name;
    public List<TimedAction> actions;
    public GoalCriteria criteria;
    public float minVal = 0f;
    public float maxVal = 5f;

    public List<float> GetGoalChanges(List<TimedGoal> goals)
    {
        List<float> changes = new List<float>();
        List<Goal> baseGoals = new List<Goal>();
        baseGoals.AddRange(goals);
        bool criteriaMet = criteria.Check(baseGoals);

        foreach (Goal goal in goals)
        {
            foreach (TimedAction action in actions)
            {
                if (criteriaMet)
                {
                    float change = action.GetGoalChange(goal);
                    if (goal.value + change > maxVal)
                        change = maxVal - goal.value;
                    else if (goal.value + change < minVal)
                        change = minVal - goal.value;
                    changes.Add(change);
                }
                else
                    changes.Add(minVal);
            }
        }
        return changes;
    }
    public virtual float GetDuration()
    {
        float duration = 0;
        foreach (TimedAction action in actions)
        {
            duration += action.duration;
        }
        return duration;
    }
}