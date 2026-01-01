using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// AR Foundation & ARCore Extensions
using Google.XR.ARCoreExtensions;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace AR_GPS
{
    public class SampleScript : MonoBehaviour
    {
        // Tracking information using Geospatial API
        public AREarthManager EarthManager;

        // Geospatial API & ARCore initialization / readiness
        public VpsInitializer Initializer;

        // UI text field for displaying tracking status + pose
        public Text OutputText;


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

            // Show the results
            ShowTrackingInfo(status, pose);
        }


        void ShowTrackingInfo(string status, GeospatialPose pose)
        {
            if (OutputText == null)
                return;

            // Convert ENU quaternion → yaw (heading degrees)
            Quaternion r = pose.EunRotation;

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

