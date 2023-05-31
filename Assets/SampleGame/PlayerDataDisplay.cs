using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace v2 {
    // Allows the referenced player to directly affect their t4d w coordinate using the 1 and 2 keyboard keys
    [RequireComponent(typeof(Camera4D))]
    public class PlayerDataDisplay : MonoBehaviour
    {
        public Camera4D camera;
        Vector4? selectedIntersect = null;

        private void Awake()
        {
            camera = GetComponent<Camera4D>();
        }

        void Update()
        {
            if (Input.GetKey(KeyCode.Mouse0)) // 1 key above alphabet keys
            {
                UpdateDataDisplay( camera.t4d.position, camera.t4d.forward );
            }
        }

        void OnGUI()
        {
            GUILayout.Label( " Viewing at w: " + camera.t4d?.position.w );
            if ( selectedIntersect == null ) {
                GUILayout.Label( " No datapoint selected ");
            } else {
                GUILayout.Label( " Datapoint x = " + selectedIntersect?.x);
                GUILayout.Label( " Datapoint y = " + selectedIntersect?.y);
                GUILayout.Label( " Datapoint z = " + selectedIntersect?.z);
                GUILayout.Label( " Datapoint w = " + selectedIntersect?.w);
            }
        }

        private void UpdateDataDisplay( Vector4 position, Vector4 forward )
        {
            Vector4 normalized = forward.normalized;

            List<Ray4D.Intersection?> collidePoints = new();

            Ray4D.Intersection? collidePoint = null;
            float bestDistance = float.PositiveInfinity;

            IEnumerable<Ray4D.Intersection> collisions = CollisionSystem.Instance.Raycast(new Ray4D
            {
                src = position,
                direction = normalized
            }, Physics.AllLayers);

            // find raycasted point closest to source
            if (collisions.Count() > 0)
            {
                Ray4D.Intersection minForThis = collisions.Min();
                float curDist = (minForThis.point - position).magnitude;
                if (curDist < bestDistance)
                {
                    bestDistance = curDist;
                    collidePoint = minForThis;
                }
            }

            if (collidePoint is Ray4D.Intersection collidePointNotNull)
            {
                selectedIntersect = collidePointNotNull.point;
            } else {
                selectedIntersect = null;
            }
        }
    }
}