using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using Unity.Loading;

public class DataPersistenceManager : MonoBehaviour
{

    [Header("File Storage Config")]
    [SerializeField] private string fileName;
    [SerializeField] private bool useEncryption;
    [SerializeField] private bool initializeDataIfNull = false;
    private GameData gameData;
    private List<IDataPersistence> dataPersistenceObjects;
    private FileDataHandler dataHandler;

    public static DataPersistenceManager instance { get; private set; }

    private void Awake() 
    {
        if (instance != null&&instance != this) 
        {
            Debug.LogError("Found more than one Data Persistence Manager in the scene.");// If there is already a game manager, destory this class, and warn in editor
            Destroy(this.gameObject);
            return;

        }
        Debug.Log($"DataPersistenceManager Awake in scene: {gameObject.scene.name}");
     
     
        instance = this;// If there is not game manager yet, let this class be it
        DontDestroyOnLoad(this.gameObject);
        this.dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, useEncryption);
        Debug.Log(fileName);

    }

    private void OnEnable() 
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable() 
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode) 
    {

        var player = FindObjectOfType<PlayerMovement>();
        Debug.Log($"Found player: {player != null}");
        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
        LoadGame();
    }

    public void OnSceneUnloaded(Scene scene)
    {
        SaveGame();
    }


     private void OnApplicationQuit() //when I quit application call save game function 
    {
        SaveGame();
    }




    public void NewGame() 
    {
        this.gameData = new GameData();// create game data for data to be saved into it 

        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.LoadData(this.gameData);
        }


    // Reload the current scene to apply the new state
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);


    }

    public void LoadGame()
    {

 



        this.gameData = dataHandler.Load(); // load any saved data from a file using the data handler
        
        
        if (this.gameData == null && initializeDataIfNull) 
        {
            Debug.Log("No data was found. Initializing data to defaults.");// if no data can be loaded, initialize to a new game
            NewGame();
        }


        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)// goes through each object in datapersistenceobjects 
        {
            dataPersistenceObj.LoadData(gameData);//updates gamedata variable by passing the data to other scripts 
        }
    }

    public void SaveGame()
    {

        Debug.Log($"Number of persistence objects: {dataPersistenceObjects?.Count ?? 0}");
        foreach (var obj in dataPersistenceObjects)
        {
            Debug.Log($"Persistence object: {obj.GetType()} on {(obj as MonoBehaviour)?.gameObject.name}");
        }


        
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects) // goes through each object in datapersistenceobjects 
        {
            dataPersistenceObj.SaveData(gameData);//updates gamedata variable by passing the data to other scripts
        }

        
        dataHandler.Save(gameData);// save that data to a file using the data handler
    }



    private List<IDataPersistence> FindAllDataPersistenceObjects() //function to find all objects on the interface 
    {

        // Debug.Log("heehee");
        // IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>()
        //     .OfType<IDataPersistence>(); //finds all objects that implement the interface
        var dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>(true).OfType<IDataPersistence>();
        List<IDataPersistence> objectsList = new List<IDataPersistence>(dataPersistenceObjects);
        Debug.Log($"Active Scene when finding objects: {SceneManager.GetActiveScene().name}"); 


        return new List<IDataPersistence>(dataPersistenceObjects);
    }

    public bool HasGameData() 
    {
        return gameData != null;
    }



}