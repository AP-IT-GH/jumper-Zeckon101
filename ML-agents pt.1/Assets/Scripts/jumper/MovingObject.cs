using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObject : MonoBehaviour
{
    public float speedMin = 5f;
    public float speedMax = 10f;

    private float speed;

    private void Start()
    {
        // Get a random speed between speedMin and speedMax for each episode
        speed = Random.Range(speedMin, speedMax);
    }

    private void Update()
    {
        transform.Translate(Vector3.back * speed * Time.deltaTime);

    }

}
