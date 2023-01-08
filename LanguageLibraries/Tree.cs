using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanguageLibrary
{
    public interface ITree<out T>
    {
        public T Value { get; }

        public IEnumerable<ITree<T>> Children { get; }
    }

    internal class Tree<T> : ITree<T>
    {
        public T Value { get; }

        public IEnumerable<ITree<T>> Children { get; }

        public Tree(T value, IEnumerable<ITree<T>> children)
        {
            Value = value;
            Children = children;
        }

        public Tree(T value) : this(value, Enumerable.Empty<ITree<T>>())
        {
        }

        public override string ToString()
        {
            return string.Join(",", this.GetDepthFirst(true));
        }
    }

    public class ExpandTree<T> : ITree<T>
    {
        public T Value { get; }

        public IEnumerable<ITree<T>> Children => _children;

        public ExpandTree<T>? Parent { get; private set; }

        private readonly List<ExpandTree<T>> _children = new();

        public ExpandTree(T value, IEnumerable<ExpandTree<T>> children)
        {
            Value = value;
            AddChildren(children);
        }

        public ExpandTree(T value) : this(value, Enumerable.Empty<ExpandTree<T>>())
        {

        }

        public void AddChild(ExpandTree<T> value)
        {
            value.Parent = this;
            _children.Add(value);
        }

        public void AddChildren(IEnumerable<ExpandTree<T>> children)
        {
            foreach (var child in children)
            {
                AddChild(child);
            }
        }

        public int Depth
        {
            get
            {
                int result = 0;
                ExpandTree<T> current = this;
                while(current.Parent != null)
                {
                    current = current.Parent;
                    result++;
                }

                return result;
            }
        }

        public override string ToString()
        {
            return string.Join(",", this.GetDepthFirst<T, ExpandTree<T>>(true).Select(tree => (tree.Depth, tree.Value)));
        }

        public ExpandTree<T> GetAncestor(int depth)
        {
            ExpandTree<T> current = this;
            while(current.Parent != null && depth < current.Depth)
            {
                current = current.Parent;
            }

            return current;
        }
    }

    public class TargetInfo<TSource, TKey>
    {
        private readonly Dictionary<int, Dictionary<string, ITree<TSource>>> _targetInfosDictionary = new();

        public Dictionary<string, ITree<TSource>> this[int depth] => _targetInfosDictionary[depth];

        public Func<TSource, TKey> Selector { get; }

        public int StartDepth { get; }

        public int BeamDepth { get; }

        public bool IsDescending { get; }

        public TargetInfo(ITree<TSource> initialTarget, int startDepth, int beamWidth, Func<TSource, TKey> selector, bool isDescending)
        {
            _targetInfosDictionary[0] = initialTarget.ToEnumerable().ToDictionary(p => string.Empty, p => p);
            Selector = selector;
            StartDepth = startDepth;
            BeamDepth = beamWidth;
            IsDescending = isDescending;
        }

        public void Ensure(int depth)
        {
            if (!_targetInfosDictionary.ContainsKey(depth))
            {
                if (!_targetInfosDictionary.ContainsKey(depth - 1))
                {
                    throw new ArgumentException(null, nameof(depth));
                }

                _targetInfosDictionary[depth] = GetTargetInfos(depth);
            }
        }

        private Dictionary<string, ITree<TSource>> GetTargetInfos(int depth)
        {
            var result = _targetInfosDictionary[depth - 1]
                        .SelectMany(target => target.Value.Children.Indexed().Select(pair => (ParentKey: target.Key, Value: pair)));

            if (depth >= StartDepth)
            {
                if (IsDescending)
                {
                    result = result.OrderByDescending(child => Selector(child.Value.Element.Value));
                }
                else
                {
                    result = result.OrderBy(child => Selector(child.Value.Element.Value));
                }

                result = result.Take(BeamDepth);
            }

            return result.ToDictionary(child => string.Join(",", child.ParentKey, child.Value.Index).Trim(','), child => child.Value.Element);
        }
    }

    public static class TreeExtension
    {
        public static IEnumerable<T> GetDepthFirst<T>(this ITree<T> source, bool isChildReverse = false)
        {
            var stack = new Stack<ITree<T>>();
            stack.Push(source);
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                yield return current.Value;
                foreach (var child in isChildReverse ? current.Children.Reverse() : current.Children)
                {
                    stack.Push(child);
                }
            }
        }

        public static IEnumerable<TTree> GetDepthFirst<T, TTree>(this TTree source, bool isChildReverse = false)
            where TTree : ITree<T>
        {
            var stack = new Stack<TTree>();
            stack.Push(source);
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                yield return current;
                foreach (var child in isChildReverse ? current.Children.Reverse() : current.Children)
                {
                    stack.Push((TTree)child);
                }
            }
        }

        public static ITree<TResult> SelectMany<TSource, TCollection, TResult>
            (this ITree<TSource> source,
            Func<TSource, ITree<TCollection>> collectionSelector,
            Func<TSource, TCollection, TResult> resultSelector)
        {
            return source.SelectMany(sourceValue => collectionSelector(sourceValue).Select(item => (sourceValue, item)).Select(tuple => resultSelector(tuple.sourceValue, tuple.item)));
        }

        public static ITree<TResult> SelectMany<TSource, TResult>(this ITree<TSource> source, Func<TSource, ITree<TResult>> resultSelector)
        {
            return source.Select(resultSelector).Flatten();
        }

        public static ITree<TSource> Flatten<TSource>(this ITree<ITree<TSource>> source)
        {
            if (source.Value.Children.Any())
            {
                return new Tree<TSource>(source.Value.Value, source.Value.Children.Select(child => new Tree<ITree<TSource>>(child, source.Children)).Select(tree => tree.Flatten()));
            }
            else
            {
                return new Tree<TSource>(source.Value.Value, source.Children.Select(child => child.Flatten()));
            }
        }

        public static ITree<TResult> Select<TSource, TResult>(this ITree<TSource> source, Func<TSource, TResult> resultSelector)
        {
            return new Tree<TResult>(resultSelector(source.Value), source.Children.Select(child => child.Select(resultSelector)));
        }

        public static ITree<TSource> CreateTree<TSource>(this TSource seed, Func<TSource, int, IEnumerable<TSource>> childrenSelector)
        {
            return seed.CreateTree(0, childrenSelector);
        }

        private static ITree<TSource> CreateTree<TSource>(this TSource seed, int depth, Func<TSource, int, IEnumerable<TSource>> childrenSelector)
        {
            return new Tree<TSource>(seed, childrenSelector(seed, depth).Select(arg => arg.CreateTree(depth + 1, childrenSelector)));
        }

        public static ITree<TSource> TakeDepth<TSource>(this ITree<TSource> source, int depth)
        {
            if (depth == 0)
            {
                return new Tree<TSource>(source.Value);
            }
            else
            {
                return new Tree<TSource>(source.Value, source.Children.Select(child => child.TakeDepth(depth - 1)));
            }
        }

        public static ExpandTree<TSource> ToExpand<TSource>(this ITree<TSource> source)
        {
            var stack = new Stack<(ITree<TSource>, ExpandTree<TSource>)>();
            var result = new ExpandTree<TSource>(source.Value);
            stack.Push((source, result));
            while (stack.Count > 0)
            {
                var (current, expandCurrent) = stack.Pop();
                var tempStack = new Stack<(ITree<TSource>, ExpandTree<TSource>)>();

                foreach (var child in current.Children)
                {
                    var expandChild = new ExpandTree<TSource>(child.Value);
                    tempStack.Push((child, expandChild));
                    expandCurrent.AddChild(expandChild);
                }

                while (tempStack.Count > 0) stack.Push(tempStack.Pop());
            }

            return result;
        }

        public static ITree<TSource> BeamSearch<TSource, TKey>(this ITree<TSource> source, int startDepth, int beamWidth, Func<TSource, TKey> selector)
        {
            if (startDepth == 0) throw new ArgumentException(nameof(startDepth));
            if (beamWidth == 0) throw new ArgumentException(nameof(beamWidth));
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            return source.BeamSearch(0, new TargetInfo<TSource, TKey>(source, startDepth, beamWidth, selector, true), Enumerable.Empty<int>());
        }

        public static ITree<TSource> BeamSearchAscending<TSource, TKey>(this ITree<TSource> source, int startDepth, int beamWidth, Func<TSource, TKey> selector)
        {
            if (startDepth == 0) throw new ArgumentException(nameof(startDepth));
            if (beamWidth == 0) throw new ArgumentException(nameof(beamWidth));
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            return source.BeamSearch(0, new TargetInfo<TSource, TKey>(source, startDepth, beamWidth, selector, false), Enumerable.Empty<int>());
        }

        private static ITree<TSource> BeamSearch<TSource, TKey>(this ITree<TSource> source, int currentDepth, TargetInfo<TSource, TKey> targetInfo, IEnumerable<int> path)
        {
            targetInfo.Ensure(currentDepth + 1);
            return new Tree<TSource>(source.Value,
                source.Children.Indexed().Where(pair => targetInfo[currentDepth + 1].ContainsKey(string.Join(",", path.Append(pair.Index))))
                .Select(pair => targetInfo[currentDepth + 1][string.Join(",", path.Append(pair.Index))].BeamSearch(currentDepth + 1, targetInfo, path.Append(pair.Index)))
            );
        }
    }
}
