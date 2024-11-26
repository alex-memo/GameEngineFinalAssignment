using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
[CreateAssetMenu(fileName = "ToolsList", menuName = "Game/ToolsList")]
public class ToolsListSO : ScriptableObject
{
	[field: SerializeField] public List<Tool> Tools { get; private set; }
}
[Serializable]
public struct Tool : IInventoryItem
{
	[field: SerializeField] public string Name { get; set; }
	[field: SerializeField] public string Description { get; set; }
	[field: SerializeField] public float Price { get; set; }
	[field: SerializeField] public Texture2D Icon { get; set; }
	[field: SerializeField] public GameObject Model { get; set; }
	public ToolAttack ToolAttack;
}