# Unity ARCore Geospatial Test Project

This project is a test application developed with Unity to explore the features of the ARCore Geospatial API. It allows you to place 3D objects at precise GPS coordinates in the real world using augmented reality.

## 🛠 Technologies Used

* **Game engine**: Unity (Compatible with Unity 6)
* **AR Foundation**: Version 6.3
* **ARCore Extensions**: Version 1.52
* **Target platform**: Android (ARCore-compatible devices)

## 📂 Project Structure

The Unity project is located in the `ProjectExamples` folder.

### Main Scenes

Several scenes are available to test different placement and tracking features:

* **`Scenes/TestGPS_1.unity`**: Test scene to confirm the placement of objects at a specific latitude/longitude.
* **`Scenes/TestGPS_2.unity`**: Additional scene for location testing.
* **`Scenes/TestGPS_3.unity`**: Advanced (or alternative) scene for geospatial placement.
* **`Scenes/Tap_To_Place.unity`**: Scene that likely allows objects to be placed manually via touch interaction (raycasting).
* **`Scenes/TestGPS_4.unity`**: Scene designed for dynamic object placement via MQTT. It listens for JSON commands to spawn and move prefabs at runtime.
* **`Samples/ARCore Extensions/.../GeospatialArf6.unity`**: The official example provided by Google for AR Foundation 6.

## 🚀 Installation and Configuration

1. **Open the project**:

* Launch Unity Hub.
* Open the project's root folder located in `ProjectExamples`.

1. **Configure the API Key**:
    * For Geospatial features to work, you must have a valid Google Cloud API key with **ARCore API** and **Geospatial API** enabled.

* In Unity, go to `Edit > Project Settings > XR Plug-in Management > ARCore Extensions`.
* Ensure your API key is entered in the corresponding field for Android.

1. **Build on Android:**

* Connect your Android device in developer mode.
* Go to `File > Build Settings`.
* Select the scene you want to test (add it to the list if necessary).
* Click **Build And Run**.

## 📱 Usage

* When launching the application, accept the permissions for **Camera** and **Location** (precise).
* The system will attempt to locate itself (VPS - Visual Positioning System). It is recommended that you be outdoors in an area covered by Google Street View for optimal accuracy.
* Once located, the objects defined in the scene should appear at their respective geographical coordinates.

## 📡 MQTT Control (TestGPS_4)

The `TestGPS_4` scene features a dynamic loading system powered by MQTT.

### Configuration

* **Broker**: `mqtt.univ-cotedazur.fr` (Port 8443, SSL/TLS)
* **Topic IN (Commands)**: `FABLAB_21_22/unity/testgps/in`
* **Topic OUT (Feedback)**: `FABLAB_21_22/unity/testgps/out`

### Remote Control via JSON

To place or move objects, send a JSON payload to the **IN** topic.

* **`name`**: Must match exactly the name of a prefab in the `Prefab Library` (script *Listeprefabs2*).
* **`altitudeOffset`**: Height relative to the detected ground/VPS anchor.

**JSON Example:**

```json
{
  "items": [
    {
      "name": "Cylinder",
      "latitude": 43.80584,
      "longitude": 7.305819,
      "altitudeOffset": 3
    },
    {
      "name": "Cube",
      "latitude": 43.80580,
      "longitude": 7.30616,
      "altitudeOffset": -1.5
    }
  ]
}
```

## ⚠️ Important Notes

* Ensure that your device supports **ARCore Geospatial API**.
* An internet connection is required to download VPS location data.

![Aperçu du projet](images/TestGPS_2.jpg)
