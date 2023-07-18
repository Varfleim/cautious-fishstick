
using System;
using System.Collections.Generic;

using UnityEngine;

namespace SandOcean.Map
{
    public class GOMapMesh : MonoBehaviour
    {
        [NonSerialized]
        List<Vector3> vertices = new();
        [NonSerialized]
        List<int> triangles = new();

        [NonSerialized]
        List<Vector3> regionIndices = new();
        [NonSerialized]
        List<Color> regionWeights = new();

        [NonSerialized]
        List<Vector2> uvs = new();
        [NonSerialized]
        List<Vector2> uv2s = new();

        public Mesh mesh;
        public MeshFilter meshFilter;
        public MeshCollider meshCollider;

        public bool useCollider;
        public bool useRegionData;
        public bool useUVCoordinates;
        public bool useUV2Coordinates;

        public void Clear()
        {
            meshFilter.mesh.Clear();

            vertices = ListPool<Vector3>.Get();

            if(useRegionData == true)
            {
                regionWeights = ListPool<Color>.Get();
                regionIndices = ListPool<Vector3>.Get();
            }

            if (useUVCoordinates == true)
            {
                uvs = ListPool<Vector2>.Get();
            }
            if (useUV2Coordinates == true)
            {
                uv2s = ListPool<Vector2>.Get();
            }

            triangles = ListPool<int>.Get();
        }

        public void Apply()
        {
            mesh.SetVertices(vertices);
            ListPool<Vector3>.Add(vertices);

            if (useRegionData == true)
            {
                mesh.SetColors(regionWeights);
                ListPool<Color>.Add(regionWeights);

                mesh.SetUVs(2, regionIndices);
                ListPool<Vector3>.Add(regionIndices);
            }

            if (useUVCoordinates == true)
            {
                mesh.SetUVs(0, uvs);
                ListPool<Vector2>.Add(uvs);
            }
            if (useUV2Coordinates == true)
            {
                mesh.SetUVs(1, uv2s);
                ListPool<Vector2>.Add(uv2s);
            }

            mesh.SetTriangles(triangles, 0);
            ListPool<int>.Add(triangles);
            mesh.RecalculateNormals();

            if (useCollider == true)
            {
                meshCollider.sharedMesh = mesh;
            }
        }

        public void AddTriangle(
            Vector3 v1, Vector3 v2, Vector3 v3)
        {
            //Определяем индекс первой вершины
            int vertexIndex = vertices.Count;

            //Заносим вершины
            vertices.Add(
                MapGenerationData.Perturb(v1));
            vertices.Add(
                MapGenerationData.Perturb(v2));
            vertices.Add(
                MapGenerationData.Perturb(v3));

            //Заносим треугольники
            triangles.Add(
                vertexIndex);
            triangles.Add(
                vertexIndex + 1);
            triangles.Add(
                vertexIndex + 2);
        }

        public void AddTriangleUnperturbed(
            Vector3 v1, Vector3 v2, Vector3 v3)
        {
            //Определяем индекс первой вершины
            int vertexIndex = vertices.Count;

            //Заносим вершины
            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);

            //Заносим треугольники
            triangles.Add(
                vertexIndex);
            triangles.Add(
                vertexIndex + 1);
            triangles.Add(
                vertexIndex + 2);
        }

        public void AddQuad(
            Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
        {
            //Определяем индекс первой вершины
            int vertexIndex = vertices.Count;

            //Заносим вершины
            vertices.Add(
                MapGenerationData.Perturb(v1));
            vertices.Add(
                MapGenerationData.Perturb(v2));
            vertices.Add(
                MapGenerationData.Perturb(v3));
            vertices.Add(
                MapGenerationData.Perturb(v4));

            //Заносим треугольники
            triangles.Add(
                vertexIndex);
            triangles.Add(
                vertexIndex + 2);
            triangles.Add(
                vertexIndex + 1);
            triangles.Add(
                vertexIndex + 1);
            triangles.Add(
                vertexIndex + 2);
            triangles.Add(
                vertexIndex + 3);
        }

        public void AddQuadUnperturbed(
            Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
        {
            //Определяем индекс первой вершины
            int vertexIndex = vertices.Count;

            //Заносим вершины
            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);
            vertices.Add(v4);

            //Заносим треугольники
            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 3);
        }

        public void AddTriangleCellData(
            Vector3 indices,
            Color weights1, Color weights2, Color weights3)
        {
            regionIndices.Add(indices);
            regionIndices.Add(indices);
            regionIndices.Add(indices);

            regionWeights.Add(weights1);
            regionWeights.Add(weights2);
            regionWeights.Add(weights3);
        }

        public void AddTriangleCellData(
            Vector3 indices,
            Color weights)
        {
            AddTriangleCellData(
                indices,
                weights, weights, weights);
        }

        public void AddQuadCellData(
            Vector3 indices,
            Color weights1, Color weights2, Color weights3, Color weights4
            )
        {
            regionIndices.Add(indices);
            regionIndices.Add(indices);
            regionIndices.Add(indices);
            regionIndices.Add(indices);

            regionWeights.Add(weights1);
            regionWeights.Add(weights2);
            regionWeights.Add(weights3);
            regionWeights.Add(weights4);
        }

        public void AddQuadCellData(
            Vector3 indices, 
            Color weights1, Color weights2)
        {
            AddQuadCellData(
                indices, 
                weights1, weights1, weights2, weights2);
        }

        public void AddQuadCellData(
            Vector3 indices, 
            Color weights)
        {
            AddQuadCellData(
                indices, 
                weights, weights, weights, weights);
        }

        public void AddTriangleUV(
            Vector2 uv1, Vector2 uv2, Vector2 uv3)
        {
            //Заносим координаты вершин
            uvs.Add(uv1);
            uvs.Add(uv2);
            uvs.Add(uv3);
        }

        public void AddQuadUV(
            Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector2 uv4)
        {
            //Заносим координаты вершин
            uvs.Add(uv1);
            uvs.Add(uv2);
            uvs.Add(uv3);
            uvs.Add(uv4);
        }

        public void AddQuadUV(
            float uMin, float uMax, float vMin, float vMax)
        {
            //Заносим координаты вершин
            uvs.Add(new(
                uMin, vMin));
            uvs.Add(new(
                uMax, vMin));
            uvs.Add(new(
                uMin, vMax));
            uvs.Add(new(
                uMax, vMax));
        }

        public void AddTriangleUV2(
            Vector2 uv1, Vector2 uv2, Vector2 uv3)
        {
            //Заносим координаты вершин
            uv2s.Add(uv1);
            uv2s.Add(uv2);
            uv2s.Add(uv3);
        }

        public void AddQuadUV2(
            Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector2 uv4)
        {
            //Заносим координаты вершин
            uv2s.Add(uv1);
            uv2s.Add(uv2);
            uv2s.Add(uv3);
            uv2s.Add(uv4);
        }

        public void AddQuadUV2(
            float uMin, float uMax, float vMin, float vMax)
        {
            //Заносим координаты вершин
            uv2s.Add(new(
                uMin, vMin));
            uv2s.Add(new(
                uMax, vMin));
            uv2s.Add(new(
                uMin, vMax));
            uv2s.Add(new(
                uMax, vMax));
        }
    }
}