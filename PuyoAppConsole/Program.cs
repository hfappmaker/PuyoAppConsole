// See https://aka.ms/new-console-template for more information
using PuyoAppConsole;

int[][] tumos = Enumerable.Range(0,1000).Select(index => new int[2] { new Random().Next(0, 4), new Random().Next(0, 4) }).ToArray();
var currentPuyoField = new PuyoField();
ConsoleKeyInfo keyInfo;
foreach (var count in Enumerable.Range(0,995))
{
    var tree = currentPuyoField.CreateTree((depth, value) =>
    {
        return PuyoOperator.Operators.Select(puyoOperator => value.Operate(puyoOperator, tumos[depth + count]));
    }).TakeDepth(3);

    var bestTree = tree.ToExpand().GetDepthFirst<PuyoField, ExpandTree<PuyoField>>().Where(field => field.Depth > 0)
        .MaxBy(field => field.Value.GetEvaluationValue())?.GetAncestor(1) ?? throw new InvalidOperationException();
    
    Console.WriteLine(bestTree.Value);
    Console.WriteLine(bestTree.Value.Chain);
    Console.WriteLine(tumos[bestTree.Parent.Depth + count][0]);
    Console.WriteLine(tumos[bestTree.Parent.Depth + count][1]);
    Console.WriteLine(bestTree.Value.Operator);
    keyInfo = Console.ReadKey();
    currentPuyoField = bestTree.Value;
}
