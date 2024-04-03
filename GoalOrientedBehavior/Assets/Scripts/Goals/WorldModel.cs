using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WorldModel
{
    public abstract float calculateDiscontentment();
    public abstract Action nextAction();
    public abstract void applyAction(Action action);
}