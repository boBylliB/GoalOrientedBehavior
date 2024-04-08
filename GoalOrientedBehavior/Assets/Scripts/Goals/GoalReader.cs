using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Linq;
using UnityEngine;

public static class GoalReader
{
    public static void Parse(TextAsset sourceFile, out List<Goal> goals, out List<Action> actions)
    {
        List<ActionSequence> sequences = new List<ActionSequence>();
        List<TimedGoal> timedGoals = new List<TimedGoal>();
        List<TimedAction> timedActions = new List<TimedAction>();
        Parse(sourceFile, out timedGoals, out timedActions, out sequences);
        goals = new List<Goal>();
        actions = new List<Action>();
        foreach (TimedGoal goal in timedGoals)
        {
            goals.Add(goal);
        }
        foreach (TimedAction action in timedActions)
        {
            actions.Add(action);
        }
    }
    public static void Parse(TextAsset sourceFile, out List<TimedGoal> goals, out List<TimedAction> actions)
    {
        List<ActionSequence> sequences = new List<ActionSequence>();
        Parse(sourceFile, out goals, out actions, out sequences);
    }
    public static void Parse(TextAsset sourceFile, out List<TimedGoal> goals, out List<ActionSequence> actionSequences)
    {
        List<TimedAction> actions = new List<TimedAction>();
        Parse(sourceFile, out goals, out actions, out actionSequences);
        foreach (TimedAction action in actions)
        {
            ActionSequence seq = new ActionSequence
            {
                name = action.name,
                actions = new List<TimedAction>(),
                criteria = new GoalCriteria()
                {
                    maximums = new List<Goal>(),
                    minimums = new List<Goal>()
                }
            };
            seq.actions.Add(action);
            actionSequences.Add(seq);
        }
    }
    public static void Parse(TextAsset sourceFile, out List<TimedGoal> goals, out List<TimedAction> actions, out List<ActionSequence> actionSequences)
    {
        XDocument xdoc = XDocument.Parse(sourceFile.text);
        goals = new List<TimedGoal>();
        actions = new List<TimedAction>();
        actionSequences = new List<ActionSequence>();
        foreach (XElement xelem in xdoc.Root.Elements())
        {
            TimedGoal newGoal = readGoal(xelem);
            TimedAction newAction = readAction(xelem);
            ActionSequence newActionSequence = readSequence(xelem);
            if (newGoal != null)
                goals.Add(newGoal);
            if (newAction != null)
                actions.Add(newAction);
            if (newActionSequence != null)
                actionSequences.Add(newActionSequence);
        }
    }

    private static TimedGoal readGoal(XElement xelem)
    {
        if (xelem.Name == "goal")
        {
            TimedGoal goal = new TimedGoal
            {
                name = xelem.Get<string>("name"),
                value = xelem.Get("value", 0f),
                importance = xelem.Get("importance", 1f),
                changeOverTime = xelem.Get("tick", 0f)
            };
            return goal;
        }
        return null;
    }

    private static TimedAction readAction(XElement xelem)
    {
        if (xelem.Name == "action")
        {
            TimedAction action = new TimedAction
            {
                name = xelem.Get<string>("name"),
                targetGoals = new List<Goal>(),
                duration = xelem.Get("duration", 0f)
            };
            foreach (XElement child in xelem.Elements("goal"))
            {
                TimedGoal childGoal = readGoal(child);
                if (childGoal != null)
                    action.targetGoals.Add(childGoal);
            }
            return action;
        }
        return null;
    }

    private static ActionSequence readSequence(XElement xelem)
    {
        // For the xml file, criteria are optional, actions are not
        if (xelem.Name == "sequence")
        {
            ActionSequence seq = new ActionSequence() {
                name = xelem.Get<string>("name"),
                actions = new List<TimedAction>()
            };
            foreach (XElement child in xelem.Elements("action"))
            {
                TimedAction action = (TimedAction)readAction(child);
                if (action != null)
                    seq.actions.Add(action);
                else
                    return null;
            }
            seq.criteria = new GoalCriteria()
            {
                minimums = new List<Goal>(),
                maximums = new List<Goal>()
            };
            foreach (XElement child in xelem.Elements("criteria"))
            {
                Goal minimum = new Goal()
                {
                    name = child.Get<string>("goal"),
                    value = child.Get("min", -1f)
                };
                Goal maximum = new Goal()
                {
                    name = child.Get<string>("goal"),
                    value = child.Get("max", -1f)
                };
                if (minimum.value >= 0f)
                    seq.criteria.minimums.Add(minimum);
                if (maximum.value >= 0f)
                    seq.criteria.maximums.Add(maximum);
            }
            return seq;
        }
        return null;
    }
}

static class Helper
{
    public static T Get<T>(this XElement xelem, string attribute, T defaultT = default)
    {
        XAttribute a = xelem.Attribute(attribute);
        return a == null ? defaultT : (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(a.Value);
    }
}