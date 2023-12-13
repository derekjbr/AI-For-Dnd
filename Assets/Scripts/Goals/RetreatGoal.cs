using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

public class RetreatGoal : Goal
{
    private GoalOrientatedController Actor;
    private List<ActorController> Enemies;
    public RetreatGoal(GoalOrientatedController actor, List<ActorController> enemies) : base()
    {
        Name = "RetreatGoal";
        goals.Add("hasRetreated", 0);
        Actor = actor;
        Enemies = enemies;  
    }
    public override float Desireability()
    {
        float fallBackPercentage = 2f / 5f;
        if (Actor == null || Actor.Stats.CurrentHealth >= fallBackPercentage * Actor.Stats.MaxHeatlh)
            return 0f;

        bool believesEnemyToBeInRange = false;

        foreach(ActorController enemy in Enemies)
        {
            if(Vector3.Distance(Actor.transform.position, enemy.transform.position) < Actor.Stats.MaxDistance)
            {
                believesEnemyToBeInRange = true;
                break;
            }
        }

        if(!believesEnemyToBeInRange)
            return 0f;

        return Mathf.Max(Actor.Stats.CurrentHealth / (Actor.Stats.CurrentHealth - 2f * fallBackPercentage * Actor.Stats.MaxHeatlh) + 1f, 0f);
    }
}
