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

public enum Devices{Aglient34405,VC170}

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
    
    public static void CalculateDeviceError(this ref ErDouble value,Devices device, DataTypes dataType, double? range = null)
    {
        value = CalculateDeviceError(device, dataType, value.Value, range);
    }

    public static ErDouble CalculateDeviceError(Devices device, DataTypes dataType, double value, double? range = null)
    {
        ErDouble newValue = value;

        if (GetAutoRangeErrorData(GetErrorData(device, dataType),ref range, out DeviceErrorsData errorData,value))
        {
            newValue.Error = (value * errorData.ErrorValuePercent + range.Value * errorData.ErrorRangePercent) * 0.01;
            return newValue;
        }
        else
        {
            throw new ArgumentException(
                $"Range '{range}' was not found in error table '{dataType}' of device '{device}'");
        }

    }

    private static bool GetAutoRangeErrorData(Dictionary<double, DeviceErrorsData> errorDataDict, ref double? range,
        out DeviceErrorsData errorsData,double value)
    {
        if (range != null)
            return errorDataDict.TryGetValue(range.Value, out errorsData);
        

        double smallestBiggerRange = double.PositiveInfinity;

        foreach (var pair in errorDataDict)
        {
            if (pair.Key > value)
                smallestBiggerRange = Math.Min(smallestBiggerRange, pair.Key);
        }

        if (smallestBiggerRange == double.PositiveInfinity)
            throw new ArgumentException($"There is no range bigger than {value}");

        range = smallestBiggerRange;
        errorsData = errorDataDict[smallestBiggerRange];
        return true;
    }
}