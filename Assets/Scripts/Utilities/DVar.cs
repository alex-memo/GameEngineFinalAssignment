using System;

namespace Memo.Utilities
{
    [Serializable]
    public class DVar<T> : IDisposable
    {
        public delegate void OnValueChangedDelegate(T _value);
        public OnValueChangedDelegate OnValueChanged;
        private T internalValue;
        public T Value
        {
            get
            {
                return internalValue;
            }
            set
            {
                internalValue = value;
                OnValueChanged?.Invoke(value);
            }
        }
        public DVar(T _value)
        {
            internalValue = _value;
        }
        public override string ToString()
        {
            return internalValue.ToString();
        }

		public void Dispose()
		{
			OnValueChanged = null;
		}
	}
}
