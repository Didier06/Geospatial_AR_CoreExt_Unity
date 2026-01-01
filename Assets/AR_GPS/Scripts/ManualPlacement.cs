using UnityEngine;

public class ManualPlacement : MonoBehaviour
{
    [Tooltip("Le prefab de votre objet ou ville à afficher")]
    public GameObject ContentPrefab;

    [Tooltip("Distance d'apparition en mètres devant le joueur")]
    public float distance = 1.0f;

    private GameObject spawnedObject;
    private Transform cameraTransform;

    void Start()
    {
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
            SpawnObject();
        }
    }

    void Update()
    {
        // Repositionner l'objet si on appuie sur la Gâchette (Quest) ou Touche Souris (PC/Editor)
        // Note: Sur Quest avec OpenXR, Input.GetMouseButton(0) mappe souvent sur la gachette en mode legacy,
        // mais pour être plus robuste, on surveille aussi la barre d'espace ou le tap écran.
        if ((Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)) && spawnedObject != null)
        {
            PositionObjectInFront();
        }
    }

    void SpawnObject()
    {
        if (ContentPrefab == null) return;

        // On instancie l'objet s'il n'existe pas
        if (spawnedObject == null)
        {
            spawnedObject = Instantiate(ContentPrefab);
        }
        
        PositionObjectInFront();
    }

    void PositionObjectInFront()
    {
        if (cameraTransform == null) return;

        // On récupère la direction du regard, mais on reste à plat
        Vector3 forwardFlat = cameraTransform.forward;
        forwardFlat.y = 0; 
        forwardFlat.Normalize();

        Vector3 targetPosition = cameraTransform.position + (forwardFlat * distance);
        
        // On place l'objet plus bas (ex: 50cm sous les yeux)
        targetPosition.y = cameraTransform.position.y - 0.5f; 

        spawnedObject.transform.position = targetPosition;
        
        // L'objet regarde l'utilisateur
        spawnedObject.transform.LookAt(new Vector3(cameraTransform.position.x, spawnedObject.transform.position.y, cameraTransform.position.z));
    }
}
