using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimulateButton : MonoBehaviour
{
    void Start()
    {
        GameManager gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        if (gm != null)
            gm.simulationStateChanged += gameManager_simulationStateChanged;
    }

    void gameManager_simulationStateChanged(bool value)
    {
        string text;
        if (value)
            text = "Stop Simulation";
        else
            text = "Start Simulation";
        GetComponentInChildren<Text>().text = text;
    }

}
