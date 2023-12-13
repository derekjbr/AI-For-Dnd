using System;
using System.Collections.Generic;
using UnityEngine;


public class GoblnController : GoalOrientatedController
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
    private int DirectionOfNodeTraversal;

    [Header("Spotting")]
    public float SpottingTime = 2f;
    public float SpottedSpeed = 2.5f;

    [Header("Scoutting")]
    public float TimeSpentScouting;
    public AnimationCurve LookDirectionCurve;
    private float DebuggingTime = 0;
    private float TestAngleDifference = 0;

    [Header("GOAP")]
    public float FriendStoppingDistance = 5f;

    private float TimeSinceLastEvent = 0f;
    private List<ActorController> Enemies;
    private List<ActorController> Friends;
    private ActorStats RememberedStats;


    override public void Start()
    {
        base.Start();
        GoblinState = GoblinStates.RoamingOnPath;

        CurrentNodeOnRoamingPath = -1;
        DirectionOfNodeTraversal = 1;

        HealthItems.Add(SmallPotion.GetInstance(), 0);
        WorldConditions.Add("canRunAway", 0);
        WorldConditions.Add("IsNotExhausted", 0);
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

    protected override void TakeAction(Action action)
    {
        if (action == null)
            return;

        Type actionType = action.GetType();
        if(actionType == typeof(MoveToEnemyAction))
        {
            MoveToEnemyAction moveToEnemyAction = (MoveToEnemyAction)action;
            Agent.stoppingDistance = Stats.Reach;
            Agent.destination = moveToEnemyAction.Enemy.transform.position;
            Agent.isStopped = false;
            RanThisTurn = true;

            if (Vector3.Distance(transform.position, moveToEnemyAction.Enemy.transform.position) < Stats.Reach)
            {
                Agent.isStopped = true;
                CompleteAction();
            }
        }
        if (actionType == typeof(AttackEnemyAction))
        {
            AttackEnemyAction attackEnemyAction = (AttackEnemyAction)action;
            transform.LookAt(attackEnemyAction.Enemy.transform.position);
            CurrentBattle.RequestAttack(this, attackEnemyAction.Enemy, WeaponInHead);
            CompleteAction();
        }
        if(actionType == typeof(RunAwayAction))
        {
            Agent.stoppingDistance = .1f;
            Agent.isStopped = false;

            if (!DontRepeatCodeDuringAction)
            {
                RememberedStats = ActorStats.Copy(Stats);
                Vector3 retreatPoint;
                float distance;
                VerifyValidRetreatPoint(transform.position, Stats.RemainingDistance, Enemies, out distance, out retreatPoint);
                Agent.destination = retreatPoint;
                DontRepeatCodeDuringAction = true;
                if (!RanThisTurn)
                {
                    NumTernsRunning += 1;
                    RanThisTurn = true;
                }
            }

            if (Vector3.Distance(transform.position, Agent.destination) < .1f)
            {
                Agent.isStopped = true;
                Stats = RememberedStats;
                CompleteAction();
            }
        }
        if(actionType == typeof(MoveTowardsFriend))
        {
            MoveTowardsFriend moveTowardsFriend = (MoveTowardsFriend)action;
            Agent.stoppingDistance = .1f;
            Agent.isStopped = false;
            RanThisTurn = true;

            if (!DontRepeatCodeDuringAction)
            {
                RememberedStats = ActorStats.Copy(Stats);
                Vector3 movePoint;
                float distance;
                FindValidPointClosestToAnother(transform.position, moveTowardsFriend.Friend.transform.position, Stats.RemainingDistance, out distance, out movePoint);
                Agent.destination = movePoint;
                DontRepeatCodeDuringAction = true;
            }

            if (Vector3.Distance(transform.position, Agent.destination) < .1f)
            {
                Agent.isStopped = true;
                Stats = RememberedStats;
                CompleteAction();
            }
        }
        if(actionType == typeof(UseHealthItemAction))
        {
            CompleteAction();
        }    
        if(actionType == typeof(CallForHelp))
        {
            Debug.Log(Name + " called for help!");
            CompleteAction();
        }
        if(actionType == typeof(DashAction))
        {
            Debug.Log(Name + " uses dash!");
            CompleteAction();
        }
        if(actionType == typeof(DoNothingAction))
        {
            NumTernsRunning = 0;
            CompleteAction();
            CurrentBattle.RequestEndOfTurn(this);
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
    public override void InformOfEnemy(ActorController actor)
    {
        if (!WorldConditions.ContainsKey("IsAwareOf" + actor.GetInstanceID()))
            WorldConditions.Add("IsAwareOf" + actor.GetInstanceID(), 0);

        if (!WorldConditions.ContainsKey("knowsAboutBattle"))
            WorldConditions.Add("knowsAboutBattle", 0);
    }
    public override void InformOfBattle()
    {
        if (!WorldConditions.ContainsKey("knowsAboutBattle"))
            WorldConditions.Add("knowsAboutBattle", 0);
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
                WorldConditions.Remove("IsAwareOf" + LastSeenPC.GetInstanceID());
                GoblinState = GoblinStates.Scouting;
                TimeSinceLastEvent = currentTime;
                return;
            }
        } else
        {
            if(!WorldConditions.ContainsKey("IsAwareOf" + LastSeenPC.GetInstanceID()))
                WorldConditions.Add("IsAwareOf"+LastSeenPC.GetInstanceID(), 0);

            if(currentTime - TimeSinceLastEvent > SpottingTime || Vector3.Distance(this.transform.position, LastSeenPC.transform.position) <= Stats.Reach)
            {
                List<ActorController> spottedEnemyAndFriends = new List<ActorController>();
                spottedEnemyAndFriends.Add(LastSeenPC);

                GameMaster.RequestBattle();
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
        if(RoamingPath.Count <= 1)
        {
            GoblinState = GoblinStates.Scouting;

        }
        else if (HasAgentReachedDestination() && GoblinState == GoblinStates.RoamingOnPath)
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

    public override void BattleInitialization()
    {
        base.BattleInitialization();
        Actions.Clear();
        Goals.Clear();
        Enemies = new List<ActorController>();
        Friends = new List<ActorController>();

        foreach (ActorController actor in this.CurrentBattle.GetParticipants())
        {
            if (actor.GetComponent<PlayableCharacterController>() != null)
            {
                Actions.Add(new MoveToEnemyAction(this, actor), 0);
                Actions.Add(new AttackEnemyAction(this, actor), 0);
                Actions.Add(new MoveTowardsEnemy(this, actor, Stats.Reach), 0);
                Goals.Add(new AttackGoal(this, actor), 0);
                Enemies.Add(actor);
            }

            if(actor.GetComponent<GoblnController>() != null && actor != this)
            {
                Actions.Add(new MoveTowardsFriend(this, actor, 5f), 0);
                Goals.Add(new AlertFriend((GoalOrientatedController)actor), 0);
                Friends.Add(actor);
            }
        }

        Actions.Add(new CallForHelp(this, Friends, 20f), 0);
        Actions.Add(new RunAwayAction(this, Enemies), 0);
        Actions.Add(new UseHealthItemAction(this), 0);
        Actions.Add(new DashAction(this), 0);
        Actions.Add(new DoNothingAction(this), 0);

        Goals.Add(new CatchBreathGoal(this, Enemies), 0);
        Goals.Add(new MovedLocations(this, Enemies), 0);
        Goals.Add(new RetreatGoal(this, Enemies), 0);
        Goals.Add(new HealGoal(this, Enemies), 0);

        this.GoblinState = GoblinStates.SpottedEnemy;
    }

    public override void ResetBeforeTurn()
    {
        foreach (ActorController actor in Enemies)
        {
            if (Vector3.Distance(this.transform.position, actor.transform.position) < Stats.Reach)
            {
                if (!WorldConditions.ContainsKey("IsCloseEnoughToAttack" + actor.GetInstanceID()))
                    WorldConditions.Add("IsCloseEnoughToAttack" + actor.GetInstanceID(), 0);
            }
            else
            {
                if (WorldConditions.ContainsKey("IsCloseEnoughToAttack" + actor.GetInstanceID()))
                    WorldConditions.Remove("IsCloseEnoughToAttack" + actor.GetInstanceID());
            }
        }

        base.ResetBeforeTurn();
    }

    protected override void TakeTurn()
    {
        foreach(ActorController actor in Enemies)
        {
            if(actor != this && actor.GetComponent<PlayableCharacterController>() != null)
            {
                if(HasLineOfSightToActor(actor, 3))
                {
                    if (!WorldConditions.ContainsKey("IsAwareOf" + actor.GetInstanceID()))
                        WorldConditions.Add("IsAwareOf"+actor.GetInstanceID(), 0);
                }

                if(Vector3.Distance(this.transform.position, actor.transform.position) < Stats.Reach)
                {
                    if (!WorldConditions.ContainsKey("IsCloseEnoughToAttack" + actor.GetInstanceID()))
                        WorldConditions.Add("IsCloseEnoughToAttack" + actor.GetInstanceID(), 0);
                } else
                {
                    if (WorldConditions.ContainsKey("IsCloseEnoughToAttack" + actor.GetInstanceID()))
                        WorldConditions.Remove("IsCloseEnoughToAttack" + actor.GetInstanceID());
                }
            }
        }
        base.TakeTurn();
    }

    protected override void UpdateAnimationState()
    {
        if (Agent.velocity.sqrMagnitude > 0.05)
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

        if(CurrentState == ActorState.Dead)
        {
            AnimatorController.SetBool("IsScouting", false);
            AnimatorController.SetBool("IsRoaming", false);
            AnimatorController.SetBool("IsDead", true);
        }
    }
}
