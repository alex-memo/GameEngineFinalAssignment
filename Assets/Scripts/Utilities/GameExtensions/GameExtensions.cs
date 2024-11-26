using Game.Logic;
using UnityEngine;

namespace Memo.Utilities.Game
{
	public static class GameExtensions
	{
		/// <summary>
		/// Returns true if the collider is enemy team or explicitly enemy
		/// </summary>
		/// <param name="_coll"></param>
		/// <param name="_ownerController"></param>
		/// <returns></returns>
		public static bool IsEnemy(this Collider _coll, Entity _ownerController)
		{
			return _coll.CompareTag(_ownerController.GetEnemyTeam()) || _coll.CompareTag("Enemy");
		}
		/// <summary>
		/// Returns true if the user datas are enemies
		/// </summary>
		/// <param name="_myData"></param>
		/// <param name="_otherData"></param>
		/// <returns></returns>
		public static bool IsEnemy(this UserData _myData, UserData _otherData)
		{
			return _otherData.Team != _myData.Team;
		}
	}
}
