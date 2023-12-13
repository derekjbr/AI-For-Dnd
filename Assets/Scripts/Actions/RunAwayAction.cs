using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
public class RunAwayAction : Action
{
    private GoalOrientatedController Actor;
    private float AffectedDistance;
    private Vector3 NewPosition;
    private List<ActorController> Enemies;
    public RunAwayAction(GoalOrientatedController actor, List<ActorController> enemies) { 
        Actor = actor;
        Enemies = enemies;
        ActionName = "RunAwayAction";
        Preconditions.Add("canRunAway", 0);
        Outcomes.Add("hasRetreated", 0);
        ActionCost = 0;
        BonusActionCost = 0;
        CanRepeat = true;
    }

    public override void ApplyOutcomes(Dictionary<string, int> conditions, Dictionary<HealthItem, int> healthItems, ref ActorStats states, bool realRoll = false)
    {
        base.ApplyOutcomes(conditions, healthItems, ref states);
        bool result = Actor.VerifyValidRetreatPoint(states.position, states.RemainingDistance, Enemies, out AffectedDistance, out NewPosition);

        states.position = NewPosition;
        states.RemainingDistance -= AffectedDistance;
    }

    public override bool IsAchievable(Dictionary<string, int> conditions, Dictionary<HealthItem, int> healthItems, ActorStats states)
    {
        if (!base.IsAchievable(conditions, healthItems, states) || states.RemainingDistance < 1f || Actor.NumTernsRunning >= 3)
            return false;

        bool knowsAboutAnyEnemy = false;
        foreach(ActorController actor in Enemies)
        {
            knowsAboutAnyEnemy = conditions.ContainsKey("IsAwareOf" + actor.GetInstanceID()) || knowsAboutAnyEnemy;
        }

        if (!knowsAboutAnyEnemy)
            return false;

        bool output = Actor.VerifyValidRetreatPoint(states.position, states.RemainingDistance, Enemies, out AffectedDistance, out NewPosition);

        if (AffectedDistance < 1f)
            return false;

        return output;
    }
}