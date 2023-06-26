using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Grasshopper.Kernel.Types;
using Rhino.DocObjects;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Linq;

namespace HyparTools
{
    /// <summary>
    /// Define the properties of the hypar.
    /// four point 0,1,2,3 in ccw order.
    /// </summary>
    public class Hypar
    {
        public Point P0 { get; set; }
        public Point P1 { get; set; }
        public Point P2 { get; set; }
        public Point P3 { get; set; }

        /// <summary>
        /// vector list of edge 01,12,23,31.
        /// </summary>
        public List<Vector3d> Vecs { get; private set; }

        /// <summary>
        /// angle list in vertex 0,1,2,3.
        /// </summary>
        public double[] Angles { get; private set; }

        /// <summary>
        /// Hypar Vertex, another expression of p1,p2,p3,p4.
        /// </summary>
        public Point[] Vertexes { get; set; }

        public Brep HyparSurface { get; private set; }

        /// <summary>
        /// some tolerence
        /// </summary>
        private const double TOLERANCE = 1;

        /// <summary>
        /// hypar from four corner points.
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        public Hypar(Point p0, Point p1, Point p2, Point p3)
        {
            this.P0 = p0;
            this.P1 = p1;
            this.P2 = p2;
            this.P3 = p3;
            this.Vertexes = new Point[4];
            Vertexes[0] = p0;
            Vertexes[1] = p1;
            Vertexes[2] = p2;
            Vertexes[3] = p3;
            SanityCheck();
            GetVector();
            GetAngle();
            GetHyparSurface();
        }

        /// <summary>
        /// hypar from point list.
        /// </summary>
        /// <param name="points"></param>
        public Hypar(List<Point> points)
        {
            if (points.Count != 4) { return; }
            this.Vertexes = points.ToArray();
            this.P0 = points[0];
            this.P1 = points[1];
            this.P2 = points[2];
            this.P3 = points[3];
            SanityCheck();
            GetVector();
            GetAngle();
            GetHyparSurface();
        }

        public Hypar()
        {
        }

        /// <summary>
        /// check input is sanity.
        /// </summary>
        /// <returns></returns>
        private bool SanityCheck()
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = i + 1; j < 4; j++)
                {
                    if (Vertexes[i].Equals(Vertexes[j].Location.EpsilonEquals(Vertexes[i].Location, TOLERANCE)))
                        throw new ArgumentException("Coincident input points. ");
                }
            }
            return true;
        }

        private Vector3d GetDirection()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// get the brep of the hypar.
        /// </summary>
        private void GetHyparSurface()
        {
            LineCurve line01 = new LineCurve(P0.Location, P1.Location);
            LineCurve line32 = new LineCurve(P3.Location, P2.Location);
            LineCurve line03 = new LineCurve(P0.Location, P3.Location);

            var sweep = new SweepTwoRail();
            var res = sweep.PerformSweep(line01, line32, line03);

            this.HyparSurface = res.First();
            //return NurbsSurface.CreateFromCorners(P0.Location, P1.Location, P2.Location, P3.Location);
        }

        private void GetVector()
        {
            List<Vector3d> vec = new List<Vector3d>();

            vec.Add((Vector3d)P1.Location - (Vector3d)P0.Location);
            vec.Add((Vector3d)P2.Location - (Vector3d)P1.Location);
            vec.Add((Vector3d)P3.Location - (Vector3d)P2.Location);
            vec.Add((Vector3d)P0.Location - (Vector3d)P3.Location);
            this.Vecs = vec;
        }

        private void GetAngle()
        {
            double[] angle = new double[4];
            angle[0] = Vector3d.VectorAngle(this.Vecs[0], -this.Vecs[3]);
            angle[1] = Vector3d.VectorAngle(-this.Vecs[0], this.Vecs[1]);
            angle[2] = Vector3d.VectorAngle(-this.Vecs[1], this.Vecs[2]);
            angle[3] = Vector3d.VectorAngle(-this.Vecs[2], this.Vecs[3]);
            this.Angles = angle;
        }

        /// <summary>
        /// Convert brep from grasshopper input into hypar and sort the order of the the hypar surface.
        /// </summary>
        /// <param name="par_brep">brep from grasshopper input</param>
        /// <param name="par_startNum">the number of point that begins.</param>
        /// <returns></returns>
        public static Hypar HyparOrientation(Brep par_brep, int par_startNum)
        {
            if ((par_startNum % 4 + 4) % 4 == 0)
            {
                return new Hypar(par_brep.Vertices[0], par_brep.Vertices[1], par_brep.Vertices[2], par_brep.Vertices[3]);
            }
            else if ((par_startNum % 4 + 4) % 4 == 1)
            {
                return new Hypar(par_brep.Vertices[1], par_brep.Vertices[2], par_brep.Vertices[3], par_brep.Vertices[0]);
            }
            else if ((par_startNum % 4 + 4) % 4 == 2)
            {
                return new Hypar(par_brep.Vertices[2], par_brep.Vertices[3], par_brep.Vertices[0], par_brep.Vertices[1]);
            }
            else if ((par_startNum % 4 + 4) % 4 == 3)
            {
                return new Hypar(par_brep.Vertices[3], par_brep.Vertices[0], par_brep.Vertices[1], par_brep.Vertices[2]);
            }
            else
                throw new ArgumentException("Conversion failed, par_startNum is not valid.");
        }

        /// <summary>
        /// change the starting number of corner point.
        /// </summary>
        /// <param name="par_hypar"></param>
        /// <param name="par_startNum"></param>
        /// <returns></returns>
        public static Hypar HyparOrientation(Hypar par_hypar, int par_startNum)
        {
            if ((par_startNum % 4 + 4) % 4 == 0)
            {
                return new Hypar(par_hypar.P0, par_hypar.P1, par_hypar.P2, par_hypar.P3);
            }
            else if ((par_startNum % 4 + 4) % 4 == 1)
            {
                return new Hypar(par_hypar.P1, par_hypar.P2, par_hypar.P3, par_hypar.P0);
            }
            else if ((par_startNum % 4 + 4) % 4 == 2)
            {
                return new Hypar(par_hypar.P2, par_hypar.P3, par_hypar.P0, par_hypar.P1);
            }
            else if ((par_startNum % 4 + 4) % 4 == 3)
            {
                return new Hypar(par_hypar.P3, par_hypar.P0, par_hypar.P1, par_hypar.P2);
            }
            else
                throw new ArgumentException("Conversion failed, par_startNum is not valid.");
        }

        /// <summary>
        /// 1+1,generate 1 hypar base on one hypar.
        /// </summary>
        /// <param name="hypar0">input hypar</param>
        /// <param name="k">input coefficient</param>
        /// <param name="angle1">angle coefficient from -1 to 1</param>
        /// <param name="angle2">angle coefficient from -1 to 1</param>
        /// <returns>output hypar</returns>
        public static Hypar HyparGenerator(Hypar hypar0, double k, double angle1, double angle2)
        {
            double tri1_length01, tri1_length12, tri2_length32, tri2_length12;
            Vector3d tri1_vec, tri2_vec, vector12;
            // remap angle
            if (angle1 > 0)
                angle1 = angle1 * (Math.PI - hypar0.Angles[1]);
            else
                angle1 = angle1 * hypar0.Angles[1];

            if (angle2 > 0)
                angle2 = angle2 * hypar0.Angles[2];
            else
                angle2 = angle2 * (Math.PI - hypar0.Angles[2]);

            // solve triangle1
            tri1_length01 = k * hypar0.Vecs[0].Length;
            tri1_length12 = tri1_length01 * Math.Sin(angle1) / (Math.Sin(Math.PI - angle1 - hypar0.Angles[1]));
            vector12 = hypar0.Vecs[1];
            vector12.Unitize();
            tri1_vec = tri1_length12 * vector12;

            // solve triangle2
            tri2_length32 = k * hypar0.Vecs[2].Length;
            tri2_length12 = tri2_length32 * Math.Sin(angle2) / (Math.Sin(hypar0.Angles[2] - angle2));
            vector12 = hypar0.Vecs[1];
            vector12.Unitize();
            tri2_vec = tri2_length12 * vector12;

            //create hypar
            Point p0 = hypar0.P1;
            Point p1 = new Point(hypar0.P1.Location + (k * hypar0.Vecs[0] + tri1_vec));
            Point p2 = new Point(hypar0.P2.Location - (k * hypar0.Vecs[2] - tri2_vec));
            Point p3 = hypar0.P2;
            return new Hypar(p0, p1, p2, p3);
        }

        /// <summary>
        /// Hypar generator in edge 12 with abusolute angle value in radius, no remap is included.
        /// </summary>
        /// <param name="hypar0">hypar type</param>
        /// <param name="k">extend coefficient</param>
        /// <param name="angle1">angle in line01</param>
        /// <param name="angle2">angle in line32</param>
        /// <returns></returns>
        public static Hypar HyparGenerator_Absolute(Hypar hypar0, double k, double angle1, double angle2)
        {
            double tri1_length01, tri1_length12, tri2_length32, tri2_length12;
            Vector3d tri1_vec, tri2_vec, vector12;

            // solve triangle1
            tri1_length01 = k * hypar0.Vecs[0].Length;
            tri1_length12 = tri1_length01 * Math.Sin(angle1) / (Math.Sin(Math.PI - angle1 - hypar0.Angles[1]));
            vector12 = hypar0.Vecs[1];
            vector12.Unitize();
            tri1_vec = tri1_length12 * vector12;

            // solve triangle2
            tri2_length32 = k * hypar0.Vecs[2].Length;
            tri2_length12 = tri2_length32 * Math.Sin(angle2) / (Math.Sin(hypar0.Angles[2] - angle2));
            vector12 = hypar0.Vecs[1];
            vector12.Unitize();
            tri2_vec = tri2_length12 * vector12;

            //create hypar
            Point p0 = hypar0.P1;
            Point p1 = new Point(hypar0.P1.Location + (k * hypar0.Vecs[0] + tri1_vec));
            Point p2 = new Point(hypar0.P2.Location - (k * hypar0.Vecs[2] - tri2_vec));
            Point p3 = hypar0.P2;
            return new Hypar(p0, p1, p2, p3);
        }

        public static Guid AddAngularDim(Point3d pM, Point3d pA, Point3d pB, string anno, double offset)
        {
            pA = pM + offset * (pA - pM) / (pA.DistanceTo(pM));
            pB = pM + offset * (pB - pM) / (pB.DistanceTo(pM));
            var dimstyle = new DimensionStyle();
            var dim = AngularDimension.Create(dimstyle, new Plane(pM, pA, pB), Vector3d.XAxis,
                pM,
                pA,
                pB,
                0.33 * (pM + pA + pB));
            dim.PlainText = anno;
            return Rhino.RhinoDoc.ActiveDoc.Objects.AddAngularDimension(dim);
        }

        /// <summary>
        /// Hypar generator in edge 23 with abusolute angle value in radius, no remap is included.
        /// </summary>
        /// <param name="hypar0">hypar type</param>
        /// <param name="k">extend coefficient</param>
        /// <param name="angle1">angle in line01</param>
        /// <param name="angle2">angle in line32</param>
        /// <returns></returns>
        public static Hypar HyparGenRight_Absolute(Hypar hypar0, double k, double angle1, double angle2)
        {
            Hypar hypar = new Hypar(hypar0.P1, hypar0.P2, hypar0.P3, hypar0.P0);
            Hypar tempHypar = HyparGenerator_Absolute(hypar, k, -angle2, -angle1);
            Point p0 = tempHypar.P3;
            Point p1 = tempHypar.P0;
            Point p2 = tempHypar.P1;
            Point p3 = tempHypar.P2;
            return new Hypar(p0, p1, p2, p3);
        }

        public static Hypar HyparGenRight(Hypar hypar0, double k, double angle1, double angle2)
        {
            Hypar hypar = new Hypar(hypar0.P1, hypar0.P2, hypar0.P3, hypar0.P0);
            Hypar tempHypar = HyparGenerator(hypar, k, -angle2, -angle1);
            Point p0 = tempHypar.P3;
            Point p1 = tempHypar.P0;
            Point p2 = tempHypar.P1;
            Point p3 = tempHypar.P2;
            return new Hypar(p0, p1, p2, p3);
        }

        public static Hypar HyparGenMiddle(Hypar hyparLeft, Hypar hyparRight)
        {
            //应增加内容，sanity check 保证input的两个hypar满足共面条件

            //HL12等比例延长条件
            Vector3d result1 = Utility.CoordinateTransformation(
                hyparRight.Vecs[1],
                hyparLeft.Vecs[3],
                hyparLeft.Vecs[2],
                Vector3d.CrossProduct(hyparLeft.Vecs[3], hyparLeft.Vecs[2])
                );
            //延长点
            Point pA = new Point(hyparLeft.P2.Location - result1.X * hyparLeft.Vecs[1]);

            //HR23等比例延长条件
            Vector3d result2 = Utility.CoordinateTransformation(
                hyparLeft.Vecs[2],
                hyparRight.Vecs[0],
                hyparRight.Vecs[1],
                Vector3d.CrossProduct(hyparRight.Vecs[0], hyparRight.Vecs[1])
                );

            Point pB = new Point(hyparRight.P2.Location + result2.X * hyparRight.Vecs[2]);
            //判别是否共面
            if (result1.Z > TOLERANCE || result2.Z > TOLERANCE)
            {
                throw new ArgumentException("input hypars do not satisified the coplanar criteria");
            }

            ////计算交点

            ////HL 延长线共面条件
            //Vector3d L12 = hyparLeft.getVector()[1];
            //Vector3d L23 = hyparLeft.getVector()[2];
            //var MatrixHL = DenseMatrix.OfArray(new double[,] {
            //    { L12.X , L23.X , Vector3d.CrossProduct(L12,L23).X },
            //    { L12.Y , L23.Y , Vector3d.CrossProduct(L12,L23).Y },
            //    { L12.Z , L23.Z , Vector3d.CrossProduct(L12,L23).Z }
            //});
            ////HR 延长线共面条件
            //Vector3d R23 = hyparRight.getVector()[2];
            //Vector3d R12 = hyparRight.getVector()[1];
            //var MatrixHR = DenseMatrix.OfArray(new double[,] {
            //    { R23.X , R12.X , Vector3d.CrossProduct(R23,R12).X },
            //    { R23.Y , R12.Y , Vector3d.CrossProduct(R23,R12).Y },
            //    { R23.Z , R12.Z , Vector3d.CrossProduct(R23,R12).Z }
            //});
            //// [result1.X,s]*MatrixHL=P[x,y,z]
            //// [result2.X,t]*MatrixHR=P[x,y,z]  解方程组
            //var tempMat = MatrixHL.Inverse().Multiply(MatrixHR);
            ////normalize the euqation
            //tempMat.AsArray()[0, 3] = 0;
            //tempMat.AsArray()[1, 3] = -1;
            //tempMat.AsArray()[2, 3] = 0;

            //两条线段，交点不定
            Vector3d uvecAC = hyparLeft.Vecs[2];
            uvecAC.Unitize();
            Vector3d uvecBC = -hyparRight.Vecs[1];
            uvecBC.Unitize();

            double extendLength = Math.Max(hyparLeft.GetBoundingBoxMaxSize() * 10, hyparRight.GetBoundingBoxMaxSize() * 10);
            Line lineAC = new Line(pA.Location, pA.Location + uvecAC);
            lineAC.Extend(extendLength, extendLength);
            Line lineBC = new Line(pB.Location, pB.Location + uvecBC);
            lineBC.Extend(extendLength, extendLength);

            double a, b;
            Rhino.Geometry.Intersect.Intersection.LineLine(lineAC, lineBC, out a, out b, TOLERANCE, false);

            Point pCatAC = new Point(lineAC.PointAt(a));
            Point pCatBC = new Point(lineBC.PointAt(b));
            if (!pCatAC.Location.EpsilonEquals(pCatBC.Location, TOLERANCE))
            {
                throw new ArgumentException("Find no solution.");
            }

            ////新算法
            ////生成两个hypar的boundingBox
            //List<Point3d> hyparPoints = new List<Point3d>();
            //hyparPoints.Add(hyparLeft.P0.Location);
            //hyparPoints.Add(hyparLeft.P1.Location);
            //hyparPoints.Add(hyparLeft.P2.Location);
            //hyparPoints.Add(hyparLeft.P3.Location);
            //hyparPoints.Add(hyparRight.P0.Location);
            //hyparPoints.Add(hyparRight.P1.Location);
            //hyparPoints.Add(hyparRight.P2.Location);
            //hyparPoints.Add(hyparRight.P3.Location);
            //var hyparBoundingBox = new BoundingBox(hyparPoints);

            ////放大boundingBox
            //var transform=Transform.Scale(hyparBoundingBox.Center, 10.0);
            //hyparBoundingBox.Transform(transform);

            //var planeL = new Plane(hyparLeft.P1.Location, hyparLeft.P2.Location, hyparLeft.P3.Location);
            //var planeR = new Plane(hyparRight.P1.Location, hyparRight.P2.Location, hyparRight.P3.Location);

            //planeL.ExtendThroughBox(hyparBoundingBox, out Interval s, out Interval t);
            //var planeSurfaceL=new PlaneSurface(planeL, s, t);

            //planeR.ExtendThroughBox(hyparBoundingBox, out Interval s1, out Interval t1);
            //var planeSurfaceR = new PlaneSurface(planeR, s1, t1);

            ////平面相交得到curve
            //Rhino.Geometry.Intersect.Intersection.SurfaceSurface(planeSurfaceL, planeSurfaceR, 0.01, out Curve[] targetCurve, out Point3d[] ppp);

            //foreach (Curve curve in targetCurve)
            //{
            //    Rhino.RhinoDoc.ActiveDoc.Objects.AddCurve(curve);
            //}

            //Rhino.RhinoDoc.ActiveDoc.Objects.AddSurface(planeSurfaceL);
            //Rhino.RhinoDoc.ActiveDoc.Objects.AddSurface(planeSurfaceR);

            //Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(pA.Location);
            //Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(pB.Location);
            //Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(pCatAC.Location);
            //Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(pCatBC.Location);
            //Rhino.RhinoDoc.ActiveDoc.Objects.AddLine(lineAC);
            //Rhino.RhinoDoc.ActiveDoc.Objects.AddLine(lineBC);

            return new Hypar(hyparLeft.P3, hyparLeft.P2, pCatAC, hyparRight.P2);
        }

        public double GetBoundingBoxMaxSize()
        {
            List<Point3d> hyparPoints = new List<Point3d>();
            hyparPoints.Add(this.P0.Location);
            hyparPoints.Add(this.P1.Location);
            hyparPoints.Add(this.P2.Location);
            hyparPoints.Add(this.P3.Location);
            var BoundingBox = new BoundingBox(hyparPoints);
            List<double> size = new List<double>() {
                (BoundingBox.Max - BoundingBox.Min).X,
                (BoundingBox.Max - BoundingBox.Min).Y,
                (BoundingBox.Max-BoundingBox.Min).Z
            };
            return size.Max();
        }
    }
}