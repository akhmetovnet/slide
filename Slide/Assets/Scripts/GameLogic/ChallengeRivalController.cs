using UnityEngine;

namespace GameLogic
{
    public sealed class ChallengeRivalController : MonoBehaviour
    {
        private HeroController _hero;
        private SpriteRenderer _heroRenderer;
        private SpriteRenderer _renderer;
        private ChallengeObjectiveType _type;

        public static ChallengeRivalController Create(HeroController hero)
        {
            var rivalObject = new GameObject("Challenge Rival");
            var controller = rivalObject.AddComponent<ChallengeRivalController>();
            controller.Initialize(hero);
            return controller;
        }

        private void Initialize(HeroController hero)
        {
            _hero = hero;
            _heroRenderer = hero.GetComponent<SpriteRenderer>();
            _renderer = gameObject.AddComponent<SpriteRenderer>();
            _renderer.sortingLayerID = _heroRenderer.sortingLayerID;
            _renderer.sortingOrder = _heroRenderer.sortingOrder - 1;
            transform.localScale = hero.transform.localScale * 0.9f;
            gameObject.SetActive(false);
        }

        public void Show(ChallengeObjectiveType type)
        {
            _type = type;
            _renderer.color = type == ChallengeObjectiveType.CatchCriminal
                ? new Color(1f, 0.35f, 0.25f, 0.92f)
                : new Color(0.25f, 0.9f, 1f, 0.92f);
            gameObject.SetActive(true);
            RefreshSprite();
        }

        public void SetProgress(float playerProgress, float rivalProgress)
        {
            if (!gameObject.activeSelf || _hero == null)
                return;

            RefreshSprite();
            var lead = Mathf.Clamp(rivalProgress - playerProgress, -2.25f, 2.25f);
            var side = _type == ChallengeObjectiveType.CatchCriminal ? 0.85f : -0.85f;
            var bob = Mathf.Sin(Time.time * 4f) * 0.08f;
            transform.position = new Vector3(
                side,
                _hero.transform.position.y - lead * 1.35f + bob,
                _hero.transform.position.z + 0.1f);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void RefreshSprite()
        {
            if (_renderer != null && _heroRenderer != null)
                _renderer.sprite = _heroRenderer.sprite;
        }
    }
}
