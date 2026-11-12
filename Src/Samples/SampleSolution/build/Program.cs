using Alphacloud.CakeBuild;
using Cake.Frosting;

var settings = new ProjectSettings("alphacloud", "ac-cake-build", "SampleSolution");

return new CakeHost()
    .UseAlphacloudCakeBuild(settings)
    .Run(args);
