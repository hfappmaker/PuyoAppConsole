// See https://aka.ms/new-console-template for more information
using LanguageLibrary;
using PuyoAppConsole;

int[][] tumos = Enumerable.Range(0,1000).Select(index => new int[2] { new Random().Next(0, 4), new Random().Next(0, 4) }).ToArray();
var pInfos = PuyoService.GetTwoChainPuyos().DistinctBy(p => p.ToString()).ToArray();
(PuyoField PuyoField, int Chain, int[][] DeletedColors) currentPuyoField = (new PuyoField(13, 6, 1, 2, 4), 0, Array.Empty<int[]>());
//foreach ((int index, PuyoTwoChainInfo info) in PuyoService.GetTwoChainPuyos().DistinctBy(p => p.ToString()).Indexed())
//{
//    var result = info.ToString();
//    Console.WriteLine("-----------------------------------");
//    Console.WriteLine(result);
//    Console.WriteLine(index);
//    Console.WriteLine("-----------------------------------");
//}
//return;
int maxSimulateChain = 0;
int maxChain = 0;
foreach (var count in Enumerable.Range(0, 995))
{
    var tree = currentPuyoField.CreateTree((value, depth) =>
    {
        return PuyoOperator.Operators.Select(puyoOperator => value.PuyoField.Operate(puyoOperator, tumos[depth + count]));
    }).TakeDepth(2);

    var bestSimulateTree = tree.Select(p => (p.PuyoField, p.Chain)).ToExpand().GetDepthFirst<(PuyoField PuyoField, int Chain), ExpandTree<(PuyoField PuyoField, int Chain)>>().Where(field => field.Depth > 0)
        .MaxBy(field => field.Value.PuyoField.GetEvaluationValue(pInfos, field.Parent?.Value.PuyoField, field.Value.Chain));

    var bestTree = bestSimulateTree?.GetAncestor(1) ?? throw new InvalidOperationException();

    maxSimulateChain = Math.Max(maxSimulateChain, bestSimulateTree.Value.Chain);
    maxChain = Math.Max(maxChain, bestTree.Value.Chain);

    Console.WriteLine(bestTree.Value.PuyoField);
    Console.WriteLine($"Chain:{bestTree.Value.Chain}");
    Console.WriteLine(tumos[count][0]);
    Console.WriteLine(tumos[count][1]);
    Console.WriteLine("Count:" + count);
    Console.WriteLine("SimulateDepth:" + bestSimulateTree.Depth);
    Console.WriteLine("MaxSimulateChain:" + maxSimulateChain);
    Console.WriteLine("MaxChain:" + maxChain);
    currentPuyoField = (bestTree.Value.PuyoField, 0, Array.Empty<int[]>());
}
