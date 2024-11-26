using UnityEngine;
[CreateAssetMenu(fileName = "AnimationEventData", menuName = "Game/AnimationEventData")]
public class AnimationEventDataSO : ScriptableObject
{
	public int Id;
	public bool IsAbility = true;
	public GameObject Prefab;
	public Vector3 SpawnOffset;
	public Vector3 SpawnRotation;
	public Vector3 SpawnScale;
	public float SpawnTime;
	public float DespawnTime;
	public bool UseWorldSpace;
}
