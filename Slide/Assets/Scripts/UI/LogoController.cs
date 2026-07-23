using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
// using Plugins;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class LogoController : MonoBehaviour
{
    [SerializeField] private GameObject _logoPanel;
    [SerializeField] private AnimationClip _logoAnimation;
    [SerializeField] private Animator _logoAnimator;
    [SerializeField] private Image _logoImage;

    [Inject] private UIController _uiController;
    
    private static readonly int _start = Animator.StringToHash("Start");

    // Start is called before the first frame update
    void Start()
    {
        var sequence = DOTween.Sequence();
        sequence.AppendInterval(.1f)
            .Append(_logoImage.DOFade(1, .5f))
            .AppendCallback(() => _logoAnimator.SetTrigger(_start))
            .AppendInterval(_logoAnimation.length)
            .OnComplete(() =>
            {
                _uiController.FinishSplashScreen();
                _logoPanel.SetActive(false);
            });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
