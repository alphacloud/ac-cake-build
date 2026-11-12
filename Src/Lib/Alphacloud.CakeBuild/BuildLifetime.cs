using Cake.Common.IO;
using Cake.Core;
using Cake.Frosting;

namespace Alphacloud.CakeBuild;

/// <summary>Prepares the artifacts folder before the first task runs.</summary>
public sealed class BuildLifetime : FrostingLifetime<BuildContext>
{
    /// <inheritdoc />
    public override void Setup(BuildContext context, ISetupContext info)
    {
        context.EnsureDirectoryExists(context.Paths.ArtifactsDir);
        context.CleanDirectory(context.Paths.ArtifactsDir);
    }

    /// <inheritdoc />
    public override void Teardown(BuildContext context, ITeardownContext info)
    {
    }
}
