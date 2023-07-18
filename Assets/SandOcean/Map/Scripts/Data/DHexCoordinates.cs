
using System;
using System.Runtime.CompilerServices;

using UnityEngine;

namespace SandOcean.Map
{
    [Serializable]
    public struct DHexCoordinates
    {
        public DHexCoordinates(
            int x,
            int z)
        {
            int oX = x + z / 2;
            if (oX < 0)
            {
                x += MapGenerationData.wrapSize;
            }
            else if(oX >= MapGenerationData.wrapSize)
            {
                x -= MapGenerationData.wrapSize;
            }

            X = x;
            Z = z;
        }

        public int X
        {
            get;
            private set;
        }

        public int Y
        {
            get
            {
                return -X - Z;
            }
        }

        public int Z
        {
            get;
            private set;
        }

        public static DHexCoordinates FromOffsetCoordinates(
            int x,int z)
        {
            return new DHexCoordinates(x - z / 2, z);
        }

        public static DHexCoordinates FromPosition(Vector3 position)
        {
            float x = position.x / MapGenerationData.innerDiameter;
            float y = -x;

            float offset = position.z / (MapGenerationData.outerRadius * 3f);
            x -= offset;
            y -= offset;

            int iX = Mathf.RoundToInt(x);
            int iY = Mathf.RoundToInt(y);
            int iZ = Mathf.RoundToInt(-x - y);

            if (iX + iY + iZ != 0)
            {
                float dX = Mathf.Abs(x - iX);
                float dY = Mathf.Abs(y - iY);
                float dZ = Mathf.Abs(-x - y - iZ);

                if (dX > dY
                    && dX > dZ)
                {
                    iX = -iY - iZ;
                }
                else if (dZ > dY)
                {
                    iZ = -iX - iY;
                }
            }

            return new DHexCoordinates(
                iX,
                iZ);
        }

        public static DHexCoordinates FromNeighbourDirection(
            DHexCoordinates coordinates,
            HexDirection direction)
        {
            DHexCoordinates neighbourCoordinates
                = new();

            switch (direction)
            {
                case HexDirection.NE:
                    {
                        neighbourCoordinates
                            = new(
                                coordinates.X,
                                coordinates.Z + 1);
                    }
                    break;
                case HexDirection.E:
                    {
                        neighbourCoordinates
                            = new(
                                coordinates.X + 1,
                                coordinates.Z);
                    }
                    break;
                case HexDirection.SE:
                    {
                        neighbourCoordinates
                            = new(
                                coordinates.X + 1,
                                coordinates.Z - 1);
                    }
                    break;
                case HexDirection.SW:
                    {
                        neighbourCoordinates
                            = new(
                                coordinates.X,
                                coordinates.Z - 1);
                    }
                    break;
                case HexDirection.W:
                    {
                        neighbourCoordinates
                            = new(
                                coordinates.X - 1,
                                coordinates.Z);
                    }
                    break;
                case HexDirection.NW:
                    {
                        neighbourCoordinates
                            = new(
                                coordinates.X - 1,
                                coordinates.Z + 1);
                    }
                    break;
            }

            return neighbourCoordinates;
        }

        public int DistanceTo(
            DHexCoordinates otherCell)
        {
            int xy 
                = (X < otherCell.X ? otherCell.X - X : X - otherCell.X) 
                + (Y < otherCell.Y ? otherCell.Y - Y : Y - otherCell.Y);

            otherCell.X += MapGenerationData.wrapSize;

            int xyWrapped 
                = (X < otherCell.X ? otherCell.X - X : X - otherCell.X) 
                + (Y < otherCell.Y ? otherCell.Y - Y : Y - otherCell.Y);

            if (xyWrapped < xy)
            {
                xy = xyWrapped;
            }
            else
            {
                otherCell.X -= 2 * MapGenerationData.wrapSize;

                xyWrapped
                    = (X < otherCell.X ? otherCell.X - X : X - otherCell.X) 
                    + (Y < otherCell.Y ? otherCell.Y - Y : Y - otherCell.Y);

                if (xyWrapped < xy)
                {
                    xy = xyWrapped;
                }
            }

            return (xy + (Z < otherCell.Z ? otherCell.Z - Z : Z - otherCell.Z)) / 2;
        }

        public override string ToString()
        {
            return "(" + X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() + ")";
        }

        public string ToStringOnSeparateLines()
        {
            return X.ToString() + "\n" + Y.ToString() + "\n" + Z.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ (Y.GetHashCode() << 2) ^ (Z.GetHashCode() >> 2);
        }
    }
}