// See https://aka.ms/new-console-template for more information
using LanguageLibrary;
using PuyoAppConsole;

int[][] tumos = Enumerable.Range(0,1000).Select(index => new int[2] { new Random().Next(0, 4), new Random().Next(0, 4) }).ToArray();
var currentPuyoField = new PuyoField();
var maxChain = 0;
var maxSimulateChain = 0;
foreach (var count in Enumerable.Range(0,995))
{
    var tree = currentPuyoField.CreateTree((depth, value) =>
    {
        return PuyoOperator.Operators.Select(puyoOperator => value.Operate(puyoOperator, tumos[depth + count]));
    }).BeamSearch(3, 1000, field => field.GetEvaluationValue()).TakeDepth(10);

    var bestSimulateTree = tree.ToExpand().GetDepthFirst<PuyoField, ExpandTree<PuyoField>>().Where(field => field.Depth > 0)
        .MaxBy(field => field.Value.GetEvaluationValue());

    var bestTree = bestSimulateTree?.GetAncestor(1) ?? throw new InvalidOperationException();

    maxSimulateChain = Math.Max(maxSimulateChain, bestSimulateTree.Value.Chain);
    maxChain = Math.Max(maxChain, bestTree.Value.Chain);

    Console.WriteLine(bestTree.Value);
    Console.WriteLine(bestTree.Value.Chain);
    Console.WriteLine(tumos[count][0]);
    Console.WriteLine(tumos[count][1]);
    Console.WriteLine(bestTree.Value.Operator);
    Console.WriteLine("Count:" + count);
    Console.WriteLine("MaxSimulateChain:" + maxSimulateChain);
    Console.WriteLine("SimulateChain:" + bestSimulateTree.Value.Chain);
    Console.WriteLine("SimulateDepth:" + bestSimulateTree.Depth);
    Console.WriteLine("MaxChain:" + maxChain);
    currentPuyoField = bestTree.Value;
}
