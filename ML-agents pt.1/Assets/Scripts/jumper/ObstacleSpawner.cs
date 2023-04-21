using Unity.MLAgents;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject movingObjectPrefab;

    private void Start()
    {
        Academy.Instance.AgentPreStep += SpawnMovingObject;
    }

    private void SpawnMovingObject(int academyStepCount)
    {
        if (academyStepCount == 0) // Spawn at the beginning of each episode
        {
            GameObject movingObject = Instantiate(movingObjectPrefab, transform.position, Quaternion.identity);
            movingObject.GetComponent<MovingObject>();
        }
    }
}
