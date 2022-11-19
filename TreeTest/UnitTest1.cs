namespace TreeTest
{
    public class UnitTest1
    {
        private static readonly IEnumerable<(int, int)> expected1 = new (int, int)[] { (0, 0), (1, 0), (2, 1), (1, 1), (2, 2) };
        private static readonly IEnumerable<(int, int)> expected2 = new (int, int)[] { (0, 0), (1, 0), (2, 0), (2, 1), (1, 1), (2, 1), (2, 2) };

        [Fact(DisplayName = "TreeTest1")]
        public void TreeTest1()
        {
            var result = 0
                .CreateTree((depth, current) => {
                    return Enumerable.Range(current, 2);
                })
                .BeamSearch(2, 2, current => current);

            Assert.Equal(string.Join(",", expected1), result.TakeDepth(2).ToExpand().ToString());
            Assert.Equal(string.Join(",", expected1), result.TakeDepth(2).ToExpand().ToString());
        }

        [Fact(DisplayName = "TreeTest2")]
        public void TreeTest2()
        {
            var result = 0
                .CreateTree((depth, current) => {
                    return Enumerable.Range(current, 2);
                })
                .TakeDepth(2)
                .ToExpand();

            Assert.Equal(string.Join(",", expected2), result.ToString());
        }
    }
}