using LanguageLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuyoAppConsole
{
    internal static class PuyoService
    {
        public static IEnumerable<PuyoTwoChainInfo> GetTwoChainPuyos()
        {
            foreach(var tumos in new[] { 0, 0, 0, 0, 1, 1, 1, 1 }.GetPermutation().Select(arg => arg.ToArray()))
            {

                //foreach (var columns in new[] {0,1,2,3,4,5}.GetDuplicatePermutation(tumos.Length))
                //{
                //    List<int>[] field = new[]
                //    {
                //        new List<int>(),
                //        new List<int>(),
                //        new List<int>(),
                //        new List<int>(),
                //        new List<int>(),
                //        new List<int>(),
                //    };

                //    foreach ((int columnIndex, int tumo) in columns.Zip(tumos))
                //    {
                //        field[columnIndex].Add(tumo);
                //    }

                //    var (newField, chain) = PuyoField.ChainSimulate(field.Select(arg => arg.ToArray()).ToArray(), 0);
                //    if (chain == 2)
                //    {
                //        yield return field.SelectMany((column, columnIndex) => column.Select((puyo, rowIndex) => (new Point(columnIndex, rowIndex), puyo)));
                //    }
                //}
                foreach (var info in Trace(new Point(0, 0), new Point(-1, -1), tumos))
                {
                    yield return info;
                }
            }
        }

        public static IEnumerable<PuyoTwoChainInfo> Trace(Point current, Point parent, int[] tumos)
        {
            return Trace(current, parent, new Dictionary<Point, int>(), tumos);
        }

        private static IEnumerable<PuyoTwoChainInfo> Trace(Point current, Point parent, Dictionary<Point, int> traced, int[] tumos)
        {
            bool isAdd = false;
            if (!traced.ContainsKey(current))
            {
                traced.Add(current, tumos[traced.Keys.Count]);
                isAdd = true;
            }
            
            if (traced.Keys.Count == tumos.Length)
            {
                var width = traced.Keys.Select(p => p.X).Max() + 1;
                if (width <= 6)
                {
                    var height = traced.Keys.Select(p => p.Y).Max() + 1;
                    var field = Enumerable.Repeat(0, width).Select(_ => new int[height]).ToArray();

                    foreach (var columnIndex in Enumerable.Range(0, width))
                    {
                        foreach (int rowIndex in Enumerable.Range(0, height))
                        {
                            var point = new Point(columnIndex, rowIndex);
                            field[columnIndex][rowIndex] = traced.ContainsKey(point) ?
                                traced[point] :
                                columnIndex + rowIndex * width + 2;
                        }
                    }

                    var newField = new PuyoField(field, height, 0, 2, 4);
                    var chainInfo = newField.ChainSimulate();
                    if (chainInfo.Chain == 2 && chainInfo.DeletedColors[0][0] == 0)
                    {
                        yield return new PuyoTwoChainInfo(traced);
                    }
                }
            }

            if (traced.Keys.Count < tumos.Length)
            {
                var nextPoints = new Point[]
                {
                                current with { X = current.X + 1 },
                                current with { Y = current.Y + 1 },
                }.Where(point => !traced.ContainsKey(point));

                var backPoints = new Point[]
                {
                current with { X = current.X - 1 },
                current with { Y = current.Y - 1 },
                }.Where(point => traced.ContainsKey(point));

                foreach (Point nextPoint in nextPoints.Concat(backPoints).Where(p => p.X < 6 && p.X >= 0 && p.Y >= 0))
                {
                    foreach (var info in Trace(nextPoint, current, traced, tumos))
                    {
                        yield return info;
                    }
                }
            }

            if (isAdd)
            {
                traced.Remove(current);
            }
        }
    }
}
