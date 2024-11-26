using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Game.Logic;
using Memo.Utilities;
using Game.Core;
public abstract class Ability : ScriptableObject, ICastable, IInventoryItem
{
	[field: Header("Ability Stats")]
	[field: SerializeField] public string Name { get; set; }
	[field: SerializeField, TextArea(10, 99)] public string Description { get; set; }
	public float Price { get; set; }
	[field: SerializeField] public Texture2D Icon { get; set; }

	[Header("Ability VFX")]
	[field: SerializeField, Header("Ability Hit VFX")] public GameObject AbilityHitVFX { get; private set; }
	[Header("Ability SFX")]
	[field: SerializeField] public AudioClip[] AbilityCastSFX { get; private set; }
	[field: SerializeField] public AudioClip[] AbilitySpawnedSFX { get; private set; }
	[field: SerializeField] public AudioClip[] AbilityHitSFX { get; private set; }

	[field: Header("Ability Scalings:"), Space(2)]
	[field: SerializeField] public Element Element { get; private set; } = Element.Water;
	[field: SerializeField] public float Damage { get; private set; }
	[field: SerializeField] public float Cooldown { get; private set; }
	public DVar<float> ActiveCooldown { get; private set; } = new(0);
	public bool IsOnCooldown { get; set; }
	protected Entity controller;
	protected int index;
	private CancellationTokenSource cts;
	public virtual void OnCast()
	{
		AbilityCooldown(cts.Token).Forget();
	}
	public T Create<T>(int _index, Entity _controller, T _abilityOriginal) where T : Ability
	{
		T _ability = Instantiate(_abilityOriginal);
		_ability.SetAbility(_index, _controller);
		return _ability;
	}
	protected void changeAnimation(string _animationName, int _layer = 1, float _crossFade = .1f)
	{
		controller.SendMessage("ChangeAnimation", new AnimationData(_animationName, _layer, _crossFade));
	}
	public virtual void SetAbility(int _index, Entity _controller)
	{
		index = _index;
		controller = _controller;
		cts = new();
	}
	/// <summary>
	/// @alex-memo 2023
	/// Starts the cooldown of the ability
	/// </summary>
	/// <returns></returns>
	private async UniTask AbilityCooldown(CancellationToken _token)
	{
		ActiveCooldown.Value = 0;
		IsOnCooldown = true;
		while (ActiveCooldown.Value < Cooldown-.05)
		{
			await UniTask.Delay(100, cancellationToken: _token);
			ActiveCooldown.Value += .1f;
		}
		IsOnCooldown = false;
	}
	public void SetActiveCooldown(float _amount)
	{
		ActiveCooldown.Value = _amount;
	}
	public void SetRemainingCooldown(float _amount)
	{
		ActiveCooldown.Value = Cooldown - _amount;
	}
	public void ReduceCooldown(float _amount)
	{
		if (ActiveCooldown.Value > Cooldown) { return; }
		ActiveCooldown.Value += _amount;
	}
	public void ResetCooldown()
	{
		ActiveCooldown.Value = Cooldown;
		IsOnCooldown = false;
	}
	protected virtual void OnDestroy()
	{
		if (cts != null) { cts.Cancel(); cts.Dispose(); }
		ActiveCooldown.Dispose();
	}
}
