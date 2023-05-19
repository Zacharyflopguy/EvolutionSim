using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnFood : MonoBehaviour
{
    public GameObject prefab;
    public int numFoodStart;
    public int spawnRate;
    public Vector2 minSpawn;
    public Vector2 maxSpawn;
    
    public List<Vector2> foodPositions;

    // Start is called before the first frame update
    void Start()
    {
        Spawn(numFoodStart);
        StartCoroutine(waitSpawn(1));
        
    }

    private void Update()
    {
        Time.timeScale = GameManager.Instance.timeSpeed;
    }

    public void Spawn(int numFood)
    {
        for (int i = 0; i < numFood; i++)
        {
            Vector3 pos = new Vector3(Random.Range(minSpawn.x, maxSpawn.x), Random.Range(minSpawn.y, maxSpawn.y), -.5f);
            
            Collider[] hitColliders = Physics.OverlapSphere(pos, 2, 3);
            
            if (hitColliders.Length != 0)
            {
                Spawn(numFood);
            }
            
            var obj = Instantiate(prefab, pos, Quaternion.identity);
            obj.SetActive(true);
            foodPositions.Add(obj.transform.position);
        }
    }
    
    public IEnumerator waitSpawn(int numFood)
    {
        while(true)
        {
            yield return new WaitForSeconds(spawnRate);
            Spawn(numFood);
        }
    }
}
