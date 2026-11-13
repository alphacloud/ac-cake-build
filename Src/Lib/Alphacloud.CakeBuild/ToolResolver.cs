using Cake.Core;
using Cake.Core.IO;

namespace Alphacloud.CakeBuild;

/// <summary>Locates executables of tools installed by <see cref="CakeHostExtensions.UseAlphacloudCakeBuild"/>.</summary>
public static class ToolResolver
{
    /// <summary>
    /// Platform-specific executable file name: appends <c>.exe</c> on Windows unless already present.
    /// </summary>
    /// <param name="name">Tool command name, e.g. <c>dotnet-coverage</c>.</param>
    /// <param name="isWindows">Target platform; defaults to the current OS.</param>
    public static string ExecutableName(string name, bool? isWindows = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var windows = isWindows ?? OperatingSystem.IsWindows();
        return windows && !name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ? name + ".exe" : name;
    }

    /// <summary>
    /// Resolves the full path of an installed tool. Throws instead of returning <c>null</c>, so a missing pinned tool
    /// never silently falls back to a different (e.g. stale or global) version.
    /// </summary>
    /// <exception cref="CakeException">The tool is not installed.</exception>
    public static FilePath Resolve(ICakeContext context, string name)
    {
        ArgumentNullException.ThrowIfNull(context);
        var executable = ExecutableName(name);
        return context.Tools.Resolve(executable)
               ?? throw new CakeException($"Tool '{executable}' was not found. Is it installed via UseAlphacloudCakeBuild()?");
    }
}
