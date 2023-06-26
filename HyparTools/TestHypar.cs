using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Grasshopper.Kernel.Types;
using Rhino.DocObjects;
using MathNet.Numerics.LinearAlgebra.Double;


namespace HyparTools
{
    /// <summary>
    /// test only, not in production.
    /// </summary>
    public class TestHypar 
    {
        //public List<Point> Points { get; set; }  
        //public List<Point3d> Point3Ds { get; set; }
        public const double TORLERANCE = 1;
        public Point P0 { get; set; }
        public Point P1 { get; set; }
        public Point P2 { get; set; }
        public Point P3 { get; set; }
        
        public Vector3d Vec01 
        { 
            get 
            {
                return new Vector3d(this.P1.Location-this.P0.Location); 
            }
        }
        public Vector3d Vec12
        {
            get
            {
                return new Vector3d(this.P2.Location - this.P1.Location);
            }
        }
        public Vector3d Vec23
        {
            get
            {
                return new Vector3d(this.P3.Location - this.P2.Location);
            }
        }
        public Vector3d Vec30
        {
            get
            {
                return new Vector3d(this.P0.Location - this.P3.Location);
            }
        }

        public TestHypar()
        {
        }

        /// <summary>
        /// create from brep
        /// </summary>
        /// <param name=""></param>
        public TestHypar CreatFromBrep(Brep brep)
        {
            TestHypar testHypar = new TestHypar();
            //
            //需要检测brep 是否符合要求 isValid；仅包含4个角点且互不重叠
            testHypar.P0 = brep.Vertices[0];
            testHypar.P1 = brep.Vertices[1];
            testHypar.P2 = brep.Vertices[2];
            testHypar.P3 = brep.Vertices[3];
            return testHypar;
        }
        /// <summary>
        ///坐标系转换
        /// </summary>
        /// <param name="p">待转换向量</param>
        /// <param name="i">当前坐标系的三个基</param>
        /// <param name="j">当前坐标系的三个基</param>
        /// <param name="k">当前坐标系的三个基</param>
        /// <param name="u">目标坐标系的三个基</param>
        /// <param name="v">目标坐标系的三个基</param>
        /// <param name="w">目标坐标系的三个基</param>
        /// <returns></returns>
        public Vector3d NormalVec()
        {
            return new Vector3d((P0.Location + P2.Location) / 2 - (P3.Location - P1.Location));
        }
        /// <summary>
        /// true if this hypar is a planar surface
        /// </summary>
        /// <returns></returns>
        public bool IsPlanar()
        {
            if (this.NormalVec().EpsilonEquals(Vector3d.Zero,TORLERANCE))
                return true;
            else
                return false;
        }
    }
}
