using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq; // Added for prefab search

using Google.XR.ARCoreExtensions;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;


namespace AR_GPS
{
    [System.Serializable]
    public class GeoPrefab2
    {
        public string name;
        public GameObject prefab;
        public double latitude;
        public double longitude;
        public double altitudeOffset = -1.5f;
    }

    [System.Serializable]
    public class GeoPrefabListWrapper
    {
        public List<GeoPrefabData> items;
    }

    [System.Serializable]
    public class GeoPrefabData
    {
        public string name;
        public double latitude;
        public double longitude;
        public double altitudeOffset;
    }

    public class Listeprefabs2 : MonoBehaviour
    {
        public AREarthManager EarthManager;
        public VpsInitializer Initializer;
        public Text OutputText;
        public ARAnchorManager AnchorManager;

        public double HeadingThreshold = 25.0;
        public double HorizontalThreshold = 20.0;

        // 👉 Liste des objets à placer
        public List<GeoPrefab2> ItemsToPlace = new List<GeoPrefab2>();

        // 👉 Référentiel des prefabs disponibles pour le chargement dynamique
        [Header("Library of Loadable Prefabs")]
        public List<GameObject> PrefabLibrary = new List<GameObject>();

        // 👉 Liste des instances créées
        private List<GameObject> spawnedInstances = new List<GameObject>();

        // 👉 Pour éviter de créer plusieurs fois
        private bool placed = false;


        void Update()
        {
            if (!Initializer.IsReady ||
                EarthManager.EarthTrackingState != TrackingState.Tracking)
                return;

            GeospatialPose pose = EarthManager.CameraGeospatialPose;

            if (pose.OrientationYawAccuracy > HeadingThreshold ||
                pose.HorizontalAccuracy > HorizontalThreshold)
            {
                ShowTracking("Low accuracy. Move outside.", pose);
                return;
            }

            ShowTracking("Good accuracy.", pose);

            if (!placed)
            {
                placed = true;
                PlaceAllPrefabs(pose);
            }
        }


        void PlaceAllPrefabs(GeospatialPose camPose)
        {
            foreach (var item in ItemsToPlace)
            {
                Quaternion rot = Quaternion.AngleAxis(
                    180f - (float)camPose.Heading, Vector3.up);

                double altitude = camPose.Altitude + item.altitudeOffset;

                ARGeospatialAnchor anchor = AnchorManager.AddAnchor(
                    item.latitude,
                    item.longitude,
                    altitude,
                    rot
                );

                if (anchor != null && item.prefab != null)
                {
                    GameObject go = Instantiate(item.prefab, anchor.transform);
                    spawnedInstances.Add(go);
                }
            }
        }

        public void UpdatePrefabsFromJSON(string json)
        {
            try
            {
                if (PrefabLibrary == null)
                {
                    Debug.LogWarning("[Listeprefabs2] PrefabLibrary was null! Initializing empty list.");
                    PrefabLibrary = new List<GameObject>();
                }

                Debug.Log($"[Listeprefabs2] Raw JSON received ({json.Length} chars): {json}");

                GeoPrefabListWrapper wrapper = JsonUtility.FromJson<GeoPrefabListWrapper>(json);
                if (wrapper != null && wrapper.items != null)
                {
                    ClearCurrentInstances();
                    ItemsToPlace.Clear();

                    Debug.Log($"[Listeprefabs2] Parsing success. Items count: {wrapper.items.Count}");
                    // Debug.Log($"[Listeprefabs2] PrefabLibrary contains {PrefabLibrary.Count} prefabs..."); 

                    foreach (var data in wrapper.items)
                    {
                        string searchName = data.name.Trim();
                        // Safe check for nulls in list
                        GameObject prefabToUse = PrefabLibrary.FirstOrDefault(p => p != null && p.name == searchName);
                        
                        if (prefabToUse != null)
                        {
                            Debug.Log($"[Listeprefabs2] MATCH FOUND: '{searchName}'");
                            GeoPrefab2 newGeo = new GeoPrefab2
                            {
                                name = data.name,
                                prefab = prefabToUse,
                                latitude = data.latitude,
                                longitude = data.longitude,
                                altitudeOffset = data.altitudeOffset
                            };
                            ItemsToPlace.Add(newGeo);
                        }
                        else
                        {
                            Debug.LogWarning($"[Listeprefabs2] NO MATCH for '{searchName}'.");
                        }
                    }

                    placed = false; // Trigger re-placement in Update
                }
                else
                {
                     Debug.LogWarning("[Listeprefabs2] JSON Parsed but wrapper or items were null.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[Listeprefabs2] CRITICAL ERROR: {e.GetType()} - {e.Message}\nStack: {e.StackTrace}");
            }
        }

        public string GetJsonFromCurrentList()
        {
            GeoPrefabListWrapper wrapper = new GeoPrefabListWrapper();
            wrapper.items = new List<GeoPrefabData>();

            foreach (var item in ItemsToPlace)
            {
                wrapper.items.Add(new GeoPrefabData
                {
                    name = item.prefab != null ? item.prefab.name : item.name,
                    latitude = item.latitude,
                    longitude = item.longitude,
                    altitudeOffset = item.altitudeOffset
                });
            }

            return JsonUtility.ToJson(wrapper);
        }

        private void ClearCurrentInstances()
        {
            foreach (var go in spawnedInstances)
            {
                if (go != null)
                {
                    // Destroy the anchor (parent) to clean up AR tracking
                    if (go.transform.parent != null)
                    {
                        Destroy(go.transform.parent.gameObject);
                    }
                    else
                    {
                        Destroy(go);
                    }
                }
            }
            spawnedInstances.Clear();
        }


        void ShowTracking(string status, GeospatialPose pose)
        {
            if (OutputText == null) return;

            Quaternion r = pose.EunRotation;

            float yaw = Mathf.Atan2(
                2f * (r.w * r.y + r.x * r.z),
                1f - 2f * (r.y * r.y + r.z * r.z)
            ) * Mathf.Rad2Deg;

            if (yaw < 0) yaw += 360f;

            OutputText.text =
                $"Latitude/Longitude: {pose.Latitude:F6}, {pose.Longitude:F6}\n" +
                $"Horizontal Accuracy: {pose.HorizontalAccuracy:F2} m\n" +
                $"Altitude: {pose.Altitude:F2} m\n" +
                $"Heading: {yaw:F1}°\n" +
                $"{status}";
        }
    }
}
