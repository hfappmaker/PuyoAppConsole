// See https://aka.ms/new-console-template for more information
using PuyoAppConsole;

Console.WriteLine("Hello, World!");

var p = new Tree<int>(5, new Tree<int>[] { new Tree<int>(3), new Tree<int>(4) });
var f = from q in p
        from r in p
        select (q,r);

Console.WriteLine(f);
