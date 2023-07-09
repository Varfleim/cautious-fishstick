
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
        List<Color> colors = new();
        [NonSerialized]
        List<Vector2> uvs = new();
        [NonSerialized]
        List<Vector2> uv2s = new();
        [NonSerialized]
        List<Vector3> terrainTypes = new();

        public Mesh mesh;
        public MeshFilter meshFilter;
        public MeshCollider meshCollider;

        public bool useCollider;
        public bool useColors;
        public bool useUVCoordinates;
        public bool useUV2Coordinates;
        public bool useTerrainTypes;

        public void Clear()
        {
            meshFilter.mesh.Clear();
            vertices = ListPool<Vector3>.Get();
            if (useColors == true)
            {
                colors = ListPool<Color>.Get();
            }
            if (useUVCoordinates == true)
            {
                uvs = ListPool<Vector2>.Get();
            }
            if (useUV2Coordinates == true)
            {
                uv2s = ListPool<Vector2>.Get();
            }
            if (useTerrainTypes == true)
            {
                terrainTypes = ListPool<Vector3>.Get();
            }
            triangles = ListPool<int>.Get();
        }

        public void Apply()
        {
            mesh.SetVertices(vertices);
            ListPool<Vector3>.Add(vertices);
            if (useColors == true)
            {
                mesh.SetColors(colors);
                ListPool<Color>.Add(colors);
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
            if (useTerrainTypes == true)
            {
                mesh.SetUVs(2, terrainTypes);
                ListPool<Vector3>.Add(terrainTypes);
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
            //���������� ������ ������ �������
            int vertexIndex = vertices.Count;

            //������� �������
            vertices.Add(
                SpaceGenerationData.Perturb(v1));
            vertices.Add(
                SpaceGenerationData.Perturb(v2));
            vertices.Add(
                SpaceGenerationData.Perturb(v3));

            //������� ������������
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
            //���������� ������ ������ �������
            int vertexIndex = vertices.Count;

            //������� �������
            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);

            //������� ������������
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
            //���������� ������ ������ �������
            int vertexIndex = vertices.Count;

            //������� �������
            vertices.Add(
                SpaceGenerationData.Perturb(v1));
            vertices.Add(
                SpaceGenerationData.Perturb(v2));
            vertices.Add(
                SpaceGenerationData.Perturb(v3));
            vertices.Add(
                SpaceGenerationData.Perturb(v4));

            //������� ������������
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
            //���������� ������ ������ �������
            int vertexIndex = vertices.Count;

            //������� �������
            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);
            vertices.Add(v4);

            //������� ������������
            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 3);
        }

        public void AddTriangleColor(
            Color c)
        {
            //������� ����� ������
            colors.Add(c);
            colors.Add(c);
            colors.Add(c);
        }

        public void AddTriangleColor(
            Color c1, Color c2, Color c3)
        {
            //������� ����� ������
            colors.Add(c1);
            colors.Add(c2);
            colors.Add(c3);
        }

        public void AddQuadColor(
            Color c1)
        {
            //������� ���� ������
            colors.Add(c1);
            colors.Add(c1);
            colors.Add(c1);
            colors.Add(c1);
        }

        public void AddQuadColor(
            Color c1, Color c2)
        {
            //������� ����� ������
            colors.Add(c1);
            colors.Add(c1);
            colors.Add(c2);
            colors.Add(c2);
        }

        public void AddQuadColor(
            Color c1, Color c2, Color c3, Color c4)
        {
            //������� ����� ������
            colors.Add(c1);
            colors.Add(c2);
            colors.Add(c3);
            colors.Add(c4);
        }

        public void AddTriangleUV(
            Vector2 uv1, Vector2 uv2, Vector2 uv3)
        {
            //������� ���������� ������
            uvs.Add(uv1);
            uvs.Add(uv2);
            uvs.Add(uv3);
        }

        public void AddQuadUV(
            Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector2 uv4)
        {
            //������� ���������� ������
            uvs.Add(uv1);
            uvs.Add(uv2);
            uvs.Add(uv3);
            uvs.Add(uv4);
        }

        public void AddQuadUV(
            float uMin, float uMax, float vMin, float vMax)
        {
            //������� ���������� ������
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
            //������� ���������� ������
            uv2s.Add(uv1);
            uv2s.Add(uv2);
            uv2s.Add(uv3);
        }

        public void AddQuadUV2(
            Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector2 uv4)
        {
            //������� ���������� ������
            uv2s.Add(uv1);
            uv2s.Add(uv2);
            uv2s.Add(uv3);
            uv2s.Add(uv4);
        }

        public void AddQuadUV2(
            float uMin, float uMax, float vMin, float vMax)
        {
            //������� ���������� ������
            uv2s.Add(new(
                uMin, vMin));
            uv2s.Add(new(
                uMax, vMin));
            uv2s.Add(new(
                uMin, vMax));
            uv2s.Add(new(
                uMax, vMax));
        }

        public void AddTriangleTerrainTypes(
            Vector3 types)
        {
            //������� ���� ���������
            terrainTypes.Add(types);
            terrainTypes.Add(types);
            terrainTypes.Add(types);
        }

        public void AddQuadTerrainTypes(
            Vector3 types)
        {
            //������� ���� ���������
            terrainTypes.Add(types);
            terrainTypes.Add(types);
            terrainTypes.Add(types);
            terrainTypes.Add(types);
        }
    }
}