using UnityEngine;
using System;

[Serializable]
public class SecretsData
{
    public string mqttUser;
    public string mqttPassword;
    public int mqttPort;
}

public class SecretsLoader : MonoBehaviour
{
    public static SecretsData Data;

    void Awake()
    {
        Debug.Log("SecretsLoader → Awake() called");

        if (Data != null)
        {
            Debug.Log("Secrets already loaded");
            return;
        }

        // Charge le fichier "secrets" (sans l'extension .json) depuis le dossier Resources
        // Cela fonctionne sur Android contrairement à File.ReadAllText
        TextAsset secretFile = Resources.Load<TextAsset>("secrets");

        if (secretFile == null)
        {
            Debug.LogError("Fichier secrets.json INTROUVABLE dans Assets/Resources/");
            return;
        }

        try
        {
            Data = JsonUtility.FromJson<SecretsData>(secretFile.text);
            Debug.Log("Secrets chargés avec succès. User: " + Data.mqttUser);
        }
        catch (Exception e)
        {
            Debug.LogError("Erreur lors du parsing de secrets.json : " + e.Message);
        }
    }
}
