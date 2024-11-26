using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AbilityList", menuName = "Game/AbilityList")]
public class AbilityListSO : ScriptableObject
{
	[field: SerializeField] public List<Ability> Abilities { get; private set; }
}