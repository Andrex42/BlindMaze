using UnityEngine;
using UnityEngine.SceneManagement;

// Questo script gestisce la vittoria quando il giocatore tocca l'uscita.
// Si assicura automaticamente che l'oggetto Goal abbia i componenti fisici necessari.

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))] // Il Rigidbody serve per garantire che OnTriggerEnter scatti sempre
public class WinHandler : MonoBehaviour
{
    private bool hasWon = false;
    private AudioSource audioSource;

    void Start()
    {
        // 1. Configurazione Fisica Automatica
        // Rendiamo il trigger bello grande (2x2x2 metri) così è impossibile mancarlo
        BoxCollider box = GetComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = new Vector3(2f, 2f, 2f); 

        // Impostiamo il Rigidbody su Kinematic (così l'oggetto sta fermo e non cade)
        // Unity richiede che almeno uno dei due oggetti che collidono abbia un Rigidbody per i Trigger
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; 
        rb.useGravity = false;

        // 2. Configurazione Audio
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0.0f; // 0.0 = Suono 2D (si sente forte nelle orecchie, non nello spazio)
        
        Debug.Log("🏁 WinHandler pronto! Corri verso questo oggetto per vincere.");
    }

    // Questo metodo viene chiamato da Unity quando qualcosa entra nel cubo invisibile del Goal
    void OnTriggerEnter(Collider other)
    {
        if (hasWon) return; // Se abbiamo già vinto, ignora

        // DEBUG: Scrive in console cosa ha toccato il goal
        Debug.Log("⚠️ Contatto con: " + other.name + " [Tag: " + other.tag + "]");

        // LOGICA DI CONTROLLO PERMISSIVA
        // Controlliamo se l'oggetto è il giocatore in tre modi diversi per essere sicuri:
        // 1. Ha il tag "Player"?
        // 2. Si chiama "Player" o "Capsule"?
        // 3. Ha un CharacterController (che usiamo per muoverci)?
        bool isPlayer = other.CompareTag("Player") || 
                        other.name.Contains("Player") || 
                        other.GetComponent<CharacterController>() != null;

        if (isPlayer)
        {
            DoWin();
        }
    }

    void DoWin()
    {
        hasWon = true;
        Debug.Log("🏆 VITTORIA! Riavvio tra 4 secondi...");

        // 1. Spegni il suono del sonar (fastidioso durante la vittoria)
        // Cerchiamo lo script ProceduralBeacon (o ProceduralAudioBeacon) e lo disabilitiamo
        MonoBehaviour beacon = GetComponent("ProceduralBeacon") as MonoBehaviour;
        if (beacon) beacon.enabled = false;
        
        // Se usavi il vecchio nome dello script:
        MonoBehaviour oldBeacon = GetComponent("ProceduralAudioBeacon") as MonoBehaviour;
        if (oldBeacon) oldBeacon.enabled = false;

        // 2. Suona la musica della vittoria
        audioSource.Stop(); // Ferma eventuali suoni precedenti
        audioSource.PlayOneShot(GenerateWinSound());

        // 3. Blocca il gioco e riavvia dopo 4 secondi
        // Nota: Non fermiamo Time.timeScale per permettere all'audio di finire
        Invoke("RestartGame", 4.0f);
    }

    // Genera un suono di "Arpa Magica" procedurale (Sintesi Additiva)
    AudioClip GenerateWinSound()
    {
        int sampleRate = 44100;
        int len = sampleRate * 3; // 3 secondi di suono
        float[] samples = new float[len];

        for (int i = 0; i < len; i++)
        {
            float t = (float)i / sampleRate;
            
            // Frequenza che sale (Sweep) per dare senso di trionfo
            float freq = 440f + (t * 880f); 
            
            // Somma di onde sinusoidali (Fondamentale + Armoniche) = Suono cristallino
            float val = Mathf.Sin(2 * Mathf.PI * freq * t);
            val += 0.5f * Mathf.Sin(2 * Mathf.PI * (freq * 1.5f) * t); // Quinta giusta
            val += 0.25f * Mathf.Sin(2 * Mathf.PI * (freq * 2.0f) * t); // Ottava
            
            // Envelope (Volume che scende dolce alla fine)
            float vol = 1.0f - (t / 3.0f);
            
            samples[i] = val * vol * 0.5f;
        }

        AudioClip clip = AudioClip.Create("WinSfx", len, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    void RestartGame()
    {
        // Ricarica la scena corrente
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Disegna la scritta a schermo (vecchio sistema GUI, infallibile per debug)
    void OnGUI()
    {
        if (hasWon)
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 50;
            style.normal.textColor = Color.yellow;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontStyle = FontStyle.Bold;

            // Disegna un'ombra nera per leggere meglio la scritta
            GUIStyle shadow = new GUIStyle(style);
            shadow.normal.textColor = Color.black;
            
            float w = Screen.width;
            float h = Screen.height;

            // Scritta ombra
            GUI.Label(new Rect(2, 2, w, h), "USCITA TROVATA!", shadow);
            // Scritta vera
            GUI.Label(new Rect(0, 0, w, h), "USCITA TROVATA!", style);
        }
    }
}