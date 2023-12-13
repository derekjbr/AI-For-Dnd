using System.Collections.Generic;
using UnityEngine;

public abstract class Action 
{
    public Dictionary<string, int> Preconditions = new Dictionary <string, int> ();
    public Dictionary<string, int> Outcomes = new Dictionary <string, int> ();
    public int ActionCost;
    public int BonusActionCost;
    public bool CanRepeat = true;
    public string ActionName;


    public bool MeetsPreconditions(Dictionary<string, int> conditions) 
    {
        bool hasMetPreconditions = true;
        foreach (string precondition in Preconditions.Keys)
        {
            if (!conditions.ContainsKey(precondition))
            { hasMetPreconditions = false; break; }
        }

        return hasMetPreconditions;
    }

    public virtual void ApplyOutcomes(Dictionary<string, int> conditions, Dictionary<HealthItem, int> healthItems, ref ActorStats states, bool realRoll = false)
    {
        states.AvaliableActions -= ActionCost;
        states.AvaliableBonusActions -= BonusActionCost;

        foreach(string outcome in Outcomes.Keys)
        {
            if (conditions.ContainsKey(outcome))
            {
                if (Outcomes[outcome] == 1)
                    conditions.Remove(outcome);
            }
            else
            {
                conditions.Add(outcome, 0);
            }
        }
    }

    public virtual bool IsAchievable(Dictionary<string, int> conditions, Dictionary<HealthItem, int> healthItems, ActorStats states)
    {
        if (states.AvaliableActions < ActionCost || states.AvaliableBonusActions < BonusActionCost)
            return false;

        return MeetsPreconditions(conditions);
    }

    public override string ToString()
    {
        return ActionName;
    }

}
