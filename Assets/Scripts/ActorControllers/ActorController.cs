using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public enum ActorState
{
    Roam,
    Battle,
    Turn,
    Dead
}

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
public abstract class ActorController : MonoBehaviour
{
    public string Name;

    public float Height;
    public float HeightOfEyes;

    public bool DrawDebugInformation;

    protected NavMeshAgent Agent;
    protected ActorState CurrentState;
    protected BattleController GameMaster;


    private static System.Random Rand = new System.Random();
    
    protected abstract void Roam();
    protected abstract void TakeTurn();
    protected abstract void UpdateAnimationState();


    public virtual int RollInitiative()
    {
        return Rand.Next(1, 21);
    }

    public void SetCurrentState(ActorState state)
    {
        if (Agent != null)
        {
            if (CurrentState == ActorState.Roam && state != ActorState.Roam)
            {
                Agent.isStopped = true;
            }
            else
            {
                Agent.isStopped = false;
            }
        }
        CurrentState = state;
    }

    public void SetBattleController(BattleController battleController)
    {
        GameMaster = battleController;
    }

    public Vector3 GetEyesLocation()
    {
        return transform.position + new Vector3(0f, HeightOfEyes, 0f);
    }

    public void Update()
    {
        UpdateAnimationState();

        switch (CurrentState)
        {
            case ActorState.Roam:
                Roam();
                break;
            case ActorState.Turn: 
                TakeTurn();
                break;
            default:
                break;
        }
    }

    public void OnDrawGizmos()
    {
        if (DrawDebugInformation)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(transform.position + new Vector3(0f, Height, 0f), new Vector3(0.1f, 0.1f, 0.1f));
            Gizmos.color = Color.green;
            Gizmos.DrawCube(transform.position + new Vector3(0f, HeightOfEyes, 0f), new Vector3(0.1f, 0.1f, 0.1f));
        }
        Gizmos.color = Color.red;
    }

    protected bool IsActorInFieldOfViewDistance(ActorController target, float fieldOfView, float fieldOfViewDistance, float degreeOffset = 0f)
    {
        Vector3 direction = degreeOffset == 0 ? (transform.forward) : Quaternion.AngleAxis(degreeOffset, Vector3.up) * transform.forward;

        Debug.DrawRay(transform.position + new Vector3(0f, 1f, 0f), direction * 5f, Color.red);

        Vector3 fromThisToActor = target.gameObject.transform.position - transform.position;
        if (Vector3.Angle(direction, fromThisToActor) < fieldOfView / 2f)
        {
            if (Vector3.Distance(target.transform.position, transform.position) < fieldOfViewDistance)
                return true;
        }
        return false;
    }

    protected bool HasLineOfSightToActor(ActorController target, int numLineOfSightRays)
    {
        bool hasActorAlreadyBeenSeen = false;
        for (int i = 0; i < numLineOfSightRays && !hasActorAlreadyBeenSeen; i++)
        {
            Vector3 eyesToCenterBody = target.transform.position + new Vector3(0f, target.Height / numLineOfSightRays * i + target.Height / (2f * numLineOfSightRays), 0f) - GetEyesLocation();
            RaycastHit hit;
            Ray ray = new Ray(GetEyesLocation(), eyesToCenterBody);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.gameObject != null && hit.transform.gameObject == target.gameObject)
                {
                    hasActorAlreadyBeenSeen = true;
                }
            }
        }

        return hasActorAlreadyBeenSeen;
    }

    protected bool HasAgentReachedDestination()
    {
        return Vector3.Distance(Agent.destination, transform.position) <= Agent.stoppingDistance;
    }
}
