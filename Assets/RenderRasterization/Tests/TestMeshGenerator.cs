using NUnit.Framework;
using RasterizationRenderer;
using System.Linq;
using UnityEngine;
using static MeshGeneratorUtils;
using static RasterizationRenderer.MeshGenerator4D;
using Manifold3D = System.Func<UnityEngine.Vector3, UnityEngine.Vector4>;

public class TestMeshGenerator
{
    TetMesh4D tetMesh;

    // A Test behaves as an ordinary method
    [Test]
    public void TestMeshGeneratorSingleCube()
    {

        // same set of equations generate positions and normals
        Manifold3D generator = params3D =>
        {
            return new Vector4(
                params3D.x,
                params3D.y,
                params3D.z,
                0
            );
        };

        // Bounds are [(0, pi), (0, pi), (0, 2pi)]
        ParameterBounds3D samplingBounds = new(
            Vector3.zero, Vector3.one, 1.0f
        );

        tetMesh = GenerateTetMesh(generator, generator, samplingBounds);

        for (int i = 0; i < 2; ++i)
            for (int j = 0; j < 2; ++j)
                for (int k = 0; k < 2; ++k)
                    Assert.Contains(new Vector4(i, j, k, 0), tetMesh.vertices.Select(vertexData => vertexData.position).ToList());

        TetMesh4D.Tet4D[] expectedTets =
        {
            new(new int[] { 7,3,6,1 }),
            new(new int[] { 7,5,1,6 }),
            new(new int[] { 5,4,1,6 }),
            new(new int[] { 3,2,6,1 }),
            new(new int[] { 6,0,1,2 }),
            new(new int[] { 6,4,1,0 }),
        };

        foreach (var expectedTet in expectedTets)
        {
            Assert.True(tetMesh.tets.Where(tet => Enumerable.SequenceEqual(expectedTet.tetPoints, tet.tetPoints)).Count() > 0);
        }
    }
}
