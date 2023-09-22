using System.Globalization;
using System.Reflection;
using Mantis.Core.Calculator;

namespace Mantis.Core.QuickTable;

public class QuickTablePropertyAccess<T>
{
    private static QuickTablePropertyAccess<T>? _instance;

    public static QuickTablePropertyAccess<T> Instance => _instance ??= new QuickTablePropertyAccess<T>();
    
    public readonly QuickTableAttribute TableData;

    public readonly ManagedField[] Fields;

    private ConstructorInfo _constructor;
        
    private QuickTablePropertyAccess()
    {
        Type managed = typeof(T);

        if (managed.GetCustomAttribute<QuickTableAttribute>() == null)
        {
            Console.WriteLine(managed);
            Console.WriteLine(managed.GetCustomAttributes().Count());
            Console.WriteLine(managed.GetCustomAttribute<QuickTableAttribute>());
            
            throw new Exception($"The type {managed.Name} you try to access is not declared as Table with the" +
                                $"[TableAttribute] !");
        }

        TableData = managed.GetCustomAttribute<QuickTableAttribute>();

        _constructor = managed.GetConstructor(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null, Type.EmptyTypes, null);
        if (_constructor == null)
            throw new Exception($"The type {managed.Name} is required to have an empty constructor");

        Fields = (from field in managed.GetFields()
            where  field.GetCustomAttribute<QuickTableField>() != null
            select new ManagedField(field, field.GetCustomAttribute<QuickTableField>())).ToArray();
        
        
    }

    public IEnumerable<QuickTableField> GetHeader()
    {
        return Fields.Select(f => f.FieldData);
    }

    public T GetNewInstance()
    {
        return (T)_constructor.Invoke(Array.Empty<object?>());
    }

    public class ManagedField
    {
        private readonly FieldInfo _field;

        private MethodInfo? _parseMethod;

        public readonly QuickTableField FieldData;

        public ManagedField(FieldInfo field, QuickTableField fieldData)
        {
            _field = field;
            FieldData = fieldData;
        }

        public object? GetValue(object instance)
        {
            return _field.GetValue(instance);
        }

        public string? GetToString(object instance)
        {
            var o = GetValue(instance);
            return o == null ? "" : o.ToString();
        }

        public void SetValue(ref object instance, object value)
        {
            _field.SetValue(instance,value);
        }

        public void ParseValueT<T>(ref T instance, string value)
        {
            object obj = instance;
            ParseValue(ref obj,value);
            instance = (T)obj;
        }

        public void ParseValue(ref object instance, string value)
        {
            if (GetType() == typeof(string))
            {
                SetValue(ref instance, value);
                return;
            }

            if (GetType() == typeof(bool))
            {
                bool v = value == "true" || value == "True";
                SetValue(ref instance,v);
                return;
            }

            if (string.IsNullOrEmpty(value))
            {
                SetValue(ref instance,GetDefaultValue());
                return;
            }

            if (GetType() == typeof(ErDouble))
            {
                SetValue(ref instance,ErDouble.ParseWithErrorLastDigit(value,null,FieldData.LastDigitError));
                return;
            }

            if (_parseMethod == null)
            {
                _parseMethod = GetType().GetMethod("Parse", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                    new Type[]{typeof(string),typeof(IFormatProvider)});
                if (_parseMethod == null)
                    throw new ArgumentException(
                        $"The Type {GetType().Name} of TableField {FieldData.Name} does not implement the IParsable interface." +
                        $"Change the Type or add a custom parser (not yet implemented)");
            }

            var obj = _parseMethod.Invoke(null, new object?[] { value, null });
            SetValue(ref instance,obj);
        }
        public Type GetType()
        {
            return _field.FieldType;
        }
        
        private object GetDefaultValue()
        {
            if (GetType().IsValueType)
                return Activator.CreateInstance(GetType());

            return null;
        }
    }
}