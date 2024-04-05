using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestaurantSeeker : MonoBehaviour
{
    public TextAsset sourceFile;

    public List<Goal> goals;
    public int numOrderTypes = 3;
    public List<ActionSequence> actions;
    public float TICK_DELAY_S = 5f;

    private SequenceSelector actionSelector;
    private float taskTimer = 0f;

    public void Start()
    {
        // Read goals and actions from xml file
        GoalReader.Parse(sourceFile, out goals, out actions);

        actionSelector = new SequenceSelector();

        Debug.Log("Starting clock. Update cycles occur every " + TICK_DELAY_S + " seconds.");
        InvokeRepeating("Tick", 0f, TICK_DELAY_S);
        Invoke("DoSomething", 0);
    }

    public void DoSomething()
    {
        List<TimedGoal> timedGoals = new List<TimedGoal>();
        foreach (Goal goal in goals)
        {
            timedGoals.Add((TimedGoal)goal);
        }
        ActionSequence chosen = actionSelector.ChooseAction(actions, timedGoals);
        taskTimer = chosen.GetDuration();
        Debug.Log("I shall " + chosen.name + " which will take " + taskTimer + " seconds");

        List<float> changes = chosen.GetGoalChanges(timedGoals);
        for (int idx = 0; idx < goals.Count; idx++)
        {
            goals[idx].value += changes[idx];
        }

        PrintGoals();

        Invoke("DoSomething", taskTimer);
    }

    public void Tick()
    {
        // add a new order
        int orderType = Random.Range(0, numOrderTypes);
        goals[orderType].value++;
        Debug.Log($"New order: {goals[orderType].name}");

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
