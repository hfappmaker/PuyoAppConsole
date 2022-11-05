using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuyoAppConsole
{
    internal class Tree<T>
    {

        public T Value { get; set; }

        //public Tree<T> Parent { get; set; }

        public Tree<T>[] Children => _children.ToArray();

        private readonly List<Tree<T>> _children = new List<Tree<T>>();

        public Tree(T value, IEnumerable<Tree<T>> children)
        {
            Value = value;
            AddChildren(children);
        }

        public Tree(T value) : this(value, Enumerable.Empty<Tree<T>>())
        {
        }

        public Tree() { }

        public void AddChild(Tree<T> child)
        {
            //child.Parent = this;
            _children.Add(child);
        }

        public void AddChildren(IEnumerable<Tree<T>> children)
        {
            foreach (var child in children)
            {
                AddChild(child);
            }
        }

        //public IEnumerator<T> GetEnumerator()
        //{
        //    yield return Value;
        //    foreach (var child in Children)
        //    {
        //        child.GetEnumerator();
        //    }
        //}

        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    return GetEnumerator();
        //}
    }

    internal static class TreeExtension
    {
        public static Tree<TResult> SelectMany<TSource, TCollection, TResult>
            (this Tree<TSource> source, 
            Func<TSource, Tree<TCollection>> collectionSelector, 
            Func<TSource, TCollection, TResult> resultSelector)
        {
            return source.SelectMany(value => collectionSelector(value).Select(p => (value, p)).Select(tuple => resultSelector(tuple.value, tuple.p)));
        }

        public static Tree<TResult> SelectMany<TSource, TResult>(this Tree<TSource> source, Func<TSource, Tree<TResult>> resultSelector)
        {
            return source.Select(resultSelector).Flatten();
        }

        public static Tree<TSource> Flatten<TSource>(this Tree<Tree<TSource>> source)
        {
            var stack = new Stack<(Tree<TSource>, Tree<TSource>)>();
            var result = new Tree<TSource>();
            stack.Push((source.Value, result));
            while (stack.Count > 0)
            {
                var (currentTree , resultCurrentTree) = stack.Pop();
                resultCurrentTree.Value = currentTree.Value;
                if (currentTree.Children.Length > 0)
                {
                    foreach (var child in currentTree.Children)
                    {
                        var resultNextTree = new Tree<TSource>();
                        resultCurrentTree.AddChild(resultNextTree);
                        stack.Push((child, resultNextTree));
                    }
                }
                else
                {
                    foreach (var child in source.Children)
                    {
                        resultCurrentTree.AddChild(child.Flatten());
                    }
                }
            }

            return result;
        }

        public static Tree<TResult> Select<TSource, TResult>(this Tree<TSource> source, Func<TSource, TResult> resultSelector)
        {
            return new Tree<TResult>(resultSelector(source.Value), source.Children.Select(child => child.Select(resultSelector)).ToArray());
        }

        public static Func<TSource, Func<TCollection, TResult>> Curry<TSource, TCollection, TResult>(this Func<TSource, TCollection, TResult> func)
            => source => collection => func(source, collection);
    }
}
