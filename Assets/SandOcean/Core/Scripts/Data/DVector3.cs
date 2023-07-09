
using System;
using System.Runtime.CompilerServices;

namespace SandOcean
{
    public struct DVector3
    {
        public double x;

        public double y;

        public double z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DVector3(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DVector3(double x, double y)
        {
            this.x = x;
            this.y = y;
            this.z = 0d;
        }

        public double Magnitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return Math.Sqrt(this.x * this.x + this.y * this.y + this.z * this.z);
            }
        }

        public double SqrMagnitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return this.x * this.x + this.y * this.y + this.z * this.z;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Distance(DVector3 a, DVector3 b)
        {
            double num = a.x - b.x;
            double num2 = a.y - b.y;
            double num3 = a.z - b.z;
            return Math.Sqrt(num * num + num2 * num2 + num3 * num3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DVector3 Scale(DVector3 a, DVector3 b)
        {
            return new DVector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ScaleSum(DVector3 a, DVector3 b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DVector3 MoveTowards(
            DVector3 a, 
            DVector3 b, 
            double step,
            out bool isReached)
        {
            //Находим вектор между стартовой позицией и целевой
            DVector3 diff 
                = b - a;

            //Находим его длину
            double magnitude 
                = diff.Magnitude;

            //Если длина меньше шага перемещения
            if (magnitude <= step
                //ИЛИ длина равна нулю
                || magnitude <= double.Epsilon)
            {
                //Отмечаем, что цель достигнута
                isReached
                    = true;

                //Возвращаем целевую позицию
                return b;
            }
            //Иначе
            else
            {
                //Отмечаем, что цель не достигнута
                isReached
                    = false;

                //Возвращаем стартовую позицию, смещённую на шаг перемещения
                return 
                    a + diff / magnitude * step;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DVector3 MoveTowardCeiling(
            DVector3 a, 
            DVector3 b, 
            double step, 
            out bool isReached)
        {
            //Находим вектор между стартовой позицией и целевой
            DVector3 diff 
                = b - a;

            //Находим его длину
            double magnitude 
                = diff.Magnitude;

            //Если длина меньше шага перемещения
            if (magnitude <= step
                //ИЛИ длина равна нулю
                || magnitude <= double.Epsilon)
            {
                //Отмечаем, что цель достигнута
                isReached
                    = true;

                //Возвращаем стартовую позицию, смещённую на шаг перемещения
                return 
                    a + diff / magnitude * step;
            }
            //Иначе
            else
            {
                //Отмечаем, что цель не достигнута
                isReached
                    = false;

                //Возвращаем стартовую позицию, смещённую на шаг перемещения
                return a + diff / magnitude * step;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DVector3 operator +(DVector3 a, DVector3 b)
        {
            return new DVector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DVector3 operator -(DVector3 a, DVector3 b)
        {
            return new DVector3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DVector3 operator -(DVector3 a)
        {
            return new DVector3(0d - a.x, 0d - a.y, 0d - a.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DVector3 operator *(DVector3 a, double d)
        {
            return new DVector3(a.x * d, a.y * d, a.z * d);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DVector3 operator *(double d, DVector3 a)
        {
            return new DVector3(a.x * d, a.y * d, a.z * d);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DVector3 operator /(DVector3 a, double d)
        {
            return new DVector3(a.x / d, a.y / d, a.z / d);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(DVector3 lhs, DVector3 rhs)
        {
            double num = lhs.x - rhs.x;
            double num2 = lhs.y - rhs.y;
            double num3 = lhs.z - rhs.z;
            double num4 = num * num + num2 * num2 + num3 * num3;
            return num4 < 9.99999944E-20d;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(DVector3 lhs, DVector3 rhs)
        {
            return !(lhs == rhs);
        }
    }
}