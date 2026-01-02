using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class BlindPlayerController : MonoBehaviour
{
    [Header("Movimento")]
    public float speed = 5.0f;
    public float mouseSensitivity = 2.0f;
    public float gravity = -9.81f;

    [Header("Audio Procedurale")]
    public AudioClip baseStepSound; 
    public float stepInterval = 0.5f;

    private CharacterController controller;
    private AudioSource audioSource;
    private float verticalRotation = 0f;
    private float stepTimer = 0f;
    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        
        // 1. FIX CLIPPING DRASTICO (1cm)
        // Permette di avvicinarsi ai muri, ma causava il problema del "cubo enorme"
        if (Camera.main != null)
        {
            Camera.main.nearClipPlane = 0.01f;
            
            // NUOVO FIX: CULLING MASK
            // Nascondiamo il corpo del giocatore alla telecamera per evitare di vederci dentro
            SetupCameraCulling();
        }

        // 2. CUSCINETTO FISICO
        controller.radius = 0.35f; 
        controller.skinWidth = 0.05f; 
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Funzione che sposta il player su un Layer invisibile alla telecamera principale
    void SetupCameraCulling()
    {
        // Usiamo il layer "Ignore Raycast" (di solito Layer 2) che è built-in
        // Oppure potremmo crearne uno nuovo, ma questo è più sicuro senza setup manuale
        int layerToHide = 2; // "Ignore Raycast"
        
        // Assegna questo layer al Player e a tutti i figli (inclusa la grafica)
        SetLayerRecursively(this.gameObject, layerToHide);

        // Dice alla Main Camera di renderizzare TUTTO TRANNE questo layer
        // La maschera di bit funziona così: ~(1 << layer) inverte la selezione
        Camera.main.cullingMask = Camera.main.cullingMask & ~(1 << layerToHide);
        
        Debug.Log("🔧 Player body nascosto alla telecamera per evitare clipping.");
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        // Non cambiamo il layer della telecamera stessa, altrimenti non vede nulla!
        if (obj.GetComponent<Camera>() == null)
        {
            obj.layer = newLayer;
        }
        
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    void Update()
    {
        HandleLook();
        HandleMove();
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(0, mouseX, 0);
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        if(Camera.main) Camera.main.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

    void HandleMove()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        
        controller.Move(move * speed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (move.magnitude > 0.1f && controller.isGrounded)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0)
            {
                PlayProceduralStep();
                stepTimer = stepInterval;
            }
        }
    }

    void PlayProceduralStep()
    {
        if (baseStepSound == null) return;

        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.volume = Random.Range(0.4f, 0.6f);

        audioSource.PlayOneShot(baseStepSound);
    }
}