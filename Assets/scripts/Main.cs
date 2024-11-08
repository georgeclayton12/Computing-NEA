using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        gameObject.AddComponent<GameManager>();
        SceneManager.LoadScene("Menu ui", LoadSceneMode.Single);
    }

}
