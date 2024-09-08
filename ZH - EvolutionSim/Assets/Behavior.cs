using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Behavior : MonoBehaviour
{
    
    public int generation; 
    
    public float[] inputs;
    public float mutationChance;
    [FormerlySerializedAs("mutationRate")] 
    public float mutationAmount;
    public int speed;
    public float stamina;
    public float staminaMax;
    public int rotateSpeed;

    public Vector2 minBounds;
    public Vector2 maxBounds;

    public GameObject foodSpawner;

    public GameObject prefab;

    private bool isMutated;

    private Vector2 initialPos;
    
    public float velocity;

    public float secondsWithoutFood;

    public float speedCap;

    public float rayDist;

    public float rayAngleOffset;

    public LayerMask layer;

    public int foodCollected;
    
    private Vector2 rightRayDirection;
    
    private Vector2 leftRayDirection;

    [NonSerialized]
    public double velocityFitness;
    
    



    // Start is called before the first frame update
    void Start()
    {
        inputs = new float[6];
        StartCoroutine(staminaDrain(1));
        //StartCoroutine(reproduce());
        isMutated = false;
        secondsWithoutFood = 0;
        foodCollected = 0;
        stamina = staminaMax;
        velocityFitness = 0;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Time.timeScale = GameManager.Instance.timeSpeed;

        if (!isMutated)
        {
            GetComponent<NeuralNetwork>().MutateNetwork(mutationChance, mutationAmount);
            isMutated = true;
            
            // GetComponent<NeuralNetwork>().PrintNetwork();
        }
        
        //Set Angle Offsets
        leftRayDirection = Quaternion.Euler(0, 0, rayAngleOffset) * transform.up;
        rightRayDirection = Quaternion.Euler(0, 0, -rayAngleOffset) * transform.up;
        
        RaycastHit2D centerRaycast = Physics2D.Raycast(gameObject.transform.position, (gameObject.transform.up), rayDist, layer);
        RaycastHit2D leftRaycast = Physics2D.Raycast(gameObject.transform.position, (leftRayDirection), rayDist, layer);
        RaycastHit2D rightRaycast = Physics2D.Raycast(gameObject.transform.position, (rightRayDirection), rayDist, layer);

        Debug.DrawRay(gameObject.transform.position, (gameObject.transform.up * rayDist), Color.red, .02f);
        Debug.DrawRay(gameObject.transform.position, (leftRayDirection * rayDist), Color.red, .02f);
        Debug.DrawRay(gameObject.transform.position, (rightRayDirection *  rayDist), Color.red, .02f);
        
        //Set Inputs
        inputs[0] = NormalizeDistance(leftRaycast.distance, rayDist);
        inputs[1] = NormalizeDistance(rightRaycast.distance, rayDist);
        inputs[2] = NormalizeDistance(centerRaycast.distance, rayDist);
        inputs[3] = NormalizeStamina(stamina, staminaMax); //Stamina
        inputs[4] = NormalizeVelocity(velocity, speedCap); //Speed
        inputs[5] = Mathf.Sin(transform.eulerAngles.z * Mathf.Deg2Rad); //Rotation

        float[] outputs = gameObject.GetComponent<NeuralNetwork>().Brain(inputs);
        
        Move(outputs[0], outputs[1]);
        
        Vector3 newPos = new Vector3();
        newPos.x = Mathf.Clamp(transform.position.x, minBounds.x, maxBounds.x);
        newPos.y = Mathf.Clamp(transform.position.y, minBounds.y, maxBounds.y);
        newPos.z = -.5f;
        
        transform.position = newPos;
        
        //Clamp Stamina
        //stamina = Mathf.Clamp(stamina, 0, staminaMax);

        VelocityFitnessCalc();
        
        Death();
    }
    
    void Move(float FrontBack, float LeftRight)
    {
        // Rotate the creature
        transform.Rotate(0, 0, LeftRight * rotateSpeed * Time.deltaTime);

        // Store the initial position before moving
        initialPos = transform.position;

        // Calculate movement speed
        var fast = FrontBack * speed * Time.deltaTime;

        // Cap the speed if necessary
        if (fast > speedCap)
        {
            fast = speedCap;
        }

        // Move the creature along the local "up" axis (forward direction)
        transform.Translate(Vector3.up * fast);

        // Calculate the movement direction vector (from initial position to new position)
        Vector2 movementDirection = ((Vector2)transform.position - initialPos).normalized;

        // Get the creature's forward direction
        Vector2 forwardDirection = transform.up.normalized;

        // Calculate the velocity magnitude (Euclidean distance between positions)
        float velocityMagnitude = Vector2.Distance(initialPos, transform.position);

        // Use dot product to determine if movement is in the forward direction or backward
        float alignment = Vector2.Dot(forwardDirection, movementDirection);

        // If alignment is negative, it means the creature is moving backward
        if (alignment < 0)
        {
            velocityMagnitude = -velocityMagnitude;  // Invert the velocity to indicate backward movement
        }

        // Set velocity to the directionally aware magnitude
        velocity = velocityMagnitude;
    }

    //TODO this is so bad pls fix
    Vector2 closestFood()
    {
        Vector2 ClosePos = Vector2.positiveInfinity;
        
        foreach (var food in foodSpawner.GetComponent<SpawnFood>().foodPositions)
        {
            if(Vector2.Distance(transform.position, food) < Vector2.Distance(transform.position, ClosePos))
            {
                ClosePos = food;
            }
        }

        return ClosePos;
    }
    
    void Death()
    {
        /*if (stamina <= 0)
        {
            GameManager.Instance.creaturesForRound.Remove(gameObject);
            Destroy(gameObject);
        }*/
    }
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Food"))
        {
            stamina += 5;
            secondsWithoutFood = 0;
            foodCollected++;
            foodSpawner.GetComponent<SpawnFood>().Despawn(col.gameObject);
            Destroy(col.gameObject);
        }
    }
    
    IEnumerator staminaDrain(int time)
    {
        while (true)
        {
            stamina -= (1 * velocity) + secondsWithoutFood;
            secondsWithoutFood += 1;
            yield return new WaitForSeconds(time);
        }
    }

    IEnumerator reproduce()
    {
        while (true)
        {
            yield return new WaitForSeconds(3);
            if (stamina >= 25)
            {
                GameManager.Instance.SummonCreature(gameObject);
            }
        }
    }
    
    public float NormalizeDistance(float distance, float maxDistance)
    {
        return Mathf.Clamp01(distance / maxDistance);
    }
    
    float NormalizeStamina(float currentStamina, float maxStamina)
    {
        return Mathf.Clamp01(currentStamina / maxStamina);
    }
    
    float NormalizeVelocity(float currentVelocity, float maxVelocity)
    {
        return Mathf.Clamp01(currentVelocity / maxVelocity);
    }

    void VelocityFitnessCalc()
    {
        if (velocity > 0)
            velocityFitness += 0.1;
        else
            velocityFitness -= 0.1;
    }
}
