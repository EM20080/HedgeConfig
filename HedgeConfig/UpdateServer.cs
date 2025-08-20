using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace HedgeConfig
{
    internal static class UpdateServer
    {
        public static void CheckForUpdate(bool forcePrompt = false)
        {
            try
            {
                var currentVersion = GetCurrentVersion();
                if (!forcePrompt && currentVersion == null) return;
                Version? remoteVersion = null;
                if (!forcePrompt)
                {
                    var latest = GetJson("https://api.github.com/repos/EM20080/HedgeConfig/releases/latest");
                    if (!string.IsNullOrWhiteSpace(latest))
                    {
                        try
                        {
                            using var doc = JsonDocument.Parse(latest);
                            if (doc.RootElement.TryGetProperty("tag_name", out var tagProp))
                            {
                                var tag = tagProp.GetString() ?? string.Empty;
                                remoteVersion = ParseVersion(tag);
                            }
                        }
                        catch { }
                    }
                }
                bool shouldPrompt = forcePrompt || (remoteVersion != null && currentVersion != null && remoteVersion > currentVersion);
                if (!shouldPrompt) return;
                if (!forcePrompt && remoteVersion != null && currentVersion != null && remoteVersion <= currentVersion) return;
                var res = MessageBox.Show("HedgeConfig has a new update. Would you like to download it?", "Update Available", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                if (res == DialogResult.Yes)
                {
                    const string url = "https://github.com/EM20080/HedgeConfig/releases/";
                    try { Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true }); } catch { }
                }
            }
            catch { }
        }

        private static Version? GetCurrentVersion()
        {
            try
            {
                var asm = Assembly.GetExecutingAssembly();
                string? verStr = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
                if (string.IsNullOrWhiteSpace(verStr))
                    verStr = FileVersionInfo.GetVersionInfo(asm.Location).ProductVersion;
                if (string.IsNullOrWhiteSpace(verStr))
                    verStr = asm.GetName().Version?.ToString();
                if (string.IsNullOrWhiteSpace(verStr)) return null;
                return ParseVersion(verStr);
            }
            catch { return null; }
        }

        private static Version? ParseVersion(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            input = input.Trim();
            if (input.StartsWith("v", StringComparison.OrdinalIgnoreCase)) input = input[1..];
            int plus = input.IndexOf('+');
            if (plus >= 0) input = input[..plus];
            var m = Regex.Match(input, "^(\\d+\\.\\d+(?:\\.\\d+){0,2})");
            if (m.Success) input = m.Groups[1].Value;
            if (Version.TryParse(input, out var v)) return v;
            var digits = Regex.Match(input, "(\\d+)(?:\\.(\\d+))?(?:\\.(\\d+))?");
            if (digits.Success)
            {
                var parts = digits.Groups.Values.Skip(1).Where(g => g.Success).Select(g => g.Value).ToArray();
                while (parts.Length < 2) parts = parts.Concat(new[]{"0"}).ToArray();
                var norm = string.Join('.', parts);
                if (Version.TryParse(norm, out v)) return v;
            }
            return null;
        }

        private static string GetJson(string url)
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("HedgeConfigUpdateChecker/1.0");
                client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
                client.Timeout = TimeSpan.FromSeconds(6);
                return client.GetStringAsync(url).GetAwaiter().GetResult();
            }
            catch { return string.Empty; }
        }
    }
}
