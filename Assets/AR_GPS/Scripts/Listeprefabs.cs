using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Google.XR.ARCoreExtensions;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;


namespace AR_GPS
{
    [System.Serializable]
    public class GeoPrefab
    {
        public string name;
        public GameObject prefab;
        public double latitude;
        public double longitude;
        public double altitudeOffset = -1.5f;
    }

    public class Listeprefabs : MonoBehaviour
    {
        public AREarthManager EarthManager;
        public VpsInitializer Initializer;
        public Text OutputText;
        public ARAnchorManager AnchorManager;

        public double HeadingThreshold = 25.0;
        public double HorizontalThreshold = 20.0;

        // 👉 Liste des objets à placer
        public List<GeoPrefab> ItemsToPlace = new List<GeoPrefab>();

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
                    Instantiate(item.prefab, anchor.transform);
                }
            }
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
