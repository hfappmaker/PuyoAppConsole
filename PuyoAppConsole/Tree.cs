using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuyoAppConsole
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
            return string.Join(",", this.GetDepthFirst());
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
            return string.Join(",", this.GetDepthFirst());
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

    internal static class TreeExtension
    {
        public static IEnumerable<T> GetDepthFirst<T>(this ITree<T> source)
        {
            var stack = new Stack<ITree<T>>();
            stack.Push(source);
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                yield return current.Value;
                foreach (var child in current.Children)
                {
                    stack.Push(child);
                }
            }
        }

        public static IEnumerable<TTree> GetDepthFirst<T, TTree>(this TTree source)
            where TTree : ITree<T>
        {
            var stack = new Stack<TTree>();
            stack.Push(source);
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                yield return current;
                foreach (var child in current.Children)
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

        public static ITree<TSource> CreateTree<TSource>(this TSource seed, Func<int, TSource, IEnumerable<TSource>> nextChildrenGetter)
        {
            return seed.CreateTree(0, nextChildrenGetter);
        }

        private static ITree<TSource> CreateTree<TSource>(this TSource seed, int depth, Func<int, TSource, IEnumerable<TSource>> childrenGetter)
        {
            return new Tree<TSource>(seed, childrenGetter?.Partial(depth).Invoke(seed).Select(arg => arg.CreateTree(depth + 1, childrenGetter)) ?? Enumerable.Empty<ITree<TSource>>());
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

                foreach (var child in current.Children)
                {
                    var expandChild = new ExpandTree<TSource>(child.Value);
                    stack.Push((child, expandChild));
                    expandCurrent.AddChild(expandChild);
                }
            }

            return result;
        }

        //public static ITree<TSource> BeamSearch<TSource, TKey>(this ITree<TSource> source, int startDepth, int BeamWidth, Func<TSource, TKey> selector)
        //{
        //    if (startDepth <= 0)
        //    {
        //        return new Tree<TSource>(source.Value);
        //    }
        //    else
        //    {
        //        return new Tree<TSource>(source.Value, source.Children.Select(child => child.BeamSearch(startDepth - 1, BeamWidth, selector)));
        //    }
        //}

        //private static ITree<TSource> BeamSearch<TSource, TKey>(this ITree<TSource> source, int startDepth, int BeamWidth, Func<TSource, TKey> selector)
        //{

        //}
    }
}
