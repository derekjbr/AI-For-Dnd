using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

public class Planner
{
    public Dictionary<Queue<Action>, float> PossiblePlans = new Dictionary<Queue<Action>, float>();
    public Dictionary<Queue<Action>, ActorStats> PossiblePlanStats = new Dictionary<Queue<Action>, ActorStats>();
    private ActorStats PreStats;

    public bool LogDebugInfo = false;

    public Queue<Action> plan(Dictionary<Action, int> actions, Dictionary<Goal, int> goals, Dictionary<string, int> conditions, Dictionary<HealthItem, int> healthItems, ActorStats stats)
    {
        if (conditions == null)
            conditions = new Dictionary<string, int>();

        PossiblePlans.Clear();
        PreStats = ActorStats.Copy(stats);

        foreach(Action action in actions.Keys)
        {
            Queue<Action> plan = new Queue<Action>();
            AttemptNextAction(action, plan, actions, goals, conditions, healthItems, stats);
        }


        if (PossiblePlans.Count == 0)
            return null;

        List<Queue<Action>> mostDesiredPlans = new List<Queue<Action>>();
        float highestDesirability = 0f;
        foreach(Queue<Action> plan in PossiblePlans.Keys)
        {
            if (mostDesiredPlans.Count == 0 || PossiblePlans[plan] > highestDesirability)
            {
                mostDesiredPlans.Clear();
                mostDesiredPlans.Add(plan);
                highestDesirability = PossiblePlans[plan];
            } else if(PossiblePlans[plan] == PossiblePlans[mostDesiredPlans[0]])
            {
                mostDesiredPlans.Add(plan);
            }
        }

        Queue<Action> mostDesired = null;
        foreach(Queue<Action> plan in mostDesiredPlans)
        {
            if(mostDesired == null || PlanDesirability(PreStats, PossiblePlanStats[mostDesired]) < PlanDesirability(PreStats, PossiblePlanStats[plan]))
            {
                mostDesired = plan;
            }
        }

        if (LogDebugInfo)
        {
            Debug.Log("Chosen plan: ");
            foreach(Action a in mostDesired)
            {
                Debug.Log("    " + a.ActionName);
            }
        }

        return mostDesired;
    }

    public void AttemptNextAction(Action takenAction, Queue<Action> plan, Dictionary<Action, int> actions, Dictionary<Goal, int> goals, Dictionary<string, int> conditions, Dictionary<HealthItem, int> healthItems, ActorStats stats, int depth = 0) {
        if (depth >= 3)
        {
            return;
        }
        
        if (!takenAction.IsAchievable(conditions, healthItems, stats))
            return;
        plan.Enqueue(takenAction);

        Dictionary<Action, int> newActions = new Dictionary<Action, int>(actions);
        Dictionary<string, int> newConditions = new Dictionary<string, int>(conditions);
        Dictionary<HealthItem, int> newHealthItems = new Dictionary<HealthItem, int>(healthItems);
        ActorStats newStats = ActorStats.Copy(stats);

        if (!takenAction.CanRepeat)
            newActions.Remove(takenAction);
         
        takenAction.ApplyOutcomes(newConditions, newHealthItems, ref newStats);

        foreach(Goal goal in goals.Keys)
        {
            if(goal.Achieved(newConditions) && !PossiblePlans.ContainsKey(plan))
            {
                if (LogDebugInfo)
                    Debug.Log("Goal " + goal.Name + "achievable with " + goal.Desireability() + " desirability.");
                if (goal.Desireability() != 0)
                {
                    PossiblePlans.Add(plan, goal.Desireability());
                    PossiblePlanStats.Add(plan, newStats);
                }
            }
        }

        foreach (Action action in newActions.Keys)
        {
            Queue<Action> newplan = new Queue<Action>(plan);
            AttemptNextAction(action, newplan, newActions, goals, newConditions, newHealthItems, newStats, depth + 1);
        }

        plan = null;
    }

    private static float PlanDesirability(ActorStats preStats, ActorStats postStats)
    {
        float actionDiff = (preStats.AvaliableActions == 0) ? 1f : postStats.AvaliableActions / (float) preStats.AvaliableActions;
        float bonusActionDiff = (preStats.AvaliableBonusActions == 0) ? 1f : postStats.AvaliableBonusActions / (float) preStats.AvaliableBonusActions;
        float distanceDiff = (preStats.RemainingDistance == 0) ? 1f : Mathf.Min(postStats.RemainingDistance, preStats.RemainingDistance) / preStats.RemainingDistance;

        return actionDiff + bonusActionDiff + distanceDiff;
    }
}