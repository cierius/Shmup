using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This class will be used to manage the objectives during each run.
 * The Objective script will be the main way of handling the actual mechanics of the objective system.
 */


public class ObjectiveManager : MonoBehaviour
{
    private GameObject objectiveSlide;
    private TextMesh[] objTexts = new TextMesh[10];

    public List<Objective> objectives = new List<Objective>();
    private int objectiveCount;

    public void Awake()
    {
        objectiveSlide = GameObject.Find("ObjectivesSlide");
        for(int i=0; i<objTexts.Length; i++)
        {
            objTexts[i] = objectiveSlide.transform.GetChild(i).GetComponent<TextMesh>();
        }

        UpdateObjectiveList();
    }

    public void Update()
    {
        if (objectiveCount > 0)
        { 
            foreach (Objective obj in objectives)
            {
                if(obj.CheckObjective())
                {
                    obj.isObjectiveComplete = true;
                    print("Objective Finished!");
                }
            }
        }
    }

    public void FixedUpdate()
    {
        UpdateObjectivesSlide();
    }


    public void UpdateObjectiveList()
    {
        objectives.Clear();
        objectiveCount = 0;

        var objs = GameObject.FindGameObjectsWithTag("Objective");

        foreach(var obj in objs)
        {
            objectives.Add(obj.GetComponent<Objective>());
            objectiveCount++;
        }

        UpdateObjectivesSlide();
    }


    private void UpdateObjectivesSlide()
    {
        int index = 1; // index 0 is the title Objectives

        foreach(var obj in objectives)
        {
            if (!obj.isObjectiveComplete)
            {
                objTexts[index].text = obj.objectiveName;
                index++;
                objTexts[index].text = obj.CurrentObjective();
                index++;
            }
            else
            {
                objTexts[index].text = obj.objectiveName;
                index++;
                objTexts[index].text = "Complete";
                index++;
            }
        }
    }


    public void Elimination()
    {
        foreach (Objective obj in objectives)
        {
            if (obj.objective == Objective.ObjectiveType.Elimination)
            {
                obj.elimsCurrent++;
            }
        }
    }
}
