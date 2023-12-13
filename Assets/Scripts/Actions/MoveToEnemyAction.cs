using System.Collections.Generic;
using UnityEngine;

public class MoveToEnemyAction : Action
{
    public ActorController Actor;
    public ActorController Enemy;
    private float AffectedDistance;
    private Vector3 NewPosition;
    
    public MoveToEnemyAction(ActorController actor, ActorController enemy)
    {
        ActionName = "MoveToEnemyAction";
        Preconditions.Add("IsAwareOf"+enemy.GetInstanceID(), 0 );
        Outcomes.Add("IsCloseEnoughToAttack"+enemy.GetInstanceID(), 0);
        Actor = actor;
        Enemy = enemy;
        ActionCost = 0;
        BonusActionCost = 0;
        CanRepeat = true;
    }

    public override void ApplyOutcomes(Dictionary<string, int> conditions, Dictionary<HealthItem, int> healthItems, ref ActorStats states, bool realRoll = false)
    {
        base.ApplyOutcomes(conditions, healthItems, ref states);
        Actor.VerifyPathIsValidFromUniqueLocation(states.position, Enemy.transform.position, states.RemainingDistance, out AffectedDistance, out NewPosition);

        states.position = NewPosition;
        states.RemainingDistance -= AffectedDistance;
    }

    public override bool IsAchievable(Dictionary<string, int> conditions, Dictionary<HealthItem, int> healthItems, ActorStats states)
    {
        if (!base.IsAchievable(conditions, healthItems, states)  || Vector3.Distance(states.position, Enemy.transform.position) < states.Reach)
            return false;

        return Actor.VerifyPathIsValidFromUniqueLocation(states.position, Enemy.transform.position, states.RemainingDistance, out AffectedDistance, out NewPosition);
    }
}