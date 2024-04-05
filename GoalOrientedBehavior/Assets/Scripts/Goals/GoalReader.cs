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
        Parse(sourceFile, out goals, out actions, out sequences);
    }
    public static void Parse(TextAsset sourceFile, out List<Goal> goals, out List<ActionSequence> actionSequences)
    {
        List<Action> actions = new List<Action>();
        Parse(sourceFile, out goals, out actions, out actionSequences);
        foreach (Action action in actions)
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
            seq.actions.Add((TimedAction)action);
            actionSequences.Add(seq);
        }
    }
    public static void Parse(TextAsset sourceFile, out List<Goal> goals, out List<Action> actions, out List<ActionSequence> actionSequences)
    {
        XDocument xdoc = XDocument.Parse(sourceFile.text);
        goals = new List<Goal>();
        actions = new List<Action>();
        actionSequences = new List<ActionSequence>();
        foreach (XElement xelem in xdoc.Root.Elements())
        {
            Goal newGoal = readGoal(xelem);
            Action newAction = readAction(xelem);
            ActionSequence newActionSequence = readSequence(xelem);
            if (newGoal != null)
                goals.Add(newGoal);
            if (newAction != null)
                actions.Add(newAction);
            if (newActionSequence != null)
                actionSequences.Add(newActionSequence);
        }
    }

    private static Goal readGoal(XElement xelem)
    {
        if (xelem.Name == "goal")
        {
            Goal goal = new Goal
            {
                name = xelem.Get<string>("name"),
                value = xelem.Get("value", 0f)
            };
            return goal;
        }
        return null;
    }

    private static Action readAction(XElement xelem)
    {
        if (xelem.Name == "action")
        {
            Action action;
            if (xelem.Get("duration", -1f) >= 0f)
            {
                action = new TimedAction() { duration = xelem.Get("duration", 0f) };
            }
            else
            {
                action = new Action();
            }
            action.name = xelem.Get<string>("name");
            action.targetGoals = new List<Goal>();
            foreach (XElement child in xelem.Elements("goal"))
            {
                Goal childGoal = readGoal(child);
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
                    name = xelem.Get<string>("name"),
                    value = xelem.Get("min", -1f)
                };
                Goal maximum = new Goal()
                {
                    name = xelem.Get<string>("name"),
                    value = xelem.Get("max", -1f)
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