using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public GameObject creature;
    
    public int timeSpeed;

    public List<GameObject> creaturesForRound;
    
    public Vector2 minSpawn;
    
    public Vector2 maxSpawn;

    public int roundNumber;

    public int numPerRound;
    
    public float timeForRound;
    
    [NonSerialized]
    public FitnessLogger fitnessLogger;
    
    public SpawnFood foodSpawner;

    // Start is called before the first frame update
    void Start()
    {
        roundNumber = 0;
        StartCoroutine(RoundControl(timeForRound));
        fitnessLogger = new FitnessLogger();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void Awake()
    {
        //Handles the first time run case
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else if (Instance != this)
        {
            //Handles every other time the script is run
            Destroy(gameObject);
        }
    }

    public GameObject SummonCreature(GameObject parent)
    {
        var obj = Instantiate(creature, new Vector3(Random.Range(minSpawn.x, maxSpawn.x), Random.Range(minSpawn.y, maxSpawn.y),
            -.5f), Quaternion.identity);
        obj.GetComponent<NeuralNetwork>().layers = parent.GetComponent<NeuralNetwork>().CopyLayers();
        obj.SetActive(true);
        obj.GetComponent<Behavior>().generation = parent.GetComponent<Behavior>().generation + 1;
        obj.GetComponentInChildren<TextMeshPro>().text = obj.GetComponent<Behavior>().generation.ToString();

        // parent.GetComponent<NeuralNetwork>().PrintNetwork();
        // parent.GetComponent<NeuralNetwork>().PrintNetwork(parent.GetComponent<NeuralNetwork>().CopyLayers());
        
        return obj;
    }
    
    public void SpawnOnNewRound(int numCreaturesPerRound)
    {
        List<GameObject> tempNewObjs = new List<GameObject>();

        if (roundNumber == 0)
        {
            for (int i = 0; i < numCreaturesPerRound; i++)
            {
                Vector3 pos = new Vector3(Random.Range(minSpawn.x, maxSpawn.x), Random.Range(minSpawn.y, maxSpawn.y),
                    -.5f);

                // Collider[] hitColliders = Physics.OverlapSphere(pos, 2, 3);
                //
                // if (hitColliders.Length != 0)
                // {
                //     Spawn(numCreaturesPerRound);
                // }

                var obj = Instantiate(creature, pos, Quaternion.identity);
                obj.SetActive(true);
                tempNewObjs.Add(obj);
            }
        }
        else
        {
            foodSpawner.ClearFoodList();
            foodSpawner.Spawn(foodSpawner.numFoodStart);
            var parent = FitnessCalc();
            for (int i = 0; i < numCreaturesPerRound; i++)
            {
                try
                {
                    //TODO treats all survivors equally, and not all may produce. See if there is a way to weight the survivors?
                    var child = SummonCreature(parent);
                    tempNewObjs.Add(child);
                }
                catch (Exception e)
                {
                    Vector3 pos = new Vector3(Random.Range(minSpawn.x, maxSpawn.x), Random.Range(minSpawn.y, maxSpawn.y),
                        -.5f);
                    var obj = Instantiate(creature, pos, Quaternion.identity);
                    obj.SetActive(true);
                    tempNewObjs.Add(obj);
                }
            }
        }

        foreach (var obj in creaturesForRound)
        {
            Destroy(obj);
        }
        
        creaturesForRound = tempNewObjs;
    }
    
    
    public IEnumerator RoundControl(float timePerRound)
    {
        while (true)
        {
            SpawnOnNewRound(numPerRound);
            roundNumber++;
            yield return new WaitForSeconds(timePerRound);
        }
    }

    /*private GameObject FitnessCalc()
    {
        if (creaturesForRound.Count <= 0)
            return null;
        
        float bestFitScore = -9999;
        int bestFitIndex = 0;

        for (int i = 0; i < creaturesForRound.Count; i++)
        {
            //Calculate fitness on stamina left on round end and food collected
            int tempFit = (int)creaturesForRound[i].GetComponent<Behavior>().stamina / 2 +
                          creaturesForRound[i].GetComponent<Behavior>().foodCollected * 2 +
                          (int)creaturesForRound[i].GetComponent<Behavior>().velocityFitness / 10;
            
            print((int)creaturesForRound[i].GetComponent<Behavior>().velocityFitness / 10);

            if (tempFit > bestFitScore)
            {
                bestFitScore = tempFit;
                bestFitIndex = i;
            }
        }
        
        fitnessLogger.LogFitness(bestFitScore);

        return creaturesForRound[bestFitIndex];
    }*/
    
    private GameObject FitnessCalc()
    {
        if (creaturesForRound.Count <= 0)
            return null;
    
        float bestFitScore = float.MinValue;  // Use float.MinValue to avoid unrealistic magic numbers
        int bestFitIndex = 0;
    
        float maxStamina = 0f;
        float maxFoodCollected = 0f;
        float maxVelocityFitness = 0f;

        // Find max values for normalization
        for (int i = 0; i < creaturesForRound.Count; i++)
        {
            var behavior = creaturesForRound[i].GetComponent<Behavior>();
            if (behavior.stamina > maxStamina) maxStamina = behavior.stamina;
            if (behavior.foodCollected > maxFoodCollected) maxFoodCollected = behavior.foodCollected;
            if (behavior.velocityFitness > maxVelocityFitness) maxVelocityFitness = (float)behavior.velocityFitness;
        }
    
        for (int i = 0; i < creaturesForRound.Count; i++)
        {
            var behavior = creaturesForRound[i].GetComponent<Behavior>();
        
            // Normalize components to avoid large disparity between them
            float normalizedStamina = (maxStamina == 0) ? 0 : behavior.stamina / maxStamina;
            float normalizedFoodCollected = (maxFoodCollected == 0) ? 0 : behavior.foodCollected / maxFoodCollected;
            float normalizedVelocityFitness = (maxVelocityFitness == 0) ? 0 : (float)behavior.velocityFitness / maxVelocityFitness;

            // Apply weights for each component (can adjust based on desired behavior)
            float staminaWeight = 0.5f;
            float foodWeight = 2f;
            float velocityWeight = 1f;

            float tempFit = normalizedStamina * staminaWeight +
                            normalizedFoodCollected * foodWeight +
                            normalizedVelocityFitness * velocityWeight;
        
            if (tempFit > bestFitScore)
            {
                bestFitScore = tempFit;
                bestFitIndex = i;
            }
        }

        fitnessLogger.LogFitness(bestFitScore);

        return creaturesForRound[bestFitIndex];
    }
}
