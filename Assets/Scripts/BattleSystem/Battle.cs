using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

public class Battle
{
    private List<ActorController> Participants;
    private List<int> InitiativeRolls;

    private int CurrentTurn;

    public Battle(List<ActorController> participants)
    {
        Participants = new List<ActorController>();
        InitiativeRolls = new List<int>();
        foreach(ActorController actor in participants)
        {
            AddActor(actor);
            actor.SetCurrentState(ActorState.Battle);
        }

        foreach(ActorController actor in participants)
        {
            actor.BattleInitialization();
        }

        PrintBattleInformation();
    }

    public void StartBattle()
    {
        CurrentTurn = 0;
        Participants[CurrentTurn].ResetBeforeTurn();
        Participants[CurrentTurn].SetCurrentState(ActorState.Turn);
    }

    public void RequestEndOfTurn(ActorController requestor)
    {
        if (requestor != Participants[CurrentTurn])
            return;

        requestor.SetCurrentState(ActorState.Battle);
        if (CurrentTurn + 1 >= Participants.Count)
            CurrentTurn = 0;
        else 
            CurrentTurn++;

        Debug.Log("Turn started for " + Participants[CurrentTurn].Name);
        Participants[CurrentTurn].SetCurrentState(ActorState.Turn);
        Participants[CurrentTurn].ResetBeforeTurn();
    }

    public void RequestAttack(ActorController attacker, ActorController attackee, Weapon weaponUsed)
    {
        attackee.InformOfEnemy(attacker);
        int attackRoll = Dice.Roll(Dice.RollType.D20);
        Debug.Log(attacker.Name + " rolls " + attackRoll);

        if (attackRoll > attackee.Stats.AC)
        {
            int damageDelt = weaponUsed.RollForDamage();
            attackee.Stats.CurrentHealth -= damageDelt;
            Debug.Log(attacker.Name + " attacks " + attackee.Name + " for " + damageDelt);
            if(attackee.Stats.CurrentHealth <= 0)
            {
                attackee.Stats.CurrentHealth = 0;
                attackee.SetCurrentState(ActorState.Dead);
            }
        } else
        {
            Debug.Log(attacker.Name + " misses.");
        }
    }

    public void AddActor(ActorController actor)
    {
        int roll = actor.RollInitiative();
        int placement = FindInitiativePlace(roll);
        Participants.Insert(placement, actor);
        InitiativeRolls.Insert(placement, roll);
        actor.SetCurrentBattle(this);
    }

    public List<ActorController> GetParticipants(ActorController dontInclude = null)
    {
        List<ActorController> ret = new List<ActorController>();
        foreach(ActorController actor in Participants)
        {
            if (actor != dontInclude)
                ret.Add(actor);
        }

        return ret;
    }

    private int FindInitiativePlace(int init)
    {
        if (Participants.Count == 0)
            return 0;

        for(int i = 0; i < Participants.Count; i++)
        {
                if (InitiativeRolls[i] < init)
                    return i;
        }

            return Participants.Count;
    }

    private void PrintBattleInformation()
    {
        for(int i = 0; i < Participants.Count;i++) 
        {
            Debug.Log("Actor: " + Participants[i].Name + " Roll: " + InitiativeRolls[i]);
        }
    }
}
