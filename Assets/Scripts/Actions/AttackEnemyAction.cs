using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class AttackEnemyAction : Action
{
    public ActorController Actor;
    public ActorController Enemy;

    public AttackEnemyAction(ActorController actor, ActorController enemy)
    {
        ActionName = "AttackEnemyAction";
        Preconditions.Add("IsCloseEnoughToAttack"+enemy.GetInstanceID(), 0);
        Preconditions.Add("IsAwareOf" + enemy.GetInstanceID(), 0);
        Outcomes.Add("hasAttacked" + enemy.GetInstanceID(), 0);
        Outcomes.Add("canRunAway", 1);

        Enemy = enemy;
        Actor = actor;
        ActionCost = 1;
        BonusActionCost = 0;
    }

    public override bool IsAchievable(Dictionary<string, int> conditions, Dictionary<HealthItem, int> healthItems, ActorStats states)
    {
        if (Vector3.Distance(states.position, Enemy.transform.position) > Actor.Stats.Reach)
            return false;

        return base.IsAchievable(conditions, healthItems, states);
    }
}