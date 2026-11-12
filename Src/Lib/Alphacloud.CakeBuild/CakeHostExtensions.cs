using Cake.Frosting;
using Microsoft.Extensions.DependencyInjection;

namespace Alphacloud.CakeBuild;

/// <summary>Registers the Alphacloud build pipeline with a Frosting host.</summary>
public static class CakeHostExtensions
{
    /// <summary>
    /// Registers <paramref name="settings"/>, <see cref="BuildContext"/>, the lifetime hook, all pipeline tasks and
    /// pinned tools. The working directory is set to <paramref name="workingDirectory"/> (default <c>..</c>: the
    /// repository root, when the host project lives in <c>build/</c> and runs with
    /// <c>RunWorkingDirectory=$(MSBuildProjectDirectory)</c>).
    /// </summary>
    public static CakeHost UseAlphacloudCakeBuild(this CakeHost host, ProjectSettings settings, string workingDirectory = "..")
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(settings);

        host.ConfigureServices(services => services.AddSingleton(settings))
            .UseContext<BuildContext>()
            .UseLifetime<BuildLifetime>()
            .UseWorkingDirectory(workingDirectory)
            .AddAssembly(typeof(CakeHostExtensions).Assembly);

        foreach (var tool in Tools.All)
            host.InstallTool(tool);

        return host;
    }
}
