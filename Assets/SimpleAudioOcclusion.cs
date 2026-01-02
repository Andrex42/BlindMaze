using UnityEngine;

// Simula il suono che passa attraverso i muri.
// Se c'è un ostacolo, applica un filtro LowPass (suono ovattato).

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(AudioLowPassFilter))] // Ensure the filter is present
public class SimpleAudioOcclusion : MonoBehaviour
{
    public Transform playerListener; // Trascina qui il Player
    public LayerMask obstacleLayer;  // Seleziona "Default" o il layer dei muri
    
    [Header("Impostazioni Filtro")]
    public float cutoffOpen = 22000f; // Suono chiaro (nessun muro)
    public float cutoffBlocked = 600f; // Suono ovattato (dietro muro)
    public float smoothSpeed = 10f;

    private AudioLowPassFilter lowPass;
    private float currentCutoff;

    void Start()
    {
        lowPass = GetComponent<AudioLowPassFilter>();
        
        // Null check for safety
        if (lowPass == null)
        {
            Debug.LogWarning("AudioLowPassFilter missing! Adding one dynamically.");
            lowPass = gameObject.AddComponent<AudioLowPassFilter>();
        }

        currentCutoff = cutoffOpen;
        
        // Se non hai assegnato il player, prova a trovarlo
        if (playerListener == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerListener = p.transform;
        }
    }

    void FixedUpdate()
    {
        if (playerListener == null) return;

        // Lancia un raggio verso il giocatore
        RaycastHit hit;
        Vector3 dir = playerListener.position - transform.position;
        float dist = dir.magnitude;

        bool hitWall = Physics.Raycast(transform.position, dir, out hit, dist, obstacleLayer);

        // Se colpiamo qualcosa che NON è il player (es. un muro), occludiamo
        // Nota: Assicurati che il Player abbia un collider o sia su un layer diverso dai muri
        if (hitWall && hit.transform != playerListener)
        {
            // Muro in mezzo -> Taglia frequenze alte
            currentCutoff = Mathf.Lerp(currentCutoff, cutoffBlocked, Time.deltaTime * smoothSpeed);
        }
        else
        {
            // Linea libera -> Suono aperto
            currentCutoff = Mathf.Lerp(currentCutoff, cutoffOpen, Time.deltaTime * smoothSpeed);
        }

        if (lowPass != null)
            lowPass.cutoffFrequency = currentCutoff;
    }
}