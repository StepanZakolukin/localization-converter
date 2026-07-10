using FluentResults;
using LocalizationConverter.Core.Readers;
using LocalizationConverter.Core.Writers;

namespace LocalizationConverter.Core.Converters;

public class Converter(ILocalizationReader _reader, ILocalizationWriter _writer) : ILocalizationConverter
{
    public Result Convert()
    {
        var result = _reader.Read();

        if (result.IsFailed)
            return result.ToResult();

        return _writer.Write(result.Value);
    }
}
