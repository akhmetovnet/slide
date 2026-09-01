#if UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using System.Reflection;
using GameLogic;
using NUnit.Framework;
using UnityEngine;

public sealed class FutureCityEnvironmentControllerTests
{
    private const BindingFlags InstancePrivate = BindingFlags.Instance | BindingFlags.NonPublic;

    [Test]
    public void FutureCityLayersShareBaselineAndVerticalParallaxConfiguration()
    {
        var root = new GameObject("Future City Environment Test");
        var controller = root.AddComponent<FutureCityEnvironmentController>();
        var content = new GameObject("Content");
        content.transform.SetParent(root.transform, false);

        try
        {
            SetField(controller, "_content", content);
            SetField(controller, "_environmentLocation", ChallengeLocation.FutureCity);
            SetField(controller, "_random", new System.Random(6010));
            Invoke(controller, "BuildFutureCityEnvironment");

            var cityLayers = new[]
            {
                new CityLayer("Far City", "Environment/city_4", 10, -0.9f),
                new CityLayer("Mid City A", "Environment/city_3", 20, -2.3f),
                new CityLayer("Mid City B", "Environment/city_2", 30, -1.65f),
                new CityLayer("Near City", "Environment/city_1", 40, 0f)
            };
            var layers = (IList)GetField(controller, "_layers");
            var sharedRepeatHeight = float.NaN;
            var sharedVerticalSpeed = float.NaN;

            foreach (var city in cityLayers)
            {
                var sprite = FutureCityTheme.LoadSprite(city.ResourcePath);
                Assert.That(sprite, Is.Not.Null, "Missing Future City layer: " + city.ResourcePath);

                var layerRoot = content.transform.Find(city.Name);
                Assert.That(layerRoot, Is.Not.Null, "Missing layer root: " + city.Name);
                var renderer = layerRoot.Find(city.Name + " Tile 1").GetComponent<SpriteRenderer>();
                Assert.That(renderer.sprite, Is.EqualTo(sprite), city.Name);
                Assert.That(renderer.sortingOrder, Is.EqualTo(city.SortingOrder), city.Name);

                var layer = FindLayer(layers, layerRoot);
                var repeatHeight = (float)GetField(layer, "_height");
                var verticalSpeed = (float)GetField(layer, "_verticalSpeed");
                var offset = (Vector2)GetField(layer, "_offset");
                var tileContentOffsetY = (float)GetField(layer, "_tileContentOffsetY");

                Assert.That(offset.y, Is.EqualTo(city.BaselineOffsetY).Within(0.0001f), city.Name);
                Assert.That(tileContentOffsetY, Is.EqualTo(-sprite.bounds.min.y), city.Name);
                Assert.That(GetField(layer, "_usesTravelParallax"), Is.EqualTo(true), city.Name);

                if (float.IsNaN(sharedRepeatHeight))
                {
                    sharedRepeatHeight = repeatHeight;
                    sharedVerticalSpeed = verticalSpeed;
                }
                else
                {
                    Assert.That(repeatHeight, Is.EqualTo(sharedRepeatHeight).Within(0.0001f), city.Name);
                    Assert.That(verticalSpeed, Is.EqualTo(sharedVerticalSpeed).Within(0.0001f), city.Name);
                }
            }

            Assert.That(sharedRepeatHeight, Is.EqualTo(8.05f).Within(0.0001f));
            Assert.That(sharedVerticalSpeed, Is.EqualTo(0.12f).Within(0.0001f));

            var cloudLayers = new[]
            {
                new CloudLayer("Far Clouds", "Environment/clouds_far", 1, 0.72f, 0.27f, 3.2f),
                new CloudLayer("Near Clouds", "Environment/clouds_near", 2, 0.36f, -0.20f, -3.2f)
            };
            foreach (var cloud in cloudLayers)
            {
                var sprite = FutureCityTheme.LoadSprite(cloud.ResourcePath);
                Assert.That(sprite, Is.Not.Null, "Missing Future City cloud layer: " + cloud.ResourcePath);

                var layerRoot = content.transform.Find(cloud.Name);
                Assert.That(layerRoot, Is.Not.Null, "Missing layer root: " + cloud.Name);
                var renderer = layerRoot.Find(cloud.Name + " Tile 1").GetComponent<SpriteRenderer>();
                Assert.That(renderer.sprite, Is.EqualTo(sprite), cloud.Name);
                Assert.That(renderer.sortingOrder, Is.EqualTo(cloud.SortingOrder), cloud.Name);
                Assert.That(renderer.color.a, Is.EqualTo(cloud.Alpha).Within(0.0001f), cloud.Name);

                var layer = FindLayer(layers, layerRoot);
                var repeatHeight = (float)GetField(layer, "_height");
                var verticalSpeed = (float)GetField(layer, "_verticalSpeed");
                var horizontalSpeed = (float)GetField(layer, "_horizontalSpeed");
                var offset = (Vector2)GetField(layer, "_offset");
                var tileContentOffsetY = (float)GetField(layer, "_tileContentOffsetY");

                Assert.That(offset.y, Is.EqualTo(cloud.OffsetY).Within(0.0001f), cloud.Name);
                Assert.That(repeatHeight, Is.EqualTo(sharedRepeatHeight).Within(0.0001f), cloud.Name);
                Assert.That(verticalSpeed, Is.EqualTo(sharedVerticalSpeed).Within(0.0001f), cloud.Name);
                Assert.That(horizontalSpeed, Is.EqualTo(cloud.HorizontalSpeed).Within(0.0001f), cloud.Name);
                Assert.That(tileContentOffsetY, Is.EqualTo(0f).Within(0.0001f), cloud.Name);
                Assert.That(GetField(layer, "_usesTravelParallax"), Is.EqualTo(false), cloud.Name);

                Invoke(layer, "Update", 17.5f, -243.25f, 3.5f);
                var firstPosition = layerRoot.position;
                Invoke(layer, "Update", 17.5f, -243.25f, 3.5f);
                Assert.That(layerRoot.position, Is.EqualTo(firstPosition), cloud.Name + " is idempotent");
            }

            foreach (var travel in new[] { 0f, -243.25f, 1937.75f })
            {
                var anchor = cityLayers[cityLayers.Length - 1];
                var anchorRoot = content.transform.Find(anchor.Name);
                var anchorRenderer = anchorRoot.Find(anchor.Name + " Tile 1").GetComponent<SpriteRenderer>();
                var anchorLayer = FindLayer(layers, anchorRoot);
                Invoke(anchorLayer, "Update", 17.5f, travel, 3.5f);
                var anchorBoundsMinY = anchorRenderer.bounds.min.y;

                foreach (var city in cityLayers)
                {
                    var layerRoot = content.transform.Find(city.Name);
                    var renderer = layerRoot.Find(city.Name + " Tile 1").GetComponent<SpriteRenderer>();
                    var layer = FindLayer(layers, layerRoot);
                    Invoke(layer, "Update", 17.5f, travel, 3.5f);

                    var currentBoundsMinY = renderer.bounds.min.y;
                    Assert.That(currentBoundsMinY,
                        Is.EqualTo(anchorBoundsMinY + city.BaselineOffsetY - anchor.BaselineOffsetY).Within(0.0001f),
                        city.Name + " at travel " + travel);
                }
            }
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
        }
    }

    [Test]
    public void FutureCityAmbientActorsUseDirectionalCarSprites()
    {
        var root = new GameObject("Future City Ambient Test");
        var controller = root.AddComponent<FutureCityEnvironmentController>();
        var content = new GameObject("Content");
        content.transform.SetParent(root.transform, false);
        var cameraObject = new GameObject("Future City Ambient Test Camera");

        try
        {
            SetField(controller, "_content", content);
            SetField(controller, "_environmentLocation", ChallengeLocation.FutureCity);
            SetField(controller, "_random", new System.Random(6010));
            SetField(controller, "_camera", cameraObject.AddComponent<Camera>());
            Invoke(controller, "BuildFutureCityEnvironment");

            var actors = (IList)GetField(controller, "_actors");
            Assert.That(actors.Count, Is.EqualTo(6), "Future City should contain 4 cars and 2 birds.");
            Assert.That(content.transform.Find("Smoke 1"), Is.Null);
            Assert.That(content.transform.Find("Smoke 2"), Is.Null);
            Assert.That(content.transform.Find("Smoke 3"), Is.Null);

            var carSpeeds = new[] { 0.25f, 0.285f, 0.32f, 0.355f };
            var expectedCarSprites = new[]
            {
                new CarSprites("Ambient/Cars/car_1", "Ambient/Cars/car_5"),
                new CarSprites("Ambient/Cars/car_2", "Ambient/Cars/car_3"),
                new CarSprites("Ambient/Cars/car_6", "Ambient/Cars/car_4"),
                new CarSprites("Ambient/Cars/car_1", "Ambient/Cars/car_5")
            };
            for (var i = 0; i < carSpeeds.Length; i++)
            {
                var actor = FindActor(actors, "Car " + (i + 1));
                Assert.That(GetField(actor, "Kind").ToString(), Is.EqualTo("Car"));
                Assert.That((float)GetField(actor, "Direction"), Is.EqualTo(1f));
                Assert.That((float)GetField(actor, "Speed"), Is.EqualTo(carSpeeds[i]).Within(0.0001f));
                Assert.That(GetField(actor, "Frames"), Is.Null,
                    "Cars must not keep an animation frameset.");

                var renderer = (SpriteRenderer)GetField(actor, "Renderer");
                Assert.That(renderer.sortingOrder, Is.EqualTo(34 + i));
                Assert.That(renderer.sprite, Is.EqualTo(FutureCityTheme.LoadSprite(
                    expectedCarSprites[i].RightResourcePath)));
                Assert.That(GetDirectionalSprite(actor, "RightSprite"), Is.EqualTo(renderer.sprite));
                Assert.That(GetDirectionalSprite(actor, "LeftSprite"), Is.EqualTo(
                    FutureCityTheme.LoadSprite(expectedCarSprites[i].LeftResourcePath)));
                Assert.That(renderer.flipX, Is.False);
            }

            var carFrames = FutureCityTheme.LoadFrames("Ambient/Cars");
            Assert.That(carFrames, Is.Not.Null.And.Length.EqualTo(6));
            for (var i = 1; i <= 6; i++)
                Assert.That(FutureCityTheme.LoadSprite("Ambient/Cars/car_" + i), Is.Not.Null);

            for (var i = 0; i < 2; i++)
            {
                var actor = FindActor(actors, "Birds " + (i + 1));
                Assert.That(GetField(actor, "Kind").ToString(), Is.EqualTo("Birds"));
                Assert.That((float)GetField(actor, "Direction"),
                    Is.EqualTo(i == 0 ? 1f : -1f));
                Assert.That((float)GetField(actor, "Speed"),
                    Is.EqualTo(0.16f + i * 0.04f).Within(0.0001f));
                Assert.That(((SpriteRenderer)GetField(actor, "Renderer")).sortingOrder, Is.EqualTo(36));
                Assert.That(((Array)GetField(actor, "Frames")).Length, Is.EqualTo(1));
                Assert.That(GetField(actor, "DirectionalSprites"), Is.Null);
            }

            for (var i = 0; i < carSpeeds.Length; i++)
            {
                var actor = FindActor(actors, "Car " + (i + 1));
                var transform = (Transform)GetField(actor, "Transform");
                var renderer = (SpriteRenderer)GetField(actor, "Renderer");
                var rightSprite = GetDirectionalSprite(actor, "RightSprite");
                var leftSprite = GetDirectionalSprite(actor, "LeftSprite");

                transform.position = Vector3.zero;
                Invoke(controller, "UpdateAmbientActors");
                Invoke(controller, "UpdateAmbientActors");
                Assert.That((float)GetField(actor, "Direction"), Is.EqualTo(1f),
                    "Car direction must stay stable away from a horizontal edge.");
                Assert.That(renderer.sprite, Is.EqualTo(rightSprite),
                    "Car sprite must stay stable while travelling right.");

                transform.position = new Vector3(1.86f, 0f, 0f);
                Invoke(controller, "UpdateAmbientActors");
                Assert.That((float)GetField(actor, "Direction"), Is.EqualTo(-1f));
                Assert.That(renderer.sprite, Is.EqualTo(leftSprite),
                    "Car must use its paired left sprite after reaching the right edge.");

                transform.position = Vector3.zero;
                Invoke(controller, "UpdateAmbientActors");
                Assert.That((float)GetField(actor, "Direction"), Is.EqualTo(-1f));
                Assert.That(renderer.sprite, Is.EqualTo(leftSprite),
                    "Car sprite must stay stable while travelling left.");

                transform.position = new Vector3(-1.86f, 0f, 0f);
                Invoke(controller, "UpdateAmbientActors");
                Assert.That((float)GetField(actor, "Direction"), Is.EqualTo(1f));
                Assert.That(renderer.sprite, Is.EqualTo(rightSprite),
                    "Car must restore its paired right sprite after reaching the left edge.");
                Assert.That(renderer.flipX, Is.False);
            }

            var firstBird = FindActor(actors, "Birds 1");
            var birdRenderer = (SpriteRenderer)GetField(firstBird, "Renderer");
            var birdSprite = birdRenderer.sprite;
            var birdTransform = (Transform)GetField(firstBird, "Transform");
            birdTransform.position = new Vector3(1.86f, 0f, 0f);
            Invoke(controller, "UpdateAmbientActors");
            Assert.That((float)GetField(firstBird, "Direction"), Is.EqualTo(1f),
                "Bird movement must retain its existing wrap direction.");
            Assert.That(birdRenderer.sprite, Is.EqualTo(birdSprite));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(cameraObject);
            UnityEngine.Object.DestroyImmediate(root);
        }
    }

    private static object FindLayer(IList layers, Transform root)
    {
        foreach (var layer in layers)
        {
            if (ReferenceEquals(GetField(layer, "_root"), root))
                return layer;
        }

        Assert.Fail("No parallax layer was created for " + root.name);
        return null;
    }

    private static object FindActor(IList actors, string name)
    {
        foreach (var actor in actors)
        {
            var transform = (Transform)GetField(actor, "Transform");
            if (transform != null && transform.name == name)
                return actor;
        }

        Assert.Fail("No ambient actor was created for " + name);
        return null;
    }

    private static object GetField(object target, string name)
    {
        var field = target.GetType().GetField(name,
            InstancePrivate | BindingFlags.Public);
        Assert.That(field, Is.Not.Null, "Missing field: " + name);
        return field.GetValue(target);
    }

    private static Sprite GetDirectionalSprite(object actor, string direction)
    {
        return (Sprite)GetField(GetField(actor, "DirectionalSprites"), direction);
    }

    private static void SetField(object target, string name, object value)
    {
        var field = target.GetType().GetField(name, InstancePrivate);
        Assert.That(field, Is.Not.Null, "Missing field: " + name);
        field.SetValue(target, value);
    }

    private static object Invoke(object target, string name, params object[] arguments)
    {
        var method = target.GetType().GetMethod(name, InstancePrivate);
        Assert.That(method, Is.Not.Null, "Missing method: " + name);
        return method.Invoke(target, arguments);
    }

    private readonly struct CityLayer
    {
        public readonly string Name;
        public readonly string ResourcePath;
        public readonly int SortingOrder;
        public readonly float BaselineOffsetY;

        public CityLayer(string name, string resourcePath, int sortingOrder, float baselineOffsetY)
        {
            Name = name;
            ResourcePath = resourcePath;
            SortingOrder = sortingOrder;
            BaselineOffsetY = baselineOffsetY;
        }
    }

    private readonly struct CloudLayer
    {
        public readonly string Name;
        public readonly string ResourcePath;
        public readonly int SortingOrder;
        public readonly float Alpha;
        public readonly float HorizontalSpeed;
        public readonly float OffsetY;

        public CloudLayer(string name, string resourcePath, int sortingOrder, float alpha,
            float horizontalSpeed, float offsetY)
        {
            Name = name;
            ResourcePath = resourcePath;
            SortingOrder = sortingOrder;
            Alpha = alpha;
            HorizontalSpeed = horizontalSpeed;
            OffsetY = offsetY;
        }
    }

    private readonly struct CarSprites
    {
        public readonly string RightResourcePath;
        public readonly string LeftResourcePath;

        public CarSprites(string rightResourcePath, string leftResourcePath)
        {
            RightResourcePath = rightResourcePath;
            LeftResourcePath = leftResourcePath;
        }
    }
}
#endif
