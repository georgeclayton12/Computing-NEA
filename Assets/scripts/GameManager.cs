using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using Unity.VisualScripting;
using Unity.PlasticSCM.Editor.WebApi;

//https://www.youtube.com/watch?v=aUi9aijvpgs about save/load 
public class GameManager : MonoBehaviour
{
    // Declaring that we only want a single insrtance of this class
    // This is using the singleton pattern

    public static GameManager Instance;

    private void Awake()
    {
        // If there is not game manager yet, let this class be it
        if (GameManager.Instance == null)
        {
            GameManager.Instance = this;
        }
        // If there is already a game manager, destory this class, and warn in editor
        else if (GameManager.Instance != this)
        {
            Destroy(this);
            Debug.LogError("There where multiple game managers in the scene");
        }
    }

    public void NewGame()
    {
        SceneManager.UnloadSceneAsync("Menu ui");
        AsyncOperation Operation =SceneManager.LoadSceneAsync("Gameplay", LoadSceneMode.Single);  
        


    }   

    public void SpawnPlayer(Vector3 pos)
    {
        GameObject player = Resources.Load<GameObject>("Player");
        Instantiate(player, pos, Quaternion.identity);
    }




    // public string CreateSeed(
    // {
    //     throw new NotImplementedException();
    // }

    // public Texture2D CreateMapData(string seed)
    // {
    //     throw new NotImplementedException();
    // }

    // public void PopulateMap(Texture2D mapData)
    // {

    // }

}