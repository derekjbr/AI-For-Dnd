using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class DashAction : Action
{
    public ActorController Actor;

    public DashAction(ActorController actor)
    {
        ActionName = "DashAction";
        Outcomes.Add("IsNotExhausted", 1);

        Actor = actor;
        ActionCost = 0;
        BonusActionCost = 1;
    }

    public override void ApplyOutcomes(Dictionary<string, int> conditions, Dictionary<HealthItem, int> healthItems, ref ActorStats states, bool realRoll = false)
    {
        base.ApplyOutcomes(conditions, healthItems, ref states);

        states.RemainingDistance += (states.MaxDistance / 2f);
    }

    public override bool IsAchievable(Dictionary<string, int> conditions, Dictionary<HealthItem, int> healthItems, ActorStats states)
    {
        return base.IsAchievable(conditions, healthItems, states);
    }
}