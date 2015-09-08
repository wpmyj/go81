using System.Linq;

namespace System.Collections.Generic
{
    public class UniqueValuesDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public new void Add(TKey key, TValue value)
        {
            if (ContainsValue(value))
                throw new ArgumentException("值出现重复。");
            base.Add(key, value);
        }
        public bool TryGetKeyOfValue(out TKey key, TValue value)
        {
            foreach (var item in this.Where(item => EqualityComparer<TValue>.Default.Equals(item.Value, value)))
            {
                key = item.Key;
                return true;
            }
            key = default(TKey);
            return false;
        }
    }
}