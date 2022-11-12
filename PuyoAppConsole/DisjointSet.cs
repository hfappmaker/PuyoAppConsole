using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuyoAppConsole
{
    internal class DisjointSet<T> where T : IEquatable<T>
    {
        /// <summary>
        /// key:leader,value:size
        /// </summary>
        private readonly Dictionary<T, int> _sizeDictionary = new();

        /// <summary>
        /// keyはleader以外
        /// </summary>
        private readonly Dictionary<T, T> _parent = new(); 

        public DisjointSet(IEnumerable<T> vertices)
        {
            foreach (var vertex in vertices)
            {
                if (_sizeDictionary.ContainsKey(vertex))
                {
                    throw new ArgumentException("duplicate");
                }

                _sizeDictionary[vertex] = 1;
            }
        }

        public T Merge(T a, T b)
        {
            T x = Leader(a), y = Leader(b);
            if (x.Equals(y)) return x;

            if (_sizeDictionary[x] > _sizeDictionary[y])
            {
                (x, y) = (y, x);
            }
            
            _sizeDictionary[y] += _sizeDictionary[x];
            _parent.Add(x, y);
            _sizeDictionary.Remove(x);

            return y;
        }

        public bool Same(T a, T b)
        {
            return Leader(a).Equals(Leader(b));
        }

        public T Leader(T a)
        {
            if (_sizeDictionary.ContainsKey(a))
            {
                return a;
            }

            return _parent[a] = Leader(_parent[a]);
        }

        public int Size(T a)
        {
            return _sizeDictionary[Leader(a)];
        }

        public IEnumerable<T[]> Groups()
        {
            var result = new Dictionary<T, List<T>>(_sizeDictionary.Count);
            foreach (var leader in _sizeDictionary.Keys)
            {
                result[leader] = new List<T>(_sizeDictionary[leader]);
            }

            foreach (var vertex in _sizeDictionary.Keys.Concat(_parent.Keys))
            {
                result[Leader(vertex)].Add(vertex);
            }

            return result.Values.Select(group => group.ToArray());
        }
    }
}
