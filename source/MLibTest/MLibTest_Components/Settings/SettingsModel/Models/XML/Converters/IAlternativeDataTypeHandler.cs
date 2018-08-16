namespace SettingsModel.Models.XML.Converters
{
    using System;

    internal interface IAlternativeDataTypeHandler
    {
        Type SourceDataType { get; }
        Type TargetDataType { get; }

        object Convert(object objectInput);
        object ConvertBack(object objectEncryptedData);
    }
}
