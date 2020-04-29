using System;
using System.Diagnostics;
using Godot;

namespace Overlords.helpers.listenables
{
    public interface IValueTypeless
    {
        bool @TrySetValue(object newValueRaw);
        void @SetValue(object newValueRaw);
            
        object @GetValueRaw();
        T @GetValueOrFail<T>();
        T @GetValueOrDefault<T>(T fallback);
    }
    
    public abstract class BaseValue<TValue>: Node, IValueTypeless
    {
        [Signal] public delegate void ValueChanged();

        protected abstract TValue ContainedValue { get; set; }
        
        public TValue Value
        {
            get => ContainedValue;
            set
            {
                var replacedValue = ContainedValue;
                ContainedValue = value;
                EmitSignal(nameof(ValueChanged), value, replacedValue);
            }
        }

        #region Typeless get/set
        public bool TrySetValue(object newValueRaw)
        {
            if (!(newValueRaw is TValue newValue)) return false;
            Value = newValue;
            return true;
        }

        public void SetValue(object newValueRaw)
        {
            Debug.Assert(newValueRaw is TValue);
            Value = (TValue) newValueRaw;
        }

        public object GetValueRaw()
        {
            return Value;
        }

        public T GetValueOrFail<T>()
        {
            var unCastedValue = Value;
            if (unCastedValue is T castedValue)
                return castedValue;
            throw new Exception("Failed to get value of desired type!");
        }

        public T GetValueOrDefault<T>(T fallback)
        {
            var unCastedValue = Value;
            return unCastedValue is T castedValue ? castedValue : fallback;
        }
        #endregion
    }
}