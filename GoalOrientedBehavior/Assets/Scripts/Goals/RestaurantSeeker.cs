using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RestaurantSeeker : Kinematic
{
    public TextAsset sourceFile;

    public List<PointOfInterest> pointsOfInterest;
    public OrderOutput orderOutput;

    public Text actionReadout;
    public Text orderReadout;

    public List<TimedGoal> goals;
    public int numOrderTypes = 3;
    public List<ActionSequence> actions;
    public float TICK_DELAY_S = 5f;

    private Arrive myMoveType;
    private Align myRotateType;

    private SequenceSelector actionSelector;
    private float taskTimer = 0f;
    private ActionSequence chosen;
    private int actionIdx = 0;

    public void Start()
    {
        myMoveType = new Arrive();
        myMoveType.character = this;
        myMoveType.target = myTarget;
        myMoveType.targetRadius = 0.3f;
        myMoveType.slowRadius = 0.6f;

        myRotateType = new Align();
        myRotateType.character = this;
        myRotateType.target = myTarget;
        myRotateType.maxAngularAcceleration = 1000f;
        myRotateType.maxRotation = 180f;

        // Read goals and actions from xml file
        GoalReader.Parse(sourceFile, out goals, out actions);

        actionSelector = new SequenceSelector();

        Debug.Log("Starting clock. Update cycles occur every " + TICK_DELAY_S + " seconds.");
        InvokeRepeating("Tick", 0f, TICK_DELAY_S);
        Invoke("DoSomething", 0);
    }

    protected override void Update()
    {
        orderReadout.text = "ORDERS:";
        for (int idx = 0; idx < goals.Count && idx < numOrderTypes; idx++)
        {
            orderReadout.text += "\n\t" + goals[idx].name + ":\t\t" + goals[idx].value;
        }

        steeringUpdate = new SteeringOutput();
        steeringUpdate.linear = myMoveType.getSteering().linear;
        steeringUpdate.angular = myRotateType.getSteering().angular;
        base.Update();
    }

    public void DoSomething()
    {
        if (CurrentDiscontentment() > 0f)
        {
            chosen = actionSelector.ChooseAction(actions, goals);
            taskTimer = chosen.GetDuration();
            Debug.Log("I shall " + chosen.name + " which will take " + taskTimer + " seconds");
            actionIdx = 0;
            taskTimer = chosen.actions[actionIdx].duration;
            Debug.Log("Entering substep " + chosen.actions[actionIdx].name + " which will take " + taskTimer + " seconds");
            updateMoveTarget();
            actionReadout.text = "Current Action: " + chosen.actions[actionIdx].name;

            Invoke("performGoalChanges", taskTimer);
        }
        else
            Invoke("DoSomething", 0);
    }
    private void updateMoveTarget()
    {
        PointOfInterest newTarget = pointsOfInterest.Find(x => x.actionNames.Contains(chosen.actions[actionIdx].name));
        if (newTarget != null)
        {
            myMoveType.target = newTarget.gameObject;
            myRotateType.target = newTarget.gameObject;
        }
        else
            throw new KeyNotFoundException("Could not find point of interest for " + chosen.actions[actionIdx].name);
    }
    private void performGoalChanges()
    {
        Debug.Log("Completed the goal \"" + chosen.actions[actionIdx].name + "\"");

        foreach (TimedGoal goal in goals)
        {
            float goalChange = chosen.actions[actionIdx].GetGoalChange(goal);
            goal.value += goalChange;
            goal.value = Mathf.Clamp(goal.value, chosen.minVal, chosen.maxVal);
            if (goalChange < 0 && goals.IndexOf(goal) < numOrderTypes)
                orderOutput.AddOrder(goals.IndexOf(goal));
        }

        //List<float> changes = chosen.GetGoalChanges(goals);
        //for (int idx = 0; idx < goals.Count; idx++)
        //{
        //    goals[idx].value += changes[idx];
        //    goals[idx].value = Mathf.Clamp(goals[idx].value, chosen.minVal, chosen.maxVal);
        //}

        PrintGoals();

        ++actionIdx;
        if (actionIdx < chosen.actions.Count)
        {
            taskTimer = chosen.actions[actionIdx].duration;
            Debug.Log("Entering substep " + chosen.actions[actionIdx].name + " which will take " + taskTimer + " seconds");
            updateMoveTarget();
            actionReadout.text = "Current Action: " + chosen.actions[actionIdx].name;

            Invoke("performGoalChanges", taskTimer);
        }
        else
        {
            actionIdx = 0;
            Debug.Log("Completed the full sequence \"" + chosen.name + "\"");
            Invoke("DoSomething", 0);
        }
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
