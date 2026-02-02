using System.Globalization;

namespace VendorMdm.Api.Services;

/// <summary>
/// Internationalization (i18n) service for multilanguage support (Pattern 18).
/// Supports UN operations across multiple languages.
/// </summary>
public interface ILocalizationService
{
    string GetString(string key, string? culture = null);
    string GetCurrentCulture();
    void SetCulture(string culture);
    List<string> GetSupportedCultures();
}

public class LocalizationService : ILocalizationService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<LocalizationService> _logger;
    
    // Supported UN languages
    private static readonly List<string> SupportedCultures = new()
    {
        "en", // English (default)
        "fr", // French
        "es", // Spanish
        "ar", // Arabic
        "zh", // Chinese
        "ru"  // Russian
    };

    // Translation dictionary (in production, load from database or resource files)
    private static readonly Dictionary<string, Dictionary<string, string>> Translations = new()
    {
        ["en"] = new()
        {
            ["vendor.status.pending"] = "Pending",
            ["vendor.status.active"] = "Active",
            ["vendor.status.suspended"] = "Suspended",
            ["vendor.status.archived"] = "Archived",
            ["common.save"] = "Save",
            ["common.cancel"] = "Cancel",
            ["common.submit"] = "Submit",
            ["common.approve"] = "Approve",
            ["common.reject"] = "Reject",
            ["gdpr.right_to_access"] = "Right to Access",
            ["gdpr.right_to_erasure"] = "Right to be Forgotten",
            ["gdpr.data_exported"] = "Your data has been exported successfully"
        },
        ["fr"] = new()
        {
            ["vendor.status.pending"] = "En attente",
            ["vendor.status.active"] = "Actif",
            ["vendor.status.suspended"] = "Suspendu",
            ["vendor.status.archived"] = "Archivé",
            ["common.save"] = "Enregistrer",
            ["common.cancel"] = "Annuler",
            ["common.submit"] = "Soumettre",
            ["common.approve"] = "Approuver",
            ["common.reject"] = "Rejeter",
            ["gdpr.right_to_access"] = "Droit d'accès",
            ["gdpr.right_to_erasure"] = "Droit à l'oubli",
            ["gdpr.data_exported"] = "Vos données ont été exportées avec succès"
        },
        ["es"] = new()
        {
            ["vendor.status.pending"] = "Pendiente",
            ["vendor.status.active"] = "Activo",
            ["vendor.status.suspended"] = "Suspendido",
            ["vendor.status.archived"] = "Archivado",
            ["common.save"] = "Guardar",
            ["common.cancel"] = "Cancelar",
            ["common.submit"] = "Enviar",
            ["common.approve"] = "Aprobar",
            ["common.reject"] = "Rechazar",
            ["gdpr.right_to_access"] = "Derecho de acceso",
            ["gdpr.right_to_erasure"] = "Derecho al olvido",
            ["gdpr.data_exported"] = "Sus datos han sido exportados exitosamente"
        },
        ["ar"] = new()
        {
            ["vendor.status.pending"] = "قيد الانتظار",
            ["vendor.status.active"] = "نشط",
            ["vendor.status.suspended"] = "معلق",
            ["vendor.status.archived"] = "مؤرشف",
            ["common.save"] = "حفظ",
            ["common.cancel"] = "إلغاء",
            ["common.submit"] = "إرسال",
            ["common.approve"] = "موافقة",
            ["common.reject"] = "رفض",
            ["gdpr.right_to_access"] = "الحق في الوصول",
            ["gdpr.right_to_erasure"] = "الحق في النسيان",
            ["gdpr.data_exported"] = "تم تصدير بياناتك بنجاح"
        }
    };

    public LocalizationService(
        IHttpContextAccessor httpContextAccessor,
        ILogger<LocalizationService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public string GetString(string key, string? culture = null)
    {
        var targetCulture = culture ?? GetCurrentCulture();
        
        // Fallback to English if culture not supported
        if (!Translations.ContainsKey(targetCulture))
        {
            targetCulture = "en";
        }

        if (Translations[targetCulture].TryGetValue(key, out var translation))
        {
            return translation;
        }

        // Fallback to English
        if (targetCulture != "en" && Translations["en"].TryGetValue(key, out var englishTranslation))
        {
            _logger.LogWarning("Translation key '{Key}' not found for culture '{Culture}', using English", key, targetCulture);
            return englishTranslation;
        }

        // Return key if no translation found
        _logger.LogWarning("Translation key '{Key}' not found", key);
        return key;
    }

    public string GetCurrentCulture()
    {
        // Try to get from claims
        var cultureClaim = _httpContextAccessor.HttpContext?.User?.Claims
            .FirstOrDefault(c => c.Type == "culture" || c.Type == "locale");
        
        if (cultureClaim != null && SupportedCultures.Contains(cultureClaim.Value))
        {
            return cultureClaim.Value;
        }

        // Try from header
        var cultureHeader = _httpContextAccessor.HttpContext?.Request.Headers["Accept-Language"].FirstOrDefault();
        if (cultureHeader != null)
        {
            var primaryLanguage = cultureHeader.Split(',').FirstOrDefault()?.Split('-').FirstOrDefault()?.Trim();
            if (primaryLanguage != null && SupportedCultures.Contains(primaryLanguage))
            {
                return primaryLanguage;
            }
        }

        // Default to English
        return "en";
    }

    public void SetCulture(string culture)
    {
        if (!SupportedCultures.Contains(culture))
        {
            _logger.LogWarning("Unsupported culture '{Culture}', using English", culture);
            culture = "en";
        }

        CultureInfo.CurrentCulture = new CultureInfo(culture);
        CultureInfo.CurrentUICulture = new CultureInfo(culture);
    }

    public List<string> GetSupportedCultures()
    {
        return SupportedCultures;
    }
}
