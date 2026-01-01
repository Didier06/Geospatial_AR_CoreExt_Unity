using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// AR Foundation & ARCore Extensions
using Google.XR.ARCoreExtensions;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Numerics;

namespace AR_GPS
{
    public class SampleScript2 : MonoBehaviour
    {
        // Tracking information using Geospatial API
        public AREarthManager EarthManager;

        // Geospatial API & ARCore initialization / readiness
        public VpsInitializer Initializer;

        // UI text field for displaying tracking status + pose
        public Text OutputText;

        // Thresholds for good enough accuracy
        public double HeadingThreshold = 25.0;
        public double HorizontalThreshold = 20.0;
        public double Latitude;
        public double Longitude;
        private double Altitude;
        public double Heading;
        // Content to be placed at the specified geospatial location     
        public GameObject ContentPrefab;
        // Instance of the placed content
        GameObject displayObject;
        // AR Anchor Manager for creating anchors
        public ARAnchorManager AnchorManager;


        void Start()
        {
        }

        void Update()
        {
            string status = "";

            // If VPS not ready or no Earth tracking, do nothing
            if (!Initializer.IsReady ||
                EarthManager.EarthTrackingState != TrackingState.Tracking)
            {
                return;
            }

            // Get tracking results
            GeospatialPose pose = EarthManager.CameraGeospatialPose;
            // Check accuracy
            if (pose.OrientationYawAccuracy > HeadingThreshold ||
                pose.HorizontalAccuracy > HorizontalThreshold)
            {
                status = "Low accuracy. Please move to open space and point device to sky.";
            }
            else
            {
                status = "Good accuracy.";
                if (displayObject == null)
                {
                    // height of the ground relative to the device
                    Altitude = pose.Altitude - 1.5f;
                    // Angle correction for the object to face the user
                    // Quaternion is a function that creates a rotation in 3D space.
                    // vector3.up means the y-axis (0,1,0)
                    UnityEngine.Quaternion quaternion = UnityEngine.Quaternion.AngleAxis(
                        180f - (float)Heading, UnityEngine.Vector3.up); // to place the object in front of the user
                    // Create anchor at specified location
                    ARGeospatialAnchor anchor = AnchorManager.AddAnchor(
                        Latitude, Longitude, Altitude, quaternion);// 

                    // if you want to place the object on the terrain, use the following line instead
                    //ARGeospatialAnchor anchor = AnchorManager.ResolveAnchorOnTerrain(
                    //    Latitude, Longitude, Altitude, quaternion);

                    //ARGeospatialAnchor anchor = AnchorManager.AddAnchor(
                    //    Latitude, Longitude, Altitude, Heading);

                    // If anchor creation was successful, instantiate the content
                    if (anchor != null)
                    {
                        // Instantiate content at anchor position
                        displayObject = Instantiate(
                            ContentPrefab,
                            anchor.transform);
                    }
                }
            }

            // Show the results
            ShowTrackingInfo2(status, pose);
        }


        void ShowTrackingInfo2(string status, GeospatialPose pose)
        {
            if (OutputText == null)
                return;

            // Convert ENU quaternion → yaw (heading degrees)
            UnityEngine.Quaternion r = pose.EunRotation;

            float yaw = Mathf.Atan2(
                2f * (r.w * r.y + r.x * r.z),
                1f - 2f * (r.y * r.y + r.z * r.z)
            ) * Mathf.Rad2Deg;

            if (yaw < 0)
                yaw += 360f;

            OutputText.text = string.Format(
                "Latitude/Longitude: {0}°, {1}°\n" +
                "Horizontal Accuracy: {2} m\n" +
                "Altitude: {3} m\n" +
                "Vertical Accuracy: {4} m\n" +
                "Heading: {5} °\n" +
                "Heading Accuracy: {6} °\n" +
                "{7}\n",

                pose.Latitude.ToString("F6"),          // {0}
                pose.Longitude.ToString("F6"),         // {1}
                pose.HorizontalAccuracy.ToString("F2"),// {2}
                pose.Altitude.ToString("F2"),          // {3}
                pose.VerticalAccuracy.ToString("F2"),  // {4}
                yaw.ToString("F1"),                    // {5}
                pose.OrientationYawAccuracy.ToString("F1"), // {6}
                status                                 // {7}
            );
        }
    }
}

