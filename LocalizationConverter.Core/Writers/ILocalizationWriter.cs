using FluentResults;
using LocalizationConverter.Core.Models;

namespace LocalizationConverter.Core.Writers;

public interface ILocalizationWriter
{
    Result Write(LocalizationDirectory directory);
}
