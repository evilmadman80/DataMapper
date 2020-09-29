using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataMapper
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public class MappingAttribute : Attribute
    {
        private string _sourceName;
        private bool _doNotMap;

        public MappingAttribute(string SourceName, bool DoNotMap = false)
        {
            this._sourceName = SourceName;
            this._doNotMap = DoNotMap;
        }

        public virtual string SourceName { get { return _sourceName; } }
        public virtual bool DoNotMap { get { return _doNotMap; } }
    }
}
