using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalSeeker : MonoBehaviour
{
    public TextAsset sourceFile;

    public List<Goal> goals;
    public List<Action> actions;
    public Action changePerTick = null;
    public const float TICK_DELAY_S = 5f;

    private OverallUtility actionSelector;

    public void Start()
    {
        // Read goals and actions from xml file
        GoalReader.Parse(sourceFile, out goals, out actions);
        foreach (Action action in actions)
        {
            if (action.name == "tick")
            {
                changePerTick = action;
                actions.Remove(action);
                break;
            }
        }
        if (changePerTick == null)
            throw new MissingReferenceException("Could not find a tick action!");

        actionSelector = new OverallUtility();

        Debug.Log("Starting clock. Update cycles occur every " + TICK_DELAY_S + " seconds.");
        InvokeRepeating("Tick", 0f, TICK_DELAY_S);

        Debug.Log("Press E to do something.");
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Action chosen = actionSelector.ChooseAction(actions, goals);
            Debug.Log("I shall " + chosen.name);

            foreach (Goal goal in goals)
            {
                goal.value += chosen.GetGoalChange(goal);
                goal.value = Mathf.Max(goal.value, 0);
            }

            PrintGoals();
        }
    }

    public void Tick()
    {
        // apply change over time
        foreach (Goal goal in goals)
        {
            goal.value += changePerTick.GetGoalChange(goal);
            goal.value = Mathf.Max(goal.value, 0);
        }

        // print results
        PrintGoals();
    }

    // Using the same print format as https://github.com/bslease/GOB/blob/master/GOB%20Project/Assets/GoalSeeker.cs
    // since it looked nice
    public void PrintGoals()
    {
        string goalString = "";
        foreach (Goal goal in goals)
        {
            goalString += goal.name + ": " + goal.value + "; ";
        }
        goalString += "Discontentment: " + CurrentDiscontentment();
        Debug.Log(goalString);
    }

    public float CurrentDiscontentment()
    {
        float total = 0f;
        foreach (Goal goal in goals)
        {
            total += goal.GetDiscontentment(goal.value);
        }
        return total;
    }
}
