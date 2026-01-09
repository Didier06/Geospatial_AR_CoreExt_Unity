using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

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
        public bool delete = false;
    }

    [System.Serializable]
    public class GeoPrefabListWrapper
    {
        public List<GeoPrefab2> items;
    }

    public class Listeprefabs2 : MonoBehaviour
    {
        private Dictionary<string, ARGeospatialAnchor> activeAnchors =
            new Dictionary<string, ARGeospatialAnchor>();

        private Dictionary<string, GameObject> activeObjects =
            new Dictionary<string, GameObject>();

        public AREarthManager EarthManager;
        public VpsInitializer Initializer;
        public Text OutputText;
        public ARAnchorManager AnchorManager;

        public double HeadingThreshold = 25.0;
        public double HorizontalThreshold = 20.0;

        public List<GeoPrefab2> ItemsToPlace = new List<GeoPrefab2>();

        [Header("Library of Loadable Prefabs")]
        public List<GameObject> PrefabLibrary = new List<GameObject>();

        private List<GameObject> spawnedInstances = new List<GameObject>();

        // Placement AR actif oui/non
        private bool placed = false;

        // 👉 NOUVEAU : indique qu’un JSON a été reçu
        private bool hasPendingJson = false;


        float GetHeadingFromPose(GeospatialPose pose)
        {
            Quaternion r = pose.EunRotation;

            float yaw = Mathf.Atan2(
                2f * (r.w * r.y + r.x * r.z),
                1f - 2f * (r.y * r.y + r.z * r.z)
            ) * Mathf.Rad2Deg;

            if (yaw < 0) yaw += 360f;

            return yaw;
        }


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

            // 👉 si JSON est arrivé tôt → relancer un placement quand prêt
            if (hasPendingJson && placed)
                placed = false;

            // 👉 placement AR réel
            if (!placed)
            {
                placed = true;
                hasPendingJson = false;   // JSON consommé
                PlaceAllPrefabs(pose);
            }
        }


        void PlaceAllPrefabs(GeospatialPose camPose)
        {
            HashSet<string> stillPresent = new HashSet<string>();

            foreach (var item in ItemsToPlace)
            {
                if (item.delete)
                {
                    if (activeAnchors.ContainsKey(item.name))
                    {
                        Destroy(activeAnchors[item.name].gameObject);
                        activeAnchors.Remove(item.name);
                    }

                    if (activeObjects.ContainsKey(item.name))
                    {
                        Destroy(activeObjects[item.name]);
                        activeObjects.Remove(item.name);
                    }

                    continue;
                }

                stillPresent.Add(item.name);

                float heading = GetHeadingFromPose(camPose);
                Quaternion rot = Quaternion.AngleAxis(180f - heading, Vector3.up);
                double altitude = camPose.Altitude + item.altitudeOffset;


                if (activeAnchors.ContainsKey(item.name))
                {
                    var oldAnchor = activeAnchors[item.name];
                    if (oldAnchor != null)
                        Destroy(oldAnchor.gameObject);

                    var newAnchor = AnchorManager.AddAnchor(
                        item.latitude,
                        item.longitude,
                        altitude,
                        rot
                    );

                    activeAnchors[item.name] = newAnchor;

                    if (activeObjects.ContainsKey(item.name))
                    {
                        var follower = activeObjects[item.name].GetComponent<GeoTargetFollower>();
                        if (follower != null)
                            follower.anchor = newAnchor != null ? newAnchor.transform : null;
                    }

                    continue;
                }


                var anchor = AnchorManager.AddAnchor(
                    item.latitude,
                    item.longitude,
                    altitude,
                    rot
                );

                if (anchor == null || item.prefab == null)
                    continue;

                GameObject go = Instantiate(item.prefab);

                var f = go.AddComponent<GeoTargetFollower>();
                f.anchor = anchor.transform;
                f.smoothTime = 0.35f;
                f.rotSmooth = 0.15f;

                activeAnchors[item.name] = anchor;
                activeObjects[item.name] = go;
            }
        }



        public void UpdatePrefabsFromJSON(string json)
        {
            try
            {
                if (PrefabLibrary == null)
                {
                    PrefabLibrary = new List<GameObject>();
                }

                GeoPrefabListWrapper wrapper = JsonUtility.FromJson<GeoPrefabListWrapper>(json);
                if (wrapper != null && wrapper.items != null)
                {
                    ItemsToPlace.Clear();

                    foreach (var data in wrapper.items)
                    {
                        string searchName = data.name.Trim();

                        GameObject prefabToUse =
                            PrefabLibrary.FirstOrDefault(p => p != null && p.name == searchName);

                        if (prefabToUse != null)
                        {
                            ItemsToPlace.Add(new GeoPrefab2
                            {
                                name = data.name,
                                prefab = prefabToUse,
                                latitude = data.latitude,
                                longitude = data.longitude,
                                altitudeOffset = data.altitudeOffset
                            });
                        }
                    }

                    // 👉 TRIGGER PLACEMENT
                    hasPendingJson = true;
                    placed = false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }



        public string GetJsonFromCurrentList()
        {
            GeoPrefabListWrapper wrapper = new GeoPrefabListWrapper();
            wrapper.items = new List<GeoPrefab2>();

            foreach (var item in ItemsToPlace)
            {
                wrapper.items.Add(new GeoPrefab2
                {
                    name = item.prefab != null ? item.prefab.name : item.name,
                    latitude = item.latitude,
                    longitude = item.longitude,
                    altitudeOffset = item.altitudeOffset
                });
            }

            return JsonUtility.ToJson(wrapper);
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

