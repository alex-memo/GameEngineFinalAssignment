using Unity.Netcode;
using UnityEngine;

public struct DamageSource : INetworkSerializable
{
	public float Damage;
	public Element Element;
	public bool IsTool;
	public int Id;
	public DamageSource(float _damage, Element _element, bool _isTool, int _id)
	{
		Damage = _damage;
		Element = _element;
		IsTool = _isTool;
		Id = _id;
	}

	public void NetworkSerialize<T>(BufferSerializer<T> _serializer) where T : IReaderWriter
	{
		if(_serializer.IsReader)
		{
			var _reader = _serializer.GetFastBufferReader();
			_reader.ReadValueSafe(out Damage);
			_reader.ReadValueSafe(out Element);
			_reader.ReadValueSafe(out IsTool);
			_reader.ReadValueSafe(out Id);
		}
		else
		{
			var _writer = _serializer.GetFastBufferWriter();
			_writer.WriteValueSafe(Damage);
			_writer.WriteValueSafe(Element);
			_writer.WriteValueSafe(IsTool);
			_writer.WriteValueSafe(Id);
		}
	}
	public override string ToString()
	{
		return $"Damage: {Damage}, Element: {Element}, IsTool: {IsTool}, Id: {Id}";
	}
}