using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace HyparTools
{
    public class HyparToolsInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "HyparTools";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("dc581d7f-4626-4af1-b0fe-6a9ef13e2042");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
