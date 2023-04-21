using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;

public class CubeAgentJump : Agent
{
    public float jumpForce = 10f;
    public Transform groundCheck;
    public LayerMask groundMask;
    public GameObject[] obstaclePrefabs;
    public float obstacleSpawnDistance = 25f;
    public float minObstacleSpawnTime = 3f;
    public float maxObstacleSpawnTime = 6f;

    //private GameObject currentObstacle;
    private List<GameObject> obstacles = new List<GameObject>();
    private Rigidbody rb;
    private bool isGrounded;
    private float obstacleSpawnTimer;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {

        // Destroy any existing obstacles
        /*if (currentObstacle != null)
        {
            Destroy(currentObstacle);
            currentObstacle = null;
        }*/


        if (obstacles.Count > 0)
        {
            foreach (GameObject obstacle in obstacles)
            {
                Destroy(obstacle);
            }
            obstacles.Clear();
        }


        rb.velocity = Vector3.zero;
        transform.localPosition = Vector3.zero;

        SpawnObstacle();

        isGrounded = false;
        obstacleSpawnTimer = 0f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(isGrounded);
        sensor.AddObservation(transform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (actions.DiscreteActions[0] == 1 && isGrounded)
        {
            Jump();
        }

        if (transform.localPosition.y < 0)
        {
            Fall();
        }

        if (actions.DiscreteActions[0] == 1 && isGrounded)
        {
            Jump();
            foreach (GameObject obstacle in obstacles)
            {
                obstacle.GetComponent<Rigidbody>().AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }

        foreach (GameObject obstacle in obstacles)
        {
            if (obstacle.transform.localPosition.y < 0 || obstacle.transform.localPosition.z < -10f)
            {
                ObstacleFall();
            }
        }

        obstacleSpawnTimer += Time.deltaTime;
        if (obstacleSpawnTimer >= Random.Range(minObstacleSpawnTime, maxObstacleSpawnTime))
        {
            SpawnObstacle();
            obstacleSpawnTimer = 0f;
        }
    }

    private void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = false;
    }

    private void Fall()
    {
        Debug.Log("Falls off the ground");
        SetReward(-1.0f);
        EndEpisode();
    }

    private void ObstacleFall()
    {
        List<GameObject> obstaclesToRemove = new List<GameObject>();
        foreach (GameObject obstacle in obstacles)
        {
            if (obstacle.transform.localPosition.y < 0 || obstacle.transform.localPosition.z < -10f)
            {
                obstaclesToRemove.Add(obstacle);
                Debug.Log("Object falls off the ground. Reward: 1.0f");
                SetReward(1.0f);
            }
        }

        foreach (GameObject obstacleToRemove in obstaclesToRemove)
        {
            obstacles.Remove(obstacleToRemove);
            Destroy(obstacleToRemove);
            //SpawnObstacle();
        }
    }


    private void SpawnObstacle()
    {
        int obstacleIndex = Random.Range(0, obstaclePrefabs.Length);
        GameObject newObstacle = Instantiate(obstaclePrefabs[obstacleIndex], new Vector3(transform.position.x, 1, obstacleSpawnDistance), Quaternion.identity);
        newObstacle.GetComponent<MovingObject>().enabled = true;

        obstacles.Add(newObstacle);
        Debug.Log("New object spawns");
    }


    private void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Obstacle"))
        {
            GameObject collidedObstacle = col.gameObject;
            if (obstacles.Contains(collidedObstacle))
            {
                obstacles.Remove(collidedObstacle);
                Destroy(collidedObstacle);
            }
            AddReward(-0.01f);
            Debug.Log("Object hit. New episode. Reward: -0.1f");
            EndEpisode();
        }

    }

    private void FixedUpdate()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, 0.1f, groundMask);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
    }
}