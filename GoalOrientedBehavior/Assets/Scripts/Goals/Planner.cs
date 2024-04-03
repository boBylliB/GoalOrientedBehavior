using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActionSelector
{
    public virtual Action chooseAction(List<Action> actions, List<Goal> goals)
    {
        // Find the most valuable goal to try and fulfill
        Goal topGoal = goals[0];
        foreach (Goal goal in goals)
            if (goal.value > topGoal.value)
                topGoal = goal;

        // Find the best action to take
        Action bestAction = actions[0];
        float bestUtility = -bestAction.getGoalChange(topGoal);

        foreach (Action action in actions)
        {
            // Invert the change because utilities are typically scaled so that high values are good,
            // but we want to reduce the value for the goal
            float utility = -action.getGoalChange(topGoal);

            // Look for the lowest change, and therefore the highest utility
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
    public override Action chooseAction(List<Action> actions, List<Goal> goals)
    {
        // Find the action leading to the lowest discontentment
        Action bestAction = null;
        float bestValue = float.PositiveInfinity;

        foreach (Action action in actions)
        {
            float value = discontentment(action, goals);
            if (value < bestValue)
            {
                bestValue = value;
                bestAction = action;
            }
        }

        return bestAction;
    }

    public virtual float discontentment(Action action, List<Goal> goals)
    {
        // Keep a running total
        float discontentment = 0f;

        // Loop through each goal
        foreach (Goal goal in goals)
        {
            // Calculate the post-action value
            float value = goal.value + action.getGoalChange(goal);

            // Get the discontentment of this value
            discontentment += goal.getDiscontentment(value);
        }

        return discontentment;
    }
}