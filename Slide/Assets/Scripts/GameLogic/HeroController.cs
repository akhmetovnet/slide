using System;
using UnityEngine;
using System.Collections;
using System.IO;
using GameLogic;
using Installers;
using Signals;
using UI;
using UniRx;
using Zenject;
using UniRx.Triggers;

public class HeroController : MonoBehaviour
{
	[SerializeField] private GameObject _shield;
	[SerializeField] private Animator _animator;
	[SerializeField] private Animator _shieldAnimator;
	[SerializeField] private SkinHerlper[] skins;
	[SerializeField] private TutorialView _tutorial;
    
    [Inject] private SoInstaller.GameSettings _gameSettings;
    [Inject] private GameController _gameController;
    [Inject] private ObjectController _objectController;
    [Inject] private SoundController _soundController;
    [Inject] private MissionsController _missionsController;
    


    private Vector2 _direction;
    private CompositeDisposable _compositeDisposable;
	private Collider2D _currentLine;
	private Collider2D _lastLine;
	private Rigidbody2D _rigidbody;
	private BoxCollider2D _collider;
	private SignalBus _signalBus;
	private int _shieldCounter;
	private bool _isAcceleration;
	private int _lineDropped;
	private int _linesToDrop;
	private Vector3 _touchLinePosition;
	private Transform _transform;
	private bool _isPerfect;
	private int _perfectCount;
	private int _stickyContacts;
	
	private static readonly int IsSlide = Animator.StringToHash("isSlide");
	private static readonly int IsBreak = Animator.StringToHash("IsBreak");

	[Inject]
	public void Construct(SignalBus signalBus)
	{
		_signalBus = signalBus;
		
		_signalBus.Subscribe<ClearAll>(OnStart);
	}

	private void OnStart()
	{
		_transform.localPosition = new Vector3(0, 0.4f, 0);
	}

	// Use this for initialization
	void Start ()
	{
	    _compositeDisposable = new CompositeDisposable();
		_rigidbody = GetComponent<Rigidbody2D>();
		_rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
		_collider = GetComponent<BoxCollider2D>();
		_transform = GetComponent<Transform>();

		SetSkin(PlayerPrefs.GetInt($"CurrentSkin", 0));
	    
	    this.OnCollisionEnter2DAsObservable()
	        .Where(x => x.transform.CompareTag("Line") && _currentLine == null)
	        .Subscribe(_ => LineCollisionEnter(_.collider))
	        .AddTo(_compositeDisposable);
	    
	    this.OnCollisionEnter2DAsObservable()
	        .Where(x =>  x.transform.CompareTag("Wall"))
	        .Subscribe(_ => Death(_.transform))
	        .AddTo(_compositeDisposable);
	    
	    this.OnTriggerEnter2DAsObservable()
		    .Where(x => x.transform.CompareTag("Thorn") && !_isAcceleration)
		    .Subscribe(_ => Death(_.transform))
		    .AddTo(_compositeDisposable);

	    this.FixedUpdateAsObservable()
		    .Where(x => _currentLine != null)
		    .Subscribe(_ => Slide())
		    .AddTo(_compositeDisposable);
	    
	    this.OnTriggerExit2DAsObservable()
		    .Where(x => x.transform.CompareTag("Line") && _gameController.IsTutorial && _tutorial.Step == 3)
		    .Subscribe(_ => _tutorial.OpenView(_tutorial.Step))
		    .AddTo(_compositeDisposable);

	    this.OnTriggerExit2DAsObservable()
		    .Where(x => x.transform.CompareTag("Line") && _isAcceleration)
		    .Subscribe(_ => UpdateAcceleration())
		    .AddTo(_compositeDisposable);

		this.OnTriggerEnter2DAsObservable()
			.Where(x => x.transform.CompareTag("WallTrigger") && _shieldCounter < _gameSettings.bonusSettings.shield)
			.Subscribe(_ => UpdateShield())
			.AddTo(_compositeDisposable);
		
		this.OnTriggerEnter2DAsObservable()
			.Where(x => x.GetComponent<BonusController>() != null)
			.Subscribe(x => CollectBonus(x.GetComponent<BonusController>()))
			.AddTo(_compositeDisposable);

		this.OnTriggerEnter2DAsObservable()
			.Where(x =>
			{
				var hazard = x.GetComponentInParent<ChallengeHazardRuntime>();
				return hazard != null && hazard.IsSticky;
			})
			.Subscribe(_ => _stickyContacts++)
			.AddTo(_compositeDisposable);

		this.OnTriggerExit2DAsObservable()
			.Where(x =>
			{
				var hazard = x.GetComponentInParent<ChallengeHazardRuntime>();
				return hazard != null && hazard.IsSticky;
			})
			.Subscribe(_ => _stickyContacts = Mathf.Max(0, _stickyContacts - 1))
			.AddTo(_compositeDisposable);
		
		this.OnTriggerEnter2DAsObservable()
			.Where(x => x.transform.CompareTag("Perfect"))
			.Subscribe(_ => PerfectJump())
			.AddTo(_compositeDisposable);
	}

	public void SetSkin(int index)
	{
		if (skins == null || skins.Length == 0)
			return;

		var safeIndex = Mathf.Clamp(index, 0, skins.Length - 1);
		if (safeIndex != index)
			PlayerPrefs.SetInt("CurrentSkin", safeIndex);

		var skin = skins[safeIndex];
		if (skin == null)
			return;

		_animator.runtimeAnimatorController = skin.GetSkin();
	}

	private void Slide()
	{
		if (!_isAcceleration)
		{
			var stickyMultiplier = _stickyContacts > 0 ? 0.52f : 1f;
			_rigidbody.linearVelocity = _direction.normalized * _gameController.CurrentPlayerSpeed * stickyMultiplier;
		}
	}
    
    private void Death(Transform otherTransform)
    {
	    if (_shieldCounter < _gameSettings.bonusSettings.shield)
	    {
		    KillHero();
		}
	    else
	    {
		    if (otherTransform.CompareTag("Thorn"))
		    {
			    _signalBus.Fire(new ThornIsOut() { thorn = otherTransform.GetComponentInParent<ThornController>() });
		    }
		    else
		    {
			    _rigidbody.MovePosition(_rigidbody.position - _direction/10);
			    Jump();
		    }
		    _shieldCounter = 0;
		    _shieldAnimator.SetBool(IsBreak, true);
		    Observable.Timer(TimeSpan.FromSeconds(0.067)).Subscribe(delegate { _shield.SetActive(false); });
	    }

	    _isPerfect = false;
	    _perfectCount = 0;
	    _signalBus.Fire(new Perfect() {count = _perfectCount});
    }

	public void FailChallenge()
	{
		if (!gameObject.activeSelf)
			return;

		_shieldCounter = 0;
		KillHero();
		_isPerfect = false;
		_perfectCount = 0;
		_signalBus.Fire(new Perfect() {count = 0});
	}

	private void KillHero()
	{
		_soundController.PlaySound("death");
		_currentLine = null;
		_stickyContacts = 0;
		_rigidbody.simulated = false;
		_rigidbody.linearVelocity = Vector2.zero;
		_rigidbody.angularVelocity = 0;
		gameObject.SetActive(false);
		_signalBus.Fire<HeroDie>();

		if (_gameController.IsVabrate)
			Handheld.Vibrate();
	}

	private void CollectBonus(BonusController bonus)
	{
		if (bonus == null)
			return;

		if (bonus.Type == BonusType.Acceleration)
		{
			SetAcceleration(bonus);
			return;
		}

		_soundController.PlaySound("get_coin");
		if (bonus.Type == BonusType.Coin)
			_missionsController.Check(_gameController.Mode, MissionTarget.Coin);
		_signalBus.Fire(new TouchBonus() {bonus = bonus});
	}

    private void LineCollisionEnter(Collider2D line)
    {
	    _soundController.PlaySound("grounding");
	    _missionsController.Check(_gameController.Mode, MissionTarget.Platforms);
	    
	    _signalBus.Fire<TouchLine>();

	    _touchLinePosition = _transform.localPosition;
        _currentLine = line;
	    var lineController = _currentLine.GetComponentInParent<LineController>();
        _direction = lineController.GetDirection();
	    _animator.SetBool(IsSlide, true);
	    var sign = Mathf.Sign(_direction.x);
	    var localScale = _transform.localScale;
	    localScale = new Vector3(sign * Mathf.Abs(localScale.x), localScale.y, localScale.z);
	    _transform.localScale = localScale;
	    _transform.Rotate(Vector3.forward, lineController.AngleDegree);
	    
	    if (!_isPerfect)
	    {
		    _perfectCount = 0;
		    _signalBus.Fire(new Perfect() {count = _perfectCount});
	    }
	    else
		    _isPerfect = false;

	    if (_gameController.IsTutorial && _tutorial.Step == 0)
		    _tutorial.OpenView(_tutorial.Step);
    }
	
	private void UpdateShield()
	{
		if (_gameController.IsTutorial && _tutorial.Step == 2)
		{
			_tutorial.SetSprite();
			_tutorial.OpenView(_tutorial.Step);
			_shieldCounter = _gameSettings.bonusSettings.shield - 1;
		}
		
		_shieldCounter++;
		_shield.SetActive(_shieldCounter >= _gameSettings.bonusSettings.shield);
		if (_shieldCounter >= _gameSettings.bonusSettings.shield)
		{
			_missionsController.Check(_gameController.Mode, MissionTarget.Shield);
			_soundController.PlaySound("energo_shield");
			_shieldAnimator.SetBool(IsBreak, false);
		}
	}

	private void UpdateAcceleration()
	{
		++_lineDropped;
		if (_lineDropped > 1)
		{
			_signalBus.Fire<TouchLine>();
		}

		if (_lineDropped == _linesToDrop)
		{
			_lineDropped = 0;
			_isAcceleration = false;
			_collider.isTrigger = false;
			_currentLine = null;
		}
	}

	private void SetAcceleration(BonusController bonus)
	{
		_missionsController.Check(_gameController.Mode, MissionTarget.Accelerate);
		
		_isAcceleration = true;
		_collider.isTrigger = true;
		_signalBus.Fire(new TouchBonus() {bonus = bonus});

		_linesToDrop = _currentLine == null ? _gameSettings.bonusSettings.accelerationLines + 1 : _gameSettings.bonusSettings.accelerationLines;
		
				
		_shieldCounter = _gameSettings.bonusSettings.shield;
		_shield.SetActive(_shieldCounter >= _gameSettings.bonusSettings.shield);
		_objectController.RemoveAccelerationThorn(_lastLine.GetComponentInParent<LineController>(), _linesToDrop);
		
		if(_currentLine == null)
			_signalBus.Fire<TouchLine>();
		else
			Jump();

	}

    public void Jump()
    {
	    if(_gameController.IsTutorial)
		    _tutorial.CloseAll();
	    if(_gameController.IsTutorial && _tutorial.Step == 2)
		    return;
	    
	    if(!_rigidbody.simulated) _rigidbody.simulated = true;
	    
        if (_currentLine != null)
        {
	        _soundController.PlaySound("jump");
	        
	        _lastLine = _currentLine;
            _currentLine.isTrigger = true;
            _animator.SetBool(IsSlide, false);
	        _currentLine = null;
	        _rigidbody.linearVelocity = new Vector2(0, _rigidbody.linearVelocity.y);
	        _rigidbody.angularVelocity = 0;
	        _transform.localRotation = Quaternion.identity;
        }
    }

    public void PerfectJump()
    {
	    if(_gameController.IsTutorial && _tutorial.Step == 1)
		    _tutorial.OpenView(_tutorial.Step);
	    
	    _missionsController.Check(_gameController.Mode, MissionTarget.PerfectJump);
	    
	    _soundController.PlaySound("lucky_jump");
	    
	    _isPerfect = true;
	    _perfectCount++;
	    
	    _signalBus.Fire(new Perfect(){count = _perfectCount});
    }

    public void Reset()
    {
	    _currentLine = null;
	    _rigidbody.linearVelocity = Vector2.zero;
	    _rigidbody.angularVelocity = 0;
        _rigidbody.simulated = false;
        _transform.localRotation = Quaternion.identity;
	    _shieldCounter = 0;
	    _stickyContacts = 0;
	    _shield.SetActive(false);
	    _animator.SetBool(IsSlide, false);
	    gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
    
    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Pause()
    {
        _rigidbody.simulated = false;
    }
    
    public void PreContinue()
    {
	    _transform.localPosition = _touchLinePosition;
	    _stickyContacts = 0;
	    if(_lastLine != null)
			_lastLine.isTrigger = false;
	    gameObject.SetActive(true);
	    _animator.SetBool(IsSlide, true);
	    _shieldCounter = _gameSettings.bonusSettings.shield;
	    _shield.SetActive(_shieldCounter >= _gameSettings.bonusSettings.shield);
    }

    public void ContinueGame()
    {
	    _soundController.PlayMusic("gameplay_theme", false);

	    
	    _rigidbody.simulated = true;
    }
}
 
