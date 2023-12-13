using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using UnityEngine.AI;

public enum ActorState
{
    Roam,
    Battle,
    Turn,
    Dead
}

[Serializable]
public struct ActorStats
{
    public float MaxDistance;
    public float RemainingDistance;
    public float MaxHeatlh;
    public float CurrentHealth;
    public int MaxActions;
    public int AvaliableActions;
    public int MaxBonusActions;
    public int AvaliableBonusActions;
    public int AC;
    public float Reach;
    public Vector3 position;
    public static ActorStats Copy(ActorStats copy)
    {
        ActorStats stats = new ActorStats();
        stats.MaxDistance = copy.MaxDistance;
        stats.RemainingDistance = copy.RemainingDistance;
        stats.MaxHeatlh = copy.MaxHeatlh;
        stats.CurrentHealth = copy.CurrentHealth;
        stats.MaxActions = copy.MaxActions;
        stats.AvaliableActions = copy.AvaliableActions;
        stats.MaxDistance = copy.MaxDistance;
        stats.MaxBonusActions = copy.MaxBonusActions;
        stats.AvaliableBonusActions = copy.AvaliableBonusActions;
        stats.AC = copy.AC;
        stats.Reach = copy.Reach;
        stats.position = copy.position;
        return stats;
}

}

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
public abstract class ActorController : MonoBehaviour
{
    public string Name;
    public float Height;
    public float HeightOfEyes;
    public ActorStats Stats;
    public bool DrawDebugInformation;

    public Weapon WeaponInHead = Sword.GetInstance();
    public Dictionary<HealthItem, int> HealthItems = new Dictionary<HealthItem, int>();

    protected NavMeshAgent Agent;
    protected Animator AnimatorController;
    protected ActorState CurrentState;
    protected BattleController GameMaster;
    protected Battle CurrentBattle;
    private static System.Random Rand = new System.Random();
    

    // During Battle/Transitioning to and from battle function
    public virtual int RollInitiative()
    {
        return Rand.Next(1, 21);
    }
    public virtual void SetCurrentState(ActorState state)
    {
        if (Agent != null)
        { 
            if ((CurrentState == ActorState.Roam && state != ActorState.Roam) || state == ActorState.Battle || state == ActorState.Turn)
            {
                Agent.isStopped = true;
            }
            else
            {
                Agent.isStopped = false;
            }
        }

        if(CurrentState != ActorState.Dead)
            CurrentState = state;
    }
    public void SetBattleController(BattleController battleController)
    {
        GameMaster = battleController;
    }
    public void SetCurrentBattle(Battle battle)
    {
        CurrentBattle = battle;
    }
    public virtual void ResetBeforeTurn()
    {
        this.Stats.RemainingDistance = this.Stats.MaxDistance;
        this.Stats.AvaliableActions = this.Stats.MaxActions;
        this.Stats.AvaliableBonusActions = this.Stats.MaxBonusActions;
        if (this.Stats.CurrentHealth <= 0)
            CurrentBattle.RequestEndOfTurn(this);
    }


    // Every actor follows the same update scheme
    public virtual void Update()
    {
        UpdateAnimationState();

        Stats.position = this.transform.position;

        switch (CurrentState)
        {
            case ActorState.Roam:
                Roam();
                break;
            case ActorState.Turn: 
                TakeTurn();
                break;
            case ActorState.Dead:
                Agent.isStopped = true;
                break;
            default:
                break;
        }
    }
    public virtual void Start()
    {
        AnimatorController = GetComponent<Animator>();
        Agent = GetComponent<NavMeshAgent>();
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

    // Overarching action functions
    protected abstract void Roam();
    protected abstract void TakeTurn();
    public virtual void BattleInitialization() { }
    protected abstract void UpdateAnimationState();
    public virtual void InformOfEnemy(ActorController actor) { }
    public virtual void InformOfFriend(ActorController actor) { }
    public virtual void InformOfBattle() { }



    // Utility functions
    public Vector3 GetEyesLocation()
    {
        return transform.position + new Vector3(0f, HeightOfEyes, 0f);
    }
    public bool IsActorInFieldOfViewDistance(ActorController target, float fieldOfView, float fieldOfViewDistance, float degreeOffset = 0f)
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
    public bool HasLineOfSightToActor(ActorController target, int numLineOfSightRays)
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
    public bool HasAgentReachedDestination()
    {
        return Vector3.Distance(Agent.destination, transform.position) <= Agent.stoppingDistance;
    }
    public float VerifyPathIsValidForTurn(Vector3 des, float remainingDistance)
    {
        NavMeshPath path = new NavMeshPath();        
        Agent.CalculatePath(des, path);

        float distance = 0f;
        Vector3 prevPoint = this.transform.position;
        foreach (Vector3 point in path.corners)
        {
            distance += Vector3.Distance(prevPoint, point);
            prevPoint = point;
        }
        path = null;

        if(distance > remainingDistance)
        {
            return -1f;
        } else
        {
            return distance;
        }
    }

    public bool VerifyPathIsValidFromUniqueLocation(Vector3 startingPoint, Vector3 destination, float remainingDistance, out float distance, out Vector3 finalPosition)
    {
        distance = 0f;
        finalPosition = startingPoint;

        // Code needed for making sure this function doesn't modify the position of the actor
        Vector3 tmpPosition = transform.position;

        NavMeshPath path = new NavMeshPath();
        Agent.nextPosition = startingPoint;
        bool isPathValid = Agent.CalculatePath(destination, path);

        Agent.nextPosition = tmpPosition;
        transform.position = tmpPosition;

        if (!isPathValid)
        {
            return false;
        }

        Vector3 prevPoint = this.transform.position;
        foreach (Vector3 point in path.corners)
        {
            distance += Vector3.Distance(prevPoint, point);
            prevPoint = point;
        }
        finalPosition = path.corners[path.corners.Length - 1];
        path = null;

        return distance <= remainingDistance;
    }

    public bool FindValidPointClosestToAnother(Vector3 startingPoint, Vector3 targetPoint, float remainingDistance, out float distance, out Vector3 finalPosition, int numDivisions = 10)
    {
        bool hasFoundValidPath = false;
        float minDistance = float.MaxValue;
        float outDistance = -1f;
        distance = 0f;
        finalPosition = startingPoint;

        Vector3 outPosition;
        for (int i = 0; i < numDivisions; i++)
        {
            for (int j = 0; j < numDivisions; j++)
            {
                Vector3 testPoint = new Vector3(startingPoint.x + ((2f * i / numDivisions) - 1f) * remainingDistance,
                                                    startingPoint.y, startingPoint.z + ((2f * j / numDivisions) - 1f) * remainingDistance);

                if (DrawDebugInformation)
                    Debug.DrawRay(testPoint, Vector3.up, Color.green, 10f);


                if (VerifyPathIsValidFromUniqueLocation(startingPoint, testPoint, remainingDistance, out distance, out outPosition))
                {
                    float distanceFromClosestPoint = float.MaxValue;
                    Vector3 tempOut;

                    if (DrawDebugInformation)
                        Debug.DrawRay(testPoint, Vector3.up, Color.grey, 10f);

                    if (VerifyPathIsValidFromUniqueLocation(outPosition, targetPoint, float.MaxValue, out distanceFromClosestPoint, out tempOut))
                    {
                        if (DrawDebugInformation)
                            Debug.DrawRay(testPoint, Vector3.up, Color.magenta, 10f);

                        hasFoundValidPath = true;
                        if (distanceFromClosestPoint < minDistance)
                        {
                            minDistance = distanceFromClosestPoint;
                            finalPosition = outPosition;
                            outDistance = distance;
                        }
                    }
                }
                    
            }
        }

        distance = outDistance;

        return hasFoundValidPath;
    }

    public bool VerifyValidRetreatPoint(Vector3 startingPoint, float remainingDistance, List<ActorController> enemies, out float distance, out Vector3 finalPosition, int numDivisions = 10)
    {
        bool hasFoundValidPath = false;
        float maxDistance = -1f;
        float maxSummedDistance = -1f;
        distance = 0f;
        finalPosition = startingPoint;

        Vector3 outPosition;
        for(int i = 0; i < numDivisions; i++)
        {
            for (int j = 0; j < numDivisions; j++)
            {
                Vector3 testPoint = new Vector3(startingPoint.x + ((2f*i / numDivisions) - 1f) * remainingDistance, 
                                                    startingPoint.y, startingPoint.z + ((2f * j / numDivisions) - 1f) * remainingDistance);

                if(VerifyPathIsValidFromUniqueLocation(startingPoint, testPoint, remainingDistance, out distance, out outPosition))
                {
                    hasFoundValidPath = true;
                    if (enemies == null || enemies.Count == 0)
                    {
                        if (distance > maxDistance)
                        {
                            maxDistance = distance;
                            finalPosition = outPosition;
                        }
                    } else
                    {
                        float summedDistance = 0f; 
                        foreach(ActorController enemy in enemies)
                        {
                            summedDistance = Vector3.Distance(outPosition, enemy.transform.position);
                        }
                        if(summedDistance > maxSummedDistance)
                        {
                            maxDistance = distance;
                            finalPosition = outPosition;
                            maxSummedDistance = summedDistance;
                        }
                    }
                }
            }
        }

        distance = maxDistance;

        return hasFoundValidPath;
    }
}
