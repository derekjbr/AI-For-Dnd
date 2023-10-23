using OpenCover.Framework.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleController : MonoBehaviour
{
    public List<ActorController> Actors;
    private List<Battle> ActiveBattles;

    // Start is called before the first frame update
    void Start()
    {
        ActiveBattles = new List<Battle>();
        foreach (ActorController actor in Actors)
        {
            actor.SetCurrentState(ActorState.Roam);
            actor.SetBattleController(this);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void RequestBattle(ActorController requestor, List<ActorController> requestedActors)
    {
        List<ActorController> everyone = new List<ActorController>(requestedActors);
        everyone.Add(requestor);

        ActiveBattles.Add(new Battle(everyone));
    }
}



