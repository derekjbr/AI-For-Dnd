using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;

[RequireComponent(typeof(LineRenderer))]
public class PlayableCharacterController : ActorController
{
    public bool IsSelected = false;

    private LineRenderer LineRenderer;
    private NavMeshPath TestPath;
    private PathType ShownPath = PathType.None;

    enum PathType
    {
        CurrentPath,
        TestPath,
        None
    }
    override public void Start()
    {
        base.Start();
        LineRenderer = GetComponent<LineRenderer>();
        TestPath = new NavMeshPath();

        // Set up Line Renderer
        LineRenderer.startWidth = 0.2f;
        LineRenderer.endWidth = 0.2f;
        LineRenderer.positionCount = 0;
    }
    private void DrawPath(NavMeshPath path)
    {
        int lineCount = path.corners.Length;

        // Don't Display Conditions
        if (lineCount < 2)
        {
            return;
        }

        // Display
        LineRenderer.enabled = true;
        LineRenderer.positionCount = lineCount;
        for (int i = 0; i < LineRenderer.positionCount; i++)
        {
            LineRenderer.SetPosition(i, new Vector3(path.corners[i].x, path.corners[i].y, path.corners[i].z));
        }
    }
    private void UpdateShownPath()
    {
        if ((Vector3.Distance(Agent.destination, transform.position) <= Agent.stoppingDistance && ShownPath == PathType.CurrentPath) || !IsSelected)
        {
            ShownPath = PathType.None;
        }

        switch (ShownPath)
        {
            case (PathType.CurrentPath):
                if (Agent.hasPath)
                    DrawPath(Agent.path);
                break;
            case (PathType.TestPath):
                if (TestPath.status == NavMeshPathStatus.PathComplete)
                    DrawPath(TestPath);
                break;
            case (PathType.None):
                LineRenderer.enabled = false;
                break;
            default:
                break;
        }
    }
    private void UpdateAgent()
    {
        if (Vector3.Distance(Agent.destination, transform.position) <= Agent.stoppingDistance)
        {
            Agent.isStopped = true;
        }
    }

    override protected void Roam()
    {
        UpdateShownPath();
        UpdateAgent();

        if(Input.GetKeyDown(KeyCode.P))
        {
            GameMaster.RequestBattle();
        }
    }
    override protected void TakeTurn()
    {
        if (CurrentBattle == null)
            return;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            CurrentBattle.RequestEndOfTurn(this);
            return;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            RaycastHit hit;
            Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(cameraRay, out hit))
            {
                GameObject hitObject = hit.transform.gameObject;
                if (hitObject != null)
                {
                    ActorController hitController = hitObject.GetComponent<ActorController>();
                    if (hitController != null && hitController != this)
                    {
                        if (Stats.AvaliableActions > 0 && Vector3.Distance(this.transform.position, hitController.transform.position) <= Stats.Reach)
                        {
                            CurrentBattle.RequestAttack(this, hitController, Sword.GetInstance());
                            Stats.AvaliableActions -= 1;
                        }
                    }
                }
            }
        }

        UpdateShownPath();
        UpdateAgent();
    }
    override protected void UpdateAnimationState()
    {
        if(Agent.velocity.sqrMagnitude > 0.2)
        {
            AnimatorController.SetBool("IsJogging", true);
        } else
        {
            AnimatorController.SetBool("IsJogging", false);
        }
    }

    public void SetDestination(Vector3 des)
    {
        if (CurrentState == ActorState.Turn)
        {
            float result = VerifyPathIsValidForTurn(des, Stats.RemainingDistance);
            if (result != -1f)
            {
                Agent.SetDestination(des);
                Agent.isStopped = false;
                Stats.RemainingDistance -= result;
                ShownPath = PathType.CurrentPath;
            }
        }
        else if(CurrentState != ActorState.Battle)
        {
            Agent.destination = des;
            ShownPath = PathType.CurrentPath;
            Agent.isStopped = false;
        }
    }

    public override int RollInitiative()
    {
        return 21;
    }

    public void SetTestDestination(Vector3 des)
    {
        if (Agent.CalculatePath(des, TestPath) && ShownPath != PathType.CurrentPath)
        {
            if (CurrentState == ActorState.Turn)
            {
                float result = VerifyPathIsValidForTurn(des, Stats.RemainingDistance);
                if (result != -1f)
                {
                    ShownPath = PathType.TestPath;
                } else
                {
                    ShownPath = PathType.None;
                }
            }
            else
            {
                ShownPath = PathType.TestPath;
            }
        }
    }

    public override void SetCurrentState(ActorState state)
    {
        base.SetCurrentState(state);
        if(state == ActorState.Battle)
        {
            ShownPath = PathType.None;
        }
    }

    public override void BattleInitialization()
    {
        // Nothing to initalize yet
    }
}
