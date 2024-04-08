using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionSelector
{
    public virtual Action ChooseAction(List<Action> actions, List<Goal> goals)
    {
        // Find the most valuable goal to try and fulfill
        Goal topGoal = goals[0];
        foreach (Goal goal in goals)
            if (goal.value > topGoal.value)
                topGoal = goal;

        // Find the best action to take
        Action bestAction = actions[0];
        float bestUtility = bestAction.GetGoalChange(topGoal);

        foreach (Action action in actions)
        {
            float utility = action.GetGoalChange(topGoal);

            // Look for the lowest change (likely negative), and therefore the highest utility
            if (utility < bestUtility)
            {
                bestUtility = utility;
                bestAction = action;
            }
        }

        return bestAction;
    }
}

public class OverallUtility : ActionSelector
{
    public override Action ChooseAction(List<Action> actions, List<Goal> goals)
    {
        // Find the action leading to the lowest discontentment
        Action bestAction = null;
        float bestValue = float.PositiveInfinity;

        foreach (Action action in actions)
        {
            float value = Discontentment(action, goals);
            if (value < bestValue)
            {
                bestValue = value;
                bestAction = action;
            }
        }

        return bestAction;
    }

    public virtual float Discontentment(Action action, List<Goal> goals)
    {
        // Keep a running total
        float discontentment = 0f;

        // Loop through each goal
        foreach (Goal goal in goals)
        {
            // Calculate the post-action value
            float value = goal.value + action.GetGoalChange(goal);
            // Clamp the post-action value to non-negative numbers
            value = Mathf.Max(value, 0);

            // Get the discontentment of this value
            discontentment += goal.GetDiscontentment(value);
        }

        return discontentment;
    }
}

public class TimedUtility : ActionSelector
{
    public override Action ChooseAction(List<Action> actions, List<Goal> goals)
    {
        // Find the action leading to the lowest discontentment
        TimedAction bestAction = null;
        float bestValue = float.PositiveInfinity;

        foreach (TimedAction action in actions)
        {
            float value = Discontentment(action, goals);
            if (value < bestValue)
            {
                bestValue = value;
                bestAction = action;
            }
        }

        return bestAction;
    }

    public virtual float Discontentment(TimedAction action, List<Goal> goals)
    {
        // Keep a running total
        float discontentment = 0f;

        // Loop through each goal
        foreach (TimedGoal goal in goals)
        {
            // Calculate the post-action value
            float value = goal.value + action.GetGoalChange(goal);
            // Calculate the change due to time alone
            value += action.duration * goal.changeOverTime;
            // Clamp the post-action value to non-negative numbers
            value = Mathf.Max(value, 0);

            // Get the discontentment of this value
            discontentment += goal.GetDiscontentment(value);
        }

        return discontentment;
    }
}

public class SequenceSelector
{
    public virtual ActionSequence ChooseAction(List<ActionSequence> actions, List<TimedGoal> goals)
    {
        // Find the action leading to the lowest discontentment
        ActionSequence bestAction = null;
        float bestValue = float.PositiveInfinity;

        foreach (ActionSequence action in actions)
        {
            float value = Discontentment(action, goals);
            if (value < bestValue)
            {
                bestValue = value;
                bestAction = action;
            }
        }

        return bestAction;
    }

    public virtual float Discontentment(ActionSequence action, List<TimedGoal> goals)
    {
        // Keep a running total
        float discontentment = 0f;

        // Loop through each goal
        // Calculate the post-action values
        List<float> changes = action.GetGoalChanges(goals);
        for (int idx = 0; idx < goals.Count; idx++)
        {
            TimedGoal goal = goals[idx];
            float value = goal.value;
            // Calculate the change due to time alone
            value += action.GetDuration() * goal.changeOverTime;
            // Calculate the impact of the action
            value += changes[idx];
            // Clamp the post-action value to the sequence min and max
            value = Mathf.Clamp(value, action.minVal, action.maxVal);

            // Get the discontentment of this value
            discontentment += goal.GetDiscontentment(value);
        }

        return discontentment;
    }
}