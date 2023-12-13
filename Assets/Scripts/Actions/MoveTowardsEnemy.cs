using System.Collections.Generic;
using UnityEngine;
public class MoveTowardsEnemy : Action
{
    private ActorController Actor;
    public ActorController Enemy;
    private float CloseDistance;
    private float AffectedDistance;
    private Vector3 NewPosition;
    public MoveTowardsEnemy(ActorController actor, ActorController enemy, float closeDistance)
    {
        Actor = actor;
        Enemy = enemy;
        CloseDistance = closeDistance;

        ActionName = "MoveTowardsEnemy";
        Preconditions.Add("knowsAboutBattle", 0);
        Preconditions.Add("AwareOf"+enemy.GetInstanceID(), 0);
        Outcomes.Add("MovedLocations", 0);
        ActionCost = 0;
        BonusActionCost = 0;
        CanRepeat = true;
    }

    public override void ApplyOutcomes(Dictionary<string, int> conditions, Dictionary<HealthItem, int> healthItems, ref ActorStats states, bool realRoll = false)
    {
        base.ApplyOutcomes(conditions, healthItems, ref states);
        bool result = Actor.FindValidPointClosestToAnother(states.position, Enemy.transform.position, states.RemainingDistance, out AffectedDistance, out NewPosition);

        states.position = NewPosition;
        states.RemainingDistance -= AffectedDistance;
    }

    public override bool IsAchievable(Dictionary<string, int> conditions, Dictionary<HealthItem, int> healthItems, ActorStats states)
    {
        if (!base.IsAchievable(conditions, healthItems, states) || Vector3.Distance(states.position, Enemy.transform.position) < states.Reach)
            return false;

        bool output = Actor.FindValidPointClosestToAnother(states.position, Enemy.transform.position, states.RemainingDistance, out AffectedDistance, out NewPosition);
        if (AffectedDistance < states.Reach)
            return false;

        return output;
    }
}