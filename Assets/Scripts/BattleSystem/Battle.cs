using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battle
{
    private List<ActorController> Participants;
    private List<int> InitiativeRolls;

    private bool HasBattleStarted;
    private int CurrentTurn;

    public Battle(List<ActorController> participants)
    {
        HasBattleStarted = false;
        Participants = new List<ActorController>();
        InitiativeRolls = new List<int>();
        foreach(ActorController actor in participants)
        {
            AddActor(actor);
            actor.SetCurrentState(ActorState.Battle);
        }

        PrintBattleInformation();
    }

    public void StartBattle()
    {
        CurrentTurn = 0;
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

        Participants[CurrentTurn].SetCurrentState(ActorState.Turn);

        Debug.Log("Turn started for " + Participants[CurrentTurn].Name);
    }

    public void RequestAttack(ActorController requestor, ActorController reciever, Weapon weapon)
    {

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
