using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuestPDF.Drawing;
using QuestPDF.Infrastructure;
using PMS.Infrastructure.Constants;

namespace PMS.Infrastructure.Extensions;

public static class PdfServiceCollectionExtensions
{
    public static IServiceCollection AddPdfInfrastructure(this IServiceCollection services)
    {
        // Set QuestPDF License
        QuestPDF.Settings.License = LicenseType.Community;

        // Register Font immediately during startup so it's done once.
        // We will build a temporary service provider just to get a logger to log the font registration status,
        // or we could depend on the caller having an ILogger. 
        // A better approach for IServiceCollection is to just log to the console if ILogger is not readily available at registration, 
        // or register a small hosted service. Given the requirements, we'll instantiate a logger factory here.
        
        using var tempProvider = services.BuildServiceProvider();
        var logger = tempProvider.GetService<ILoggerFactory>()?.CreateLogger("PdfInfrastructure");

        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(PdfConstants.FontResourcePath);
            
            if (stream != null)
            {
                FontManager.RegisterFont(stream);
                logger?.LogInformation("Successfully registered PDF font: {FontName}", PdfConstants.ArabicFontName);
                
                // Set a global flag if needed by the service, though QuestPDF handles it internally.
                // It's requested to log a critical error if missing.
            }
            else
            {
                logger?.LogCritical("Failed to register PDF font: The embedded resource '{ResourceName}' was not found.", PdfConstants.FontResourcePath);
            }
        }
        catch (Exception ex)
        {
            logger?.LogCritical(ex, "An exception occurred while registering the PDF font: {FontName}", PdfConstants.ArabicFontName);
        }

        return services;
    }
}
