using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public GameObject creature;
    
    public int timeSpeed;
    // Start is called before the first frame update
    void Start()
    {
        
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

    public void SummonCreature(GameObject parent)
    {
        var obj = Instantiate(creature, new Vector3(parent.transform.position.x + Random.Range(-3f, 3f), parent.transform.position.y + Random.Range(-3f, 3f), -.5f), Quaternion.identity);
        obj.GetComponent<NeuralNetwork>().layers = parent.GetComponent<NeuralNetwork>().CopyLayers();
        obj.SetActive(true);
        obj.GetComponent<Behavior>().generation = parent.GetComponent<Behavior>().generation + 1;
        obj.GetComponentInChildren<TextMeshPro>().text = obj.GetComponent<Behavior>().generation.ToString();
    }
}
