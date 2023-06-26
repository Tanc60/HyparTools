using System;
using System.Collections.Generic;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace HyparTools
{
    public class HyparGen2plus2 : GH_Component
    {
        public Guid Temp1, Temp2;

        /// <summary>
        /// Initializes a new instance of the HyparGen2plus2 class.
        /// </summary>
        public HyparGen2plus2()
          : base("HyparGen2plus2", "HyparGen2plus2", "Generate 2 hypars from 2 hypars (2 hypars on each side)", "HyparTools", "1")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddSurfaceParameter("InputHypar1", "InputHypar1", "input 4 point surface", GH_ParamAccess.item);
            pManager.AddSurfaceParameter("InputHypar2", "InputHypar2", "input 4 point surface", GH_ParamAccess.item);
            pManager.AddNumberParameter("k1", "k1", "k1>0", GH_ParamAccess.item);
            pManager.AddNumberParameter("angle1L", "angle1L", "angle1L range(-1,1)", GH_ParamAccess.item);
            pManager.AddNumberParameter("angle2L", "angle2L", "angle2L range(-1,1)", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Flip", "Flip", "Flip if the vertex of input hypars not in ccw order. default value is fause.", GH_ParamAccess.item, false);

            pManager.AddNumberParameter("Tolerance", "Tolerance", "Tolerance to determine the shared point of two input hypars, default to 0.01.", GH_ParamAccess.item, 0.01);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Hypar1", "Hypar1", "Output hypar surface1", GH_ParamAccess.item);
            pManager.AddSurfaceParameter("Hypar2", "Hypar2", "Output hypar surface2", GH_ParamAccess.item);
            //pManager.AddSurfaceParameter("Hypar3", "Hypar3", "Output hypar surface3", GH_ParamAccess.item);
            //pManager.AddSurfaceParameter("Hypar4", "Hypar4", "Output hypar surface4", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double angle1L = 0, angle2L = 0, k1 = 1;

            //get parameter Data

            if (!DA.GetData("angle1L", ref angle1L)) { return; }
            if (!DA.GetData("angle2L", ref angle2L)) { return; }
            if (!DA.GetData("k1", ref k1)) { return; }

            //get hypar data
            GH_Surface gh_surface1 = null;
            GH_Surface gh_surface2 = null;
            double tolerance = 0.01;
            bool flip = false;
            if (!DA.GetData("InputHypar1", ref gh_surface1)) { return; }
            if (!DA.GetData("InputHypar2", ref gh_surface2)) { return; }
            if (!DA.GetData("Flip", ref flip)) { return; }
            if (!DA.GetData("Tolerance", ref tolerance)) { return; }
            Brep inputBrep1 = gh_surface1.Value;
            Brep inputBrep2 = gh_surface2.Value;
            Hypar hyparLeft = Hypar.HyparOrientation(inputBrep1, 0);
            Hypar hyparRight = Hypar.HyparOrientation(inputBrep2, 0);

            //convert hypars to point3d type
            List<Point3d> hL = new List<Point3d>();
            hL.Add(hyparLeft.P0.Location);
            hL.Add(hyparLeft.P1.Location);
            hL.Add(hyparLeft.P2.Location);
            hL.Add(hyparLeft.P3.Location);

            List<Point3d> hR = new List<Point3d>();
            hR.Add(hyparRight.P0.Location);
            hR.Add(hyparRight.P1.Location);
            hR.Add(hyparRight.P2.Location);
            hR.Add(hyparRight.P3.Location);

            //remap point sequence
            var hyparLeft1 = new Hypar();
            var hyparRight1 = new Hypar();
            var hyparLeft2 = new Hypar();
            var hyparRight2 = new Hypar();

            var hL23 = new Vector3d();
            var hL30 = new Vector3d();
            var hR01 = new Vector3d();
            var hR12 = new Vector3d();

            //判断顺逆时针并调整
            for (int p = 0; p <= 3; p++)
            {
                for (int q = 0; q <= 3; q++)
                {
                    if (hL[p].EpsilonEquals(hR[q], tolerance))
                    {
                        //编号顺逆时针判断
                        hL23 = (Vector3d)hL[p] - (Vector3d)hL[(p + 3) % 4];
                        //hL03 = (Vector3d)hL[(p % 4 + 4) % 4] - (Vector3d)hL[((p + 1) % 4 + 4) % 4];
                        hL30 = (Vector3d)hL[(p + 1) % 4] - (Vector3d)hL[p];

                        hR01 = (Vector3d)hR[q] - (Vector3d)hR[(q + 3) % 4];
                        hR12 = (Vector3d)hR[(q + 1) % 4] - (Vector3d)hR[q];
                        if (Vector3d.Multiply(Vector3d.CrossProduct(hL23, hL30), Vector3d.CrossProduct(hR01, hR12)) < 0)
                        {
                            hyparLeft.P1 = new Point(hL[3]);
                            hyparLeft.P3 = new Point(hL[1]);
                        }
                    }
                }
            }
            //flip orientation if necessary
            if (flip)
            {
                var temp1 = new Point(hyparLeft.P1.Location);
                hyparLeft.P1 = hyparLeft.P3;
                hyparLeft.P3 = temp1;
                var temp2 = new Point(hyparRight.P1.Location);
                hyparRight.P1 = hyparRight.P3;
                hyparRight.P3 = temp2;
            }

            //find common point and sort point order
            for (int i = 0; i <= 3; i++)
            {
                for (int j = 0; j <= 3; j++)
                {
                    if (hL[i].EpsilonEquals(hR[j], tolerance))//公共点判断
                    {
                        //side a
                        hyparLeft1 = Hypar.HyparOrientation(hyparLeft, i - 3);
                        hyparRight1 = Hypar.HyparOrientation(hyparRight, j - 1);
                        //side b
                        hyparLeft2 = Hypar.HyparOrientation(hyparLeft, i - 1);
                        hyparRight2 = Hypar.HyparOrientation(hyparRight, j + 1);
                        break;
                    }
                }
            }

            //remap angle
            // 1. 求三角形中线夹角
            Vector3d vec32 = -hyparLeft1.Vecs[2];
            Vector3d vec12 = hyparRight1.Vecs[1];
            Vector3d vecM = vec32 + vec12;
            double angleM = Vector3d.VectorAngle(vec32, vecM); //中线与一边的夹角
            double angleA = Vector3d.VectorAngle(vec32, vec12); //两边的夹角
                                                                // remap angle
            if (angle1L > 0)
                angle1L = angle1L * angleM + (Math.PI - hyparLeft1.Angles[3] - angleM);
            else
                angle1L = angle1L * (angleA - angleM) + (Math.PI - hyparLeft1.Angles[3] - angleM);

            if (angle2L > 0)
                angle2L = angle2L * hyparRight1.Angles[2];
            else
                angle2L = angle2L * (Math.PI - hyparRight1.Angles[2]);

            var hypar1 = Hypar.HyparGenRight_Absolute(hyparLeft1, k1, angle1L, angle2L);

            var hypartemp = new Hypar(hypar1.P1, hypar1.P2, hypar1.P3, hypar1.P0);

            var hypar2 = Hypar.HyparGenMiddle(hypartemp, hyparRight1);

            var hypar3 = Hypar.HyparGenerator_Absolute(hyparLeft2, k1, angle1L, angle2L);

            var hypartemp2 = new Hypar(hypar3.P3, hypar3.P0, hypar3.P1, hypar3.P2);

            var hypar4 = Hypar.HyparGenMiddle(hyparRight2, hypartemp2);
            /*
            //create angular dimension
            var temp_1 = Hypar.AddAngularDim(hyparLeft1.P3.Location,
                 hypar1.P3.Location,
                 hyparLeft1.P3.Location - hyparLeft1.getVector()[3],
                 "angle1L=\r\n<>",
                 2000);

            var temp_2 = Hypar.AddAngularDim(hyparLeft1.P2.Location,
                hyparLeft1.P2.Location + hyparLeft1.getVector()[1],
                hypar1.P2.Location,
                "angle2L=\r\n<>",
                2000);

            //删除标注
            Rhino.RhinoDoc.ActiveDoc.Objects.Delete(Temp1, true);
            Temp1 = temp_1;
            Rhino.RhinoDoc.ActiveDoc.Objects.Delete(Temp2,true);
            Temp2 = temp_2;
            */
            //set data

            DA.SetData("Hypar1", hypar1.HyparSurface);
            DA.SetData("Hypar2", hypar2.HyparSurface);
            //DA.SetData("Hypar3", hypar3.HyparSurface());
            //DA.SetData("Hypar4", hypar4.HyparSurface());
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return HyparTools.Properties.Resources._2_2;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("0A98E50F-306C-42BE-A299-E2592A6251A4"); }
        }
    }
}