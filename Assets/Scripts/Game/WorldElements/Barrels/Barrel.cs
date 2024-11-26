using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Game.Logic;
namespace Game.Barrels
{
	public class Barrel : NetworkBehaviour, IDamageable
	{
		public NetworkVariable<float> CurrentHealth { get; protected set; } = new(0);
		public static float MaxHp => 800;
		public NetworkVariable<float> Shield { get; protected set; } = new(0);
		[SerializeField] protected List<BarrelCommand> commands = new();
		protected BarrelCommand myCommand;
		protected int commandIndex = 0;
		private MeshRenderer meshRenderer;
		private bool hasExploded = false;
		[ServerRpc(RequireOwnership = false)]
		public void TakeDamageServerRpc(float _damage, Element _element, UserData _dmgDealerData, DamageSource _damageSource)
		{
			takeDamage(_damage, _element, _dmgDealerData, _damageSource);
		}
		public void ServerTakeDamage(float _damage, Element _element, UserData _dmgDealerData, DamageSource _damageSource)
		{
			takeDamage(_damage, _element, _dmgDealerData, _damageSource);
		}
		protected void takeDamage(float _damage, Element _element, UserData _dmgDealerData, DamageSource _damageSource)
		{
			if (hasExploded) { return; }
			if (_damage <= 0) { return; }
			if (CurrentHealth.Value - _damage <= 0) { _damage = CurrentHealth.Value; }
			if (_damage <= 0) { return; }
			CurrentHealth.Value -= _damage;
			if (CurrentHealth.Value <= 0)
			{
				CurrentHealth.Value = 0;
				Explode();
			}

			InstantiateDamageCanvasClientRpc(_damage, _element);
		}
		[ClientRpc]
		private void InstantiateDamageCanvasClientRpc(float _damage, Element _damageType)
		{
			if (IsLocalPlayer) { return; }
			DamageCanvas _damageCanvas = GetComponentInChildren<DamageCanvas>();
			if (_damageCanvas != null)
			{
				_damageCanvas.AddDamage(_damage, _damageType);
				return;
			}
			GameObject _canvas = Instantiate(Resources.Load<GameObject>("DamageCanvas"), transform);

			_canvas.GetComponent<DamageCanvas>().SetDamage(_damage, _damageType);
		}
		protected virtual void Explode()
		{
			if (!IsServer) { return; }
			hasExploded = true;
			Debug.Log("Explode");
			if (myCommand == null) { return; }
			myCommand.Execute();
			NetworkObject.Despawn();
		}

		public override void OnNetworkSpawn()
		{
			meshRenderer = GetComponent<MeshRenderer>();
			if (!IsServer)
			{
				getMyCommandRpc();
				return;
			}
			CurrentHealth.Value = MaxHp;
			if (commands.Count == 0)
			{
				Debug.LogWarning("No commands assigned to the barrel");
				return;
			}
			setToRandomBarrel();
		}
		private void setToRandomBarrel()
		{
			commandIndex = Random.Range(0, commands.Count);
			myCommand = commands[commandIndex].Create<BarrelCommand>();
			setMyCommandRpc(commandIndex, RpcTarget.NotServer);
			initLocal();
		}
		[Rpc(SendTo.Server)]
		private void getMyCommandRpc(RpcParams _rpcParams = default)
		{
			setMyCommandRpc(commandIndex, RpcTarget.Single(_rpcParams.Receive.SenderClientId, RpcTargetUse.Temp));
		}
		[Rpc(SendTo.SpecifiedInParams)]
		private void setMyCommandRpc(int _index, RpcParams _rpcParams)
		{
			myCommand = commands[_index];
			initLocal();
		}
		private void initLocal()
		{
			if (myCommand == null) { return; }
			myCommand.Init(this);
		}
#if !UNITY_SERVER || UNITY_EDITOR
		public void SetStripes(Material _mat)
		{
			List<Material> _materials = new();
			meshRenderer.GetMaterials(_materials);
			_materials[1] = _mat;
			meshRenderer.SetMaterials(_materials);
		}
#endif
	}
}