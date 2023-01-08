using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LanguageLibrary;
using System.Threading.Tasks;
using System.Diagnostics;

namespace PuyoAppConsole
{
    internal class PuyoField
    {
        public int RowCount { get; }

        public int ColumnCount { get; }

        public int HideCount { get; }

        public int OutputColumn { get; }

        public int DeleteCount { get; }

        public static Point[] Axises { get; } = new Point[4]
        {
            new Point(1,0),
            new Point(0,1),
            new Point(-1,0),
            new Point(0,-1),
        };

        private readonly int[][] _field;

        public bool IsGameOver
        {
            get
            {
                return _field[OutputColumn].Length >= RowCount - HideCount;
            }
        }

        public int this[int column, int row]
        {
            get
            {
                if (_field[column].Length <= row)
                {
                    return -1;
                }

                return _field[column][row];
            }
        }

        public PuyoField(int[][] field, int rowCount, int hideConut, int outputColumn, int deleteCount)
        {
            _field = field;
            RowCount = rowCount;
            ColumnCount = field.GetLength(0);
            HideCount = hideConut;
            OutputColumn = outputColumn;
            DeleteCount = deleteCount;
        }

        public PuyoField(int rowCount, int columnCount, int hideConut, int outputColumn, int deleteCount) : this(Enumerable.Repeat(Array.Empty<int>(), columnCount).ToArray(), rowCount, hideConut, outputColumn, deleteCount)
        {

        }

        public (PuyoField PuyoField, int Chain, int[][] DeletedColors) Operate(PuyoOperator puyoOperator, int[] tumo)
        {
            return IsGameOver? (this, -1, new int[0][]) : puyoOperator.Vec switch
            {
                0 => Operate(puyoOperator.Column,     puyoOperator.Column + 1,    tumo[0], tumo[1]),
                1 => Operate(puyoOperator.Column,     puyoOperator.Column,        tumo[0], tumo[1]),
                2 => Operate(puyoOperator.Column - 1, puyoOperator.Column,        tumo[1], tumo[0]),
                3 => Operate(puyoOperator.Column,     puyoOperator.Column,        tumo[1], tumo[0]),
                _ => throw new ArgumentException(nameof(puyoOperator.Vec)),
            };
        }

        public double GetEvaluationValue(PuyoTwoChainInfo[] infos, PuyoField? parentPuyoField, int parentChain)
        {
            if (IsGameOver) return double.MinValue;

            double res = 0;
            foreach (var info in infos)
            {
                res += info.Match(this, parentPuyoField, parentChain);
            }
            return res;
        }

        private (PuyoField PuyoField, int Chain, int[][] DeletedColors) Operate(int firstColumn, int secondColumn, int first, int second)
        {
            var currentFirstColumn = OutputColumn;
            var result = new List<List<int>>(_field.GetLength(0));
            foreach (var column in _field)
            {
                result.Add(new List<int>(column));
            }

            var delta = firstColumn < OutputColumn ? -1 : 1;
            while (firstColumn * delta > currentFirstColumn * delta && _field[currentFirstColumn + delta].Length < RowCount - Convert.ToInt32(firstColumn == secondColumn)
                && _field[currentFirstColumn + delta + secondColumn - firstColumn].Length < RowCount)
            {
                currentFirstColumn += delta;
            }

            result[currentFirstColumn].Add(first);

            if (_field[currentFirstColumn + secondColumn - firstColumn].Length == RowCount)
            {
                result[currentFirstColumn].Add(second);
            }
            else
            {
                result[currentFirstColumn + secondColumn - firstColumn].Add(second);
            }

            Debug.Assert(result.All(column => column.Count <= RowCount));

            var nextField = new PuyoField(result.Select(column => column.ToArray()).ToArray(), RowCount, HideCount, OutputColumn, DeleteCount);
            return nextField.ChainSimulate();
        }

        public (PuyoField PuyoField, int Chain, int[][] DeletedColors) ChainSimulate()
        {
            return ChainSimulate(0, new List<HashSet<int>>());
        }

        private (PuyoField PuyoField, int Chain, int[][] DeletedColors) ChainSimulate(int chain, IList<HashSet<int>> deletedColors)
        {
            var fieldInfos = _field.SelectMany((column, ColumnIndex) => column.Select((cell, RowIndex) => (ColumnIndex, RowIndex))).ToArray();

            DisjointSet<(int Column, int Row)> disjointSet = new(fieldInfos);

            bool IsSame((int ColumnIndex, int RowIndex) item1, (int ColumnIndex, int RowIndex) item2)
            {
                int x1 = item1.ColumnIndex;
                int y1 = item1.RowIndex;
                int x2 = item2.ColumnIndex;
                int y2 = item2.RowIndex;

                if (0 > x1 || x1 >= ColumnCount) return false;
                if (0 > x2 || x2 >= ColumnCount) return false;
                if (0 > y1 || y1 >= Math.Min(_field[x1].Length, RowCount - HideCount)) return false;
                if (0 > y2 || y2 >= Math.Min(_field[x2].Length, RowCount - HideCount)) return false;

                return _field[x1][y1] == _field[x2][y2];
            }

            foreach (var item in fieldInfos)
            {
                foreach ((int ColumnIndex, int RowIndex) nextItem in Axises.Select(axis => (item.ColumnIndex + axis.X, item.RowIndex + axis.Y)))
                {
                    if (IsSame(item, nextItem))
                    {
                        disjointSet.Merge(item, nextItem);
                    }
                }
            }

            var deleteTargets = disjointSet.Groups().Where(group => group.Length >= DeleteCount).SelectMany(arg => arg).ToArray();
            var afterField = _field.Select(column => column.ToArray()).ToArray();

            if (deleteTargets.Length > 0)
            {
                var deletedColor = new HashSet<int>();

                foreach (var (columnIndex, rowIndex) in deleteTargets)
                {
                    deletedColor.Add(afterField[columnIndex][rowIndex]);
                    afterField[columnIndex][rowIndex] = -2;
                }

                deletedColors.Add(deletedColor);

                afterField = afterField.Select(column => column.Where(cell => cell != -2).ToArray()).ToArray();
            }

            var nextField = new PuyoField(afterField, RowCount, HideCount, OutputColumn, DeleteCount);

            if (deleteTargets.Length > 0)
            {
                return nextField.ChainSimulate(chain + 1, deletedColors);
            }
            else
            {
                return (nextField, chain, deletedColors.Select(set => set.ToArray()).ToArray());
            }
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, (from row in Enumerable.Range(0, RowCount)
                                                    from column in Enumerable.Range(0, ColumnCount)
                                                    select this[column, row] == -1 ? "X" : this[column, row].ToString()).Chunk(ColumnCount).Select(rowline => string.Join(",", rowline)).Reverse());
        }
    }
}
