using UnityEngine;

public interface IInventoryItem
{
	public string Name { get; set; }
	public string Description { get; set; }
	public float Price { get; set; }
	public Texture2D Icon { get; set; }
	public bool IsNullOrEmpty()
	{
		return string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(Description) && Price == 0 && Icon == null;
	}
}