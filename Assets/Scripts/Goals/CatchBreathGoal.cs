using System.Collections.Generic;
using UnityEngine;

public class CatchBreathGoal : Goal
{
    private GoalOrientatedController Actor;
    private List<ActorController> Enemies;
    public CatchBreathGoal(GoalOrientatedController actor, List<ActorController> enemies) : base()
    {
        Name = "CatchBreathGoal";
        goals.Add("hasCaughtBreath", 0);
        Actor = actor;
        Enemies = enemies;
    }
    public override float Desireability()
    {
        if(Actor.Stats.CurrentHealth <= 2f * Actor.Stats.MaxHeatlh / 5f)
        {
            float closetEnemy = float.MaxValue;
            foreach(ActorController enemy in Enemies)
            {
                float distance = Vector3.Distance(enemy.transform.position, Actor.transform.position);
                if (distance < closetEnemy)
                {
                    closetEnemy = distance;
                }
            }

            return Mathf.Clamp01((closetEnemy - Actor.Stats.MaxDistance / 4f) / Actor.Stats.MaxDistance);
        }

        return Actor.NumTernsRunning >= 3 ? 1 : 0;
    }
}
