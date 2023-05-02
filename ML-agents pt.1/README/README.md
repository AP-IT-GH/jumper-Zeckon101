Jordy Van Mol
Chadley De Weggheleire

Voorwoord

Beste lezer,

Wat je nu gaat lezen is een leuke jumper game die gemaakt is in Unity. Tijdens deze opdracht zijn mijn labopartner en ik op problemen gestuit. Het grootste probleem was om de agent te laten springen. Dit heeft ons soms veel moeite gekost. Chadley was al aan het nadenken over complexe zaken, terwijl Jordy eerder nadacht over de basis.

We zijn begonnen met kleine stappen om te zien hoe alles afzonderlijk werkt en hoe het er visueel uitziet. Het moeilijkste was het schrijven van een script. We hebben scripts geschreven voor zowel de agent als de objecten.

Het grootste probleem dat we tegenkwamen was dat de agent ons jumper game te makkelijk vond. Waarom te makkelijk? We zagen onze rewards ineens dalen en sindsdien kregen we alleen nog maar 0 waardes. Eerst hebben we geprobeerd om de hyperparameters aan te passen, maar dit had weinig effect. Uiteindelijk hebben we het probleem kunnen oplossen door de code aan te passen en de rewards te veranderen.

We hadden ook heel veel kleine problemen waar we niet al te diep op ingaan. Deze problemen waren onder andere:

- Geen apart spawner script maken voor de objecten aangezien we met episodes werken.
- Objecten die van het platform vallen worden niet vernietigd.
- Objecten gaan de verkeerde richting uit als ze worden gespawned.
- Extra bewegingsmogelijkheden, zoals rond de eigen as draaien, mogen niet samen in het bewegingsscript komen, maar moeten apart in een leeg game object worden geplaatst, anders krijgen we rare bewegingen.
- Jumper die niet wilde stoppen met springen, ook al drukten we niet op de spatiebalk in de heuristic mode.
- Heel raar gedrag van de bewegende objecten.
- Objecten die niet willen spawnen of maar één keer spawnen.
- ...

Inleiding

De CubeAgentJump is een Unity ML-Agent die is getraind om obstakels te ontwijken en zo lang mogelijk op de grond te blijven. In deze aangepaste implementatie kan de agent nu zelf springen om obstakels te ontwijken. Dit werd bereikt door het model te trainen en de Jump()-functie aan te roepen wanneer de agent de "Spring"-actie uitvoert.

Vereisten

• Unity 2019.4.28f1 of hoger
• Unity ML-Agents 2.1.0 of hoger
• Een machine met een GPU

Installatie
• Download en installeer Unity 2019.4.28f1 of hoger.
• Open de Unity Editor en maak een nieuw project aan.
• Importeer de Unity ML-Agents 2.1.0 of hoger package in uw project.
• Importeer het aangepaste CubeAgentJump-script in uw project.
• Installeer Anaconda.

Gebruik

1. Voeg het CubeAgentJump-script toe aan een game object in uw scene.
2. Voeg de volgende componenten toe aan het CubeAgentJump-game object:
a. Rigidbody-component: Voeg een Rigidbody-component toe aan het CubeAgentJump-game object om fysica-simulatie te krijgen.
b. Collider-component: Voeg een Collider-component toe aan het CubeAgentJump-game object om te controleren of de agent op de grond staat.
c. Ground Check-game object: Voeg een nieuw game object toe aan het CubeAgentJump-game object om te controleren of de agent op de grond staat.
3. Voeg de volgende parameters toe aan het CubeAgentJump-script:
a. JumpForce: De kracht die de agent nodig heeft om te springen.
b. GroundCheck: Een Transform die het CubeAgentJump-game object boven de grond plaatst om te controleren of de agent op de grond staat.
c. GroundMask: De laag van de grond waar de agent op kan staan. Alleen objecten met deze laag worden gezien als grond.
d. ObstaclePrefabs: een array van obstacle prefabs die worden gebruikt om willekeurig objecten te spawnen die de agent moet vermijden.
e. ObstacleSpawnDistance: de afstand vanaf de agent waarop obstakels worden gespawned.
f. MinObstacleSpawnTime: de minimale tijd tussen het spawnen van obstakels.
g. MaxObstacleSpawnTime: de maximale tijd tussen het spawnen van obstakels.
h. Obstacles: een lijst met alle obstakels die momenteel in de scène aanwezig zijn.
i. Rb: de Rigidbody-component van de agent.
j. IsGrounded: een boolean die aangeeft of de agent op de grond staat.
k. ObstacleSpawnTimer: een timer die bijhoudt hoe lang het geleden is dat er een obstakel gespawned is.
4. Voeg of maak de gewenste prefabs en sleep deze naar de ObstaclePrefabs-array.
5. Gebruik een Ray Perception Sensor 3D om de agent ogen te geven.
6. Zet de branch 0 size op 2 om ervoor te zorgen dat de jumper daadwerkelijk kan springen.

CubeAgentJump script:

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

MovingObject script:

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


RotatingObject script: 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingObject : MonoBehaviour
{
    private void Update()
    {
        //transform.Translate(Vector3.back * speed * Time.deltaTime);
        transform.Rotate(new Vector3(0.5f, 0, 0));
    }
}





Rewards:

![](Aspose.Words.c70436d2-24e3-4e90-ae1b-18f13a8637d3.004.png)

Uiteindelijke jumper chart:

![](Aspose.Words.c70436d2-24e3-4e90-ae1b-18f13a8637d3.005.png)

Jumper chart waarbij de jumping na bepaalde tijd in staking ging:

![](Aspose.Words.c70436d2-24e3-4e90-ae1b-18f13a8637d3.006.png)
