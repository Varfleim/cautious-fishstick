
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
            //���� �������� ��� ����������
            if (regionTexture)
            {
                //�������� � ������
                regionTexture.Reinitialize(x, z);
            }
            else
            {
                //��������� ������ ��������
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

            //���� ������ �������� �� ���������� ��� ����� ������������ �����
            if (regionTextureData == null || regionTextureData.Length != x * z)
            {
                //��������� ������ ��������
                regionTextureData = new Color32[x * z];
            }
            else
            {
                //������� ������ ��������
                for (int a = 0; a < regionTextureData.Length; a++)
                {
                    regionTextureData[a] = new Color32(0, 0, 0, 0);
                }
            }

            //���������, ��� ������ ���� ��������
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
            //���� ������ �������
            int index = region.Index;

            //��������� ������� ��������� � ������������ ������� � ������ ��������
            regionTextureData[index].r = region.IsVisible ? (byte)255 : (byte)0;
            regionTextureData[index].g = region.IsExplored ? (byte)255 : (byte)0;

            //���������, ��� ������ ���� ��������
            isChanged = true;
        }

        public void RefreshTerrain(
            ref CHexRegion region)
        {
            //���� ������� ������ ������� �� ��������
            Color32 data = regionTextureData[region.Index];

            //��������� ���������� �� ������ ����
            data.b = region.IsUnderwater ? (byte)(region.WaterSurfaceY * (255f / 30f)) : (byte)0;

            //��������� ��� ���������
            data.a = (byte)region.TerrainTypeIndex;

            //��������� ������ � ��������
            regionTextureData[region.Index] = data;

            //���������, ��� ������ ���� ��������
            isChanged = true;
        }

        public void ViewElevationChanged(
            ref CHexRegion region)
        {
            //��������� ���������� �� ������ ����
            regionTextureData[region.Index].b = region.IsUnderwater ? (byte)(region.WaterSurfaceY * (255f / 30f)) : (byte)0;

            //���������, ��� ������ ���� ��������
            isChanged = true;
        }

        public void SetMapData(
            ref CHexRegion region,
            float data)
        {
            //�������������� ���������� �� ������ ���� �� ������������ ������
            regionTextureData[region.Index].b = data < 0f ? (byte)0 : (data < 1f ? (byte)(data * 255f) : (byte)255);

            //���������, ��� ������ ���� ��������
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