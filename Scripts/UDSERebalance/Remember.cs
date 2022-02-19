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
        private readonly TDef Def;
        private readonly TVal OriginalValue;
        private readonly Action<TDef, TVal> VSetter;

        public Remember(TDef def, Func<TDef, TVal> getter, Action<TDef, TVal> setter, TVal value)
        {
            Def = def;
            VSetter = setter;

            OriginalValue = getter.Invoke(Def);
            VSetter.Invoke(Def, value);
        }

        public void Restore()
        {
            VSetter.Invoke(Def, OriginalValue);
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
