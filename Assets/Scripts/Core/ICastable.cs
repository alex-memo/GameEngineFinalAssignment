using Memo.Utilities;

namespace Game.Core
{
	public interface ICastable
	{
		public float Cooldown { get; }
		public DVar<float> ActiveCooldown { get; }
		public bool IsOnCooldown { get; }
	}
}
