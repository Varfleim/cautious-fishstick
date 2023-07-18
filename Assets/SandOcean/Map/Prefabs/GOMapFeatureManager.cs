
using System;
using System.Collections.Generic;

using UnityEngine;

namespace SandOcean.Map
{
    public class GOMapFeatureManager : MonoBehaviour
    {
        public Transform container;

        public GOMapMesh walls;
        public Transform wallTower;

        public DHexFeatureCollection[] urbanCollections;
        public DHexFeatureCollection[] farmCollections;
        public DHexFeatureCollection[] plantCollections;
        public Transform[] special;

        public Transform bridgePrefab;

        public void Clear() 
        {
            //���� ��������� ����������
            if (container)
            {
                //������� ���
                Destroy(container.gameObject);
            }

            //������ ����� ��������� � ����������� ��� � �������
            container = new GameObject("FeaturesContainer").transform;
            container.SetParent(
                transform, 
                false);

            //������� ��� ����
            walls.Clear();
        }

        public void Apply() 
        {
            //��������� ��� ����
            walls.Apply();
        }

        public void AddFeature(
            ref CHexRegion cell,
            Vector3 position)
        {
            //���� ������ ����� ������ ������
            if (cell.IsSpecial == true)
            {
                //������� �� �������
                return;
            }

            //���� ���
            DHexHash hash = MapGenerationData.SampleHashGrid(position);

            //���� ������� ������ � �����
            Transform prefab = PickPrefab(
                ref urbanCollections,
                cell.UrbanLevel,
                hash.a,
                hash.d);
            Transform otherPrefab = PickPrefab(
                ref farmCollections,
                cell.FarmLevel,
                hash.b,
                hash.d);

            //��������� �������������� ���
            float usedHash = hash.a;
            //���� ������ �� ����
            if (prefab)
            {
                //���� ������ ������ �� ����
                if (otherPrefab
                    //� ��� B ������ A
                    && hash.b > hash.a)
                {
                    //�������� ������
                    prefab = otherPrefab;

                    //��������� ��� �����
                    usedHash = hash.b;
                }
            }
            //�����, ���� ������ ������ �� ����
            else if (otherPrefab)
            {
                //�������� ������
                prefab = otherPrefab;
            }

            //���� ������ ��������
            otherPrefab = PickPrefab(
                ref plantCollections,
                cell.PlantLevel,
                hash.c,
                hash.d);

            //���� ������ �� ����
            if (prefab)
            {
                //���� ������ ������ �� ����
                if (otherPrefab
                    //� ��� C ������ �����������
                    && hash.c > usedHash)
                {
                    //�������� ������
                    prefab = otherPrefab;
                }
            }
            //�����, ���� ������ ������ �� ����
            else if (otherPrefab)
            {
                //�������� ������
                prefab = otherPrefab;
            }
            //�����
            else
            {
                //������� �� �������
                return;
            }

            //������ ������
            Transform feature = Instantiate(prefab);

            //���������� ������
            position.y += feature.localScale.y * 0.5f;
            feature.localPosition = MapGenerationData.Perturb(position);

            //�������� ������������ ������
            feature.localRotation = Quaternion.Euler(
                0f, 360f * hash.e, 0f);

            //����������� ������ � ����������
            feature.SetParent(
                container,
                false);
        }

        Transform PickPrefab(
            ref DHexFeatureCollection[] collection,
            int level,
            float hash,
            float choice)
        {
            //���� ������� ������ ����
            if (level > 0)
            {
                //���� ������ �������
                float[] tresholds = MapGenerationData.GetFeatureThresholds(
                    level - 1);

                //��� ������� ������ � �������
                for (int a = 0; a < tresholds.Length; a++)
                {
                    //���� ��� ������ ������
                    if (hash < tresholds[a])
                    {
                        //���������� ��������� ������
                        return collection[a].Pick(choice);
                    }
                }
            }

            //���������� ������ ������
            return null;
        }

        public void AddWall(
            ref CHexRegion nearCell, DHexEdgeVertices near,
            ref CHexRegion farCell, DHexEdgeVertices far,
            bool hasRiver,
            bool hasRoad)
        {
            //���� ������� ���� � ������� ������ � ������� ����������
            if (nearCell.Walled != farCell.Walled
                //� ������ �� ��� �����, � ����� ����� ���� - �� �����
                && nearCell.IsUnderwater == false
                && farCell.IsUnderwater == false
                && MapGenerationData.GetEdgeType(nearCell.Elevation, farCell.Elevation) != HexEdgeType.Cliff)
            {
                //������ ������� �����
                AddWallSegment(
                    near.v1, far.v1, near.v2, far.v2);

                //���� ������� ���� ��� ������
                if (hasRiver == true
                    || hasRoad == true)
                {
                    AddWallCap(
                        near.v2, far.v2);
                    AddWallCap(
                        far.v4, near.v4);
                }
                //�����
                else
                {
                    AddWallSegment(
                        near.v2, far.v2, near.v3, far.v3);
                    AddWallSegment(
                        near.v3, far.v3, near.v4, far.v4);
                }

                AddWallSegment(
                    near.v4, far.v4, near.v5, far.v5);
            }
        }

        public void AddWall(
            ref CHexRegion cell1, Vector3 c1,
            ref CHexRegion cell2, Vector3 c2,
            ref CHexRegion cell3, Vector3 c3)
        {
            //���� ������ ������ ����� �����
            if (cell1.Walled == true)
            {
                //���� ������ ������ ����� �����
                if (cell2.Walled == true)
                {
                    //���� ������ ������ �� ����� ����
                    if (cell3.Walled == false)
                    {
                        AddWallSegment(
                            ref cell3, c3,
                            ref cell1, c1,
                            ref cell2, c2);
                    }
                }
                //�����, ���� ������ ������ ����� �����
                else if (cell3.Walled == true)
                {
                    AddWallSegment(
                        ref cell2, c2,
                        ref cell3, c3, 
                        ref cell1, c1);
                }
                //�����
                else
                {
                    AddWallSegment(
                        ref cell1, c1,
                        ref cell2, c2,
                        ref cell3, c3);
                }
            }
            //�����, ���� ������ ������ ����� �����
            else if (cell2.Walled == true)
            {
                //���� ������ ������ ����� �����
                if (cell3.Walled == true)
                {
                    AddWallSegment(
                        ref cell1, c1,
                        ref cell2, c2,
                        ref cell3, c3);
                }
                //�����
                else
                {
                    AddWallSegment(
                        ref cell2, c2,
                        ref cell3, c3,
                        ref cell1, c1);
                }
            }
            //�����, ���� ������ ������ ����� �����
            else if (cell3.Walled == true)
            {
                AddWallSegment(
                    ref cell3, c3,
                    ref cell1, c1,
                    ref cell2, c2);
            }
        }

        void AddWallSegment(
            Vector3 nearLeft, Vector3 farLeft, Vector3 nearRight, Vector3 farRight,
            bool addTower = false)
        {
            //���������� �������� ������
            nearLeft = MapGenerationData.Perturb(nearLeft);
            farLeft = MapGenerationData.Perturb(farLeft);
            nearRight = MapGenerationData.Perturb(nearRight);
            farRight = MapGenerationData.Perturb(farRight);

            //���������� ������� ������� �����
            Vector3 left = MapGenerationData.WallLerp(
                nearLeft, farLeft);
            Vector3 right = MapGenerationData.WallLerp(
                nearRight, farRight);

            //���������� �������� ������
            Vector3 leftThicknessOffset
                = MapGenerationData.WallThicknessOffset(
                    nearLeft, farLeft);
            Vector3 rightThicknessOffset
                = MapGenerationData.WallThicknessOffset(
                    nearRight, farRight);
            float leftTop = left.y + MapGenerationData.wallHeight;
            float rightTop = right.y + MapGenerationData.wallHeight;

            //���������� ������� �����
            Vector3 v1, v2, v3, v4;
            v1 = v3 = left - leftThicknessOffset;
            v2 = v4 = right - rightThicknessOffset;
            v3.y = leftTop;
            v4.y = rightTop;
            //������� ���� � ���
            walls.AddQuadUnperturbed(
                v1, v2, v3, v4);

            //���������� ������� ��� ����� �����
            Vector3 t1 = v3;
            Vector3 t2 = v4;

            //������� ������� ��� ������� �����
            v1 = v3 = left + leftThicknessOffset;
            v2 = v4 = right + rightThicknessOffset;
            v3.y = leftTop;
            v4.y = rightTop;
            //������� ���� � ���
            walls.AddQuadUnperturbed(
                v2, v1, v4, v3);

            //������� ���� ����� � ���
            walls.AddQuadUnperturbed(
                t1, t2, v3, v4);

            //���� ��������� �������� �����
            if (addTower == true)
            {
                //������ �����
                Transform tower = Instantiate(wallTower);

                //���������� �����
                tower.transform.localPosition = (left + right) * 0.5f;

                //���������� ������� �����
                Vector3 rightDirection = right - left;
                rightDirection.y = 0f;
                tower.transform.right = rightDirection;

                //����������� ����� � ����������
                tower.transform.SetParent(
                    container,
                    false);
            }
        }

        void AddWallSegment(
            ref CHexRegion pivotCell, Vector3 pivot,
            ref CHexRegion leftCell, Vector3 left,
            ref CHexRegion rightCell, Vector3 right)
        {
            //���� ������ ��������� ��� �����
            if (pivotCell.IsUnderwater == true)
            {
                //������� �� �������
                return;
            }

            //����������, ���� �� ����� � ������ � ������� �������
            bool hasLeftWall = !leftCell.IsUnderwater &&
                MapGenerationData.GetEdgeType(pivotCell.Elevation, leftCell.Elevation) != HexEdgeType.Cliff;
            bool hasRighWall = !rightCell.IsUnderwater &&
                MapGenerationData.GetEdgeType(pivotCell.Elevation, rightCell.Elevation) != HexEdgeType.Cliff;

            //���� ����� ����� ����� �����
            if (hasLeftWall == true)
            {
                //���� ������ ����� ����� �����
                if (hasRighWall == true)
                {
                    //����������, ����� �� ��������� �����
                    bool hasTower = false;
                    //���� ������ ������ ������ ����� ������ �������
                    if (leftCell.Elevation == rightCell.Elevation)
                    {
                        //���� ���
                        DHexHash hash = MapGenerationData.SampleHashGrid(
                            (pivot + left + right) * (1f / 3f));

                        hasTower = hash.e < MapGenerationData.wallTowerTreshhold;
                    }

                    //��������� ������� �����
                    AddWallSegment(
                        pivot, left,
                        pivot, right,
                        hasTower);
                }
                //�����, ���� ������ ������ ������ ������ ������ �������
                else if(leftCell.Elevation < rightCell.Elevation)
                {
                    AddWallWedge(
                        pivot, left, right);
                }
                //�����
                else
                {
                    //��������� ������ �����
                    AddWallCap(
                        pivot,
                        left);
                }
            }
            //�����, ���� ������ ������ ����� �����
            else if (hasRighWall == true)
            {
                //���� ������ ������� ������ ������ ������ ������
                if (rightCell.Elevation < leftCell.Elevation)
                {
                    AddWallWedge(
                        right, pivot, left);
                }
                //�����
                else
                {
                    //��������� ������ �����
                    AddWallCap(
                        right, pivot);
                }
            }
        }

        void AddWallCap(
            Vector3 near, Vector3 far)
        {
            //������� �������
            near = MapGenerationData.Perturb(near);
            far = MapGenerationData.Perturb(far);

            //���������� ����������� ����� ����� � �������
            Vector3 center = MapGenerationData.WallLerp(
                near, far);
            Vector3 thickness = MapGenerationData.WallThicknessOffset(
                near, far);

            //���������� �������
            Vector3 v1, v2, v3, v4;
            v1 = v3 = center - thickness;
            v2 = v4 = center + thickness;
            v3.y = v4.y = center.y + MapGenerationData.wallHeight;

            //������� ���� � ���
            walls.AddQuadUnperturbed(
                v1, v2, v3, v4);
        }

        void AddWallWedge(
            Vector3 near, Vector3 far, Vector3 point)
        {
            //������� �������
            near = MapGenerationData.Perturb(near);
            far = MapGenerationData.Perturb(far);
            point = MapGenerationData.Perturb(point);

            //���������� ����������� ����� ����� � �������
            Vector3 center = MapGenerationData.WallLerp(
                near, far);
            Vector3 thickness = MapGenerationData.WallThicknessOffset(
                near, far);

            //���������� �������
            Vector3 v1, v2, v3, v4;
            Vector3 pointTop = point;
            point.y = center.y;

            v1 = v3 = center - thickness;
            v2 = v4 = center + thickness;
            v3.y = v4.y = pointTop.y = center.y + MapGenerationData.wallHeight;

            //������� ������� � ���
            walls.AddQuadUnperturbed(
                v1, point, v3, pointTop);
            walls.AddQuadUnperturbed(
                point, v2, pointTop, v4);
            walls.AddTriangleUnperturbed(
                pointTop, v3, v4);
        }

        public void AddBridge(
            Vector3 roadCenter1, Vector3 roadCenter2)
        {
            //������� �������
            roadCenter1 = MapGenerationData.Perturb(roadCenter1);
            roadCenter2 = MapGenerationData.Perturb(roadCenter2);

            //������ ����
            Transform bridge = Instantiate(bridgePrefab);

            //���������� ����
            bridge.localPosition = (roadCenter1 + roadCenter2) * 0.5f;
            bridge.forward = roadCenter2 - roadCenter1;

            //���������� ����������
            float length = Vector3.Distance(roadCenter1, roadCenter2);
            //�������� ����� �����
            bridge.localScale = new(
                1f, 1f, length * (1f / MapGenerationData.bridgeDesignLength));

            //����������� ���� � ����������
            bridge.SetParent(container, false);
        }

        public void AddSpecialFeature(
            ref CHexRegion cell,
            Vector3 position)
        {
            //������ ������
            Transform instance = Instantiate(special[cell.SpecialIndex - 1]);

            //���������� ������
            instance.localPosition = MapGenerationData.Perturb(position);

            //���� ���
            DHexHash hash = MapGenerationData.SampleHashGrid(position);
            //������������ ������
            instance.localRotation = Quaternion.Euler(
                0f, 360f * hash.e, 0f);

            //����������� ������ � ����������
            instance.SetParent(container, false);
        }
    }
}