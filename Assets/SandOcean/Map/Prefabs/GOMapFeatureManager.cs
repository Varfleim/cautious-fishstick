
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
            //Если контейнер существует
            if (container)
            {
                //Удаляем его
                Destroy(container.gameObject);
            }

            //Создаём новый контейнер и прикрепляем его к объекту
            container = new GameObject("FeaturesContainer").transform;
            container.SetParent(
                transform, 
                false);

            //Очищаем меш стен
            walls.Clear();
        }

        public void Apply() 
        {
            //Применяем меш стен
            walls.Apply();
        }

        public void AddFeature(
            ref CHexRegion cell,
            Vector3 position)
        {
            //Если ячейка имеет особый объект
            if (cell.IsSpecial == true)
            {
                //Выходим из функции
                return;
            }

            //Берём шум
            DHexHash hash = SpaceGenerationData.SampleHashGrid(position);

            //Берём префабы города и фермы
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

            //Сохраняем использованный хэш
            float usedHash = hash.a;
            //Если префаб не пуст
            if (prefab)
            {
                //Если другой префаб не пуст
                if (otherPrefab
                    //И хэш B больше A
                    && hash.b > hash.a)
                {
                    //Заменяем префаб
                    prefab = otherPrefab;

                    //Сохраняем хэш фермы
                    usedHash = hash.b;
                }
            }
            //Иначе, если другой префаб не пуст
            else if (otherPrefab)
            {
                //Заменяем префаб
                prefab = otherPrefab;
            }

            //Берём префаб растения
            otherPrefab = PickPrefab(
                ref plantCollections,
                cell.PlantLevel,
                hash.c,
                hash.d);

            //Если префаб не пуст
            if (prefab)
            {
                //Если другой префаб не пуст
                if (otherPrefab
                    //И хэш C больше сохранённого
                    && hash.c > usedHash)
                {
                    //Заменяем префаб
                    prefab = otherPrefab;
                }
            }
            //Иначе, если другой префаб не пуст
            else if (otherPrefab)
            {
                //Заменяем префаб
                prefab = otherPrefab;
            }
            //Иначе
            else
            {
                //Выходим из функции
                return;
            }

            //Создаём объект
            Transform feature = Instantiate(prefab);

            //Перемещаем объект
            position.y += feature.localScale.y * 0.5f;
            feature.localPosition = SpaceGenerationData.Perturb(position);

            //Случайно поворачиваем объект
            feature.localRotation = Quaternion.Euler(
                0f, 360f * hash.e, 0f);

            //Прикрепляем объект к контейнеру
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
            //Если уровень больше нуля
            if (level > 0)
            {
                //Берём массив порогов
                float[] tresholds = SpaceGenerationData.GetFeatureThresholds(
                    level - 1);

                //Для каждого порога в массиве
                for (int a = 0; a < tresholds.Length; a++)
                {
                    //Если хэш больше порога
                    if (hash < tresholds[a])
                    {
                        //Возвращаем указанный префаб
                        return collection[a].Pick(choice);
                    }
                }
            }

            //Возвращаем пустой объект
            return null;
        }

        public void AddWall(
            ref CHexRegion nearCell, DHexEdgeVertices near,
            ref CHexRegion farCell, DHexEdgeVertices far,
            bool hasRiver,
            bool hasRoad)
        {
            //Если наличие стен у ближней ячейки и дальней отличается
            if (nearCell.Walled != farCell.Walled
                //И ячейки не под водой, и ребро между ними - не обрыв
                && nearCell.IsUnderwater == false
                && farCell.IsUnderwater == false
                && SpaceGenerationData.GetEdgeType(nearCell.Elevation, farCell.Elevation) != HexEdgeType.Cliff)
            {
                //Создаём сегмент стены
                AddWallSegment(
                    near.v1, far.v1, near.v2, far.v2);

                //Если имеется река или дорога
                if (hasRiver == true
                    || hasRoad == true)
                {
                    AddWallCap(
                        near.v2, far.v2);
                    AddWallCap(
                        far.v4, near.v4);
                }
                //Иначе
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
            //Если первая ячейка имеет стены
            if (cell1.Walled == true)
            {
                //Если вторая ячейка имеет стены
                if (cell2.Walled == true)
                {
                    //Если третья ячейка не имеет стен
                    if (cell3.Walled == false)
                    {
                        AddWallSegment(
                            ref cell3, c3,
                            ref cell1, c1,
                            ref cell2, c2);
                    }
                }
                //Иначе, если третья ячейка имеет стены
                else if (cell3.Walled == true)
                {
                    AddWallSegment(
                        ref cell2, c2,
                        ref cell3, c3, 
                        ref cell1, c1);
                }
                //Иначе
                else
                {
                    AddWallSegment(
                        ref cell1, c1,
                        ref cell2, c2,
                        ref cell3, c3);
                }
            }
            //Иначе, если вторая ячейка имеет стены
            else if (cell2.Walled == true)
            {
                //Если третья ячейка имеет стены
                if (cell3.Walled == true)
                {
                    AddWallSegment(
                        ref cell1, c1,
                        ref cell2, c2,
                        ref cell3, c3);
                }
                //Иначе
                else
                {
                    AddWallSegment(
                        ref cell2, c2,
                        ref cell3, c3,
                        ref cell1, c1);
                }
            }
            //Иначе, если третья ячейка имеет стены
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
            //Определяем смешение вершин
            nearLeft = SpaceGenerationData.Perturb(nearLeft);
            farLeft = SpaceGenerationData.Perturb(farLeft);
            nearRight = SpaceGenerationData.Perturb(nearRight);
            farRight = SpaceGenerationData.Perturb(farRight);

            //Определяем крайние вершины стены
            Vector3 left = SpaceGenerationData.WallLerp(
                nearLeft, farLeft);
            Vector3 right = SpaceGenerationData.WallLerp(
                nearRight, farRight);

            //Определяем смещение вершин
            Vector3 leftThicknessOffset
                = SpaceGenerationData.WallThicknessOffset(
                    nearLeft, farLeft);
            Vector3 rightThicknessOffset
                = SpaceGenerationData.WallThicknessOffset(
                    nearRight, farRight);
            float leftTop = left.y + SpaceGenerationData.wallHeight;
            float rightTop = right.y + SpaceGenerationData.wallHeight;

            //Определяем вершины стены
            Vector3 v1, v2, v3, v4;
            v1 = v3 = left - leftThicknessOffset;
            v2 = v4 = right - rightThicknessOffset;
            v3.y = leftTop;
            v4.y = rightTop;
            //Заносим квад в меш
            walls.AddQuadUnperturbed(
                v1, v2, v3, v4);

            //Определяем вершины для крыши стены
            Vector3 t1 = v3;
            Vector3 t2 = v4;

            //Смещаем вершины для второго квада
            v1 = v3 = left + leftThicknessOffset;
            v2 = v4 = right + rightThicknessOffset;
            v3.y = leftTop;
            v4.y = rightTop;
            //Заносим квад в меш
            walls.AddQuadUnperturbed(
                v2, v1, v4, v3);

            //Заносим квад крыши в меш
            walls.AddQuadUnperturbed(
                t1, t2, v3, v4);

            //Если требуется добавить башню
            if (addTower == true)
            {
                //Создаём башню
                Transform tower = Instantiate(wallTower);

                //Перемещаем башню
                tower.transform.localPosition = (left + right) * 0.5f;

                //Определяем поворот башни
                Vector3 rightDirection = right - left;
                rightDirection.y = 0f;
                tower.transform.right = rightDirection;

                //Прикрепляем башню к контейнеру
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
            //Если ячейка находится под водой
            if (pivotCell.IsUnderwater == true)
            {
                //Выходим из функции
                return;
            }

            //Определяем, есть ли стены у левого и правого соседей
            bool hasLeftWall = !leftCell.IsUnderwater &&
                SpaceGenerationData.GetEdgeType(pivotCell.Elevation, leftCell.Elevation) != HexEdgeType.Cliff;
            bool hasRighWall = !rightCell.IsUnderwater &&
                SpaceGenerationData.GetEdgeType(pivotCell.Elevation, rightCell.Elevation) != HexEdgeType.Cliff;

            //Если левый сосед имеет стены
            if (hasLeftWall == true)
            {
                //Если правый сосед имеет стены
                if (hasRighWall == true)
                {
                    //Определяем, нужно ли создавать башню
                    bool hasTower = false;
                    //Если высота левого соседа равна высоте правого
                    if (leftCell.Elevation == rightCell.Elevation)
                    {
                        //Берём хэш
                        DHexHash hash = SpaceGenerationData.SampleHashGrid(
                            (pivot + left + right) * (1f / 3f));

                        hasTower = hash.e < SpaceGenerationData.wallTowerTreshhold;
                    }

                    //Добавляем сегмент стены
                    AddWallSegment(
                        pivot, left,
                        pivot, right,
                        hasTower);
                }
                //Иначе, если высота левого соседа меньше высоты правого
                else if(leftCell.Elevation < rightCell.Elevation)
                {
                    AddWallWedge(
                        pivot, left, right);
                }
                //Иначе
                else
                {
                    //Добавляем крышку стены
                    AddWallCap(
                        pivot,
                        left);
                }
            }
            //Иначе, если правая ячейка имеет стену
            else if (hasRighWall == true)
            {
                //Если высота правого соседа меньше высоты левого
                if (rightCell.Elevation < leftCell.Elevation)
                {
                    AddWallWedge(
                        right, pivot, left);
                }
                //Иначе
                else
                {
                    //Добавляем крышку стены
                    AddWallCap(
                        right, pivot);
                }
            }
        }

        void AddWallCap(
            Vector3 near, Vector3 far)
        {
            //Смещаем вершины
            near = SpaceGenerationData.Perturb(near);
            far = SpaceGenerationData.Perturb(far);

            //Определяем центральную точку стены и толщину
            Vector3 center = SpaceGenerationData.WallLerp(
                near, far);
            Vector3 thickness = SpaceGenerationData.WallThicknessOffset(
                near, far);

            //Определяем вершины
            Vector3 v1, v2, v3, v4;
            v1 = v3 = center - thickness;
            v2 = v4 = center + thickness;
            v3.y = v4.y = center.y + SpaceGenerationData.wallHeight;

            //Заносим квад в меш
            walls.AddQuadUnperturbed(
                v1, v2, v3, v4);
        }

        void AddWallWedge(
            Vector3 near, Vector3 far, Vector3 point)
        {
            //Смещаем вершины
            near = SpaceGenerationData.Perturb(near);
            far = SpaceGenerationData.Perturb(far);
            point = SpaceGenerationData.Perturb(point);

            //Определяем центральную точку стены и толщину
            Vector3 center = SpaceGenerationData.WallLerp(
                near, far);
            Vector3 thickness = SpaceGenerationData.WallThicknessOffset(
                near, far);

            //Определяем вершины
            Vector3 v1, v2, v3, v4;
            Vector3 pointTop = point;
            point.y = center.y;

            v1 = v3 = center - thickness;
            v2 = v4 = center + thickness;
            v3.y = v4.y = pointTop.y = center.y + SpaceGenerationData.wallHeight;

            //Заносим объекты в меш
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
            //Смещаем вершины
            roadCenter1 = SpaceGenerationData.Perturb(roadCenter1);
            roadCenter2 = SpaceGenerationData.Perturb(roadCenter2);

            //Создаём мост
            Transform bridge = Instantiate(bridgePrefab);

            //Перемещаем мост
            bridge.localPosition = (roadCenter1 + roadCenter2) * 0.5f;
            bridge.forward = roadCenter2 - roadCenter1;

            //Определяем расстояние
            float length = Vector3.Distance(roadCenter1, roadCenter2);
            //Изменяем длину моста
            bridge.localScale = new(
                1f, 1f, length * (1f / SpaceGenerationData.bridgeDesignLength));

            //Прикрепляем мост к контейнеру
            bridge.SetParent(container, false);
        }

        public void AddSpecialFeature(
            ref CHexRegion cell,
            Vector3 position)
        {
            //Создаём объект
            Transform instance = Instantiate(special[cell.SpecialIndex - 1]);

            //Перемещаем объект
            instance.localPosition = SpaceGenerationData.Perturb(position);

            //Берём хэш
            DHexHash hash = SpaceGenerationData.SampleHashGrid(position);
            //Поворачиваем объект
            instance.localRotation = Quaternion.Euler(
                0f, 360f * hash.e, 0f);

            //Прикрепляем объект к контейнеру
            instance.SetParent(container, false);
        }
    }
}