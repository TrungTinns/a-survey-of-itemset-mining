using System.Collections;
using System.Diagnostics;

namespace AprioriAlgorithm
{
	public class Apriori
	{
		public static bool NextCombination(IList<int> num, int n, int k)
		{
			bool finished;

			var changed = finished = false;

			if (k <= 0) return false;

			for (var i = k - 1; !finished && !changed; i--)
			{
				if (num[i] < n - 1 - (k - 1) + i)
				{
					num[i]++;

					if (i < k - 1)
					{
						for (var j = i + 1; j < k; j++)
						{
							num[j] = num[j - 1] + 1;
						}
					}

					changed = true;
				}

				finished = i == 0;
			}

			return changed;
		}

		public static IEnumerable Combinations<T>(IEnumerable<T> elements, int k)
		{
			var elem = elements.ToArray();
			var size = elem.Length;

			if (k > size) yield break;

			var numbers = new int[k];

			for (var i = 0; i < k; i++)
				numbers[i] = i;

			do
			{
				yield return numbers.Select(n => elem[n]);
			} while (NextCombination(numbers, size, k));
		}

		static void AprioriAlgorithm(HashSet<int> distinctItems, List<List<int>> transactions, int minsup, string output)
		{
			

			HashSet<int> frequentItems = new HashSet<int> { 0 };
			IEnumerable allCombinations = Combinations(distinctItems, 1);
			Dictionary<string, int> dictItemsCountReal = new();

			for (int i = 1; i <= distinctItems.Count; i++)
			{
				if (frequentItems.Count < i) break;

				SortedDictionary<string, int> dictItemsCount = new SortedDictionary<string, int>();

				if (i != 1)
				{
					allCombinations = Combinations(frequentItems, i);
				}

				foreach (IEnumerable<int> itemset in allCombinations)
				{
					foreach (List<int> transaction in transactions)
					{
						if (CheckIsNotIn(itemset, transaction))
						{
							continue;
						}

						string strItemset = string.Join(" ", itemset.ToArray());

						if (dictItemsCount.ContainsKey(strItemset))
						{
							dictItemsCount[strItemset]++;
						}
						else
						{
							dictItemsCount.Add(strItemset, 1);
						}
					}
				}

				frequentItems.Clear();

				foreach (KeyValuePair<string, int> keyValuePair in dictItemsCount)
				{
					dictItemsCountReal.Add(keyValuePair.Key, keyValuePair.Value);

					if (keyValuePair.Value < minsup)
					{
						dictItemsCountReal.Remove(keyValuePair.Key);
					}
					else
					{
						AddDistinctItems(keyValuePair.Key, frequentItems);
					}
				}

				WriteToFile(dictItemsCountReal, output);
			}
		}

		public static void AddDistinctItems(string items, HashSet<int> frequentItems)
		{
			string[] splitedItems = items.Split(' ');

			foreach (string item in splitedItems)
			{
				frequentItems.Add(int.Parse(item));
			}
		}

		static bool CheckIsNotIn(IEnumerable<int> itemset, List<int> transaction)
		{
			bool isNotIn = false;

			foreach (int item in itemset)
			{
				if (!transaction.Contains(item))
				{
					isNotIn = true;
					break;
				}
			}

			if (isNotIn) return true;

			return isNotIn;
		}

		public static void WriteToFile(Dictionary<string, int> dictItemsCount, string output)
		{
			StreamWriter sw = new StreamWriter(output);

			foreach (KeyValuePair<string, int> keyValuePair in dictItemsCount)
			{
				sw.WriteLine(keyValuePair.Key + " #SUP: " + keyValuePair.Value);
			}

			sw.Close();
		}

		static void Main(string[] args)
		{
			string input = "../../../input.txt";
			string output = "../../../output.txt";

			List<List<int>> transactions = new();
			HashSet<int> distictItems = new HashSet<int>();

			StreamReader sr = new StreamReader(input);

			string line = sr.ReadLine();

			while (line != null)
			{
				List<int> itemset = new();

				string[] splitedLine = line.Split(' ');

				foreach (string item in splitedLine)
				{
					itemset.Add(int.Parse(item));
					distictItems.Add(int.Parse(item));
				}

				transactions.Add(itemset);

				line = sr.ReadLine();
			}

			sr.Close();

			int minsup = (int)(0.4 * transactions.Count);

			Process currProcess = Process.GetCurrentProcess();
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			AprioriAlgorithm(distictItems, transactions, minsup, output);

			stopwatch.Stop();

			Console.WriteLine("Total time ~ " + stopwatch.Elapsed.TotalMilliseconds + " ms");
			Console.WriteLine("Total memory usage ~ " + (currProcess.PrivateMemorySize64 / (1024 * 1024)) + " mb");
		}
	}
}