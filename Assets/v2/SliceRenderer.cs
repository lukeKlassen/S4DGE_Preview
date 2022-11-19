using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace v2
{
    public class SliceRenderer : MonoBehaviour
    {
        Transform4D t4d;
        RenderHelper3D render3d;
        Geometry3D slice = new();

        public InterpolationBasedShape shape;

        //
        // BEGIN editor variables/methods
        //

        // TODO: decide acceptable w range here
        [Range(-10.0f, 10.0f)]
        public float previewW = 0;

        // 
        // END editor variables/methods
        //

        // Start is called before the first frame update
        void Start()
        {
            t4d = gameObject.GetComponent<Transform4D>();
            render3d = gameObject.GetComponent<RenderHelper3D>();
            
        }

        static int cameraPosShaderID = Shader.PropertyToID("_4D_Camera_Pos");

        void Update()
        {
            if (shape == null) return;

            slice.Clear();
            if (Application.IsPlaying(gameObject))
            {
                // display one slice every 0.1 units from -1 to 1
                // TODO discussion on how visuals should look
                for (float dlt = -1; dlt <= 1; dlt += 0.1f)
                {
                    drawSliceAt(Camera4D.main.t4d.position.w + dlt);
                }
            } else
            {
                drawSliceAt(previewW);
            }

            // pass camera position to shader
            Shader.SetGlobalVector(cameraPosShaderID, Camera4D.main.t4d.position);
            render3d.SetGeometry(slice);
        }

        // Iterates over all lines
        // Draws points between them
        // Draws all lines
        void drawSliceAt(float w)
        {
            // lines
            // Debug.Log("Lines");
            foreach (Line<InterpolationPoint4D> line in shape.lines4D)
            { // iterate over every line 

                // Find point location given w
                // for each line, get each point from their start and end location and w
                // and make sure to apply the transform
                Vector3 p1 = line.p1.GetPoint(w, t4d.Transform);
                Vector3 p2 = line.p2.GetPoint(w, t4d.Transform);
                slice.line(p1, p2); // draw line
            }

            // faces
            // Debug.Log("Faces");
            foreach (Face<InterpolationPoint4D> face in shape.faces4D)
            { // face is an array of faces
              // Limit to drawing triangles and squares

                // generate calculated points
                //  1. Select = for each point x apply getPoint(x, w)
                //  3. Pass all calculated points to drawPolygon
                var slicedPoints = face.points
                    .Select(x =>
                    {
                        Vector3 slicedPoint = x.GetPoint(w, t4d.Transform);
                        return new PointInfo()
                        {
                            position = slicedPoint,
                            w = w, // w coordinate of sliced point (equals slice).
                                   // This is used by shader to determine opacity/color
                            uv = new () //texture mapping TODO 
                        };
                    })
                    .ToArray();
                slice.fillPolygon(slicedPoints);
            }
        }
    }

}