using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance => instance;

    [SerializeField] private DigimonIndex digimonIndex;
    [SerializeField] private DigivolutionIndex digivolutionIndex;

    private DigimonRuntime testDigimon;

    public DigimonIndex DigimonIndex => digimonIndex;
    public DigivolutionIndex DigivolutionIndex => digivolutionIndex;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else 
            gameObject.SetActive(false);

        digimonIndex.Init();
        digivolutionIndex.Init();
    }

    private void Start()
    {
        Debug.Log("start");
        testDigimon = DigimonRuntime.Convert("Bibimon");
        Debug.Log(testDigimon);

        testDigimon.Digivolution();
        Debug.Log(testDigimon);

        testDigimon.Digivolution();
        Debug.Log(testDigimon);

        testDigimon.Digivolution();
        Debug.Log(testDigimon);

        testDigimon.Digivolution();
        Debug.Log(testDigimon);

        testDigimon.Digivolution();
        Debug.Log(testDigimon);
    }
}