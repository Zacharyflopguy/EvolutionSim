using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FitnessGraph : MonoBehaviour
{
    public LineRenderer lineRenderer;
    
    // Set a scale for the graph
    public float graphHeight = 5f;
    public float graphWidth = 10f;
    public Vector3 graphOffset;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void UpdateGraph()
    {
        // Get the list of logged fitness scores
        List<float> fitnessScores = GameManager.Instance.fitnessLogger.GetFitnessScores();

        // Check if there are enough points to create a graph
        if (fitnessScores.Count <= 1)
        {
            Debug.LogWarning("Not enough points to create a graph.");
            return;
        }

        // Set the number of points in the LineRenderer
        lineRenderer.positionCount = fitnessScores.Count;

        float stepX = graphWidth / (fitnessScores.Count - 1);  // Distance between points on X-axis
        float maxFitness = GameManager.Instance.fitnessLogger.GetMaxFitness();  // Get the maximum fitness score

        // Make sure maxFitness is valid (not zero or NaN)
        if (maxFitness <= 0)
        {
            Debug.LogWarning("Invalid max fitness value.");
            return;
        }

        // Loop through all fitness scores and place points on the graph
        for (int i = 0; i < fitnessScores.Count; i++)
        {
            float normalizedFitness = fitnessScores[i] / maxFitness;  // Normalize fitness value

            // Validate normalized fitness values
            if (float.IsNaN(normalizedFitness) || float.IsInfinity(normalizedFitness))
            {
                Debug.LogWarning("Invalid normalized fitness value.");
                continue;
            }

            Vector3 position = new Vector3((i * stepX), normalizedFitness * graphHeight, -1f) + graphOffset;  // Position the point
            lineRenderer.SetPosition(i, position);  // Set point in LineRenderer
        }
    }

    void Update()
    {
        UpdateGraph();
    }
}
