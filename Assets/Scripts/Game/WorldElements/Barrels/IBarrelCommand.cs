using UnityEngine;
namespace Game.Barrels
{
	public interface IBarrelCommand
	{
		void Execute();
	}
	public abstract class BarrelCommand : ScriptableObject, IBarrelCommand
	{
		[SerializeField] protected Material barrelLinesMat;
		protected Barrel barrel;
		public abstract void Execute();
		public T Create<T>() where T : BarrelCommand
		{
			T _command = Instantiate(this) as T;
			return _command;
		}
		public virtual void Init(Barrel _barrel)
		{
			barrel = _barrel;
#if !UNITY_SERVER || UNITY_EDITOR
			barrel.SetStripes(new(barrelLinesMat));
#endif
		}
	}
}