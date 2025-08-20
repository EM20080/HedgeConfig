using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Net.Http;

namespace HedgeConfig
{
    internal static class UpdateServer
    {
        public static void CheckForUpdate(bool forcePrompt = false)
        {
            try
            {
                var currentVersion = GetCurrentVersion();
                if (currentVersion == null && !forcePrompt) return;

                Version? nextMinorRelease = null;
                if (!forcePrompt && currentVersion != null)
                {
                    var releasesJson = GetJson("https://api.github.com/repos/EM20080/HedgeConfig/releases?per_page=100");
                    if (!string.IsNullOrWhiteSpace(releasesJson))
                    {
                        try
                        {
                            using var doc = JsonDocument.Parse(releasesJson);
                            var allVersions = new List<Version>();
                            foreach (var el in doc.RootElement.EnumerateArray())
                            {
                                if (el.TryGetProperty("tag_name", out var tagProp))
                                {
                                    var v = ParseVersion(tagProp.GetString() ?? string.Empty);
                                    if (v != null) allVersions.Add(v);
                                }
                            }
                            if (allVersions.Count > 0)
                            {
                                var candidateMinor = new Version(currentVersion.Major, currentVersion.Minor + 1, 0);
                                var matching = allVersions
                                    .Where(v => v.Major == currentVersion.Major && v.Minor == candidateMinor.Minor && v > currentVersion)
                                    .OrderBy(v => v)
                                    .FirstOrDefault();
                                if (matching != null)
                                    nextMinorRelease = matching;
                            }
                        }
                        catch { }
                    }
                }

                bool shouldPrompt = forcePrompt || (nextMinorRelease != null && currentVersion != null && nextMinorRelease > currentVersion);
                if (!shouldPrompt) return;

                var targetVersionLabel = nextMinorRelease != null ? $" v{nextMinorRelease}" : string.Empty;
                var res = MessageBox.Show($"HedgeConfig has a new minor update{targetVersionLabel}. Would you like to open the releases page?", "Update Available", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
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
                client.Timeout = TimeSpan.FromSeconds(8);
                return client.GetStringAsync(url).GetAwaiter().GetResult();
            }
            catch { return string.Empty; }
        }
    }
}
