using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LanguageLibrary;
using System.Threading.Tasks;

namespace PuyoAppConsole
{
    internal class PuyoField
    {
        public const int RowCount = 13;

        public const int ColumnCount = 6;

        public const int HideCount = 1;

        public const int OutputColumn = 2;

        public const int DeleteCount = 4;

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

        public int Chain { get; }

        public PuyoOperator? Operator { get; }

        public PuyoField(int[][] field, int chain, PuyoOperator? puyoOperator)
        {
            _field = field;
            Chain = chain;
            Operator = puyoOperator;
        }

        public PuyoField((int[][] Field, int Chain) tuple, PuyoOperator puyoOperator) : this(tuple.Field, tuple.Chain, puyoOperator)
        {
        }

        public PuyoField() : this(Enumerable.Repeat(Array.Empty<int>(), ColumnCount).ToArray(), 0, null)
        {

        }

        public PuyoField Operate(PuyoOperator puyoOperator, int[] tumo)
        {
            return IsGameOver? this : puyoOperator.Vec switch
            {
                0 => new PuyoField(Operate(_field, puyoOperator.Column,     puyoOperator.Column + 1,    tumo[0], tumo[1]), puyoOperator),
                1 => new PuyoField(Operate(_field, puyoOperator.Column,     puyoOperator.Column,        tumo[0], tumo[1]), puyoOperator),
                2 => new PuyoField(Operate(_field, puyoOperator.Column - 1, puyoOperator.Column,        tumo[1], tumo[0]), puyoOperator),
                3 => new PuyoField(Operate(_field, puyoOperator.Column,     puyoOperator.Column,        tumo[1], tumo[0]), puyoOperator),
                _ => throw new ArgumentException(nameof(puyoOperator.Vec)),
            };
        }

        public int GetEvaluationValue()
        {
            //if (_field.Length % 12 == 0) return int.MaxValue;

            return IsGameOver ? -1 : Chain;
        }

        private static (int[][] Field, int Chain) Operate(int[][] field, int firstColumn, int secondColumn, int first, int second)
        {
            var currentFirstColumn = OutputColumn;
            var result = new List<List<int>>(field.GetLength(0));
            foreach (var column in field)
            {
                result.Add(new List<int>(column));
            }

            var delta = firstColumn < OutputColumn ? -1 : 1;
            while (firstColumn * delta > currentFirstColumn * delta && field[currentFirstColumn + delta].Length < RowCount - Convert.ToInt32(firstColumn == secondColumn))
            {
                currentFirstColumn += delta;
            }

            result[currentFirstColumn].Add(first);
            result[currentFirstColumn + secondColumn - firstColumn].Add(second);

            return ChainSimulate(result.Select(column => column.ToArray()).ToArray(), 0);
        }

        private static (int[][] Field, int Chain) ChainSimulate(int[][] field, int chain)
        {
            var fieldInfos = field.SelectMany((column, ColumnIndex) => column.Select((cell, RowIndex) => (ColumnIndex, RowIndex))).ToArray();

            DisjointSet<(int Column, int Row)> disjointSet = new(fieldInfos);

            bool IsSame((int ColumnIndex, int RowIndex) item1, (int ColumnIndex, int RowIndex) item2)
            {
                int x1 = item1.ColumnIndex;
                int y1 = item1.RowIndex;
                int x2 = item2.ColumnIndex;
                int y2 = item2.RowIndex;

                if (0 > x1 || x1 >= ColumnCount) return false;
                if (0 > x2 || x2 >= ColumnCount) return false;
                if (0 > y1 || y1 >= Math.Min(field[x1].Length, RowCount - HideCount)) return false;
                if (0 > y2 || y2 >= Math.Min(field[x2].Length, RowCount - HideCount)) return false;

                return field[x1][y1] == field[x2][y2];
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
            foreach (var (columnIndex, rowIndex) in deleteTargets)
            {
                field[columnIndex][rowIndex] = -2;
            }

            var afterField = field.Select(column => column.Where(cell => cell != -2).ToArray()).ToArray();

            if (deleteTargets.Length > 0)
            {
                return ChainSimulate(afterField, chain + 1);
            }
            else
            {
                return (afterField, chain);
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
