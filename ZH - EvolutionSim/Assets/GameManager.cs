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

    // Start is called before the first frame update
    void Start()
    {
        roundNumber = 0;
        StartCoroutine(RoundControl(timeForRound));
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
            print(obj);
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

    private GameObject FitnessCalc()
    {
        if (creaturesForRound.Count <= 0)
            return null;
        
        float bestFitScore = -9999;
        int bestFitIndex = 0;

        for (int i = 0; i < creaturesForRound.Count; i++)
        {
            //Calculate fitness on stamina left on round end and food collected
            int tempFit = ((int)creaturesForRound[i].GetComponent<Behavior>().stamina / 2) +
                          creaturesForRound[i].GetComponent<Behavior>().foodCollected * 2;

            if (tempFit > bestFitScore)
            {
                bestFitScore = tempFit;
                bestFitIndex = i;
            }
        }

        return creaturesForRound[bestFitIndex];
    }
}
