using BCD.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCD.Parse
{
    class FileParse:IFileParse
    {

        string _TypeName;
        protected string TypeName
        {
            set { _TypeName = value; }
            get { return String.Format("BCD.FileTypes.{0}", _TypeName); }
        }


        public void Go(string filingName)
        {
            throw new NotImplementedException();
        }

        public GenericFileTypeClass ObjGetPublisherClass(string typeName)
        {
            this.TypeName = typeName;
            var type = Type.GetType(TypeName, true);
            var newInstance = Activator.CreateInstance(type);
            return newInstance as GenericFileTypeClass;
        }
    }
}
