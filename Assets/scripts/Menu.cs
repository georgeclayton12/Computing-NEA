using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Menu : MonoBehaviour
{
    //variable set to data about menu
    // uidocument is the variable type
    // dom is variable name
    // public so we can assign a uidocument to it in editor 
    public UIDocument dom;

    private void Start()
    {
        //searching the root variable for an ID and storing it in a variable 
        // Example: root.Q<Button>("new-gamebutton") => search root for the id of `new-gamebutton` and make that = (store it in) newGamebtn
        VisualElement root = dom.rootVisualElement;
        Button newGamebtn = root.Q<Button>("new-gamebutton");
        Button continuebtn = root.Q<Button>("continuebutton");
        Button settingsbtn = root.Q<Button>("settingsbutton");

        //when a button is clicked the function is called 
        newGamebtn.clicked += NewGame;
        continuebtn.clicked += Continue;
        settingsbtn.clicked += Settings;

    }

    private void Continue()
    {
        Debug.Log("continue");
        GameManager.Instance.NewGame();
    }

    private void NewGame()
    {
        GameManager.Instance.NewGame();

    }
    private void Settings()
    {
        Debug.Log("settings");
    }

}
