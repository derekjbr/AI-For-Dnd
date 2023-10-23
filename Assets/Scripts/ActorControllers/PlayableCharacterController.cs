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

    private Animator AnimatorController;
    private LineRenderer LineRenderer;
    private NavMeshPath TestPath;
    private PathType ShownPath = PathType.None;

    enum PathType
    {
        CurrentPath,
        TestPath,
        None
    }
    private void Start()
    {
        // Fetch Required Components
        Agent = GetComponent<NavMeshAgent>();
        AnimatorController = GetComponent<Animator>();
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
        } else
        {
            Agent.isStopped = false;
        }
    }

    override protected void Roam()
    {
        UpdateShownPath();
        UpdateAgent();
    }

    protected override void TakeTurn()
    {
        throw new NotImplementedException();
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
        Agent.destination = des;
        ShownPath = PathType.CurrentPath;
    }

    public void SetTestDestination(Vector3 des)
    {
        if (Agent.CalculatePath(des, TestPath) && ShownPath != PathType.CurrentPath)
        {
            ShownPath = PathType.TestPath;
        }
    }
}
