using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Objective : MonoBehaviour
{
    public string objectiveName;
    public enum ObjectiveType
    {
        Null,
        Elimination,
        CaptureAndHold,
        FindExit,
        Survival
    }
    public ObjectiveType objective;

    public bool isObjectiveComplete;


    //Elim data
    [Header("Elimination")]
    public int elimsRequired;
    public int elimsCurrent;


    //Capture and Hold data
    [Header("Capture and Hold")]
    public Transform captureTarget;
    public int holdTime;
    public int holdTimeCurr;


    //Find Exit data
    [Header("Find the Exit")]
    public Transform exitTarget;


    //Survival Data
    [Header("Survival")]
    public int survivalTime;
    public int survivalTimeCurr;


    // Called by the objective manager script
    public bool CheckObjective()
    {
        switch(objective)
        {
            case ObjectiveType.Null: return false;

            case ObjectiveType.Elimination:
                if (elimsCurrent >= elimsRequired)
                    return true;
                else
                    return false;

            case ObjectiveType.FindExit:
                if (Mathf.Abs(Vector3.Distance(GameObject.FindGameObjectWithTag("Player").transform.position, exitTarget.transform.position)) < 1.5f)
                    return true;
                else
                    return false;
        
        }
        return false;
    }

    public string CurrentObjective()
    {
        switch (objective)
        {
            case ObjectiveType.Null: return "Null";

            case ObjectiveType.Elimination:
                if (elimsCurrent >= elimsRequired) // Win Condition
                    return "COMPLETE";
                else
                    return elimsCurrent + "/" + elimsRequired;

            case ObjectiveType.FindExit:
                if (Mathf.Abs(Vector3.Distance(GameObject.FindGameObjectWithTag("Player").transform.position, exitTarget.transform.position)) < 1.5f) // Win Condition
                    return "COMPLETE"; 
                else
                    return "Explore!";

        }
        return "Null";
    }
}

