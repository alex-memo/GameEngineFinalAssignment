using System;
using System.Collections.Generic;
using UnityEngine;

public class StaticListManager : MonoBehaviour
{
	public static StaticListManager Instance { get; private set; }
	[SerializeField]private AbilityListSO abilityList;
	[SerializeField]private ToolsListSO toolList;
	[SerializeField] private Material zoneLut;

	private void Awake()
	{
		if (Instance != null)
		{
			Destroy(gameObject);
			return;
		}
		DontDestroyOnLoad(gameObject);
		zoneLut.SetFloat("_Contribution", 0);//guarantee that the lut is set to 0
		Instance = this;
	}
	private void OnApplicationQuit()
	{
		zoneLut.SetFloat("_Contribution", 0);//guarantee that the lut is set to 0
	}

	#region Abilities
	public Ability GetAbility(int _abilityID)
	{
		if (_abilityID < 0) { return null; }
		return abilityList.Abilities[_abilityID];
	}
	#endregion
	#region Tools
	public ToolAttack GetToolScript(int _toolID)
	{
		if (_toolID < 0) { return null; }
		if (toolList == null) { return null; }
		if (_toolID >= toolList.Tools.Count) { return null; }

		return toolList.Tools[_toolID].ToolAttack;
	}
	public int GetToolId(ToolAttack _tool)
	{
		return toolList.Tools.FindIndex(_tempTool => _tempTool.ToolAttack == _tool);
	}

	public Texture GetIcon(int playerIconID)
	{
		return null;
	}
	#endregion
}