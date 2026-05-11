using System.Globalization;
using Avalonia;
using Avalonia.Markup.Xaml.Styling;

namespace SDAHymns.Desktop.Services;

public class LocalizationManager
{
    private static LocalizationManager? _instance;
    public static LocalizationManager Instance => _instance ??= new LocalizationManager();

    private ResourceInclude? _currentLanguage;

    private LocalizationManager() { }

    public string GetString(string key, string defaultValue = "")
    {
        var app = Application.Current;
        if (app != null && app.Resources.TryGetResource(key, null, out var value) && value is string str)
        {
            return str;
        }
        return defaultValue;
    }

    public void SetLanguage(string languageCode)
    {
        var app = Application.Current;
        if (app == null) return;

        // Map short codes to full resource names
        string fullCode = languageCode.ToLowerInvariant() switch
        {
            "ro" => "ro-RO",
            "en" => "en-US",
            _ => languageCode
        };

        try
        {
            // Find existing language dictionary
            if (_currentLanguage != null)
            {
                app.Resources.MergedDictionaries.Remove(_currentLanguage);
            }

            // Create new language dictionary
            var uri = new Uri($"avares://SDAHymns.Desktop/Resources/Languages/{fullCode}.axaml");
            _currentLanguage = new ResourceInclude(uri)
            {
                Source = uri
            };

            // Add to merged dictionaries
            app.Resources.MergedDictionaries.Add(_currentLanguage);
            
            // Update current culture
            var culture = new CultureInfo(fullCode);
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.CurrentCulture = culture;
        }
        catch
        {
            // Fallback to Romanian if something goes wrong
            if (fullCode != "ro-RO")
            {
                SetLanguage("ro-RO");
            }
        }
    }
}
