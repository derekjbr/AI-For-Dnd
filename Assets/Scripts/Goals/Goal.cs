using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Goal
{
    public string Name;
    public Dictionary<string, int> goals;
    public Goal() 
    {
        goals = new Dictionary<string, int>();
    }

    public bool Achieved(Dictionary<string, int> conditoins)
    {
        bool achieved = true;
        foreach(string goal in goals.Keys)
        {
            if(!conditoins.ContainsKey(goal))
                achieved = false;
        }

        return achieved;
    }


    // Return a number between 0-1
    public abstract float Desireability();
}
