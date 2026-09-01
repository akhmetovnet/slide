using UnityEngine;

namespace GameLogic
{
    [CreateAssetMenu(fileName = "JungleEnvironmentConfig", menuName = "Slide/Jungle Environment Config")]
    public sealed class JungleEnvironmentConfig : ScriptableObject
    {
        [SerializeField] private JungleParallaxLayer[] _layers = new JungleParallaxLayer[0];
        [SerializeField] private JungleVisualResources _visuals = new JungleVisualResources();
        [SerializeField] private JungleHazardSettings _hazards = new JungleHazardSettings();
        [SerializeField] private float _cityBaselineY;

        public JungleParallaxLayer[] Layers => _layers;
        public JungleVisualResources Visuals => _visuals;
        public JungleHazardSettings Hazards => _hazards;
        public float CityBaselineY => _cityBaselineY;
    }

    [System.Serializable]
    public sealed class JungleParallaxLayer
    {
        public string Name;
        public string ResourcePath;
        public Vector2 Offset;
        [Range(0f, 1f)] public float VerticalSpeed;
        [Min(1f)] public float VerticalRepeatMultiplier = 1f;
        public bool AlignBottomToBaseline;
        [Range(-1f, 1f)] public float HorizontalSpeed;
        public int SortingOrder;
        [Range(0f, 1f)] public float Alpha = 1f;
    }

    [System.Serializable]
    public sealed class JungleVisualResources
    {
        [Header("Start area")]
        public string LeftWallPath;
        public string RightWallPath;
        public string StartPlatformPath;
        public Vector2 StartPlatformOffset;
        public string StartDoorFramePath;
        public string LeftDoorPath;
        public string RightDoorPath;
        public string WallVfxPath;
        public float LeftWallLightningLocalX = 0.175f;
        public float RightWallLightningLocalX = -0.175f;

        [Header("Platforms")]
        public string[] PlatformPaths = new string[0];

        [Header("Existing hazards")]
        public string StaticBombPath;
        public string MovingBombPath;
        public string BarrierLeftPath;
        public string BarrierRightPath;
        public string StaticBombVfxPath;
        public string MovingBombVfxPath;
        public string BarrierVfxPath;
    }

    [System.Serializable]
    public sealed class JungleHazardSettings
    {
        [Header("Generator")]
        public ChallengeHazardWeights HazardWeights;

        [Header("Rotating spikes")]
        public string RotatingSpikesPlaceholderPath;
        public float RotatingSpikesDegreesPerSecond = 105f;
        public float RotatingSpikesStartAngle;
        public int RotatingSpikesSectionCount = 4;
        public float RotatingSpikesSectionDistance = 0.48f;
        public float RotatingSpikesColliderRadius = 0.18f;
        public float RotatingSpikesScale = 0.75f;

        [Header("Pop-up spikes")]
        public string PopUpSpikesPlaceholderPath;
        public float PopUpWarningTime = 0.75f;
        public float PopUpExtendTime = 0.28f;
        public float PopUpActiveTime = 0.7f;
        public float PopUpRetractTime = 0.28f;
        public float PopUpCooldownTime = 0.75f;
        public float PopUpHiddenHeight = 0.55f;
        public float PopUpActiveHeight = 0.95f;
        public Vector2 PopUpColliderSize = new Vector2(0.85f, 0.42f);

        [Header("Sticky surface")]
        public string StickyVisualPath;
        [Range(0.1f, 1f)] public float StickyMovementMultiplier = 0.52f;
        public Vector2 StickyColliderSize = new Vector2(1.2f, 0.36f);

        [Header("Rotating electric barrier")]
        public float RotatingBarrierDegreesPerSecond = 72f;
        public float RotatingBarrierStartAngle;
        public float RotatingBarrierLength = 1.3f;
        public float RotatingBarrierThickness = 0.18f;
        public bool RotatingBarrierIsContinuous = true;
        public float RotatingBarrierActiveTime = 1f;
        public float RotatingBarrierInactiveTime = 0.4f;
    }
}
