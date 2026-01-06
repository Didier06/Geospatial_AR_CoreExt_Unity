using UnityEngine;

namespace AR_GPS
{
    public class GeoTargetFollower : MonoBehaviour
    {
        public Transform anchor;
        public float smoothTime = 0.35f;
        public float rotSmooth = 0.15f;

        private Vector3 vel = Vector3.zero;

        void LateUpdate()
        {
            if (anchor == null) return;

            // Dead-zone : ignore les micro-mouvements < 3 cm
            if (Vector3.Distance(transform.position, anchor.position) < 0.03f)
                return;

            transform.position = Vector3.SmoothDamp(
                transform.position,
                anchor.position,
                ref vel,
                smoothTime);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                anchor.rotation,
                rotSmooth);
        }
    }
}

