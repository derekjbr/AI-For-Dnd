using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battle
{
    private List<ActorController> Participants;
    private List<int> InitiativeRolls;

    private bool HasBattleStarted;

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

    public void AddActor(ActorController actor)
    {
        int roll = actor.RollInitiative();
        int placement = FindInitiativePlace(roll);
        Debug.Log("Actor: " + actor.Name + " Roll: " + roll);
        Participants.Insert(placement, actor);
        InitiativeRolls.Insert(placement, roll);
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
