using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataMapper
{
    delegate T ConstructorDelegate<T>();
    public delegate T ConvertDelegate<T>(object source);
    public delegate void MapValue<T, TIn>(T targetObject, object source, ConvertDelegate<TIn> getValue);
}
