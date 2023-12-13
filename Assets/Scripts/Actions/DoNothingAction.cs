using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
public class DoNothingAction : Action
{
    private GoalOrientatedController Actor;
    public DoNothingAction(GoalOrientatedController actor)
    {
        Actor = actor;
        ActionName = "DoNothingAction";
        Preconditions.Add("IsNotExhausted", 0);
        Outcomes.Add("hasCaughtBreath", 0);
        Outcomes.Add("canRunAway", 1);
        CanRepeat = false;
    }

    public override void ApplyOutcomes(Dictionary<string, int> conditions, Dictionary<HealthItem, int> healthItems, ref ActorStats states, bool realRoll = false)
    {
        base.ApplyOutcomes(conditions, healthItems, ref states);
    }

    public override bool IsAchievable(Dictionary<string, int> conditions, Dictionary<HealthItem, int> healthItems, ActorStats states)
    {
        return !Actor.RanThisTurn;
    }
}