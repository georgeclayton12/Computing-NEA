using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class FileDataHandler
{
    private string dataDirPath = ""; // the directory that the file will be saved in 
    private string dataFileName = "";//file name 
    private bool useEncryption = false;
    private readonly string encryptionCodeWord = "George";//encryption word 

    public FileDataHandler(string dataDirPath, string dataFileName, bool useEncryption) 
    {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
        this.useEncryption = useEncryption;
    }

    public GameData Load() 
    {
        
        string fullPath = Path.Combine(dataDirPath, dataFileName);//concatenates the directory and the file name to create the file path 
        GameData loadedData = null;
        if (File.Exists(fullPath)) 
        {
            try 
            {
                // load the serialized data from the file
                string dataToLoad = "";
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))//using using so I dont have to close file and opening the correct file 
                {
                    using (StreamReader reader = new StreamReader(stream))//getting the data 
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                // optionally decrypt the data
                if (useEncryption) // if useEncryption is True 
                {
                    dataToLoad = EncryptDecrypt(dataToLoad);//run decrypt function 
                }

                loadedData = JsonUtility.FromJson<GameData>(dataToLoad);// deserialize the data from Json back into the C# object
            }
            catch (Exception e) 
            {
                Debug.LogError("Error occured when trying to load data from file: " + fullPath + "\n" + e);//error messsage 
            }
        }
        return loadedData;
    }

    public void Save(GameData data) 
    {
        
        string fullPath = Path.Combine(dataDirPath, dataFileName); //concatenates the directory and the file name to create the file path works across all oss
        try 
        {
            
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));// create the directory the file will be written to if it doesn't already exist

            
            string dataToStore = JsonUtility.ToJson(data, true);// serialize the C# game data object into Json

        
            if (useEncryption) // if use encryption is set to true 
            {
                dataToStore = EncryptDecrypt(dataToStore);//call encrypt function 
            }

            
            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream)) //store the data to write in writer variable 
                {
                    writer.Write(dataToStore);//write in data 
                }
            }
        }
        catch (Exception e) 
        {
            Debug.LogError("Error occured when trying to save data to file: " + fullPath + "\n" + e);
        }
    }

    private string EncryptDecrypt(string data) 
    {
        string modifiedData = ""; //stores the encrypted data 
        for (int i = 0; i < data.Length; i++) 
        {
            modifiedData += (char) (data[i] ^ encryptionCodeWord[i % encryptionCodeWord.Length]);//encrypt data 
        }
        return modifiedData;
    }
}