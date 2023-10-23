using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    // Seen Variables
    public float CameraSpeed = 10.0f;
    public float SlowDownTime = 0.5f;
    public float SpeedUpTime = 1.0f;
    public float HeigtOfCamera = 5.0f;
    public float MoveBackTime = 5.0f;
    public float RotationSpeed = 60f;

    public bool DebugRotationPoint = false;
    
    public GameObject Selected = null;

    private float RotationDistance = 7.81f;
    private float[] MovementStartTimes = new float[4];
    private float[] MovementEndTimes = new float[4];
    private Vector3[] CurrentSpeedImpact = new Vector3[4];
    private Vector3[] SpeedAtEndOfMovement = new Vector3[4];

    private bool IsInMoveBack = false;
    private Vector3 MoveBackStartPosition = Vector3.zero;
    private Vector3 MoveBackEndPosition = Vector3.zero;
    private float MoveBackStartTime = 0f;

    private static int FORWARD = 0;
    private static int BACKWARD = 1;
    private static int LEFT = 2;
    private static int RIGHT = 3;



    void Start()
    {
        CalculateRotationPoint();
    }

    // Update is called once per frame
    void Update()
    {
        float currentTime = Time.time;

        // FORWARD
        if (Input.GetKeyDown(KeyCode.W))
        {
            MovementStartTimes[FORWARD] = currentTime;
        } 
        else if (Input.GetKey(KeyCode.W)) 
        {
            CurrentSpeedImpact[FORWARD] = Vector3.Lerp(
                Vector3.zero, 
                new Vector3(transform.forward.x, 0, transform.forward.z).normalized * CameraSpeed, 
                (currentTime - MovementStartTimes[FORWARD]) / SpeedUpTime);
        } 
        else if(Input.GetKeyUp(KeyCode.W))
        {
            MovementEndTimes[FORWARD] = currentTime;
            SpeedAtEndOfMovement[FORWARD] = CurrentSpeedImpact[FORWARD];
        } 
        else if(!Input.GetKey(KeyCode.W))
        {
            CurrentSpeedImpact[FORWARD] = Vector3.Lerp(
                SpeedAtEndOfMovement[FORWARD],
                Vector3.zero,
                (currentTime - MovementEndTimes[FORWARD]) / SlowDownTime);
        }

        // BACKWARD
        if (Input.GetKeyDown(KeyCode.S))
        {
            MovementStartTimes[BACKWARD] = currentTime;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            CurrentSpeedImpact[BACKWARD] = Vector3.Lerp(
                Vector3.zero,
                new Vector3(transform.forward.x, 0, transform.forward.z).normalized * CameraSpeed * -1f,
                (currentTime - MovementStartTimes[BACKWARD]) / SpeedUpTime);
        }
        else if (Input.GetKeyUp(KeyCode.S))
        {
            MovementEndTimes[BACKWARD] = currentTime;
            SpeedAtEndOfMovement[BACKWARD] = CurrentSpeedImpact[BACKWARD];
        }
        else if (!Input.GetKey(KeyCode.S))
        {
            CurrentSpeedImpact[BACKWARD] = Vector3.Lerp(
                SpeedAtEndOfMovement[BACKWARD],
                Vector3.zero,
                (currentTime - MovementEndTimes[BACKWARD]) / SlowDownTime);
        }

        // LEFT
        if (Input.GetKeyDown(KeyCode.A))
        {
            MovementStartTimes[LEFT] = currentTime;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            CurrentSpeedImpact[LEFT] = Vector3.Lerp(
                Vector3.zero,
                new Vector3(transform.right.x, 0, transform.right.z).normalized * CameraSpeed * -1f,
                (currentTime - MovementStartTimes[LEFT]) / SpeedUpTime);
        }
        else if (Input.GetKeyUp(KeyCode.A))
        {
            MovementEndTimes[LEFT] = currentTime;
            SpeedAtEndOfMovement[LEFT] = CurrentSpeedImpact[LEFT];
        }
        else if (!Input.GetKey(KeyCode.A))
        {
            CurrentSpeedImpact[LEFT] = Vector3.Lerp(
                SpeedAtEndOfMovement[LEFT],
                Vector3.zero,
                (currentTime - MovementEndTimes[LEFT]) / SlowDownTime);
        }

        // RIGHT
        if (Input.GetKeyDown(KeyCode.D))
        {
            MovementStartTimes[RIGHT] = currentTime;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            CurrentSpeedImpact[RIGHT] = Vector3.Lerp(
                Vector3.zero,
                new Vector3(transform.right.x, 0, transform.right.z).normalized * CameraSpeed,
                (currentTime - MovementStartTimes[RIGHT]) / SpeedUpTime);
        }
        else if (Input.GetKeyUp(KeyCode.D))
        {
            MovementEndTimes[RIGHT] = currentTime;
            SpeedAtEndOfMovement[RIGHT] = CurrentSpeedImpact[RIGHT];
        }
        else if (!Input.GetKey(KeyCode.D))
        {
            CurrentSpeedImpact[RIGHT] = Vector3.Lerp(
                SpeedAtEndOfMovement[RIGHT],
                Vector3.zero,
                (currentTime - MovementEndTimes[RIGHT]) / SlowDownTime);
        }

        if(DebugRotationPoint)
        {
            Vector3 rotationPoint = (transform.position + (transform.forward * RotationDistance));
            Debug.DrawRay(rotationPoint, Vector3.up * 3f, Color.green);
        }

        if(Input.GetKey(KeyCode.Q) && !IsInMoveBack)
        {
            Vector3 rotationPoint = (transform.position + (transform.forward * RotationDistance));
            transform.RotateAround(rotationPoint, Vector3.up, RotationSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.E) && !IsInMoveBack)
        {
            Vector3 rotationPoint = (transform.position + (transform.forward * RotationDistance));
            transform.RotateAround(rotationPoint, Vector3.up, RotationSpeed * Time.deltaTime * -1f);
        }

        if (Input.GetKeyDown(KeyCode.Space) && Selected != null)
        {
            if (!IsInMoveBack)
            {
                // Angle Between -Z and Camera Vector 
                float theta = Mathf.Acos(Vector3.Dot(transform.forward.normalized, Vector3.down));
                // Length of projected vector
                float length = HeigtOfCamera * Mathf.Tan(theta);
                // Projected vector plus height of camera 
                MoveBackEndPosition = Selected.transform.position
                    - new Vector3(transform.forward.x, 0.0f, transform.forward.z).normalized * length
                    + new Vector3(0f, HeigtOfCamera, 0f);

                MoveBackStartPosition = transform.transform.position;

                // Cancel Movement  
                for (int i = 0; i < 4; i++)
                {
                    CurrentSpeedImpact[i] = Vector3.zero;
                    SpeedAtEndOfMovement[i] = Vector3.zero;
                    MovementStartTimes[i] = currentTime;
                    MovementEndTimes[i] = currentTime;
                }

                MoveBackStartTime = currentTime;
                IsInMoveBack = true;
            }
        }
        else if(!IsInMoveBack)
        {
            Vector3 movementSpeed = Vector3.zero;
            foreach (Vector3 impact in CurrentSpeedImpact)
            {
                movementSpeed += impact;
            }

            if (movementSpeed.magnitude > CameraSpeed)
            {
                movementSpeed = movementSpeed.normalized * CameraSpeed;
            }

            transform.position += movementSpeed * Time.deltaTime;
        } 
        else if(IsInMoveBack)
        {
            float percent = (currentTime - MoveBackStartTime) / MoveBackTime;
            transform.position = Vector3.Lerp(MoveBackStartPosition, MoveBackEndPosition, percent);
            if (percent > 1) IsInMoveBack = false;
        }
    }

    private void CalculateRotationPoint()
    {
        float theta = Mathf.Acos(Vector3.Dot(transform.forward.normalized, Vector3.down));
        RotationDistance = HeigtOfCamera / Mathf.Cos(theta);
    }
}
