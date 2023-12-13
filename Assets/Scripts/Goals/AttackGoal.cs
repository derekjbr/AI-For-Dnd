using UnityEngine;

public class AttackGoal : Goal
{
    public ActorController Attackee { get; set; }
    public ActorController Actor { get; set; }

    public AttackGoal(ActorController actor, ActorController attackee) : base()
    {
        Name = "AttackGoal"+attackee.GetInstanceID();
        goals.Add("hasAttacked"+attackee.GetInstanceID(), 0);

        Attackee = attackee; 
        Actor = actor;
    }
    
    public override float Desireability()
    {
        if (Attackee == null)
            return 0f;

        // As attackee's health declines, make attacking more desireable
        float attackeePercent = Mathf.Min(Mathf.Abs(Functions.Sigmoid(Attackee.Stats.CurrentHealth, 2f * Attackee.Stats.MaxHeatlh / 3f) - 0.5f) + .10f, 1f);
        // As actor healh declines, make attacking less desireable
        float actorHeatlhPerctage = Actor.Stats.CurrentHealth / Actor.Stats.MaxHeatlh;

        return (9f * attackeePercent + actorHeatlhPerctage) / 10f;
    }
}