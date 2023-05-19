using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Behavior : MonoBehaviour
{
    public float[] inputs;
    public float mutationChance;
    [FormerlySerializedAs("mutationRate")] 
    public float mutationAmount;
    public int speed;
    public int stamina;
    public int rotateSpeed;

    public Vector2 minBounds;
    public Vector2 maxBounds;

    public GameObject foodSpawner;

    public GameObject prefab;

    private bool isMutated;


    // Start is called before the first frame update
    void Start()
    {
        inputs = new float[2];
        StartCoroutine(staminaDrain(1));
        StartCoroutine(reproduce());
        isMutated = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isMutated)
        {
            GetComponent<NeuralNetwork>().MutateNetwork(mutationChance, mutationAmount);
            isMutated = true;
        }
        
        
        Vector2 newPos = new Vector2(Mathf.Clamp(transform.position.x, minBounds.x, maxBounds.x), Mathf.Clamp(transform.position.y, minBounds.y, maxBounds.y));
        transform.position = newPos;

        inputs[0] = closestFood().x;
        inputs[1] = closestFood().y;

        float[] outputs = gameObject.GetComponent<NeuralNetwork>().Brain(inputs);
        
        
        Move(outputs[0], outputs[1]);
        
        Death();
    }

    void Move(float FrontBack, float LeftRight)
    {
        transform.Rotate(0, 0, LeftRight * rotateSpeed * Time.deltaTime);
        
        transform.Translate(Vector3.forward * (FrontBack * speed * Time.deltaTime));
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
            stamina += 10;
            Destroy(col.gameObject);
        }
    }
    
    IEnumerator staminaDrain(int time)
    {
        while (true)
        {
            stamina -= 1;
            yield return new WaitForSeconds(time);
        }
    }

    IEnumerator reproduce()
    {
        while (true)
        {
            yield return new WaitForSeconds(2);
            if (stamina >= 15)
            {
                var obj = Instantiate(prefab, new Vector3(transform.position.x + Random.Range(-1, 2), transform.position.y + Random.Range(-1, 2), 0), Quaternion.identity);
                obj.GetComponent<NeuralNetwork>().layers = GetComponent<NeuralNetwork>().CopyLayers();
                stamina -= 5;
            }
        }
    }
    
}
