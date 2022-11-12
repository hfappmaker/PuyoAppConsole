using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuyoAppConsole
{
    internal record PuyoOperator
    {
        public int Column { get; }

        public int Vec { get; }

        public static PuyoOperator[] Operators { get; }
        = new PuyoOperator[]
        {
            new PuyoOperator(0,0),
            new PuyoOperator(0,1),
            new PuyoOperator(0,3),
            new PuyoOperator(1,0),
            new PuyoOperator(1,1),
            new PuyoOperator(1,2),
            new PuyoOperator(1,3),
            new PuyoOperator(2,0),
            new PuyoOperator(2,1),
            new PuyoOperator(2,2),
            new PuyoOperator(2,3),
            new PuyoOperator(3,0),
            new PuyoOperator(3,1),
            new PuyoOperator(3,2),
            new PuyoOperator(3,3),
            new PuyoOperator(4,0),
            new PuyoOperator(4,1),
            new PuyoOperator(4,2),
            new PuyoOperator(4,3),
            new PuyoOperator(5,1),
            new PuyoOperator(5,2),
            new PuyoOperator(5,3),
        };

        private PuyoOperator(int column, int vec)
        {
            Column = column;
            Vec = vec;
        }

        public override string ToString()
        {
            return (Column, Vec).ToString();
        }
    }
}
