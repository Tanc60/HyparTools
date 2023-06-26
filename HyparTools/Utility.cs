using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;
using Rhino.Geometry;

namespace HyparTools
{
    class Utility
    {
        public static Vector3d CoordinateTransformation(Vector3d p, Vector3d u, Vector3d v, Vector3d w)
        {
            //定义坐标转换需要的三个矩阵
            double[,] t1 ={
                {u.X,u.Y,u.Z},
                {v.X,v.Y,v.Z},
                {w.X,w.Y,w.Z},
            };
            double[,] t2 ={
                {u.X,v.X,w.X},
                {u.Y,v.Y,w.Y},
                {u.Z,v.Z,w.Z},
            };
            var matrixUVW = DenseMatrix.OfArray(t2);

            double[,] t3 = {
                {p.X},
                {p.Y},
                {p.Z},
            };
            var matrixP = DenseMatrix.OfArray(t3);
            //变换
            var resultMat = matrixUVW.Inverse().Multiply(matrixP);
            //将结果转换为vector3d
            Vector3d resultVec = new Vector3d(resultMat.ToArray()[0, 0], resultMat.ToArray()[1, 0], resultMat.ToArray()[2, 0]);

            return resultVec;
        }
    }
}
