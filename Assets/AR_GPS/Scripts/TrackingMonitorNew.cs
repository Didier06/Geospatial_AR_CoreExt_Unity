using System;
using Google.XR.ARCoreExtensions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;

namespace AR_Fukuoka
{
    public class TrackingMonitor : MonoBehaviour
    {
        [Header("AR Components")]
        [SerializeField] private AREarthManager earthManager;

        [Header("UI")]
        [SerializeField] private Text outputText;

        [Header("Accuracy thresholds")]
        [SerializeField] private double headingThreshold = 25;
        [SerializeField] private double horizontalThreshold = 20;

        void Update()
        {
            if (earthManager == null)
                return;

            // 🔥 Tracking state now comes from EarthManager
            if (earthManager.EarthTrackingState != TrackingState.Tracking)
                return;

            // Retrieve camera geospatial pose
            GeospatialPose pose = earthManager.CameraGeospatialPose;

            string status;

            if (pose.OrientationYawAccuracy > headingThreshold ||
                pose.HorizontalAccuracy > horizontalThreshold)
            {
                status = "Low Tracking Accuracy — please look around";
            }
            else
            {
                status = "High Tracking Accuracy";
            }

            ShowTrackingInfo(status, pose);
        }

        void ShowTrackingInfo(string status, GeospatialPose pose)
        {
            if (outputText == null)
                return;

            // Convert ENU rotation → heading (yaw degrees, 0–360)
            Quaternion r = pose.EunRotation;

            float yaw = Mathf.Atan2(
                2f * (r.w * r.y + r.x * r.z),
                1f - 2f * (r.y * r.y + r.z * r.z)
            ) * Mathf.Rad2Deg;

            if (yaw < 0)
                yaw += 360f;

            outputText.text = string.Format(
                "\nLatitude / Longitude: {0}°, {1}°\n" +
                "Horizontal Accuracy: {2} m\n" +
                "Altitude: {3} m\n" +
                "Vertical Accuracy: {4} m\n" +
                "Heading: {5}°\n" +
                "Heading Accuracy: {6}°\n" +
                "{7}\n",
                pose.Latitude.ToString("F6"),
                pose.Longitude.ToString("F6"),
                pose.HorizontalAccuracy.ToString("F2"),
                pose.Altitude.ToString("F2"),
                pose.VerticalAccuracy.ToString("F2"),
                yaw.ToString("F1"),
                pose.OrientationYawAccuracy.ToString("F1"),
                status
            );
        }
    }
}
