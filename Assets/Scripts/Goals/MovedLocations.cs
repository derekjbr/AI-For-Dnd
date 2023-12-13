using System.Collections.Generic;
using UnityEngine;

public class MovedLocations : Goal
{
    GoalOrientatedController Actor;
    List<ActorController> Enemies;
    public MovedLocations(GoalOrientatedController actor, List<ActorController> enemies) : base()
    {
        Name = "MovedLocations";
        goals.Add("MovedLocations", 0);

        Actor = actor;
        Enemies = enemies;
    }
    public override float Desireability()
    {
        if (Actor != null && !Actor.HasCurrnetCondition("knowsAboutBattle"))
            return 0f;

        bool hasPerformedAction = Actor.Stats.AvaliableActions < Actor.Stats.MaxActions || Actor.Stats.AvaliableBonusActions < Actor.Stats.MaxBonusActions;
        bool hasMoved = Actor.Stats.RemainingDistance < Actor.Stats.MaxDistance;

        return  hasPerformedAction || hasMoved ? 0f : .01f;
    }
}
