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
    


    // Start is called before the first frame update
    void Start()
    {
        inputs = new float[3];
        StartCoroutine(staminaDrain(1));
        StartCoroutine(reproduce());
        isMutated = false;
        secondsWithoutFood = 0;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Time.timeScale = GameManager.Instance.timeSpeed;
        
        if (!isMutated)
        {
            GetComponent<NeuralNetwork>().MutateNetwork(mutationChance, mutationAmount);
            isMutated = true;
        }
        

        inputs[0] = closestFood().x;
        inputs[1] = closestFood().y;
        inputs[2] = stamina;

        float[] outputs = gameObject.GetComponent<NeuralNetwork>().Brain(inputs);

        //print(outputs[0]);
        
        Move(outputs[0], outputs[1]);
        
        Vector3 newPos = new Vector3();
        newPos.x = Mathf.Clamp(transform.position.x, minBounds.x, maxBounds.x);
        newPos.y = Mathf.Clamp(transform.position.y, minBounds.y, maxBounds.y);
        newPos.z = -.5f;
        
        transform.position = newPos;
        
        Death();
    }

    void Move(float FrontBack, float LeftRight)
    {
        transform.Rotate(0, 0, LeftRight * rotateSpeed * Time.deltaTime);

        initialPos = transform.position;

        var fast = FrontBack * speed * Time.deltaTime;

        if (fast > speedCap)
        {
            fast = speedCap;
        }
        
        transform.Translate(Vector3.up * (fast));

        velocity = Mathf.Sqrt(Mathf.Pow(Mathf.Abs(transform.position.x - initialPos.x), 2) +
                              Mathf.Pow(Mathf.Abs(transform.position.y - initialPos.y), 2));
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
        if (stamina <= 0)
        {
            Destroy(gameObject);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Food"))
        {
            stamina += 5;
            secondsWithoutFood = 0;
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
            if (stamina >= 20)
            {
                GameManager.Instance.SummonCreature(gameObject);
            }
        }
    }
    
}
