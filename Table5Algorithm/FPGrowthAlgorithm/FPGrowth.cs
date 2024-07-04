using AprioriAlgorithm;
using System.Diagnostics;

namespace FPGrowthAlgorithm
{
	class FPGrowth
	{
		// store every transaction in a list
		private int _minsup;

		public FPGrowth()
		{
		}

		public void RunAlgorithm(string input, string output, double minsup)
		{
			// read all transactions from file
			List<List<int>> transactions = GetTransactionsFromFile(input);

			// calculate relative minsup from percent user type
			_minsup = (int)(minsup * transactions.Count);

			Process process = Process.GetCurrentProcess();
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			// generate first itemsets like {{1}, {2}, ..}
			List<List<int>> candidates = GetSingleItemset(transactions);

			// count support of each itemsets like {{1}: 4, {2}: 5, ...}
			Dictionary<List<int>, int> frequentItems = CountSupport(transactions, candidates);

			// sort transaction following frequentItems
			transactions = SortTransactionsFollowFrequentCount(transactions, frequentItems);

			Dictionary<int, List<List<int>>> conditionalPatternBase = GenerateConditionalPatternBase(transactions, frequentItems);

			Dictionary<int, List<List<int>>> conditionalFPTree = GenerateConditionalFPTree(conditionalPatternBase, frequentItems);

			Dictionary<List<int>, int> frequentItemsets = GenerateFrequentItemsets(transactions, conditionalFPTree);

			AddSingleItemsIntoFrequentItemsets(frequentItemsets, frequentItems);

			Dictionary<string, int> rightFrequentItemsets = ConvertRightForm(frequentItemsets);

			Apriori.WriteToFile(rightFrequentItemsets, output);

			stopwatch.Stop();

			Console.WriteLine("Total time ~ " + stopwatch.Elapsed.TotalMilliseconds + " ms");
			Console.WriteLine("Total memory usage ~ " + (process.PrivateMemorySize64 / (1024 * 1024)) + " mb");
		}

		Dictionary<string, int> ConvertRightForm(Dictionary<List<int>, int> frequentItemsets)
		{
			Dictionary<string, int> database = new();

			foreach (KeyValuePair<List<int>, int> kp in frequentItemsets.OrderBy(e => e.Key.Count))
			{
				string key = string.Join(" ", kp.Key.ToArray());
				
				database.Add(key, kp.Value);
			}

			return database;
		}

		void AddSingleItemsIntoFrequentItemsets(Dictionary<List<int>, int> frequentItemsets, Dictionary<List<int>, int> frequentItems)
		{
			foreach (KeyValuePair<List<int>, int> frequentItem in frequentItems)
			{
				frequentItemsets.Add(frequentItem.Key, frequentItem.Value);
			}
		}

		List<List<int>> GetTransactionsFromFile(string input)
		{
			List<List<int>> transactions = new();

			StreamReader sr = new StreamReader(input);

			// read first line in file input
			string line = sr.ReadLine();

			while (line != null)
			{
				// split 1 2 3 to [1, 2, 3]
				string[] splitedString = line.Split(' ');

				// store item in a set in the form of list
				List<int> itemset = new();

				foreach (string item in splitedString)
				{
					// convert str to int
					int value = int.Parse(item);

					itemset.Add(value);
				}

				// add itemset into transactions
				transactions.Add(itemset);

				// read the next line
				line = sr.ReadLine();
			}

			sr.Close();

			return transactions;
		}

		List<List<int>> GetSingleItemset(List<List<int>> transactions)
		{
			List<List<int>> firstItemsets = new();

			foreach (List<int> transaction in transactions)
			{
				foreach (int item in transaction)
				{
					List<int> itemset = new();

					// make item like 1 transform to {1}
					itemset.Add(item);

					// check whether {1} is in {{1}, {2},..} to not duplicate
					if (CheckListInLists(firstItemsets, itemset))
					{
						continue;
					}
					else
					{
						firstItemsets.Add(itemset);
					}
				}
			}

			return firstItemsets;
		}

		bool CheckListInLists(List<List<int>> firstItemsets, List<int> itemset)
		{
			bool isVisited = false;

			foreach (List<int> visitedList in firstItemsets)
			{
				if (visitedList.All(itemset.Contains))
				{
					isVisited = true;
					break;
				}
			}

			return isVisited;
		}

		Dictionary<List<int>, int> CountSupport(List<List<int>> transactions, List<List<int>> candidates)
		{
			Dictionary<List<int>, int> frequentItems = new Dictionary<List<int>, int>();

			foreach (List<int> candidateFormList in candidates)
			{
				int support = 0;

				foreach (List<int> transaction in transactions)
				{
					bool isChecked = candidateFormList.All(c => transaction.Contains(c));

					if (isChecked)
					{
						support++;
					}
				}

				// support greater than or equal minsup will add
				if (support >= _minsup)
				{
					frequentItems.Add(candidateFormList, support);
				}
			}

			// sort dictionary by value like {{4}: 4, {3}: 2,..}
			frequentItems = frequentItems.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

			return frequentItems;
		}

		Dictionary<List<int>, int> CountSupport(List<List<int>> transactions, Dictionary<List<int>, int> candidates)
		{
			Dictionary<List<int>, int> frequentItems = new Dictionary<List<int>, int>();

			foreach (List<int> candidateFormList in candidates.Keys)
			{
				int support = 0;

				foreach (List<int> transaction in transactions)
				{
					foreach (int item in candidateFormList)
					{
						if (transaction.Contains(item))
						{
							support++;
						}
					}
				}

				// support greater than or equal minsup will add
				if (support >= _minsup)
				{
					frequentItems.Add(candidateFormList, support);
				}
			}

			// sort dictionary by value like {{4}: 4, {3}: 2,..}
			frequentItems = frequentItems.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

			return frequentItems;
		}

		List<List<int>> SortTransactionsFollowFrequentCount(List<List<int>> transactions, Dictionary<List<int>, int> frequentItems)
		{
			// loop through transaction count
			for (int i = 0; i < transactions.Count; i++)
			{
				List<int> transaction = new();

				foreach (List<int> itemset in frequentItems.Keys)
				{
					// check if transaction has item then add in descending order
					if (transactions[i].Contains(itemset[0]))
					{
						transaction.Add(itemset[0]);
					}
				}

				transactions[i] = transaction;
			}

			return transactions;
		}

		// generate tree in the form of dictionary
		// {{1}: {{3}, {3,2}, {3, 4}}} means we have 3 paths to go to item 1
		Dictionary<int, List<List<int>>> GenerateConditionalPatternBase(List<List<int>> transactions, Dictionary<List<int>, int> frequentItems)
		{
			Dictionary<int, List<List<int>>> tree = new();

			foreach (List<int> transaction in transactions)
			{
				// loop through keys of frequentItems
				// itemset[0] = {1}, itemset[0] = {2}, ...
				foreach (List<int> itemset in frequentItems.Keys)
				{
					// check if transaction has item 1 or 2 or 3,...
					if (transaction.Contains(itemset[0]))
					{
						// get index of item 1 or 2 or 3,... in transaction
						int childIndex = transaction.IndexOf(itemset[0]);
						const int IndexOfParentNode = 0;

						// check if index = 0 then it is a parent node and skip
						if (childIndex == IndexOfParentNode)
						{
							continue;
						}

						int childNode = transaction[childIndex];

						// if has already child node then add into
						if (tree.ContainsKey(childNode))
						{
							tree[childNode].Add(GetElementFrom0ToIndex(transaction, childIndex));
						}
						// if not then create new one
						else
						{
							List<List<int>> values = new() { GetElementFrom0ToIndex(transaction, childIndex) };

							tree.Add(childNode, values);
						}
					}
				}
			}

			return tree;
		}

		List<int> GetElementFrom0ToIndex(List<int> transaction, int index)
		{
			List<int> elementsFrom0ToIndex = new();

			for (int i = 0; i < index; i++)
			{
				elementsFrom0ToIndex.Add(transaction[i]);
			}

			return elementsFrom0ToIndex;
		}

		// get all the common paths in Conditional Pattern Tree
		Dictionary<int, List<List<int>>> GenerateConditionalFPTree(Dictionary<int, List<List<int>>> conditionalPatternBase, Dictionary<List<int>, int> frequentItems)
		{

			// item = key
			foreach (int item in conditionalPatternBase.Keys)
			{
				// count frequent of all set 
				Dictionary<List<int>, int> itemSupport = CountSupport(conditionalPatternBase[item], frequentItems);

				List<List<int>> itemsets = FilterDictionaryToListFollowingKey(itemSupport);

				conditionalPatternBase[item] = itemsets;
			}

			return conditionalPatternBase;
		}

		Dictionary<List<int>, int> GenerateFrequentItemsets(List<List<int>> transactions, Dictionary<int, List<List<int>>> conditionalFPTree)
		{
			// store all frequent itemsets
			Dictionary<List<int>, int> frequentPattern = new();

			// loop through key of FPTree
			foreach (int keyNode in conditionalFPTree.Keys)
			{
				const int IndexOfSingleSet = 1;

				// itemsets = conditionalFPTree[keyNode]
				// If itemsets = 1 then it just has a single set
				// {{1}} <-- like this
				if (conditionalFPTree[keyNode].Count == IndexOfSingleSet)
				{
					// if just single set then add keyNode into
					// => a frequent itemset
					GetCombineFromItemsetsWithKeyNode(conditionalFPTree[keyNode], keyNode);

					// count support of each itemsets like {{1}: 4, {2}: 5, ...}
					Dictionary<List<int>, int> frequentItems1 = CountSupport(transactions, conditionalFPTree[keyNode]);

					// assign itemset + their minsup into frequent pattern
					AddFrequentItemToFrequentPattern(frequentPattern, frequentItems1);
				}
				// If itemsets > 1 then we must combine them
				else
				{
					// combine itemsets with keyNode
					GetCombineFromItemsetsWithKeyNode(conditionalFPTree[keyNode], keyNode);

					// count support of each itemsets like {{1}: 4, {2}: 5, ...}
					Dictionary<List<int>, int> frequentItems = CountSupport(transactions, conditionalFPTree[keyNode]);

					// assign itemset + their minsup into frequent pattern
					AddFrequentItemToFrequentPattern(frequentPattern, frequentItems);

					int numberOfCandidateNeedToBeGenerated = 2;

					while (numberOfCandidateNeedToBeGenerated <= conditionalFPTree[keyNode].Count)
					{
						numberOfCandidateNeedToBeGenerated++;

						List<List<int>> frequentItemsetsWithNoValue = FilterDictionaryToListFollowingKey(frequentItems);

						List<List<int>> candidateK = GenerateCandidateK(frequentItemsetsWithNoValue, numberOfCandidateNeedToBeGenerated);

						frequentItems = CountSupport(transactions, candidateK);

						// assign itemset + their minsup into frequent pattern
						AddFrequentItemToFrequentPattern(frequentPattern, frequentItems);
					}
				}
			}

			return frequentPattern;
		}

		// {{3}, {2}, {5}}, keyNode = 1
		// => {{3,1}, {2,1}, {5,1}}
		void GetCombineFromItemsetsWithKeyNode(List<List<int>> itemsets, int keyNode)
		{
			for (int i = 0; i < itemsets.Count; i++)
			{
				// store itemset with keyNode
				List<int> itemsetWithKeyNode = new();

				// loop through every element of itemset at index i
				// add it into itemsetWithKeyNode
				foreach (int itemset in itemsets[i])
				{
					itemsetWithKeyNode.Add(itemset);
				}

				// add keyNode into itemsetWithKeyNode
				itemsetWithKeyNode.Add(keyNode);

				// assign again itemset at index i with itemsetWithKeyNode
				itemsets[i] = itemsetWithKeyNode;
			}
		}

		void AddFrequentItemToFrequentPattern(Dictionary<List<int>, int> frequentPattern, Dictionary<List<int>, int> frequentItems)
		{
			foreach (KeyValuePair<List<int>, int> frequentItem in frequentItems)
			{
				if (!frequentPattern.ContainsKey(frequentItem.Key))
				{
					frequentPattern.Add(frequentItem.Key, frequentItem.Value);
				}
			}
		}

		List<List<int>> FilterDictionaryToListFollowingKey(Dictionary<List<int>, int> itemSupport)
		{
			// store in the form { {1, 3}, {4, 1} }
			List<List<int>> itemLists = new();

			foreach (List<int> itemKey in itemSupport.Keys)
			{
				itemLists.Add(itemKey);
			}

			return itemLists;
		}

		List<List<int>> GenerateCandidateK(List<List<int>> frequentItemsetsWithNoValue, int numberOfCandidateNeedToBeGenerated)
		{
			// store all candi
			List<List<int>> candidates = new List<List<int>>();

			// loop through each itemsets
			for (int i = 0; i < frequentItemsetsWithNoValue.Count; i++)
			{
				for (int j = i + 1; j < frequentItemsetsWithNoValue.Count; j++)
				{
					// union {1, 3} & {1, 4} => {1, 3, 4}
					List<int> pattern = frequentItemsetsWithNoValue[i].Union(frequentItemsetsWithNoValue[j]).ToList();

					// check if visited then not add
					bool isChecked = CheckListInLists(candidates, pattern);

					if ((!isChecked) && (pattern.Count == numberOfCandidateNeedToBeGenerated))
					{
						candidates.Add(pattern);
					}
				}
			}

			return candidates;
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			string input = "../../../input.txt";
			string output = "../../../output.txt";

			FPGrowth algo = new FPGrowth();

			algo.RunAlgorithm(input, output, 0.4);
		}
	}
}