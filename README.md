# Unity ARCore Geospatial Test Project

This project is a test application developed with Unity to explore the features of the ARCore Geospatial API. It allows placing 3D objects at precise GPS coordinates in the real world using augmented reality.

Objects can also be dynamically added, moved, resized, or deleted at runtime via MQTT messages encoded in JSON.

## Technologies Used

* **Game Engine**: Unity (Compatible with Unity 6)
* **AR Foundation**: Version 6.3
* **ARCore Extensions**: Version 1.52
* **Target Platform**: Android (ARCore-compatible devices)
* **Messaging**: MQTT (JSON-formatted payloads)

## Project Structure

The Unity project is located in the `Geospatial_AR_CoreExt_Unity` folder.

### Main Scenes

Several scenes are available to test different placement and tracking features:

* **`Scenes/TestGPS_1.unity`** – Test scene to confirm object placement at a specific latitude/longitude
* **`Scenes/TestGPS_2.unity`** – Additional scene for location testing
* **`Scenes/TestGPS_3.unity`** – Advanced (or alternative) scene for geospatial placement
* **`Scenes/TestGPS_4.unity`** – Scene using **dynamic prefab placement via MQTT and JSON messages**
* **`Scenes/Tap_To_Place.unity`** – Manual placement using touch raycasts
* **`Samples/ARCore Extensions/.../GeospatialArf6.unity`** – Official Google AR Foundation 6 sample

## Available Prefabs (For Geospatial Placement)

These prefabs are available in the built-in prefab library and can be instantiated dynamically by name via MQTT JSON commands.

> [!NOTE]
> At startup, any items placed in the Inspector with generic names (e.g., "1", "2") are automatically renamed to their Prefab name. This simplifies control via MQTT.

| Name in JSON        | Description              |
| ------------------- | ------------------------ |
| **stirling_engine** | Stirling engine 3D model |
| **PrefabFablab**    | FabLab building/model    |
| **Cube**            | Standard Unity Cube      |
| **Cylinder**        | Standard Unity Cylinder  |
| **saturn**          | Saturn planet model      |

## Dynamic Object Placement via MQTT

### Configuration

* **Topic IN (Commands)**: `FABLAB_21_22/unity/testgps/in`
* **Topic OUT (Feedback)**: `FABLAB_21_22/unity/testgps/out`

### Add, Move or Scale an Object

To place or update an object, send a JSON message to the **Topic IN**:

```json
{
  "items": [
    {
      "name": "saturn",
      "latitude": 43.700000,
      "longitude": 7.260000,
      "altitudeOffset": 15,
      "scale": 2.5
    }
  ]
}
```

* **name**: Must match one of the available prefabs.
* **latitude / longitude**: GPS coordinates.
* **altitudeOffset**: Height in meters relative to the ground (VPS estimate).
* **scale** (Optional): Uniform scale factor (X, Y, Z). If omitted or 0, the prefab's default scale is used.

### Delete an Object

To remove an object from the scene:

```json
{
  "items": [
    {
      "name": "saturn",
      "delete": true
    }
  ]
}
```

## Installation and Configuration

1. **Open the project**
   * Launch Unity Hub
   * Open the project folder

2. **API Key Configuration**
   * A valid Google Cloud API Key is required with connected:
     * **ARCore API**
     * **Geospatial API**
   * Configure in: `Edit → Project Settings → XR Plug-in Management → ARCore Extensions`

3. **Build on Android**
   * Activate developer mode on your phone
   * Connect device via USB
   * Open: `File → Build Settings`
   * Select your test scene (e.g., `TestGPS_4`)
   * Click **Build And Run**

## Usage

When starting the application:

1. Allow **Camera access**
2. Allow **Precise Location access**

The system initializes ARCore + Geospatial tracking. Best accuracy is obtained outdoors with good sky visibility.

## ⚠️ Important Notes

* Device must support **ARCore Geospatial API**
* Internet connectivity is required for VPS data downloading
* Accuracy improves with motion and sky visibility
* Objects are positioned using **Geospatial Anchors** for stability
