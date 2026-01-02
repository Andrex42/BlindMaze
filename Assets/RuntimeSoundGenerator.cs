using UnityEngine;

// QUESTO SCRIPT SOSTITUISCE I FILE .WAV
// Genera un AudioClip "Passo" matematicamente all'avvio del gioco.
// Soddisfa il requisito di "Procedural Sound Generation".

public class RuntimeSoundGenerator : MonoBehaviour
{
    [Header("Collegamenti")]
    // Trascina qui il tuo Player che ha lo script BlindPlayerController
    public BlindPlayerController playerController;

    void Start()
    {
        // 1. Generiamo il clip audio via codice
        AudioClip proceduralStep = GenerateFootstepClip();
        
        // 2. Lo assegniamo allo script del giocatore
        if (playerController != null)
        {
            playerController.baseStepSound = proceduralStep;
            Debug.Log("✅ Audio Passo Generato Proceduralmente e Assegnato!");
        }
    }

    // Algoritmo di sintesi: Rumore Bianco con inviluppo ADSR semplificato e filtro LowPass
    AudioClip GenerateFootstepClip()
    {
        int sampleRate = 44100;
        float duration = 0.2f; // Durata 200ms
        int sampleCount = (int)(sampleRate * duration);
        
        float[] data = new float[sampleCount];
        float lastVal = 0f; // Per il filtro passa-basso
        
        // MODIFICA: Volume Master drasticamente ridotto (da 1.0 a 0.15)
        // Questo evita che il passo sembri un'esplosione
        float masterVolume = 0.15f; 

        for (int i = 0; i < sampleCount; i++)
        {
            // A. Genera rumore casuale (White Noise)
            float noise = Random.Range(-1f, 1f);

            // B. Filtro Passa-Basso (Simple Low Pass) Aggiornato
            // Prima era: (lastVal + (noise * 0.5f)) / 1.5f;
            // Ora diamo più peso al valore precedente (lastVal) per tagliare più alti
            // Risultato: Suono più "cupo" e morbido, meno "statico radio"
            noise = (lastVal + (noise * 0.2f)) / 1.2f;
            lastVal = noise;

            // C. Inviluppo (Envelope)
            // Creiamo un attacco veloce e un rilascio morbido
            float t = (float)i / sampleCount;
            float envelope = 0f;

            if (t < 0.1f) // Attack (primi 10%)
            {
                envelope = t / 0.1f;
            }
            else // Decay/Release (restanti 90%)
            {
                envelope = 1f - ((t - 0.1f) / 0.9f);
                // Applichiamo una curva esponenziale al rilascio per renderlo più naturale
                envelope = Mathf.Pow(envelope, 2); 
            }

            // D. Applica l'inviluppo e il VOLUME RIDOTTO
            data[i] = noise * envelope * masterVolume;
        }

        // Crea l'oggetto AudioClip di Unity
        AudioClip clip = AudioClip.Create("ProceduralStep", sampleCount, 1, sampleRate, false);
        clip.SetData(data, 0);
        
        return clip;
    }
}