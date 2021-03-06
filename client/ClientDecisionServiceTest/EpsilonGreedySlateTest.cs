﻿using Microsoft.Research.MultiWorldTesting.ExploreLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientDecisionServiceTest
{
    [TestClass]
    public class EpsilonGreedySlateTestClass
    {
        private void RunTest(float epsilon, Action<double, Dictionary<int, int>> validate)
        {
            var context = new int[] { 1, 2, 3 };

            var explorer = new EpsilonGreedySlateExplorer(epsilon);

            var rnd = new Random(123);

            var histogram = new Dictionary<int, int>();
            var runs = 1024;
            for (int i = 0; i < runs; i++)
            {
                var seed = (ulong)rnd.Next(1024);
                var decision = explorer.MapContext(new PRG(seed), context, 0);

                var index = decision.Value.Aggregate((input, value) => input * 10 + value);

                int count;
                if (histogram.TryGetValue(index, out count))
                    histogram[index] = count + 1;
                else
                    histogram.Add(index, 1);
            }

            Console.WriteLine("Run");
            foreach (var kv in histogram)
                Console.WriteLine("{0}:{1}", kv.Key, kv.Value);

            // chi2 = sum (o_i - e_i)^2 / e_1 
            double e_i = runs / 6;
            var chi2 = histogram.Values.Aggregate(0.0, (acc, value) => acc + ((value - e_i) * (value - e_i) / e_i));

            validate(chi2, histogram);
        }

        [TestMethod]
        [TestCategory("Client Library")]
        [Priority(0)]
        public void EpsilonGreedySlateTest()
        {
            RunTest(1f, (chi2, _) => Assert.IsTrue(chi2 < 12.592));
            RunTest(.5f, (chi2, histogram) => 
                {
                    Assert.IsTrue(chi2 > 12.592);
                    Assert.IsTrue(histogram[123] > 512);
                });
            RunTest(0f, (_, histogram) => Assert.AreEqual(histogram[123], 1024));
        }

        public class MockupRanker : IRanker<int[]>
        {
            public PolicyDecision<int[]> MapContext(int[] context)
            {
                return context;
            }
        }
    }
}
