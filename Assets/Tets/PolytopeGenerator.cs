using RasterizationRenderer;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using v2;

public class PolytopeGenerator : MonoBehaviour
{
    public enum Polytope {
        cell5,
        cell8,
        cell16,
        cell24,
        cell120,
        cell600
    }

    public Polytope type = Polytope.cell5;
    public bool generateButton = false;

    void OnValidate()
    {
        if (generateButton)
        {
            List<TetMesh4D.VertexData> v = new();
            List<TetMesh4D.Tet4D> t = new();

            switch (type)
            {
                case Polytope.cell5:
                {
                    float goldenRatio = 1.618033988749894f;
                    for (int i = 0; i < 4; i++)
                    {
                        Vector4 pos = Vector4.zero;
                        pos[i] = 2;
                        TetMesh4D.VertexData vertex = new TetMesh4D.VertexData(pos, Vector4.zero);
                        v.Add(vertex);
                    }
                    Vector4 midPoint = new Vector4(goldenRatio, goldenRatio, goldenRatio, goldenRatio);
                    v.Add(new TetMesh4D.VertexData(midPoint, Vector4.zero));
                    
                    for (int i = 0; i < 5; i++) // 5 choose 4 (5) tets
                    {
                        for (int j = i + 1; j < 5; j++)
                        {
                            for (int k = j + 1; k < 5; k++)
                            {
                                for (int l = k + 1; l < 5; l++)
                                {
                                    int[] tet = {i, j, k, l};
                                    t.Add(new TetMesh4D.Tet4D(tet));
                                }
                            }
                        }
                    }
                    break;
                }
                case Polytope.cell8:
                {
                    TetMesh4D mesh = HypercubeGenerator.GenerateHypercube();
                    v = mesh.vertices.ToList();
                    t = mesh.tets.ToList();
                    break;
                }
                case Polytope.cell16:
                {
                    TetMesh_raw raw = Generate16Cell(1, 0, 0);
                    v = raw.vertices;
                    t = raw.tets;
                    break;
                }
                case Polytope.cell24:
                {
                    TetMesh4D centerCube = HypercubeGenerator.GenerateHypercube(); // center tesseract at (1,1,1,1) permutations
                    v = centerCube.vertices.ToList();
                    t = centerCube.tets.ToList();

                    TetMesh_raw outer16 = Generate16Cell(2, 0, v.Count); // outer 16 cell at (2,0,0,0) permutations
                    v.AddRange(outer16.vertices);
                    t.AddRange(outer16.tets);
                    break;
                }
                default:
                {
                    Debug.LogError($"Tetmesh Generation of Polytope: {type} Not Implemented.");
                    break;
                }
            }
            
            CreateScriptableObject(v, t);

            generateButton = false;
        }
    }

    TetMesh_raw Generate16Cell(float radius, float offset, int startIndex)
    {
        TetMesh_raw mesh = new();

        float[] vals = {radius + offset, -radius + offset};
        foreach (float i in vals)
        {
            for (int j = 0; j < 4; j++)
            {
                Vector4 pos = Vector4.zero;
                pos[j] = i;
                TetMesh4D.VertexData vertex = new TetMesh4D.VertexData(pos, Vector4.zero);
                mesh.vertices.Add(vertex);
            }
        }

        int numVertices = 8;
        int half = numVertices / 2;
        int[] option = {0, half};

        foreach (int one in option)
        {
            foreach (int two in option)
            {
                foreach (int three in option)
                {
                    foreach (int four in option)
                    {
                        int[] tet = {one + startIndex, two + 1 + startIndex, three + 2 + startIndex, four + 3 + startIndex};
                        mesh.tets.Add(new TetMesh4D.Tet4D(tet));
                    }
                }
            }
        }

        return mesh;
    }

    void CreateScriptableObject(List<TetMesh4D.VertexData> v, List<TetMesh4D.Tet4D> t)
    {
        TetMesh_UnityObj mesh = ScriptableObject.CreateInstance<TetMesh_UnityObj>();
        mesh.mesh_Raw = new()
        {
            tets = t,
            vertices = v,
        };
        UnityEditor.AssetDatabase.CreateAsset(mesh, $"Assets/Tets/Polytopes/{type}.asset");
        Debug.Log($"Created polytope: Assets/Tets/Polytopes/{type}.asset");
    }
}
