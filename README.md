# Unity ARCore Geospatial Test Project

This project is a test application developed with Unity to explore the features of the ARCore Geospatial API. It allows placing 3D objects at precise GPS coordinates in the real world using augmented reality.

Objects can also be dynamically added, moved, or deleted at runtime via MQTT messages encoded in JSON.

## Technologies Used

* **Game Engine**: Unity (Compatible with Unity 6)
* **AR Foundation**: Version 6.3
* **ARCore Extensions**: Version 1.52
* **Target Platform**: Android (ARCore-compatible devices)
* **Messaging**: MQTT (JSON-formatted payloads)

## Project Structure

The Unity project is located in the `ProjectExamples` folder.

### Main Scenes

Several scenes are available to test different placement and tracking features:

* **`Scenes/TestGPS_1.unity`** – Test scene to confirm object placement at a specific latitude/longitude
* **`Scenes/TestGPS_2.unity`** – Additional scene for location testing
* **`Scenes/TestGPS_3.unity`** – Advanced (or alternative) scene for geospatial placement
* **`Scenes/TestGPS_4.unity`** – Scene using **dynamic prefab placement via MQTT and JSON messages**
* **`Scenes/Tap_To_Place.unity`** – Manual placement using touch raycasts
* **`Samples/ARCore Extensions/.../GeospatialArf6.unity`** – Official Google AR Foundation 6 sample

## Available Prefabs (For Geospatial Placement)

These prefabs are available in the built-in prefab library and can be instantiated dynamically by name via MQTT JSON commands:

| Name in JSON        | Description              |
| ------------------- | ------------------------ |
| **stirling_engine** | Stirling engine 3D model |
| **PrefabFablab**    | FabLab building/model    |
| **Cube**            | Standard Unity Cube      |
| **Cylinder**        | Standard Unity Cylinder  |

✔ Names must match exactly to be recognized
✔ One active object exists per name


## Installation and Configuration

1. **Open the project**

   * Launch Unity Hub
   * Open the `ProjectExamples` folder

2. **API Key Configuration**

   * A valid Google Cloud API Key is required with:

     * **ARCore API**
     * **Geospatial API**
   * Configure in:

     ```
     Edit → Project Settings → XR Plug-in Management → ARCore Extensions
     ```

3. **Build on Android**

   * Activate developer mode
   * Connect device via USB
   * Open:

     ```
     File → Build Settings
     ```
   * Select your test scene(s)
   * Click **Build And Run**

---

## Dynamic Object Placement via MQTT

### Add or Move an Object

```json
{
  "items": [
    {
      "name": "Cube",
      "latitude": 43.700000,
      "longitude": 7.260000,
      "altitudeOffset": 0
    }
  ]
}
```

If the object already exists, it will smoothly move to the new location.

### Delete an Object

```json
{
  "items": [
    {
      "name": "Cube",
      "delete": true
    }
  ]
}
```

Only that object is removed.
Other objects remain visible.

### System Behaviour

✔ Objects not referenced remain
✔ Updates can be sent on demand
✔ Anchor tracking ensures stability
✔ A smoothing follower reduces visual jitter

## Usage

When starting the application:

✔ Allow **Camera access**
✔ Allow **Precise Location access**

The system initializes ARCore + Geospatial tracking.
Best accuracy is obtained outdoors with open sky visibility.

## ⚠️ Important Notes

* Device must support **ARCore Geospatial API**
* Internet connectivity is required for VPS data
* Accuracy improves with motion and sky visibility
* Objects are positioned using:
  Geospatial Anchor + Local Smoothing to minimize visual jumps

