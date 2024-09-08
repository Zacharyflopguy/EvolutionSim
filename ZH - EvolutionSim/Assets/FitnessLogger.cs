using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FitnessLogger : MonoBehaviour
{
    private List<float> fitnessScores;

    public FitnessLogger()
    {
        fitnessScores = new List<float>();
    }

    // Add a new fitness score for a generation
    public void LogFitness(float fitnessScore)
    {
        fitnessScores.Add(fitnessScore);
    }

    // Get the list of all logged fitness scores
    public List<float> GetFitnessScores()
    {
        return fitnessScores;
    }

    // Get the maximum fitness score logged
    public float GetMaxFitness()
    {
        if (fitnessScores.Count == 0)
            return 0f;
        
        return Mathf.Max(fitnessScores.ToArray());
    }

    // Get the latest fitness score logged (most recent generation)
    public float GetLatestFitness()
    {
        if (fitnessScores.Count == 0)
            return 0f;

        return fitnessScores[fitnessScores.Count - 1];
    }
}
