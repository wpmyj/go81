using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace System
{
    public abstract class TimedCacheProvider<T>
    {
        public abstract T this[string key, DateTime time] { get; set; }
        public abstract bool TryGetValue(string key, DateTime time, out T value);
        public abstract T GetCacheOrUpdate(string key, DateTime time, Func<T> updater);
        public abstract bool Remove(string key);
    }
    public class TimedCache<T> : TimedCacheProvider<T>
    {
        private readonly Dictionary<string, Tuple<DateTime, T>> _cache = new Dictionary<string, Tuple<DateTime, T>>();
        public override T this[string key, DateTime time]
        {
            get
            {
                T value;
                TryGetValue(key, time, out value);
                return value;
            }
            set
            {
                _cache[key] = new Tuple<DateTime, T>(time, value);
            }
        }
        public override bool TryGetValue(string key, DateTime time, out T value)
        {
            value = default(T);
            Tuple<DateTime, T> data;
            if (!_cache.TryGetValue(key, out data)) return false;
            if (data.Item1 != time)
            {
                _cache.Remove(key);
                return false;
            }
            value = data.Item2;
            return true;
        }
        public override T GetCacheOrUpdate(string key, DateTime time, Func<T> updater)
        {
            Tuple<DateTime, T> data;
            if (_cache.TryGetValue(key, out data) && data.Item1 == time) return data.Item2;
            var value = updater();
            _cache[key] = new Tuple<DateTime, T>(time, value);
            return value;
        }
        public override bool Remove(string key)
        {
            return _cache.Remove(key);
        }
    }
}