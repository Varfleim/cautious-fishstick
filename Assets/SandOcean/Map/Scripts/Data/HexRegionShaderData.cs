
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace SandOcean.Map
{
    public class HexRegionShaderData : MonoBehaviour
    {
        Texture2D regionTexture;

        /// <summary>
        /// R-component - visibility
        /// G-component - explored
        /// B-component - water surface
        /// A-component - terrain type
        /// </summary>
        Color32[] regionTextureData;

        int cellHighlightingId = Shader.PropertyToID("_CellHighlighting");

        public bool isChanged;

        public void Initialize(
            int x, int z)
        {
            //Если текстура уже существует
            if (regionTexture)
            {
                //Изменяем её размер
                regionTexture.Reinitialize(x, z);
            }
            else
            {
                //Заполняем данные текстуры
                regionTexture = new Texture2D(
                    x, z,
                    TextureFormat.RGBA32,
                    false,
                    true);
                regionTexture.filterMode = FilterMode.Point;
                regionTexture.wrapModeU = TextureWrapMode.Repeat;
                regionTexture.wrapModeV = TextureWrapMode.Clamp;

                Shader.SetGlobalTexture(
                    "_HexCellData", regionTexture);
            }

            Shader.SetGlobalVector(
                "_HexCellData_TexelSize",
                new Vector4(1f / x, 1f / z, x, z));

            //Если данные текстуры не существуют или имеют неподходящую длину
            if (regionTextureData == null || regionTextureData.Length != x * z)
            {
                //Обновляем данные текстуры
                regionTextureData = new Color32[x * z];
            }
            else
            {
                //Очищаем данные текстуры
                for (int a = 0; a < regionTextureData.Length; a++)
                {
                    regionTextureData[a] = new Color32(0, 0, 0, 0);
                }
            }

            //Указываем, что данные были изменены
            isChanged = true;
        }
        public void Refresh()
        {
            regionTexture.SetPixels32(regionTextureData);
            regionTexture.Apply();
        }

        public void RefreshVisibility(
            ref CHexRegion region)
        {
            //Берём индекс региона
            int index = region.Index;

            //Обновляем уровень видимости и исследования региона в данных текстуры
            regionTextureData[index].r = region.IsVisible ? (byte)255 : (byte)0;
            regionTextureData[index].g = region.IsExplored ? (byte)255 : (byte)0;

            //Указываем, что данные были изменены
            isChanged = true;
        }

        public void RefreshTerrain(
            ref CHexRegion region)
        {
            //Берём текущие данные региона из текстуры
            Color32 data = regionTextureData[region.Index];

            //Обновляем затемнение от уровня воды
            data.b = region.IsUnderwater ? (byte)(region.WaterSurfaceY * (255f / 30f)) : (byte)0;

            //Обновляем тип ландшафта
            data.a = (byte)region.TerrainTypeIndex;

            //Обновляем данные в текстуре
            regionTextureData[region.Index] = data;

            //Указываем, что данные были изменены
            isChanged = true;
        }

        public void ViewElevationChanged(
            ref CHexRegion region)
        {
            //Обновляем затемнение от уровня воды
            regionTextureData[region.Index].b = region.IsUnderwater ? (byte)(region.WaterSurfaceY * (255f / 30f)) : (byte)0;

            //Указываем, что данные были изменены
            isChanged = true;
        }

        public void SetMapData(
            ref CHexRegion region,
            float data)
        {
            //Перезаписываем затемнение от уровня воды на произвольные данные
            regionTextureData[region.Index].b = data < 0f ? (byte)0 : (data < 1f ? (byte)(data * 255f) : (byte)255);

            //Указываем, что данные были изменены
            isChanged = true;
        }

        public void UpdateRegionHighlightData(
            ref CHexRegion region)
        {
            Shader.SetGlobalVector(
                cellHighlightingId,
                new Vector4(
                    region.coordinates.X, region.coordinates.Z,
                    1,
                    105));
        }

        public void ClearRegionHighlightData()
        {
            Shader.SetGlobalVector(
                cellHighlightingId,
                new Vector4(
                    0, 0,
                    -1,
                    0));
        }
    }
}