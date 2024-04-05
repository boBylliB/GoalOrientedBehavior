using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Linq;
using UnityEngine;

public static class GoalReader
{
    public static void Parse(TextAsset sourceFile, out List<Goal> goals, out List<Action> actions)
    {
        XDocument xdoc = XDocument.Parse(sourceFile.text);
        goals = new List<Goal>();
        actions = new List<Action>();
        foreach (XElement xelem in xdoc.Root.Elements())
        {
            Goal newGoal = readGoal(xelem);
            Action newAction = readAction(xelem);
            if (newGoal != null)
                goals.Add(newGoal);
            if (newAction != null)
                actions.Add(newAction);
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
            Action action = new Action
            {
                name = xelem.Get<string>("name"),
                targetGoals = new List<Goal>()
            };
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
}

static class Helper
{
    public static T Get<T>(this XElement xelem, string attribute, T defaultT = default)
    {
        XAttribute a = xelem.Attribute(attribute);
        return a == null ? defaultT : (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(a.Value);
    }
}