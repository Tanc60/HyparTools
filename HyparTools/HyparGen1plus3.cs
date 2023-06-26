using System;
using System.Collections.Generic;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace HyparTools
{
    public class HyparGen1plus3 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the HyparGen1plus3 class.
        /// </summary>
        public HyparGen1plus3()
          : base("HyparGen1plus3", "HyparGen1plus3", "Generate 3 hypar from 1 hypar", "HyparTools", "1")
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
        }
    

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //pManager.AddSurfaceParameter("Hypar0", "Hypar0", "Output hypar surface0", GH_ParamAccess.item);
            pManager.AddBrepParameter("Hypar1", "Hypar1", "Output hypar surface1", GH_ParamAccess.item);
            pManager.AddBrepParameter("Hypar2", "Hypar2", "Output hypar surface2", GH_ParamAccess.item);
            pManager.AddBrepParameter("Hypar3", "Hypar3", "Output hypar surface3", GH_ParamAccess.item);
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
            Hypar hypar0;
            Hypar hypar1 = new Hypar();
            Hypar hypar2 = new Hypar();
            Hypar hypar3 = new Hypar();
            GH_Surface gh_surface = null;

            //get Data
            if (!DA.GetData("InputHypar", ref gh_surface)) { return; }
            if (!DA.GetData("startNum", ref startNum)) { return; }
            if (!DA.GetData("isSymmetric", ref isSymmetric)) { return; }

            if (!DA.GetData("angle1L", ref angle1L)) { return; }
            if (!DA.GetData("angle2L", ref angle2L)) { return; }
            if (!DA.GetData("k1", ref k1)) { return; }

            if (!DA.GetData("angle1R", ref angle1R)) { return; }
            if (!DA.GetData("angle2R", ref angle2R)) { return; }
            if (!DA.GetData("k2", ref k2)) { return; }

            // is symmetric
            if (isSymmetric == true)
            {
                k2 = k1;
                angle1R = angle1L;
                angle2R = angle2L;
            }

            //Brep to Surface
            Brep inputBrep = gh_surface.Value;

            //Create Hypar in specific orientation
            hypar0 = Hypar.HyparOrientation(inputBrep, startNum);
            hypar1 = Hypar.HyparGenerator(hypar0, k1, angle1L, angle2L);
            hypar2 = Hypar.HyparGenRight(hypar0, k2, angle1R, angle2R);
            hypar3 = Hypar.HyparGenMiddle(hypar1,hypar2);
            //set data
            //DA.SetData("Hypar0", hypar0.HyparSurface());
            DA.SetData("Hypar1", hypar1.HyparSurface);
            DA.SetData("Hypar2", hypar2.HyparSurface);
            DA.SetData("Hypar3", hypar3.HyparSurface);
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
                return HyparTools.Properties.Resources._1_3;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("16B40A45-FB7F-48A2-AC84-0BC7CEFA51F3"); }
        }
    }
}