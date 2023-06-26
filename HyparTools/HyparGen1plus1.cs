using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Grasshopper.Kernel.Types;
using Rhino.DocObjects;


namespace HyparTools
{
    public class HyparGen1plus1 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the HyparGen1to4 class.
        /// </summary>
        public HyparGen1plus1()
          : base("HyparGen1plus1", "HyparGen1plus1", "Generate 1 hypar from 1 hypar", "HyparTools", "1")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddSurfaceParameter("InputHypar", "InputHypar", "input 4 point surface", GH_ParamAccess.item);
            pManager.AddIntegerParameter("startNum", "startNum", "startNumber range(0,1,2,3)", GH_ParamAccess.item);
            pManager.AddNumberParameter("k1", "k1", "k1>0", GH_ParamAccess.item);
            pManager.AddNumberParameter("angle1L", "angle1L", "angle1L range(-1,1)", GH_ParamAccess.item);
            pManager.AddNumberParameter("angle2L", "angle2L", "angle2L range(-1,1)", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("OutputHypar", "OutputHypar", "Output hypar surface", GH_ParamAccess.item);
            //pManager.AddTextParameter("message", "message", "debug message", GH_ParamAccess.item);
            //pManager.AddNumberParameter("test", "test", "debug test", GH_ParamAccess.list);
/*            pManager.AddCircleParameter("cir1", "cir1", "cir1", GH_ParamAccess.item);
            pManager.AddGenericParameter("dim", "dim", "dim", GH_ParamAccess.item);*/
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int startNum=0;
            double angle1L = 0, angle2L = 0, k1 = 1;
            Hypar hypar0;
            Hypar hypar1 = new Hypar();
            GH_Surface gh_surface = null;
            //get Data
            if (!DA.GetData("InputHypar", ref gh_surface)) { return; }
            if (!DA.GetData("startNum", ref startNum)) { return; }
            if (!DA.GetData("angle1L", ref angle1L)) { return; }
            if (!DA.GetData("angle2L", ref angle2L)) { return; }
            if (!DA.GetData("k1", ref k1)) { return; }
            //Brep to Surface
            Brep inputBrep= gh_surface.Value;
            //Create Hypar in specific orientation
            hypar0 = Hypar.HyparOrientation(inputBrep,startNum);
            hypar1 = Hypar.HyparGenerator(hypar0,k1,angle1L,angle2L);
            /*
            Guid guid_now = new Guid();
            Rhino.RhinoDoc.ActiveDoc.Objects.Delete(guid_now, true);
            var dimstyle = new DimensionStyle();
            var dim1 = AngularDimension.Create(dimstyle, Plane.WorldXY, new Vector3d(k1, 0, 0), new Point3d(0, 0, 0), new Point3d(0, 10, 0), new Point3d(10, 0, 0), new Point3d(10, 10, 0));
            guid_now = Rhino.RhinoDoc.ActiveDoc.Objects.AddAngularDimension(dim1);
            */

            /*var func_info = Rhino.NodeInCode.Components.FindComponent("AngularDimension");
            if (func_info == null) { throw new ArgumentException("nothing"); }
            var func = func_info.Delegate as dynamic;
            func(new Point3d(0,0,0), new Point3d(0, 1, 0), new Point3d(1, 0, 0),false,"111",10);*/
          
            //set data
            DA.SetData("OutputHypar", hypar1.HyparSurface);

            
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
                return HyparTools.Properties.Resources._1_1;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("9e7fec80-3147-477d-a6e7-3720cdb82342"); }
        }
    }
}