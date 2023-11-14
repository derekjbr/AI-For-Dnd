using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;

public class GoblnController : ActorController
{
    private enum GoblinStates
    {
        Lost,
        RoamingOnPath,
        ReturnToPath,
        Scouting,
        SpottedEnemy
    }

    private GoblinStates GoblinState;

    [Header("Perception")]
    public bool VisualizePerceptionSystem;
    public Vector3 PerceptionOffset;
    public float FieldOfView;
    public float FieldOfViewDistance;
    public int NumberOfRaysPerActor = 1;

    private Vector3 LastSeenPCLocation;
    private ActorController LastSeenPC;

    [Header("Roaming")]
    public bool VisualizeRoamingPath;
    public List<Vector3> RoamingPath = new List<Vector3>();
    public float WaitTimeAtEndPoints = 5f;
    public float RoamingSpeed = 1.5f;
    //Locally Used Roaming Variables
    private int CurrentNodeOnRoamingPath;
    private float ArrivalTimeAtEndNode;
    private int DirectionOfNodeTraversal;

    [Header("Spotting")]
    public float SpottingTime = 2f;
    public float SpottedSpeed = 2.5f;

    [Header("Scoutting")]
    public float TimeSpentScouting;
    public AnimationCurve LookDirectionCurve;
    private float DebuggingTime = 0;
    private float TestAngleDifference = 0;


    private float TimeSinceLastEvent = 0f;


    private Animator AnimatorController;

    public void Start()
    {
        AnimatorController = GetComponent<Animator>();
        Agent = GetComponent<NavMeshAgent>();

        GoblinState = GoblinStates.RoamingOnPath;

        CurrentNodeOnRoamingPath = -1;
        ArrivalTimeAtEndNode = 0f;
        DirectionOfNodeTraversal = 1;

        // Setting Actor Stats
        Stats.MaxDistance = 8f;
        Stats.Reach = 2f;
    }

    protected override void Roam()
    {
        switch(GoblinState)
        {
            case GoblinStates.RoamingOnPath:
                FollowRoamingPath();
                break;
            case GoblinStates.SpottedEnemy:
                FollowSpottedEnemy();
                break;
            case GoblinStates.Scouting:
                ScoutLocation();
                break;
        }
        
    }

    protected override void TakeTurn()
    {
        if (CurrentBattle == null)
        {
            return;
        }
    }
    private void ScoutLocation()
    {
        Agent.isStopped = true;
        DebuggingTime = AnimatorController.GetCurrentAnimatorStateInfo(0).normalizedTime % 1f;
        TestAngleDifference = LookDirectionCurve.Evaluate(DebuggingTime);

        if(HasSeenPCActor(TestAngleDifference))
        {
            Agent.destination = LastSeenPCLocation;
            GoblinState = GoblinStates.SpottedEnemy;
            TimeSinceLastEvent = Time.time;
            return;
        } else
        {
            if(Time.time - TimeSinceLastEvent > TimeSpentScouting)
            {
                GoblinState = GoblinStates.RoamingOnPath;
                TimeSinceLastEvent = Time.time;
                return;
            }
        }
    }

    private void FollowSpottedEnemy()
    {
        Agent.isStopped = false;
        float currentTime = Time.time;
        Agent.speed = SpottedSpeed;
        if(!HasSeenPCActor())
        {
            if (HasAgentReachedDestination())
            {
                GoblinState = GoblinStates.Scouting;
                TimeSinceLastEvent = currentTime;
                return;
            }
        } else
        {
            if(currentTime - TimeSinceLastEvent > SpottingTime || Vector3.Distance(this.transform.position, LastSeenPC.transform.position) <= Stats.Reach)
            {
                List<ActorController> spottedEnemyAndFriends = new List<ActorController>();
                spottedEnemyAndFriends.Add(LastSeenPC);

                GameMaster.RequestBattle(this, spottedEnemyAndFriends);
            } 
            Agent.destination = LastSeenPCLocation;
        }
    }

    private void FollowRoamingPath()
    {
        Agent.isStopped = false;
        if (HasSeenPCActor())
        {
            GoblinState = GoblinStates.SpottedEnemy;
            TimeSinceLastEvent = Time.time;
            return;
        }

        Agent.speed = RoamingSpeed;
        if (HasAgentReachedDestination() && GoblinState == GoblinStates.RoamingOnPath)
        {
            // If I have reached either end of the roaming path, scount for a bit
            if(CurrentNodeOnRoamingPath == 0 || CurrentNodeOnRoamingPath == RoamingPath.Count - 1)
            {
                GoblinState = GoblinStates.Scouting;
            }

            // Change directions if I have reached the end
            if (CurrentNodeOnRoamingPath + 1 * DirectionOfNodeTraversal < 0 || CurrentNodeOnRoamingPath + 1 * DirectionOfNodeTraversal >= RoamingPath.Count)
            {
                DirectionOfNodeTraversal *= -1;
            }

            // Get the next node
            CurrentNodeOnRoamingPath += 1 * DirectionOfNodeTraversal;

            // Set the path of the next node
            Vector3 nextNode = RoamingPath[CurrentNodeOnRoamingPath];
            Agent.destination = nextNode;

        }
    }

    private bool HasSeenPCActor(float viewDegreeOffset = 0)
    {
        foreach(ActorController actor in GameMaster.Actors)
        {
            if (actor == this)
                continue;

            if(IsActorInFieldOfViewDistance(actor, FieldOfView, FieldOfViewDistance, viewDegreeOffset))
            {
                if (HasLineOfSightToActor(actor, NumberOfRaysPerActor))
                {
                    if (actor.gameObject.GetComponent<PlayableCharacterController>() != null)
                    {
                        LastSeenPCLocation = actor.gameObject.transform.position;
                        LastSeenPC = actor;
                        return true;
                    }
                }
            }
        }

        return false;
    }


    protected override void UpdateAnimationState()
    {
        if (Agent.velocity.sqrMagnitude > 0.05 || GoblinState == GoblinStates.Scouting)
        {
            AnimatorController.SetBool("IsRoaming", true);
        }
        else
        {
            AnimatorController.SetBool("IsRoaming", false);
        }

        if(GoblinState == GoblinStates.Scouting)
        {
            AnimatorController.SetBool("IsScouting", true);
        } else
        {
            AnimatorController.SetBool("IsScouting", false);
        }
    }

    private new void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        if (VisualizePerceptionSystem)
        {

            Vector3 leftEdge = Quaternion.AngleAxis(-FieldOfView / 2f + TestAngleDifference, Vector3.up) * transform.forward;
            Vector3 rightEdge = Quaternion.AngleAxis(FieldOfView / 2f + TestAngleDifference, Vector3.up) * transform.forward;

            Gizmos.DrawLine(gameObject.transform.position + PerceptionOffset, gameObject.transform.position + PerceptionOffset + leftEdge.normalized * FieldOfViewDistance);
            Gizmos.DrawLine(gameObject.transform.position + PerceptionOffset, gameObject.transform.position + PerceptionOffset + rightEdge.normalized * FieldOfViewDistance);

            Gizmos.DrawWireSphere(gameObject.transform.position, FieldOfViewDistance);
        }

        if(VisualizeRoamingPath)
        {
            foreach(Vector3 point in RoamingPath)
            {
                Gizmos.DrawSphere(point, 0.1f);
            }
        }
    }
}
