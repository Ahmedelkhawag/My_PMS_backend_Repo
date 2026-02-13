using PMS.Application.Validation;
using PMS.Domain.Constants;
using PMS.Domain.Enums;
using PMS.Infrastructure.Implmentations.Services;
using System.Reflection;

namespace PMS.Tests;

public class ColorStandardsTests
{
    [Theory]
    [InlineData(StatusColorPalette.Success)]
    [InlineData(StatusColorPalette.Danger)]
    [InlineData(StatusColorPalette.Warning)]
    [InlineData(StatusColorPalette.Info)]
    [InlineData(StatusColorPalette.Secondary)]
    public void Palette_Colors_Are_Valid_Hex(string color)
    {
        Assert.True(HexColorValidator.IsValid(color));
    }

    [Theory]
    [InlineData(ReservationStatus.Pending)]
    [InlineData(ReservationStatus.Confirmed)]
    [InlineData(ReservationStatus.CheckIn)]
    [InlineData(ReservationStatus.CheckOut)]
    [InlineData(ReservationStatus.Cancelled)]
    [InlineData(ReservationStatus.NoShow)]
    public void ReservationStatusColor_Mapping_Returns_Valid_Hex(ReservationStatus status)
    {
        var color = InvokeGetStatusColor(status);
        Assert.True(HexColorValidator.IsValid(color));
    }

    private static string InvokeGetStatusColor(ReservationStatus status)
    {
        var method = typeof(ReservationsService)
            .GetMethod("GetStatusColor", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var result = method!.Invoke(null, new object[] { status });
        Assert.IsType<string>(result);
        return (string)result;
    }
}
