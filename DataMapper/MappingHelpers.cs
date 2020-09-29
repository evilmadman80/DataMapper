using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataMapper
{
    internal static class MappingHelpers
    {
        internal const string DO_NOT_MAP = "--**__DONOTMAP__**--";
        internal static string GetSourceColumn(PropertyInfo prop)
        {
            MappingAttribute dma = prop.GetCustomAttribute(typeof(MappingAttribute)) as MappingAttribute;
            if (dma != null)
            {
                if (dma.DoNotMap)
                {
                    return DO_NOT_MAP;
                }
                else if (!string.IsNullOrWhiteSpace(dma.SourceName))
                {
                    return dma.SourceName;
                }
            }
            return prop.Name;
        }
        internal static void MapType<T>(T target, object source, PropertyInfo prop)
        {
            try
            {
                //since convert does not work with nullable types we must check and get the underlying type
                Type targetType = prop.PropertyType;
                if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    targetType = Nullable.GetUnderlyingType(targetType);
                }

                if (targetType == typeof(string))
                {
                    prop.SetMethod.Invoke(target, new object[] { source.ToString().Trim() });
                }
                else
                {
                    prop.SetMethod.Invoke(target, new object[] { Convert.ChangeType(source, targetType) });
                }
            }
            catch (Exception) { }
        }
    }
}
