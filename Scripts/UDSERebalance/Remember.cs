using System;

// Original code written by Digi:
// https://discord.com/channels/125011928711036928/126460115204308993/934084145737515098
namespace UDSERebalance
{
    public interface IRemember
    {
        void Restore();
    }

    public class Remember<TDef, TVal> : IRemember
    {
        private readonly TDef _def;
        private readonly TVal _originalValue;
        private readonly Action<TDef, TVal> _vSetter;

        public Remember(TDef def, Func<TDef, TVal> getter, Action<TDef, TVal> setter, TVal value)
        {
            _def = def;
            _vSetter = setter;

            _originalValue = getter.Invoke(_def);
            _vSetter.Invoke(_def, value);
        }

        public void Restore()
        {
            _vSetter.Invoke(_def, _originalValue);
        }
    }

    public static class Remember
    {
        public static Remember<TDef, TVal> Create<TDef, TVal>(TDef def, Func<TDef, TVal> getter,
            Action<TDef, TVal> setter, TVal value)
        {
            return new Remember<TDef, TVal>(def, getter, setter, value);
        }
    }
}