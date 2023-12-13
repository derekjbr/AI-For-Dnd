using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class CallForHelp : Action
{ 
    public ActorController Caller;
    public List<ActorController> Friends;
    private float HearingDistance;

    public CallForHelp(ActorController caller, List<ActorController> friends, float hearingDistance)
    {
        ActionName = "CallForHelp";
        Preconditions.Add("knowsAboutBattle", 0);
        HearingDistance = hearingDistance;
        CanRepeat = false;

        Caller = caller;
        Friends = friends;
        BonusActionCost = 1;
        ActionCost = 0;
    }

    public override void ApplyOutcomes(Dictionary<string, int> conditions, Dictionary<HealthItem, int> healthItems, ref ActorStats states, bool realRoll = false)
    {
        base.ApplyOutcomes(conditions, healthItems, ref states);

        foreach(ActorController friend in Friends)
        {
            if(Vector3.Distance(Caller.Stats.position, friend.transform.position) <= HearingDistance)
            {
                conditions.Add("friendAlerted" + friend.GetInstanceID(), 0);
                if (realRoll)
                {
                    friend.InformOfBattle();
                }
            }
        }
    }


    public override bool IsAchievable(Dictionary<string, int> conditions, Dictionary<HealthItem, int> healthItems, ActorStats states)
    {
        if (Friends.Count == 0)
            return false;

        bool foundAnyEnemy = false;

        foreach (ActorController friend in Friends)
        {
            if (Vector3.Distance(Caller.Stats.position, friend.transform.position) <= HearingDistance)
            {
                foundAnyEnemy = true;
            }
        }

        if (!foundAnyEnemy)
            return false;

        return base.IsAchievable(conditions, healthItems, states);
    }
}