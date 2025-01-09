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
    private DataPersistenceManager dataPersistenceManager;

    private void Awake()
    {
        if (GameManager.Instance == null)
        {
            GameManager.Instance = this;
            dataPersistenceManager = FindObjectOfType<DataPersistenceManager>();
            DontDestroyOnLoad(this.gameObject); // Add this line
        }
        else if (GameManager.Instance != this)
        {
            Destroy(gameObject);
            Debug.LogError("Duplicate GameManager destroyed");
        }
    }
    public void NewGame()
    {
        SceneManager.UnloadSceneAsync("Menu ui");
        AsyncOperation Operation =SceneManager.LoadSceneAsync("Gameplay", LoadSceneMode.Single);  
        


    }   

    public void SpawnPlayer(Vector3 pos)
    {
        if (this == null) return; // Safety check

        
        
        GameObject player = Resources.Load<GameObject>("Player");
        GameObject instantiatedPlayer = Instantiate(player, pos, Quaternion.identity);
        StartCoroutine(RefreshPersistenceAfterSpawn());
    }

    private IEnumerator RefreshPersistenceAfterSpawn()
    {
        yield return new WaitForEndOfFrame();
        if (dataPersistenceManager != null)
        {
            dataPersistenceManager.OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }
    }
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

