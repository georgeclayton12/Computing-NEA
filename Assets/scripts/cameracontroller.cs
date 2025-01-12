using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CameraController : MonoBehaviour
{
    [Header("Camera Setup")]
    public Camera sceneCamera;
    private Camera playerCamera;
    private Vector3 originalPosition;
    private float originalSize;
    private bool isInBirdsEyeView = false;
    private Transform playerTransform;

    [Header("Birds Eye View Settings")]
    public float birdsEyeHeight = 10f;  // Lowered height
    public float transitionSpeed = 2f;
    public float birdsEyeOrthographicSize = 100f;
    public float followSpeed = 5f;
    public Vector3 offset = new Vector3(0, -20f, -10f);  // Added Y offset to move view down

    private CaveGeneration caveGeneration;  // Reference to cave generator

    [Header("UI Elements")]
    public Button viewToggleButton;

    private void Start()
    {
        if (sceneCamera == null)
        {
            Debug.LogError("[CameraController] No scene camera assigned!");
            return;
        }

        // Find cave generation component
        caveGeneration = FindObjectOfType<CaveGeneration>();
        if (caveGeneration != null)
        {
            float mapWidth = caveGeneration.width;
            float mapHeight = caveGeneration.height;
            birdsEyeOrthographicSize = Mathf.Max(mapWidth, mapHeight) * 0.75f;
            Debug.Log($"[CameraController] Adjusted orthographic size to {birdsEyeOrthographicSize} based on map size");
        }

        // Set up button and ensure it's not auto-selected
        if (viewToggleButton != null)
        {
            viewToggleButton.onClick.RemoveAllListeners();
            viewToggleButton.onClick.AddListener(ToggleView);
            // Remove default selection
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }

        // Initially disable scene camera
        sceneCamera.gameObject.SetActive(false);

        // Find player and camera
        FindPlayerAndCamera();

        originalPosition = sceneCamera.transform.position;
        originalSize = sceneCamera.orthographicSize;

    }

    private void FindPlayerAndCamera()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerCamera = player.GetComponentInChildren<Camera>();
        }
    }

    private void Update()
    {
        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }

        if (playerTransform == null || playerCamera == null)
        {
            FindPlayerAndCamera();
            return;
        }

        // Handle keyboard input
        if (Input.GetKeyDown(KeyCode.V))
        {
            ToggleView();
        }

        // If in bird's eye view, follow the player
        if (isInBirdsEyeView && playerTransform != null)
        {
            Vector3 targetPosition = new Vector3(
                playerTransform.position.x,
                birdsEyeHeight,
                offset.z  // Maintain the z-offset for 2D viewing
            );
            
            sceneCamera.transform.position = Vector3.Lerp(
                sceneCamera.transform.position,
                targetPosition,
                Time.deltaTime * followSpeed
            );
    }
    }

    public void ToggleView()
    {
        if (playerCamera == null || sceneCamera == null) return;

        isInBirdsEyeView = !isInBirdsEyeView;
        
        if (isInBirdsEyeView)
        {
            // Switch to birds-eye view
            // Calculate center of the cave instead of using zero position
            Vector3 targetPosition = Vector3.zero;
            if (caveGeneration != null)
            {
                // Calculate cave center based on its dimensions
                float caveMiddleX = -caveGeneration.width / 2f;
                float caveMiddleY = (-caveGeneration.height / 2f) - 30f; // Decreased offset to move view down
                
                targetPosition = new Vector3(
                    caveMiddleX,
                    caveMiddleY,
                    offset.z
                );
            }
            else
            {
                targetPosition = new Vector3(
                    playerTransform.position.x,
                    birdsEyeHeight,
                    offset.z
                );
            }
            
            playerCamera.gameObject.SetActive(false);
            sceneCamera.gameObject.SetActive(true);
            
            StartCoroutine(TransitionCamera(targetPosition, birdsEyeOrthographicSize));
        }
        else
        {
            StartCoroutine(TransitionToPlayerCamera());
        }
    }

    private IEnumerator TransitionCamera(Vector3 targetPosition, float targetSize)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = sceneCamera.transform.position;
        float startSize = sceneCamera.orthographicSize;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * transitionSpeed;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime);

            sceneCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            sceneCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, t);

            yield return null;
        }

        sceneCamera.transform.position = targetPosition;
        sceneCamera.orthographicSize = targetSize;
    }

    private IEnumerator TransitionToPlayerCamera()
    {
        // Get the position where the player camera should be
        Vector3 targetPosition = new Vector3(
            playerCamera.transform.position.x,
            playerCamera.transform.position.y,
            offset.z
        );
        
        float targetSize = playerCamera.orthographicSize;

        yield return StartCoroutine(TransitionCamera(targetPosition, targetSize));

        sceneCamera.gameObject.SetActive(false);
        playerCamera.gameObject.SetActive(true);
    }
}