using UnityEngine;

public class CameraController : MonoBehaviour
{
   private Camera mainCamera;
   private bool birdseye = false;
   private Vector3 playerPosition;
   private float playerCameraSize = 5f;

   void Start()
   {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("No camera found");
        }
   }

   void LateUpdate()
   {
        if (!birdseye)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                mainCamera.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, -10);
            }
        }
   }

   public void ToggleBirdsEye()
   {
        Debug.Log("pressed");
        birdseye = !birdseye;
        if (birdseye)
        {
            mainCamera.transform.position = new Vector3(200, 200, -10);
            mainCamera.orthographicSize = 100;
        }
        else
        {
            mainCamera.orthographicSize = playerCameraSize;
        }
   }
}