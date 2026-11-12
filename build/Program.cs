using Alphacloud.CakeBuild;
using Cake.Frosting;

var settings = new ProjectSettings("alphacloud", "ac-cake-build", "Alphacloud.CakeBuild");

return new CakeHost()
    .UseAlphacloudCakeBuild(settings)
    .Run(args);
