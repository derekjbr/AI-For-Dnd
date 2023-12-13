using System.Collections.Generic;
using UnityEngine;
public class MoveTowardsFriend : Action
{
    private ActorController Actor;
    public ActorController Friend;
    private float CloseDistance;
    private float AffectedDistance;
    private Vector3 NewPosition;
    public MoveTowardsFriend(ActorController actor, ActorController friend, float closeDistance)
    {
        Actor = actor;
        Friend = friend;
        CloseDistance = closeDistance;

        ActionName = "MoveTowardsFriend";
        Preconditions.Add("knowsAboutBattle", 0);
        Preconditions.Add("canRunAway", 0);
        Outcomes.Add("MovedLocations", 0);
        ActionCost = 0;
        BonusActionCost = 0;
        CanRepeat = false;
    }

    public override void ApplyOutcomes(Dictionary<string, int> conditions, Dictionary<HealthItem, int> healthItems, ref ActorStats states, bool realRoll = false)
    {
        base.ApplyOutcomes(conditions, healthItems, ref states);
        bool result = Actor.FindValidPointClosestToAnother(states.position, Friend.transform.position, states.RemainingDistance, out AffectedDistance, out NewPosition);

        states.position = NewPosition;
        states.RemainingDistance -= AffectedDistance;

        if(Vector3.Distance(states.position, Friend.transform.position) <= CloseDistance)
        {
            if(!Outcomes.ContainsKey("CloseToFriend" + Friend.GetInstanceID()))
                Outcomes.Add("CloseToFriend"+Friend.GetInstanceID(), 0);
        }
    }

    public override bool IsAchievable(Dictionary<string, int> conditions, Dictionary<HealthItem, int> healthItems, ActorStats states)
    {
        if (!base.IsAchievable(conditions, healthItems, states) || states.RemainingDistance < 1f)
            return false;

        return Actor.FindValidPointClosestToAnother(states.position, Friend.transform.position, states.RemainingDistance, out AffectedDistance, out NewPosition);
    }
}