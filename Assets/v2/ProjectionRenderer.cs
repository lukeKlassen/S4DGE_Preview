using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace v2
{
    [ExecuteAlways]
    [RequireComponent(typeof(Transform4D))]
    public class ProjectionRenderer : MonoBehaviour, IShape4DRenderer
    {
        Transform4D t4d;
        public Mesh mesh;

        public InterpolationBasedShape Shape { get=>shape; set=>shape=value; }
        [DoNotSerialize] //TODO for now
        InterpolationBasedShape shape;

        public Material material;
        public List<Vector3> v;
        public List<int> t;
        //
        // BEGIN editor variables/methods
        //

        // 
        // END editor variables/methods
        //

        void Start()
        {
            t4d = gameObject.GetComponent<Transform4D>();
            mesh = new();
            v = mesh.vertices.ToList();
        }

        //
        // Here is the rendering code
        // We add a callback for every camera before it begins rendering
        //  - Upon any camera render, we calculate the slice and submit the geometry for that camera
        //

        private void OnEnable()
        {
            Camera4D.onBeginCameraRendering += RenderForCamera;
        }

        private void OnDisable()
        {
            Camera4D.onBeginCameraRendering -= RenderForCamera;
        }

        static int cameraPosShaderID = Shader.PropertyToID("_4D_Camera_Pos");
        private void RenderForCamera(ScriptableRenderContext ctx, Camera4D cam)
        {
            if (shape == null) return;

            ProjectToMesh(cam);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            MaterialPropertyBlock blk = new();
            blk.SetVector(cameraPosShaderID, Vector4.zero); //pass in camera position to shader (zero for now cause we are in camera local coordinates)
            Graphics.DrawMesh(
                mesh: mesh, 
                matrix: Matrix4x4.identity, 
                material: material, 
                layer: gameObject.layer, 
                camera: cam.camera3D,
                submeshIndex: 0,
                properties: blk
            );
        }

        private void ProjectToMesh(Camera4D cam) {
            if (shape == null) return;

            var transform = cam.t4d.worldToLocalMatrix * t4d.localToWorldMatrix;
            Dictionary<InterpolationPoint4D, PointInfo> pts = new();

            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();

            foreach (var point in shape.points.Values)
            {
                // apply transform of object to point first
                // then apply camera world-to-local transform to that
                PointInfo point4d = point.subpoints[0];
                point4d.position4D = transform * point4d.position4D;
                pts.Add(point, point4d);
            }

            List<PointInfo> vertices4d = pts.Values.ToList();

            // accumulate the triangles for the shape based on its faces
            foreach (Face<InterpolationPoint4D> face in shape.faces4D)
            {
                for (int i = 1; i + 1 < face.points.Count; i++)
                {
                    //front face
                    triangles.Add(vertices4d.IndexOf(pts[face.points[0]]));
                    triangles.Add(vertices4d.IndexOf(pts[face.points[i]]));
                    triangles.Add(vertices4d.IndexOf(pts[face.points[i+1]]));
                }
            }

            // project the points into 3d positions
            for (int i = 0; i < vertices4d.Count; i++)
            {
                float deltaW = vertices4d[i].w - cam.t4d.position.w;
                Debug.Log(deltaW);
                if (Mathf.Abs(deltaW) < 0.0001f) 
                { // offset to prevent division by 0
                    deltaW += (deltaW < 0 ? -1 : 1) * 0.0001f;
                }
                PointInfo p = vertices4d[i];
                // Debug.Log(vertices4d[i].position);
                Vector4 projected = new Vector4(p.position.x / deltaW, p.position.y / deltaW, p.position.z / deltaW, p.position4D.w);
                vertices4d[i] = new PointInfo(){ position4D = projected, uv = vertices4d[i].uv };
                // Debug.Log(p.position);
                // Debug.Log(vertices4d[i].position);
            }
            mesh.Clear();
            mesh.SetVertices(vertices4d.Select(p => p.position).ToList());
            v = mesh.vertices.ToList();
            mesh.SetTriangles(triangles, 0);
            t = mesh.triangles.ToList();
        }
       
    }

}