using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnFood : MonoBehaviour
{
    public GameObject prefab;
    public int numFoodStart;
    public float spawnRate;
    public Vector2 minSpawn;
    public Vector2 maxSpawn;

    private List<GameObject> spawnedFood;
    
    public List<Vector2> foodPositions;

    // Start is called before the first frame update
    void Start()
    {
        spawnedFood = new List<GameObject>();
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
            //obj = new GameObject();
            obj.SetActive(true);
            spawnedFood.Add(obj);
            foodPositions.Add(obj.transform.position);
        }
    }

    public void Despawn(GameObject foodObj)
    {
        foodPositions.Remove(foodObj.transform.position);
    }

    public void ClearFoodList()
    {
        foreach (GameObject obj in spawnedFood)
        {
            Destroy(obj);
        }
        spawnedFood.Clear();
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
