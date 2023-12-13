using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Search;
using UnityEngine;

public abstract class GoalOrientatedController : ActorController
{
    public Dictionary<Goal, int> Goals = new Dictionary<Goal, int>();
    public Dictionary<Action, int> Actions = new Dictionary<Action, int>();
    public Dictionary<Action, int> AvaliableActions;
    public Dictionary<string, int> WorldConditions = new Dictionary<string, int>();
    private Dictionary<string, int> CurrentConditions;
    
    public Queue<Action> CurrentPlan;

    public Planner Planner = new Planner();
    public bool LogPlanning = false;
    public int SmallPotionCount;
    private bool IsInAction = false;
    private Action CurrentAction = null;

    protected bool DontRepeatCodeDuringAction = false;
    public int NumTernsRunning = 0;
    public bool RanThisTurn = false;

    public override void Update()
    {
        base.Update();
        Planner.LogDebugInfo = LogPlanning;
        SmallPotionCount = HealthItems[SmallPotion.GetInstance()];
    }

    public override void ResetBeforeTurn()
    {
        base.ResetBeforeTurn();
        RanThisTurn = false;
        CurrentConditions = new Dictionary<string, int>(WorldConditions);
        AvaliableActions = new Dictionary<Action, int>(Actions);
    }

    protected void CompleteAction()
    {
        CurrentAction.ApplyOutcomes(CurrentConditions, HealthItems, ref this.Stats, true);
        IsInAction = false;
    }

    protected override void TakeTurn()
    {
        if (CurrentBattle == null)
        {
            return;
        }

        if (IsInAction == false)
        {
            // Reset conditions to reduce plans that
            if (CurrentPlan != null && CurrentPlan.Count == 0)
                CurrentConditions = new Dictionary<string, int>(WorldConditions);

            CurrentPlan = Planner.plan(AvaliableActions, Goals, CurrentConditions, HealthItems, this.Stats);
            if (CurrentPlan == null)
            {
                CurrentBattle.RequestEndOfTurn(this);
                return;
            }
            CurrentAction = CurrentPlan.Dequeue();

            if (!CurrentAction.CanRepeat)
                AvaliableActions.Remove(CurrentAction);

            DontRepeatCodeDuringAction = false;
            IsInAction = true;
        } else {
            TakeAction(CurrentAction);
        }
    }

    protected abstract void TakeAction(Action actionType);

    public bool HasCurrnetCondition(string condition)
    {
        return WorldConditions.ContainsKey(condition); 
    }
}