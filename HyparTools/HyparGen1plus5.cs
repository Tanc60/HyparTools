using System;
using System.Collections.Generic;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace HyparTools
{
    public class HyparGen1plus5 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the HyparGen1plus5 class.
        /// </summary>
        public HyparGen1plus5()
            : base("HyparGen1plus5", "HyparGen1plus5", "Generate 5 hypar from 1 hypar", "HyparTools", "1")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddSurfaceParameter("InputHypar", "InputHypar", "input 4 point surface", GH_ParamAccess.item);
            pManager.AddIntegerParameter("startNum", "startNum", "startNumber range(0,1,2,3)", GH_ParamAccess.item);
            pManager.AddBooleanParameter("isSymmetric", "isSymmetric", "Generate symmetirc hypar or not (bool)", GH_ParamAccess.item);

            pManager.AddNumberParameter("k1", "k1", "k1>0", GH_ParamAccess.item);
            pManager.AddNumberParameter("angle1L", "angle1L", "angle1L range(-1,1)", GH_ParamAccess.item);
            pManager.AddNumberParameter("angle2L", "angle2L", "angle2L range(-1,1)", GH_ParamAccess.item);

            pManager.AddNumberParameter("k2", "k2", "k2>0", GH_ParamAccess.item);
            pManager.AddNumberParameter("angle1R", "angle1R", "angle1R range(-1,1)", GH_ParamAccess.item);
            pManager.AddNumberParameter("angle2R", "angle2R", "angle2R range(-1,1)", GH_ParamAccess.item);

            pManager.AddNumberParameter("k3", "k3", "k3>0", GH_ParamAccess.item);
            pManager.AddNumberParameter("angle3L", "angle3L", "angle3L range(-1,1)", GH_ParamAccess.item);
            pManager.AddNumberParameter("angle4L", "angle4L", "angle4L range(-1,1)", GH_ParamAccess.item);

            pManager.AddNumberParameter("k4", "k4", "k4>0", GH_ParamAccess.item);
            pManager.AddNumberParameter("angle3R", "angle3R", "angle3R range(-1,1)", GH_ParamAccess.item);
            pManager.AddNumberParameter("angle4R", "angle4R", "angle4R range(-1,1)", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Hypar0", "Hypar0", "Output hypar surface0", GH_ParamAccess.item);
            pManager.AddBrepParameter("Hypar1", "Hypar1", "Output hypar surface1", GH_ParamAccess.item);
            pManager.AddBrepParameter("Hypar2", "Hypar2", "Output hypar surface2", GH_ParamAccess.item);
            pManager.AddBrepParameter("Hypar3", "Hypar3", "Output hypar surface3", GH_ParamAccess.item);
            pManager.AddBrepParameter("Hypar4", "Hypar4", "Output hypar surface4", GH_ParamAccess.item);
            pManager.AddBrepParameter("Hypar5", "Hypar5", "Output hypar surface5", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int startNum = 0;
            bool isSymmetric = false;
            double angle1L = 0, angle2L = 0, k1 = 1;
            double angle1R = 0, angle2R = 0, k2 = 1;
            double angle3L = 0, angle4L = 0, k3 = 1;
            double angle3R = 0, angle4R = 0, k4 = 1;
            Hypar hypar0;
            Hypar hypar1 = new Hypar();
            Hypar hypar2 = new Hypar();
            Hypar hypar3 = new Hypar();
            Hypar hypar4 = new Hypar();
            Hypar hypar5 = new Hypar();

            GH_Surface gh_surface = null;

            //get Data
            if (!DA.GetData("InputHypar", ref gh_surface)) { return; }
            if (!DA.GetData("startNum", ref startNum)) { return; }
            if (!DA.GetData("isSymmetric", ref isSymmetric)) { return; }

            if (startNum < 0 || startNum > 3)
            {
                DA.SetData("message", "startNum out of range");
                return;
            }
            if (!DA.GetData("angle1L", ref angle1L)) { return; }
            if (!DA.GetData("angle2L", ref angle2L)) { return; }
            if (!DA.GetData("k1", ref k1)) { return; }

            if (!DA.GetData("angle1R", ref angle1R)) { return; }
            if (!DA.GetData("angle2R", ref angle2R)) { return; }
            if (!DA.GetData("k2", ref k2)) { return; }

            if (!DA.GetData("angle3L", ref angle3L)) { return; }
            if (!DA.GetData("angle3R", ref angle3R)) { return; }
            if (!DA.GetData("k3", ref k3)) { return; }

            if (!DA.GetData("angle4L", ref angle4L)) { return; }
            if (!DA.GetData("angle4R", ref angle4R)) { return; }
            if (!DA.GetData("k4", ref k4)) { return; }

            // is symmetric
            if (isSymmetric == true)
            {
                k2 = k1;
                k4 = k3;
                angle1R = angle1L;
                angle2R = angle2L;
                angle3R = angle3L;
                angle4R = angle4L;
            }
            // hypar 0 generation
            Brep inputBrep = gh_surface.Value;
            hypar0 = Hypar.HyparOrientation(inputBrep, startNum);

            //hypar 1 generation
            //angle2L remap
            angle2L = (angle2L - 1) * 0.5;
            hypar1 = Hypar.HyparGenerator(hypar0, k1, angle1L, angle2L);

            //hypar 2 generation
            //angle2R remap
            angle2R = (angle2R - 1) * 0.5;
            hypar2 = Hypar.HyparGenRight(hypar0, k2, angle1R, angle2R);

            //hypar 3 generation
            //angle4L remap
            if (angle4L >= 0)
                angle4L = ((angle4L - 1) * hypar0.Angles[2]) / (Math.PI - hypar1.Angles[3]);
            else
                angle4L = (angle4L * (Math.PI - hypar0.Angles[2] - hypar1.Angles[3]) - hypar0.Angles[2]) / (Math.PI - hypar1.Angles[3]);
            var hypartemp = new Hypar();
            hypartemp.P0 = hypar1.P1;
            hypartemp.P1 = hypar1.P2;
            hypartemp.P2 = hypar1.P3;
            hypartemp.P3 = hypar1.P0;
            hypartemp = Hypar.HyparGenerator(hypartemp, k3, angle3L, angle4L);
            hypar3.P0 = hypartemp.P3;
            hypar3.P1 = hypartemp.P0;
            hypar3.P2 = hypartemp.P1;
            hypar3.P3 = hypartemp.P2;

            //hypar 4  generation
            //angle 4R remap
            if (angle4R >= 0)
                angle4R = ((angle4R - 1) * hypar0.Angles[2]) / (Math.PI - hypar2.Angles[1]);
            else
                angle4R = (angle4R * (Math.PI - hypar0.Angles[2] - hypar2.Angles[1]) - hypar0.Angles[2]) / (Math.PI - hypar2.Angles[1]);
            hypar4 = Hypar.HyparGenerator(hypar2, k4, -angle4R, -angle3R);

            //hypar 5 generation
            var hyparLeft = new Hypar();
            hyparLeft.P0 = hypar3.P1;
            hyparLeft.P1 = hypar3.P2;
            hyparLeft.P2 = hypar3.P3;
            hyparLeft.P3 = hypar3.P0;

            var hyparRight = new Hypar();
            hyparRight.P0 = hypar4.P3;
            hyparRight.P1 = hypar4.P0;
            hyparRight.P2 = hypar4.P1;
            hyparRight.P3 = hypar4.P2;

            hypar5 = Hypar.HyparGenMiddle(hyparLeft, hyparRight);

            //set data
            DA.SetData("Hypar0", hypar0.HyparSurface);
            DA.SetData("Hypar1", hypar1.HyparSurface);
            DA.SetData("Hypar2", hypar2.HyparSurface);
            DA.SetData("Hypar3", hypar3.HyparSurface);
            DA.SetData("Hypar4", hypar4.HyparSurface);
            DA.SetData("Hypar5", hypar5.HyparSurface);
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
                return HyparTools.Properties.Resources._1_5;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("A24B61D8-EB36-49C5-BED7-DF2D2843B983"); }
        }
    }
}