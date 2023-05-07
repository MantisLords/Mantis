using System.Reflection;

namespace Mantis.Core.QuickTable;

public class QuickTablePropertyAccess<T>
{
    private static QuickTablePropertyAccess<T>? _instance;

    public static QuickTablePropertyAccess<T> Instance => _instance ??= new QuickTablePropertyAccess<T>();
    
    public readonly QuickTableAttribute TableData;

    public readonly ManagedField[] Fields;
        
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

        Fields = (from field in managed.GetFields()
            where  field.GetCustomAttribute<QuickTableField>() != null
            select new ManagedField(field, field.GetCustomAttribute<QuickTableField>())).ToArray();
    }

    public string[] GetHeader()
    {
        return Fields.Select(f => f.Name).ToArray();
    }

    public class ManagedField
    {
        private readonly FieldInfo _field;
        public readonly string Name;
        public readonly string Symbol;
        public readonly string Unit;

        public ManagedField(FieldInfo field, QuickTableField fieldData)
        {
            _field = field;
            Name = fieldData.Name;
            Symbol = fieldData.Symbol;
            if (string.IsNullOrEmpty(Symbol))
                Symbol = _field.Name;
            Unit = fieldData.Unit;
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

        public Type GetType()
        {
            return _field.GetType();
        }
    }
}