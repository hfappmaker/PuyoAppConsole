using LanguageLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuyoAppConsole
{
    /// <summary>
    /// 2連鎖情報
    /// </summary>
    internal class PuyoTwoChainInfo
    {
        private static readonly Dictionary<string, double> Scores = new Dictionary<string, double>() { { new PuyoField(13, 6, 1, 2, 4).ToString(), 0 } };
        /// <summary>
        /// 座標情報を囲む最小矩形の幅
        /// </summary>
        internal int Width { get; }

        /// <summary>
        /// 座標情報を囲む最小矩形の高さ
        /// </summary>
        internal int Height { get; }

        /// <summary>
        /// 座標情報
        /// Keyは座標 Valueはぷよの色(0は1連鎖目で消えるぷよ、1は2連鎖目で消えるぷよ)
        /// </summary>
        internal IReadOnlyDictionary<Point, int> Points { get; }

        /// <summary>
        /// 同色連鎖可能かどうか
        /// </summary>
        internal bool CanSeparate { get; }

        public PuyoTwoChainInfo(IReadOnlyDictionary<Point, int> points)
        {
            Points = points.ToDictionary(pair => pair.Key, pair => pair.Value);
            Width = points.Keys.Select(p => p.X).Max() + 1;
            Height = points.Keys.Select(p => p.Y).Max() + 1;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"{nameof(Width)}:{Width}");
            builder.AppendLine($"{nameof(Height)}:{Height}");
            builder.AppendLine($"{nameof(CanSeparate)}:{CanSeparate}");
            builder.AppendLine(string.Join(Environment.NewLine, Points.OrderBy(p => p.Key.X).ThenBy(p => p.Key.Y).Select(pair => (pair.Key, pair.Value))));
            return builder.ToString();
        }

        public double Match(PuyoField puyoField, PuyoField? parentPuyoField, int parentChain)
        {
            if (Scores.ContainsKey(puyoField.ToString()))
            {
                return Scores[puyoField.ToString()];
            }

            if (parentPuyoField is null || !Scores.ContainsKey(parentPuyoField.ToString())) throw new ArgumentException(nameof(parentPuyoField));
            double res = 0;
            if (parentChain == 0)
            {
                res = Scores[parentPuyoField.ToString()];
                var diffPuyos = from columnIndex in Enumerable.Range(0, puyoField.ColumnCount)
                                from rowIndex in Enumerable.Range(0, puyoField.RowCount)
                                where puyoField[columnIndex, rowIndex] != parentPuyoField[columnIndex, rowIndex]
                                select (ColumnIndex: columnIndex, RowIndex: rowIndex, PuyoColor: puyoField[columnIndex, rowIndex]);
                Point leftDown = new Point(diffPuyos.Select(p => p.ColumnIndex).Min(), diffPuyos.Select(p => p.RowIndex).Min());
                Point rightUp = new Point(diffPuyos.Select(p => p.ColumnIndex).Max(), diffPuyos.Select(p => p.RowIndex).Max());
                foreach (var columnDiff in Enumerable.Range(Math.Max(leftDown.X - Width + 1, 0), Math.Min(rightUp.X + Width, puyoField.ColumnCount) - Width + 1))
                {
                    foreach (var rowDiff in Enumerable.Range(Math.Max(leftDown.Y - Height + 1, 0), Math.Min(rightUp.Y + Height, puyoField.RowCount) - Height + 1))
                    {
                        var temp = new Dictionary<int, int>();
                        foreach (var firstColor in Enumerable.Range(0, 4))
                        {
                            foreach (var secondColor in Enumerable.Range(0, 4))
                            {
                                if (firstColor == secondColor) continue;
                                temp[0] = firstColor;
                                temp[1] = secondColor;
                                var diff = Points.Where(pair => diffPuyos.Any(diffPuyo => diffPuyo.ColumnIndex == columnDiff + pair.Key.X && diffPuyo.RowIndex == rowDiff + pair.Key.Y))
                                    .Count(pair => puyoField[columnDiff + pair.Key.X, rowDiff + pair.Key.Y] == temp[pair.Value]);
                                if (diff > 2) throw new Exception();
                                if (res < 0) throw new Exception();
                                if (diff > 0)
                                {
                                    res += Math.Pow(10, Points.Count(pair => puyoField[columnDiff + pair.Key.X, rowDiff + pair.Key.Y] == temp[pair.Value]) % Points.Count);
                                    res -= Math.Pow(10, (Points.Count(pair => puyoField[columnDiff + pair.Key.X, rowDiff + pair.Key.Y] == temp[pair.Value]) - diff) % Points.Count);
                                }
                            }
                        }
                    }
                }

            }
            else
            {
                res = 0;
                foreach (var columnDiff in Enumerable.Range(0, puyoField.ColumnCount - Width + 1))
                {
                    foreach (var rowDiff in Enumerable.Range(0, puyoField.RowCount - Height + 1))
                    {
                        var temp = new Dictionary<int, int>();
                        foreach (var firstColor in Enumerable.Range(0, 4))
                        {
                            foreach (var secondColor in Enumerable.Range(0, 4))
                            {
                                if (firstColor == secondColor) continue;
                                temp[0] = firstColor;
                                temp[1] = secondColor;
                                var value = Math.Pow(10, Points.Count(pair => puyoField[columnDiff + pair.Key.X, rowDiff + pair.Key.Y] == temp[pair.Value]) % Points.Count);
                                value /= Points.Count(pair => puyoField[columnDiff + pair.Key.X, rowDiff + pair.Key.Y] != temp[pair.Value] && puyoField[columnDiff + pair.Key.X, rowDiff + pair.Key.Y] != -1)+1;
                                res += value;
                            }
                        }
                    }
                }
            }

            Scores[puyoField.ToString()] = res;
            return res;
        }
    }
}
