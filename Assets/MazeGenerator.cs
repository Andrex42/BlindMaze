using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [Header("Impostazioni Labirinto")]
    // Numeri dispari consigliati per questo algoritmo
    public int width = 21; 
    public int depth = 21; 
    
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    
    [Header("Soffitto")]
    public GameObject ceilingPrefab; // Trascina qui lo stesso prefab del pavimento (Floor)
    public bool addCeiling = true;   // Checkbox per attivare/disattivare il soffitto

    public GameObject goalPrefab;

    [Header("Riferimenti Giocatore")]
    public Transform player;

    private int[,] map;

    void Start()
    {
        GenerateMaze();
        // Aggiunge riverbero per atmosfera
        GameObject reverbObj = new GameObject("GlobalReverb");
        AudioReverbZone reverb = reverbObj.AddComponent<AudioReverbZone>();
        reverb.reverbPreset = AudioReverbPreset.StoneCorridor;
        reverb.minDistance = 0;
        reverb.maxDistance = 1000; 
    }

    void GenerateMaze()
    {
        map = new int[width, depth];

        // 1. Inizializza tutto a Muro
        for (int x = 0; x < width; x++)
        for (int z = 0; z < depth; z++)
            map[x, z] = 1;

        // 2. Scava (Recursive Backtracker)
        Carve(1, 1);

        // 3. Crea oggetti 3D (Pavimento, Muri, Soffitto)
        BuildLevel();

        // 4. Posiziona Player in sicurezza
        if (player != null)
        {
            // FIX ABISSO: Disabilitiamo momentaneamente il CharacterController
            // Questo permette di teletrasportare il giocatore senza conflitti fisici
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            // Alziamo leggermente la Y a 2.0f per sicurezza (meglio cadere per 0.5s che attraversare il pavimento)
            player.position = new Vector3(1, 2.0f, 1); 
            
            // Ruotiamo il giocatore per guardare verso l'interno del labirinto (opzionale ma carino)
            player.rotation = Quaternion.identity;

            // Riabilitiamo il controller
            if (cc != null) cc.enabled = true;
        }
        
        // 5. Posiziona Goal
        PlaceGoal();
    }

    void PlaceGoal()
    {
        // Cerca il punto vuoto più lontano dall'origine (angolo in alto a destra)
        for (int x = width - 2; x > 0; x--)
        {
            for (int z = depth - 2; z > 0; z--)
            {
                if (map[x, z] == 0) // Se è un corridoio vuoto
                {
                    Instantiate(goalPrefab, new Vector3(x, 1.5f, z), Quaternion.identity);
                    return; // Fatto, esci
                }
            }
        }
    }

    void Carve(int x, int z)
    {
        map[x, z] = 0;
        
        // Ordine casuale direzioni
        List<Vector2Int> dirs = new List<Vector2Int> {
            new Vector2Int(0, 2), new Vector2Int(0, -2), 
            new Vector2Int(2, 0), new Vector2Int(-2, 0) 
        };
        
        // Shuffle (Fisher-Yates)
        for (int i = 0; i < dirs.Count; i++) {
            Vector2Int temp = dirs[i];
            int rnd = Random.Range(i, dirs.Count);
            dirs[i] = dirs[rnd];
            dirs[rnd] = temp;
        }

        foreach (var dir in dirs)
        {
            int nx = x + dir.x;
            int nz = z + dir.y;

            if (nx > 0 && nx < width - 1 && nz > 0 && nz < depth - 1 && map[nx, nz] == 1)
            {
                map[x + dir.x / 2, z + dir.y / 2] = 0; // Abbatti muro intermedio
                Carve(nx, nz);
            }
        }
    }

    void BuildLevel()
    {
        // 1. Pavimento unico scalato
        if (floorPrefab != null)
        {
            GameObject floor = Instantiate(floorPrefab, new Vector3(width / 2f, 0, depth / 2f), Quaternion.identity);
            floor.transform.localScale = new Vector3(width, 1, depth);
            floor.name = "Floor";
        }

        // 2. Soffitto unico scalato (NUOVO)
        if (addCeiling && ceilingPrefab != null)
        {
            // Posizionato a Y=3 (sopra i muri alti 3 metri)
            GameObject ceil = Instantiate(ceilingPrefab, new Vector3(width / 2f, 3, depth / 2f), Quaternion.identity);
            ceil.transform.localScale = new Vector3(width, 1, depth);
            // Ruotato di 180 gradi su X per guardare in giù (così si vede la texture dal basso)
            ceil.transform.Rotate(180, 0, 0); 
            ceil.name = "Ceiling";
        }

        // 3. Generazione Muri
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                if (map[x, z] == 1)
                {
                    Vector3 pos = new Vector3(x, 1.5f, z);
                    GameObject wall = Instantiate(wallPrefab, pos, Quaternion.identity);
                    // Muri alti 3 metri per toccare il soffitto
                    wall.transform.localScale = new Vector3(1, 3, 1);
                }
            }
        }
    }
}
