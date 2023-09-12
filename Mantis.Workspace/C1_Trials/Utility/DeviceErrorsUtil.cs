using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.FileManagement;
using Mantis.Core.QuickTable;

namespace Mantis.Workspace.C1_Trials.Utility;

[QuickTable("","")]
public struct DeviceErrorsData
{
    [QuickTableField("range")] public double Range = 0;
    [QuickTableField("errorValue", "%")] public double ErrorValuePercent = 0;
    [QuickTableField("errorRange", "%")] public double ErrorRangePercent = 0;

    public DeviceErrorsData(){}
}

public enum Devices{Aglient34405}

public enum DataTypes
{
    VoltageDC,
    CurrentDC,
    Resistance
};

public static class DeviceErrorsUtil
{
    private static Dictionary<Devices, SimpleTableProtocolReader?> _readers =
        new Dictionary<Devices, SimpleTableProtocolReader?>();

    private static SimpleTableProtocolReader? GetReader(Devices device)
    {
        if (_readers.TryGetValue(device, out SimpleTableProtocolReader? reader))
            return reader;

        var path = "Errors_" + device.ToString() + ".csv";

        FileManager.CurrentInputDir = "C1_Trials/Utility";
        var newReader = new SimpleTableProtocolReader(path);
        FileManager.ResetCurrentInputDir();
        _readers[device] = newReader;
        return newReader;
    }

    private static Dictionary<(Devices, string), Dictionary<double,DeviceErrorsData>> _errorDataDictionary =
        new Dictionary<(Devices, string), Dictionary<double,DeviceErrorsData>>();

    private static Dictionary<double,DeviceErrorsData> GetErrorData(Devices device, string table)
    {
        if (_errorDataDictionary.TryGetValue((device, table), out Dictionary<double,DeviceErrorsData> data))
        {
            return data;
        }

        var newDataList = GetReader(device).ExtractTable<DeviceErrorsData>(table);

        var newData = newDataList.ToDictionary(e => e.Range);
        
        _errorDataDictionary[(device, table)] = newData;
        return newData;
    }

    private static Dictionary<double,DeviceErrorsData> GetErrorData(Devices device, DataTypes dataType)
    {
        var table = "tab:" + dataType.ToString();
        return GetErrorData(device, table);
    }
    
    public static void CalculateDeviceError(this ref ErDouble value,Devices device, DataTypes dataType, double range)
    {
        value = CalculateDeviceError(device, dataType, range, value.Value);
    }

    public static ErDouble CalculateDeviceError(Devices device, DataTypes dataType, double range, double value)
    {
        ErDouble newValue = value;

        if (GetErrorData(device, dataType).TryGetValue(range, out DeviceErrorsData errorData))
        {
            newValue.Error = (value * errorData.ErrorValuePercent + range * errorData.ErrorRangePercent) * 0.01;
            return newValue;
        }
        else
        {
            throw new ArgumentException(
                $"Range '{range}' was not found in error table '{dataType}' of device '{device}'");
        }

    }
}