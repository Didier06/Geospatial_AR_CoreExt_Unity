using System.Collections;
using Google.XR.ARCoreExtensions;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.XR.ARFoundation;

namespace AR_GPS
{
    public class VpsInitializer : MonoBehaviour
    {
        [Header("AR Components")]
        [SerializeField] private ARCoreExtensions arCoreExtensions;
        [SerializeField] private AREarthManager earthManager;
        [SerializeField] private ARSession arSession;

        private bool _isReturning = false;
        private bool _enablingGeospatial = false;
        private float _configurePrepareTime = 3f;

        private IEnumerator _startLocationService = null;

        private bool _isReady = false;
        public bool IsReady => _isReady;

        public bool _lockScreenToPortrait = false;

        void Awake()
        {
            if (_lockScreenToPortrait)
            {
                Screen.orientation = ScreenOrientation.Portrait;
                Screen.autorotateToLandscapeLeft = false;
                Screen.autorotateToLandscapeRight = false;
                Screen.autorotateToPortraitUpsideDown = false;
            }
            else
            {
                // Active la rotation automatique
                Screen.orientation = ScreenOrientation.AutoRotation;
                Screen.autorotateToLandscapeLeft = true;
                Screen.autorotateToLandscapeRight = true;
                Screen.autorotateToPortrait = true;
                Screen.autorotateToPortraitUpsideDown = true;
            }

            Application.targetFrameRate = 60;

            if (!arCoreExtensions || !earthManager || !arSession)
            {
                Debug.LogError("Missing ARCore components");
            }
        }

        void OnEnable()
        {
            _isReturning = false;
            _enablingGeospatial = false;
            _isReady = false;

            _startLocationService = StartLocationService();
            StartCoroutine(_startLocationService);
        }

        void OnDisable()
        {
            if (_startLocationService != null)
                StopCoroutine(_startLocationService);

            Input.location.Stop();
        }

        IEnumerator StartLocationService()
        {
#if UNITY_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            {
                Permission.RequestUserPermission(Permission.FineLocation);
                yield return new WaitForSeconds(2f);
            }
#endif
            if (!Input.location.isEnabledByUser)
                yield break;

            Input.location.Start();

            while (Input.location.status == LocationServiceStatus.Initializing)
                yield return null;

            if (Input.location.status != LocationServiceStatus.Running)
                Input.location.Stop();
        }

        void Update()
        {
            LifecycleUpdate();
            if (_isReturning)
                return;

            if (ARSession.state != ARSessionState.SessionInitializing &&
                ARSession.state != ARSessionState.SessionTracking)
                return;

            // 🔥 NOUVELLE API
            var support = earthManager.IsGeospatialModeSupported(GeospatialMode.Enabled);

            if (support == FeatureSupported.Unsupported)
            {
                ReturnWithReason("This device does not support Geospatial API.");
                return;
            }

            if (support == FeatureSupported.Supported &&
                arCoreExtensions.ARCoreExtensionsConfig.GeospatialMode == GeospatialMode.Disabled)
            {
                Debug.Log("Enabling Geospatial Mode...");
                arCoreExtensions.ARCoreExtensionsConfig.GeospatialMode = GeospatialMode.Enabled;
                _configurePrepareTime = 3f;
                _enablingGeospatial = true;
                return;
            }

            if (_enablingGeospatial)
            {
                _configurePrepareTime -= Time.deltaTime;
                if (_configurePrepareTime <= 0)
                    _enablingGeospatial = false;
                else
                    return;
            }

            // Vérifier état Earth
            if (earthManager.EarthState != EarthState.Enabled)
                return;

            bool ready =
                ARSession.state == ARSessionState.SessionTracking &&
                Input.location.status == LocationServiceStatus.Running;

            _isReady = ready;
        }

        void LifecycleUpdate()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
                Application.Quit();

            Screen.sleepTimeout =
                ARSession.state == ARSessionState.SessionTracking ?
                SleepTimeout.NeverSleep :
                SleepTimeout.SystemSetting;

            string reason = "";

            if (ARSession.state == ARSessionState.None ||
                ARSession.state == ARSessionState.Unsupported)
            {
                reason = "AR session error — restart app";
            }
            else if (Input.location.status == LocationServiceStatus.Failed)
            {
                reason = "Location service failed — check permissions";
            }

            ReturnWithReason(reason);
        }

        void ReturnWithReason(string reason)
        {
            if (string.IsNullOrEmpty(reason))
                return;

            Debug.LogError(reason);
            _isReturning = true;
            _isReady = false;
        }
    }
}
