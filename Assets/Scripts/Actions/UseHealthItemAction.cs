using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
public class UseHealthItemAction : Action
{
    private ActorController Actor;
    private List<ActorController> Enemies;
    public UseHealthItemAction(ActorController actor)
    {
        Actor = actor;
        ActionName = "UseHealthItemActoin";
        Outcomes.Add("hasHealed", 0);
        ActionCost = 1;
        BonusActionCost = 0;
        CanRepeat = false;
    }

    public override void ApplyOutcomes(Dictionary<string, int> conditions, Dictionary<HealthItem, int> healthItems, ref ActorStats states, bool realRoll = false)
    {
        base.ApplyOutcomes(conditions, healthItems, ref states);
        HealthItem bestPotion = null;

        foreach(HealthItem healthItem in healthItems.Keys)
        {
            if(bestPotion == null || bestPotion.ExpectedRoll() < healthItem.ExpectedRoll())
            {
                bestPotion = healthItem;
            }
        }

        if (realRoll)
        {
            int amount = bestPotion.RollForHealth();
            Debug.Log(Actor.Name + " uses health item and heals " + amount);
            states.CurrentHealth += amount;
        }
        else
        {
            states.CurrentHealth += bestPotion.ExpectedRoll();
        }

        if(states.CurrentHealth > states.MaxHeatlh)
        {
            states.CurrentHealth = states.MaxHeatlh;
        }

        healthItems[bestPotion] = healthItems[bestPotion] - 1;
    }

    public override bool IsAchievable(Dictionary<string, int> conditions, Dictionary<HealthItem, int> healthItems, ActorStats states)
    {
        if (!base.IsAchievable(conditions, healthItems, states) || Actor.Stats.MaxHeatlh <= Actor.Stats.CurrentHealth)
            return false;

        bool hasHealthItem = false;
        foreach(HealthItem item in healthItems.Keys)
        {
            hasHealthItem = hasHealthItem || healthItems[item] != 0;
        }

        return hasHealthItem;
    }
}