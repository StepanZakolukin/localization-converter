using FluentResults;
using LocalizationConverter.Core.Models;

namespace LocalizationConverter.Core.Readers;

public interface ILocalizationReader
{
    Result<LocalizationDirectory> Read();
}
