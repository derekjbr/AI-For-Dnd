using System.Collections.Generic;
using UnityEngine;

public class HealGoal : Goal
{
    public ActorController Actor { get; set; }
    public ActorController KnownEnemies { get; set; }

    public HealGoal(ActorController actor, List<ActorController> knownEnemies) : base()
    {
        Name = "HealGoal";
        goals.Add("hasHealed", 0);

        Actor = actor;
    }

    public override float Desireability()
    {
        if (Actor == null)
            return 0f;

        if (Actor.Stats.CurrentHealth >= Actor.Stats.MaxHeatlh)
            return 0f;

        // Perctange of health in which going to use a potion becomes desirable
        const float percentage = 1f / 3f;
        return (-Mathf.Atan(Actor.Stats.CurrentHealth - Actor.Stats.MaxHeatlh * percentage) * 2f / Mathf.PI + 1) / 2f;
    }
}