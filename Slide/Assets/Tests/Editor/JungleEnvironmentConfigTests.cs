#if UNITY_INCLUDE_TESTS
using GameLogic;
using NUnit.Framework;
using UnityEngine;

public sealed class JungleEnvironmentConfigTests
{
    [Test]
    public void JungleSkyTilesCoverTheGameCameraDuringLongDescent()
    {
        var config = Resources.Load<JungleEnvironmentConfig>("Jungle/JungleEnvironmentConfig");
        Assert.That(config, Is.Not.Null);
        Assert.That(config.CityBaselineY, Is.EqualTo(0f));
        Assert.That(config.Layers, Has.Length.EqualTo(8));

        JungleParallaxLayer sky = null;
        foreach (var layer in config.Layers)
        {
            if (layer != null && layer.Name == "Sky")
            {
                sky = layer;
                break;
            }
        }

        Assert.That(sky, Is.Not.Null);
        Assert.That(sky.Alpha, Is.EqualTo(1f));

        var sprite = Resources.Load<Sprite>("Jungle/" + sky.ResourcePath);
        Assert.That(sprite, Is.Not.Null);

        const float cameraHalfHeight = 6f;
        var tileHeight = sprite.bounds.size.y;
        for (var travel = -10000f; travel <= 10000f; travel += 10f)
        {
            var offset = Mathf.Repeat(travel * sky.VerticalSpeed + tileHeight * 0.5f, tileHeight) -
                         tileHeight * 0.5f;
            Assert.That(offset - tileHeight * 1.5f, Is.LessThanOrEqualTo(-cameraHalfHeight));
            Assert.That(offset + tileHeight * 1.5f, Is.GreaterThanOrEqualTo(cameraHalfHeight));
        }
    }

    [Test]
    public void JungleVisualConfigurationResolvesAllRuntimeSprites()
    {
        var config = Resources.Load<JungleEnvironmentConfig>("Jungle/JungleEnvironmentConfig");
        Assert.That(config, Is.Not.Null);

        foreach (var layer in config.Layers)
            Assert.That(Resources.Load<Sprite>("Jungle/" + layer.ResourcePath), Is.Not.Null,
                "Missing jungle parallax layer: " + layer.ResourcePath);

        var visuals = config.Visuals;
        Assert.That(visuals.LeftWallLightningLocalX, Is.EqualTo(0.175f));
        Assert.That(visuals.RightWallLightningLocalX, Is.EqualTo(-0.175f));
        var spritePaths = new[]
        {
            visuals.LeftWallPath, visuals.RightWallPath, visuals.StartPlatformPath,
            visuals.StartDoorFramePath, visuals.LeftDoorPath, visuals.RightDoorPath,
            visuals.StaticBombPath, visuals.MovingBombPath, visuals.BarrierLeftPath,
            visuals.BarrierRightPath
        };
        foreach (var path in spritePaths)
            Assert.That(Resources.Load<Sprite>("Jungle/" + path), Is.Not.Null,
                "Missing jungle sprite: " + path);

        Assert.That(JungleTheme.LoadPlatformFrames(), Has.Length.EqualTo(3));
        Assert.That(Resources.LoadAll<Sprite>("Jungle/" + visuals.WallVfxPath).Length,
            Is.GreaterThan(1));
        Assert.That(Resources.LoadAll<Sprite>("Jungle/" + visuals.StaticBombVfxPath).Length,
            Is.GreaterThan(1));
        Assert.That(Resources.LoadAll<Sprite>("Jungle/" + visuals.MovingBombVfxPath).Length,
            Is.GreaterThan(1));
        Assert.That(Resources.LoadAll<Sprite>("Jungle/" + visuals.BarrierVfxPath).Length,
            Is.GreaterThan(1));

        var weights = config.Hazards.HazardWeights;
        Assert.That(weights.RotatingSpikes, Is.GreaterThan(0f));
        Assert.That(weights.PopUpSpikes, Is.GreaterThan(0f));
        Assert.That(weights.StickySurface, Is.GreaterThan(0f));
        Assert.That(weights.RotatingLaser, Is.GreaterThan(0f));
    }

    [Test]
    public void JungleLayersKeepForegroundAnchorAndShareCityBaseline()
    {
        var config = Resources.Load<JungleEnvironmentConfig>("Jungle/JungleEnvironmentConfig");
        Assert.That(config, Is.Not.Null);

        var expectedNames = new[]
        {
            "Sky", "Sky 1", "Sky 2", "City 4", "City 3", "City 2", "City 1", "Glass"
        };
        var expectedPaths = new[]
        {
            "Environment/sky", "Environment/clouds_far", "Environment/clouds_near",
            "Environment/city_far", "Environment/city_mid", "Environment/city_near",
            "Environment/city_foreground", "Environment/glass"
        };
        var expectedSortingOrders = new[] { 0, 1, 2, 10, 30, 20, 40, 50 };
        for (var index = 0; index < expectedNames.Length; index++)
        {
            Assert.That(config.Layers[index].Name, Is.EqualTo(expectedNames[index]));
            Assert.That(config.Layers[index].ResourcePath, Is.EqualTo(expectedPaths[index]));
            Assert.That(config.Layers[index].SortingOrder, Is.EqualTo(expectedSortingOrders[index]));
        }

        const float sharedRepeatHeight = 8.05f;
        for (var index = 3; index <= 5; index++)
        {
            var city = config.Layers[index];
            var sprite = Resources.Load<Sprite>("Jungle/" + city.ResourcePath);
            Assert.That(city.AlignBottomToBaseline, Is.True, city.Name);
            Assert.That(city.Offset.y, Is.EqualTo(0f), city.Name);
            Assert.That(sprite.bounds.size.y * city.VerticalRepeatMultiplier,
                Is.EqualTo(sharedRepeatHeight).Within(0.01f), city.Name);
            Assert.That(city.VerticalSpeed, Is.EqualTo(0.12f), city.Name);
            Assert.That(city.HorizontalSpeed, Is.Zero, city.Name);
            if (index > 3)
                Assert.That(city.VerticalSpeed,
                    Is.EqualTo(config.Layers[index - 1].VerticalSpeed), city.Name);
        }

        Assert.That(config.Layers[6].AlignBottomToBaseline, Is.False,
            "City 1 uses its built-in grass horizon as the foreground anchor.");
        Assert.That(config.Layers[1].HorizontalSpeed, Is.Zero, "Sky 1");
        Assert.That(config.Layers[2].HorizontalSpeed, Is.Zero, "Sky 2");
        Assert.That(config.Layers[7].HorizontalSpeed, Is.Zero, "Glass");
    }
}
#endif
