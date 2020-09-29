using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataMapper
{
    internal class SqlMappingInfo<T> : IDisposable
    {
        public int ColumnNumber { get; set; }
        public string SourceColumn { get; private set; }
        public Action<T, object> SetValue { get; private set; }

        private SqlMappingInfo(PropertyInfo prop)
        {
            ColumnNumber = -1;
            SourceColumn = MappingHelpers.GetSourceColumn(prop);
        }

        public SqlMappingInfo(PropertyInfo prop, bool createMapping) : this(prop)
        {
            if (createMapping && SourceColumn != MappingHelpers.DO_NOT_MAP)
                SetValue = CreateSetter(prop);
        }

        public SqlMappingInfo(PropertyInfo prop, string[] columns) : this(prop)
        {
            if (SourceColumn == MappingHelpers.DO_NOT_MAP || !columns.Contains(SourceColumn.ToUpper()))
            {
                SetValue = null;
                return;
            }

            SetValue = CreateSetter(prop);
            ColumnNumber = Array.IndexOf(columns, SourceColumn.ToUpper());
        }

        public SqlMappingInfo(PropertyInfo prop, DataColumnCollection columns) : this(prop)
        {
            if (SourceColumn == MappingHelpers.DO_NOT_MAP || !columns.Contains(SourceColumn))
            {
                SetValue = null;
                return;
            }

            SetValue = CreateSetter(prop);
            ColumnNumber = columns.IndexOf(SourceColumn);
        }

        public static Action<T, object> CreateSetter(PropertyInfo propInfo)
        {
            Action<T, object> setter = null;
            //Action<T, object> internalSetter = null;

            if (propInfo.PropertyType.IsArray)
            {
                //Type targetType = propInfo.PropertyType.GetElementType();
                InternalSetterDelegate<object> internalSetter = ((target, source) => propInfo.SetValue(target, source));
                setter = ((target, source) =>
                {
                    SetValueFunction<object>(target, source, internalSetter, ((src) => Convert.ChangeType(src, propInfo.PropertyType)));
                });
            }
            else if (propInfo.PropertyType.IsGenericType && propInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                Type targetType = propInfo.PropertyType.IsGenericType && propInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ?
                                  Nullable.GetUnderlyingType(propInfo.PropertyType)
                                  : propInfo.PropertyType;
                switch (Type.GetTypeCode(targetType))
                {
                    case TypeCode.Boolean:
                        {
                            var internalSetter = (InternalSetterDelegate<bool?>)Delegate.CreateDelegate(typeof(InternalSetterDelegate<bool?>), null, propInfo.GetSetMethod());
                            setter = ((target, source) =>
                            {
                                SetValueFunction<bool?>(target, source, internalSetter, ((src) => Convert.ToBoolean(src)));
                            });
                        }
                        break;
                    case TypeCode.Byte:
                        {
                            var internalSetter = (InternalSetterDelegate<byte?>)Delegate.CreateDelegate(typeof(InternalSetterDelegate<byte?>), null, propInfo.GetSetMethod());
                            setter = ((target, source) =>
                            {
                                SetValueFunction<byte?>(target, source, internalSetter, ((src) => Convert.ToByte(src)));
                            });
                        }
                        break;
                    case TypeCode.Char:
                        {
                            var internalSetter = (InternalSetterDelegate<char?>)Delegate.CreateDelegate(typeof(InternalSetterDelegate<char?>), null, propInfo.GetSetMethod());
                            setter = ((target, source) =>
                            {
                                SetValueFunction<char?>(target, source, internalSetter, ((src) => source.ToString()[0]));
                            });
                        }
                        break;
                    case TypeCode.DateTime:
                        {
                            var internalSetter = (InternalSetterDelegate<DateTime?>)Delegate.CreateDelegate(typeof(InternalSetterDelegate<DateTime?>), null, propInfo.GetSetMethod());
                            setter = ((target, source) =>
                            {
                                SetValueFunction<DateTime?>(target, source, internalSetter, ((src) => Convert.ToDateTime(src)));
                            });
                        }
                        break;
                    case TypeCode.Decimal:
                        {
                            var internalSetter = (InternalSetterDelegate<decimal?>)Delegate.CreateDelegate(typeof(InternalSetterDelegate<decimal?>), null, propInfo.GetSetMethod());
                            setter = ((target, source) =>
                            {
                                SetValueFunction<decimal?>(target, source, internalSetter, ((src) => Convert.ToDecimal(src)));
                            });
                        }
                        break;
                    case TypeCode.Double:
                        {
                            var internalSetter = (InternalSetterDelegate<double?>)Delegate.CreateDelegate(typeof(InternalSetterDelegate<double?>), null, propInfo.GetSetMethod());
                            setter = ((target, source) =>
                            {
                                SetValueFunction<double?>(target, source, internalSetter, ((src) => Convert.ToDouble(src)));
                            });
                        }
                        break;
                    case TypeCode.Int16:
                        {
                            var internalSetter = (InternalSetterDelegate<short?>)Delegate.CreateDelegate(typeof(InternalSetterDelegate<short?>), null, propInfo.GetSetMethod());
                            setter = ((target, source) =>
                            {
                                SetValueFunction<short?>(target, source, internalSetter, ((src) => Convert.ToInt16(src)));
                            });
                        }
                        break;
                    case TypeCode.Int32:
                        {
                            var internalSetter = (InternalSetterDelegate<int?>)Delegate.CreateDelegate(typeof(InternalSetterDelegate<int?>), null, propInfo.GetSetMethod());
                            setter = ((target, source) =>
                            {
                                SetValueFunction<int?>(target, source, internalSetter, ((src) => Convert.ToInt32(src)));
                            });
                        }
                        break;
                    case TypeCode.Int64:
                        {
                            var internalSetter = (InternalSetterDelegate<long?>)Delegate.CreateDelegate(typeof(InternalSetterDelegate<long?>), null, propInfo.GetSetMethod());
                            setter = new Action<T, object>((target, source) =>
                            {
                                SetValueFunction<Int64?>(target, source, internalSetter, ((src) => Convert.ToInt64(src)));
                            });
                        }
                        break;
                    case TypeCode.Single:
                        {
                            var internalSetter = (InternalSetterDelegate<float?>)Delegate.CreateDelegate(typeof(InternalSetterDelegate<float?>), null, propInfo.GetSetMethod());
                            setter = ((target, source) =>
                            {
                                SetValueFunction<float?>(target, source, internalSetter, ((src) => Convert.ToSingle(src)));
                            });
                        }
                        break;
                    case TypeCode.SByte:
                        {
                            var internalSetter = (InternalSetterDelegate<sbyte?>)Delegate.CreateDelegate(typeof(InternalSetterDelegate<sbyte?>), null, propInfo.GetSetMethod());
                            setter = ((target, source) =>
                            {
                                SetValueFunction<sbyte?>(target, source, internalSetter, ((src) => Convert.ToSByte(src)));
                            });
                        }
                        break;
                    case TypeCode.String:
                        {
                            var internalSetter = (InternalSetterDelegate<string>)Delegate.CreateDelegate(typeof(InternalSetterDelegate<string>), null, propInfo.GetSetMethod());
                            setter = ((target, source) =>
                            {
                                SetValueFunction<string>(target, source, internalSetter, ((src) => src.ToString().Trim()));
                            });
                        }
                        break;
                    case TypeCode.UInt16:
                        {
                            var internalSetter = (InternalSetterDelegate<UInt16?>)Delegate.CreateDelegate(typeof(InternalSetterDelegate<UInt16?>), null, propInfo.GetSetMethod());
                            setter = ((target, source) =>
                            {
                                SetValueFunction<UInt16?>(target, source, internalSetter, ((src) => Convert.ToUInt16(src)));
                            });
                        }
                        break;
                    case TypeCode.UInt32:
                        {
                            var internalSetter = (InternalSetterDelegate<UInt32?>)Delegate.CreateDelegate(typeof(InternalSetterDelegate<UInt32?>), null, propInfo.GetSetMethod());
                            setter = ((target, source) =>
                            {
                                SetValueFunction<UInt32?>(target, source, internalSetter, ((src) => Convert.ToUInt32(src)));
                            });
                        }
                        break;
                    case TypeCode.UInt64:
                        {
                            var internalSetter = (InternalSetterDelegate<UInt64?>)Delegate.CreateDelegate(typeof(InternalSetterDelegate<UInt64?>), null, propInfo.GetSetMethod());
                            setter = new Action<T, object>((target, source) =>
                            {
                                SetValueFunction<UInt64?>(target, source, internalSetter, ((src) => Convert.ToUInt64(src)));
                            });
                        }
                        break;
                    case TypeCode.Object:
                    default:
                        {
                            InternalSetterDelegate<object> internalSetter = ((target, source) => propInfo.SetValue(target, source));
                            setter = ((target, source) =>
                            {
                                SetValueFunction<object>(target, source, internalSetter, ((src) => Convert.ChangeType(src, targetType)));
                            });
                        }
                        break;
                }
            }
            else
            {
                Type targetType = propInfo.PropertyType;
                switch (Type.GetTypeCode(targetType))
                {
                    case TypeCode.Boolean:
                        {
                            var internalSetter = (InternalSetterDelegate<bool>)Delegate.CreateDelegate(typeof(InternalSetterDelegate<bool>), null, propInfo.GetSetMethod());
                            setter = ((target, source) =>
                            {
                                SetValueFunction<bool>(target, source, internalSetter, ((src) => Convert.ToBoolean(src)));
                            });
                        }
                        break;
                    case TypeCode.Byte:
                        {
                            var internalSetter = (InternalSetterDelegate<byte>)Delegate.CreateDelegate(typeof(InternalSetterDelegate<byte>), null, propInfo.GetSetMethod());
                            setter = ((target, source) =>
                            {
                                SetValueFunction<byte>(target, source, internalSetter, ((src) => Convert.ToByte(src)));
                            });
                        }
                        break;
                    case TypeCode.Char:
                        {
                            var internalSetter = (InternalSetterDelegate<char>)Delegate.CreateDelegate(typeof(InternalSetterDelegate<char>), null, propInfo.GetSetMethod());
                            setter = ((target, source) =>
                            {
                                SetValueFunction<char>(target, source, internalSetter, ((src) => source.ToString()[0]));
                            });
                        }
                        break;
                    case TypeCode.DateTime:
                        {
                            var internalSetter = (InternalSetterDelegate<DateTime>)Delegate.CreateDelegate(typeof(InternalSetterDelegate<DateTime>), null, propInfo.GetSetMethod());
                            setter = ((target, source) =>
                            {
                                SetValueFunction<DateTime>(target, source, internalSetter, ((src) => Convert.ToDateTime(src)));
                            });
                        }
                        break;
                    case TypeCode.Decimal:
                        {
                            var internalSetter = (InternalSetterDelegate<decimal>)Delegate.CreateDelegate(typeof(InternalSetterDelegate<decimal>), null, propInfo.GetSetMethod());
                            setter = ((target, source) =>
                            {
                                SetValueFunction<decimal>(target, source, internalSetter, ((src) => Convert.ToDecimal(src)));
                            });
                        }
                        break;
                    case TypeCode.Double:
                        {
                            var internalSetter = (InternalSetterDelegate<double>)Delegate.CreateDelegate(typeof(InternalSetterDelegate<double>), null, propInfo.GetSetMethod());
                            setter = ((target, source) =>
                            {
                                SetValueFunction<double>(target, source, internalSetter, ((src) => Convert.ToDouble(src)));
                            });
                        }
                        break;
                    case TypeCode.Int16:
                        {
                            var internalSetter = (InternalSetterDelegate<short>)Delegate.CreateDelegate(typeof(InternalSetterDelegate<short>), null, propInfo.GetSetMethod());
                            setter = ((target, source) =>
                            {
                                SetValueFunction<Int16>(target, source, internalSetter, ((src) => Convert.ToInt16(src)));
                            });
                        }
                        break;
                    case TypeCode.Int32:
                        {
                            var internalSetter = (InternalSetterDelegate<int>)Delegate.CreateDelegate(typeof(InternalSetterDelegate<int>), null, propInfo.GetSetMethod());
                            setter = ((target, source) =>
                            {
                                SetValueFunction<Int32>(target, source, internalSetter, ((src) => Convert.ToInt32(src)));
                            });
                        }
                        break;
                    case TypeCode.Int64:
                        {
                            var internalSetter = (InternalSetterDelegate<long>)Delegate.CreateDelegate(typeof(InternalSetterDelegate<long>), null, propInfo.GetSetMethod());
                            setter = ((target, source) =>
                            {
                                SetValueFunction<Int64>(target, source, internalSetter, ((src) => Convert.ToInt64(src)));
                            });
                        }
                        break;
                    case TypeCode.Single:
                        {
                            var internalSetter = (InternalSetterDelegate<float>)Delegate.CreateDelegate(typeof(InternalSetterDelegate<float>), null, propInfo.GetSetMethod());
                            setter = ((target, source) =>
                            {
                                SetValueFunction<float>(target, source, internalSetter, ((src) => Convert.ToSingle(src)));
                            });
                        }
                        break;
                    case TypeCode.SByte:
                        {
                            var internalSetter = (InternalSetterDelegate<sbyte>)Delegate.CreateDelegate(typeof(InternalSetterDelegate<sbyte>), null, propInfo.GetSetMethod());
                            setter = ((target, source) =>
                            {
                                SetValueFunction<sbyte>(target, source, internalSetter, ((src) => Convert.ToSByte(src)));
                            });
                        }
                        break;
                    case TypeCode.String:
                        {
                            var internalSetter = (InternalSetterDelegate<string>)Delegate.CreateDelegate(typeof(InternalSetterDelegate<string>), null, propInfo.GetSetMethod());
                            setter = ((target, source) =>
                            {
                                SetValueFunction<string>(target, source, internalSetter, ((src) => src.ToString().Trim()));
                            });
                        }
                        break;
                    case TypeCode.UInt16:
                        {
                            var internalSetter = (InternalSetterDelegate<UInt16>)Delegate.CreateDelegate(typeof(InternalSetterDelegate<UInt16>), null, propInfo.GetSetMethod());
                            setter = ((target, source) =>
                            {
                                SetValueFunction<UInt16>(target, source, internalSetter, ((src) => Convert.ToUInt16(src)));
                            });
                        }
                        break;
                    case TypeCode.UInt32:
                        {
                            var internalSetter = (InternalSetterDelegate<UInt32>)Delegate.CreateDelegate(typeof(InternalSetterDelegate<UInt32>), null, propInfo.GetSetMethod());
                            setter = ((target, source) =>
                            {
                                SetValueFunction<UInt32>(target, source, internalSetter, ((src) => Convert.ToUInt32(src)));
                            });
                        }
                        break;
                    case TypeCode.UInt64:
                        {
                            var internalSetter = (InternalSetterDelegate<UInt64>)Delegate.CreateDelegate(typeof(InternalSetterDelegate<UInt64>), null, propInfo.GetSetMethod());
                            setter = ((target, source) =>
                            {
                                SetValueFunction<UInt64>(target, source, internalSetter, ((src) => Convert.ToUInt64(src)));
                            });
                        }
                        break;
                    case TypeCode.Object:
                    default:
                        {
                            InternalSetterDelegate<object> internalSetter = ((target, source) => propInfo.SetValue(target, source));
                            setter = ((target, source) =>
                            {
                                SetValueFunction<object>(target, source, internalSetter, ((src) => Convert.ChangeType(src, propInfo.PropertyType)));
                            });
                        }
                        break;
                }
            }

            return setter;
        }

        delegate void InternalSetterDelegate<TSet>(T target, TSet value);
        static void SetValueFunction<TSet>(T target, object source, InternalSetterDelegate<TSet> setter, ConvertDelegate<TSet> converter)
        {
            try
            {
                setter(target, converter(source));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Dispose()
        {
            ColumnNumber = -1;
            SourceColumn = null;
            SetValue = null;
        }
    }
}
