using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuyoAppConsole
{
    internal class PuyoField
    {
        public const int RowCount = 13;

        public const int ColumnCount = 6;

        public const int HideCount = 1;

        public const int OutputColumn = 3;

        public const int DeleteCount = 4;

        private readonly int[][] _field = Enumerable.Repeat(Array.Empty<int>(), ColumnCount).ToArray();

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

        public PuyoField(int[][] field)
        {
            _field = field;
        }

        public PuyoField((int[][] Field, int Chain) tuple, PuyoOperator puyoOperator) : this(tuple.Field, tuple.Chain, puyoOperator)
        {
        }

        public PuyoField(int[][] field, int chain, PuyoOperator puyoOperator) : this(field)
        {
            Chain = chain;
            Operator = puyoOperator;
        }

        public PuyoField()
        {

        }

        public PuyoField Operate(PuyoOperator puyoOperator, int[] tumo)
        {
            return IsGameOver? this : puyoOperator.Vec switch
            {
                0 => new PuyoField(Operate(_field, OutputColumn, puyoOperator.Column, puyoOperator.Column + 1, tumo[0], tumo[1]), puyoOperator),
                1 => new PuyoField(Operate(_field, OutputColumn, puyoOperator.Column, puyoOperator.Column, tumo[0], tumo[1]), puyoOperator),
                2 => new PuyoField(Operate(_field, OutputColumn, puyoOperator.Column - 1, puyoOperator.Column, tumo[1], tumo[0]), puyoOperator),
                3 => new PuyoField(Operate(_field, OutputColumn, puyoOperator.Column, puyoOperator.Column, tumo[1], tumo[0]), puyoOperator),
                _ => throw new ArgumentException(nameof(puyoOperator.Vec)),
            };
        }

        public int GetEvaluationValue()
        {
            if (IsGameOver) return -1;
            return Chain;
        }

        private static (int[][] Field, int Chain) Operate(int[][] field, int outputColumn, int firstColumn, int secondColumn, int first, int second)
        {
            var currentFirstColumn = outputColumn;
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
            (int X, int Y)[] axises = new (int X, int Y)[4]
            {
                (1,0),
                (0,1),
                (-1,0),
                (0,-1),
            };

            bool IsSame(int x1, int y1, int x2, int y2)
            {
                if (0 > x1 || x1 >= ColumnCount) return false;
                if (0 > x2 || x2 >= ColumnCount) return false;
                if (0 > y1 || y1 >= field[x1].Length) return false;
                if (0 > y2 || y2 >= field[x2].Length) return false;

                return field[x1][y1] == field[x2][y2];
            }

            foreach (var item in fieldInfos)
            {
                foreach (var axis in axises)
                {
                    if (IsSame(item.ColumnIndex, item.RowIndex, item.ColumnIndex + axis.X, item.RowIndex + axis.Y))
                    {
                        disjointSet.Merge(item, (item.ColumnIndex + axis.X, item.RowIndex + axis.Y));
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
