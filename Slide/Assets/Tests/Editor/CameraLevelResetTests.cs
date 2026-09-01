#if UNITY_INCLUDE_TESTS
using System.Collections;
using System.Linq;
using System.Reflection;
using GameLogic;
using Installers;
using NUnit.Framework;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

public sealed class CameraLevelResetTests
{
    private const float StartDoorHeroOffsetY = -0.08f;
    private const BindingFlags InstancePrivate = BindingFlags.Instance | BindingFlags.NonPublic;

    [UnityTest]
    public IEnumerator ResultRestartSnapsCameraToTheNewLevelStartWithoutChangingContinueCameraBehavior()
    {
        yield return new EnterPlayMode();

        try
        {
            SceneManager.LoadScene("Game");
            yield return null;
            yield return null;

            var gameController = Object.FindObjectsByType<GameController>(FindObjectsInactive.Exclude).Single();
            var heroController = Object.FindObjectsByType<HeroController>(FindObjectsInactive.Exclude).Single();
            var cameraController = Object.FindObjectsByType<CameraController>(FindObjectsInactive.Exclude).Single();
            var uiController = Object.FindObjectsByType<UIController>(FindObjectsInactive.Exclude).Single();
            var startDoor = ((Transform)GetField(gameController, "_leftDoor")).parent;
            var settings = (SoInstaller.GameSettings)GetField(gameController, "_settings");
            var expectedHeroPosition = startDoor.TransformPoint(Vector3.up * StartDoorHeroOffsetY);
            var expectedCameraY = expectedHeroPosition.y - settings.offset.y;

            gameController.Mode = GameMode.Challenge;
            SetField(uiController, "_isGame", false);
            heroController.transform.position = new Vector3(0f, expectedHeroPosition.y - 20f, 0f);
            cameraController.transform.position = new Vector3(0f, expectedCameraY - 20f, -10f);

            uiController.PlayGame(false);

            Assert.That(heroController.transform.position, Is.EqualTo(expectedHeroPosition).Using(Vector3ComparerWithEqualsOperator.Instance),
                "A result-menu restart must reset the hero to the new level start door.");
            Assert.That(cameraController.transform.position.y, Is.EqualTo(expectedCameraY).Within(0.0001f),
                "A result-menu restart must snap the camera to the reset hero position.");

            var cameraYBeforeUpwardTarget = cameraController.transform.position.y;
            heroController.transform.position = new Vector3(0f, expectedHeroPosition.y + 10f, 0f);
            Invoke(cameraController, "LateUpdate");
            Assert.That(cameraController.transform.position.y, Is.EqualTo(cameraYBeforeUpwardTarget).Within(0.0001f),
                "Normal gameplay camera follow must not move the camera upward.");

            var cameraPositionBeforeContinue = cameraController.transform.position;
            gameController.PreContinue();
            gameController.ContinueGame();
            Assert.That(cameraController.transform.position, Is.EqualTo(cameraPositionBeforeContinue).Using(Vector3ComparerWithEqualsOperator.Instance),
                "Continue must preserve the current camera position instead of snapping to a level start.");
        }
        finally
        {
            Time.timeScale = 1f;
        }

        yield return new ExitPlayMode();
    }

    private static object GetField(object target, string fieldName)
    {
        return target.GetType().GetField(fieldName, InstancePrivate).GetValue(target);
    }

    private static void SetField(object target, string fieldName, object value)
    {
        target.GetType().GetField(fieldName, InstancePrivate).SetValue(target, value);
    }

    private static void Invoke(object target, string methodName)
    {
        target.GetType().GetMethod(methodName, InstancePrivate).Invoke(target, null);
    }
}
#endif
