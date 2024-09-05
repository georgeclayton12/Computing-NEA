using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DataPersistenceManager : MonoBehaviour
{
    [Header("File Storage Config")]
    [SerializeField] private string fileName;
    [SerializeField] private bool useEncryption;

    private GameData gameData;
    private List<IDataPersistence> dataPersistenceObjects;
    private FileDataHandler dataHandler;

    public static DataPersistenceManager instance { get; private set; }

    private void Awake() 
    {
        if (instance != null) 
        {
            Debug.LogError("Found more than one Data Persistence Manager in the scene.");// If there is already a game manager, destory this class, and warn in editor
            
        }
        instance = this;// If there is not game manager yet, let this class be it
    }

    private void Start() 
    {
        this.dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, useEncryption);//summon FileDataHandler 
        this.dataPersistenceObjects = FindAllDataPersistenceObjects();//call function 
        LoadGame();
    }

    public void NewGame() 
    {
        this.gameData = new GameData();// create game data for data to be saved into it 
    }

    public void LoadGame()
    {
        
        this.gameData = dataHandler.Load(); // load any saved data from a file using the data handler
        
        
        if (this.gameData == null) 
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
        
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects) // goes through each object in datapersistenceobjects 
        {
            dataPersistenceObj.SaveData(gameData);//updates gamedata variable by passing the data to other scripts
        }

        
        dataHandler.Save(gameData);// save that data to a file using the data handler
    }

    private void OnApplicationQuit() //when I quit application call save game function 
    {
        SaveGame();
    }

    private List<IDataPersistence> FindAllDataPersistenceObjects() //function to find all objects on the interface 
    {
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>()
            .OfType<IDataPersistence>(); //finds all objects that implement the interface 

        return new List<IDataPersistence>(dataPersistenceObjects);
    }
}