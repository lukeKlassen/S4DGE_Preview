using UnityEngine;

namespace RasterizationRenderer
{
    public class TetSlicer
    {
        [SerializeField]
        public ComputeShader sliceShader;
        ComputeBuffer tetrahedraBuffer;
        VariableLengthComputeBuffer triangleVertices;
        VariableLengthComputeBuffer slicedTriangles;
        VariableLengthComputeBuffer.BufferList bufferList;
        int numTets;
        int sliceShaderKernel;
        uint threadGroupSize;

        readonly int PTS_PER_TRIANGLE = 3;

        public TetSlicer(ComputeBuffer tetrahedra, int numTets)
        {
            this.tetrahedraBuffer = tetrahedra;
            this.numTets = numTets;
        }

        /*
         * modelViewRotation4D, modelViewTranslation4D: transformation of 4D object
         * zSlice: z-coordinate of slicing plane for camera
         * vanishingW: camera clip plane - vanishing point at (0, 0, 0, vanishingW)
         * nearW: camera viewport plane at w = nearW
         */
        public VariableLengthComputeBuffer Render(ComputeBuffer vertexBuffer)
        {
            bufferList.PrepareForRender();

            // Run vertex shader to transform points and perform perspective projection
            sliceShader.SetBuffer(sliceShaderKernel, "transformedVertices", vertexBuffer);
            sliceShader.SetBuffer(sliceShaderKernel, "tetsToDraw", tetrahedraBuffer);
            int numThreadGroups = (int)((numTets + (threadGroupSize - 1)) / threadGroupSize);
            sliceShader.Dispatch(sliceShaderKernel, numThreadGroups, 1, 1);

            bufferList.UpdateBufferLengths();

            // return array of tetrahedra that drawTetrahedron says should be drawn
            return slicedTriangles;
        }

        public void OnEnable()
        {
            sliceShaderKernel = sliceShader.FindKernel("TetrahedronSlicer");
            sliceShader.GetKernelThreadGroupSizes(sliceShaderKernel, out threadGroupSize, out _, out _);

            slicedTriangles = new("slicedTriangles", numTets * 3 / 2, sizeof(float) * PTS_PER_TRIANGLE);
            triangleVertices = new("triangleVertices", numTets * 4, sizeof(float) * 4);
            bufferList = new(new VariableLengthComputeBuffer[2] { slicedTriangles, triangleVertices }, sliceShader, sliceShaderKernel);
        }

        public void OnDisable()
        {
            slicedTriangles = null;
            triangleVertices = null;
            bufferList = null;
        }
    }
}
