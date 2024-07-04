using System.Diagnostics;

namespace AprioriTIDAlgorithmVer2
{
	class Itemset
	{
		/** The list of items contained in this itemset, ordered by 
		lexical order */
		public int[] itemset;

		/** The set of transactions/sequences id containing this itemset */
		public HashSet<int> transactionsIds = new();

		/**
		 * Constructor 
		 * <param name="item">an item that should be added to the new itemset</param>
		 */
		public Itemset(int item)
		{
			itemset = new int[] { item };
		}

		public Itemset(int[] items)
		{
			itemset = items;
		}

		/**
		 * Set the list of transaction/sequence ids containing this itemset
		 * <param name="listTransactionIds">the list of transaction/sequence ids</param>
		 */
		public void SetTIDs(HashSet<int> listTransactionIds)
		{
			transactionsIds = listTransactionIds;
		}

		public HashSet<int> TransactionsIds
		{
			get
			{
				return transactionsIds;
			}
		}

		public int Count
		{
			get
			{
				return itemset.Length;
			}
		}

		public int Get(int index)
		{
			return itemset[index];
		}

		public int[] GetItems()
		{
			return itemset;
		}

		public override string ToString()
		{
			return string.Join(" ", itemset);
		}
	}

	class AprioriTID
	{
		/**
		 * the current level
		 */
		protected int k;

		/**
		 * variables for counting support of items
		 */
		Dictionary<int, HashSet<int>> mapItemTIDS = null;

		/**
		 * the number of transactions
		 */
		private int _databaseSize = 0;

		/**
		 * the minimum support threshold
		 */
		int minSupRelative;

		StreamWriter sw = null;

		public void RunAlgorithm(string input, string output, double minsup)
		{
			sw = new StreamWriter(output, false);

			// (1) count the tid set of each item in the database in one database
			// pass
			mapItemTIDS = new(); // id item, count

			_databaseSize = 1;

			// read
			StreamReader sr = new StreamReader(input);

			string line = sr.ReadLine();

			while (line != null)
			{
				// split the line into tokens according to spaces
				string[] splitedLine = line.Split(' ');

				// for each token (item)
				foreach (string token in splitedLine)
				{
					// convert from string item to integer
					int item = int.Parse(token);

					HashSet<int> tids;

					if (mapItemTIDS.ContainsKey(item))
					{
						tids = mapItemTIDS[item];
					}
					else
					{
						tids = new();
						mapItemTIDS.Add(item, tids);
					}

					// add the current transaction id (tid) to the set of the current item
					tids.Add(_databaseSize);
				}

				_databaseSize++; // increment the tid number

				line = sr.ReadLine();
			}

			sr.Close();

			Process currProcess = Process.GetCurrentProcess();
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			// convert the support from a relative minimum support (%) to an
			// absolute minimum support
			minSupRelative = (int)(minsup * _databaseSize);

			// To build level 1, we keep only the frequent items.
			// We scan the database one time to calculate the support of each
			// candidate.
			List<Itemset> level = new();

			// For each item
			foreach (KeyValuePair<int, HashSet<int>> mapItemTID in mapItemTIDS)
			{
				// if the item is frequent
				if (mapItemTID.Value.Count >= minSupRelative)
				{
					int item = mapItemTID.Key;

					Itemset itemset = new(item);
					itemset.SetTIDs(mapItemTIDS[item]);

					level.Add(itemset);

					// save the itemset
					sw.WriteLine(itemset.ToString() + " #SUP: " + itemset.TransactionsIds.Count);
				}
				else
				{
					mapItemTIDS.Remove(mapItemTID.Key);
				}
			}

			// sort itemsets of size 1 according to lexicographical order.
			level.Sort((itemset1, itemset2) => itemset1.Get(0) - itemset2.Get(0));

			// Generate candidates with size k = 1 (all itemsets of size 1)
			k = 2;

			// While the level is not empty
			while (level.Count > 0)
			{
				// We build the level k+1 with all the candidates that have
				// a support higher than the minsup threshold.
				level = GenerateCandidateSizeK(level);
			}

			sw.Close();
			stopwatch.Stop();

			Console.WriteLine("Total time ~ " + stopwatch.Elapsed.TotalMilliseconds + " ms");
			Console.WriteLine("Total memory usage ~ " + (currProcess.PrivateMemorySize64 / (1024 * 1024)) + " mb");
		}

		List<Itemset> GenerateCandidateSizeK(List<Itemset> levelK_1)
		{
			// create a variable to store candidates
			List<Itemset> candidates = new List<Itemset>();

			// For each itemset I1 and I2 of level k-1
			for (int i = 0; i < levelK_1.Count; i++)
			{
				Itemset itemset1 = levelK_1[i];

				bool flagLoop1 = false;

				for (int j = i + 1; j < levelK_1.Count; j++)
				{
					Itemset itemset2 = levelK_1[j];

					bool flagLoop2 = false;

					// we compare items of itemset1 and itemset2.
					// If they have all the same k-1 items and the last item of
					// itemset1 is smaller than
					// the last item of itemset2, we will combine them to generate a
					// candidate
					for (int k = 0; k < itemset1.Count; k++)
					{
						// if they are the last items
						if (k == (itemset1.Count - 1))
						{
							// the one from itemset1 should be smaller (lexical
							// order)
							// and different from the one of itemset2
							if (itemset1.GetItems()[k] >= itemset2.Get(k))
							{
								flagLoop1 = true;
								break;
							}
						}
						// if the k-th items is smaller than itemset1
						else if (itemset1.GetItems()[k] < itemset2.GetItems()[k])
						{
							flagLoop2 = true;
							break;
						}
						else if (itemset1.GetItems()[k] > itemset2.GetItems()[k])
						{
							flagLoop1 = true;
							break;
						}
					}

					// continue loop 1
					if (flagLoop1)
					{
						break;
					}

					// continue loop 2
					if (flagLoop2)
					{
						continue;
					}

					// create list of common tids
					HashSet<int> list = new();

					// for each tid from the tidset of itemset1
					foreach (int val1 in itemset1.TransactionsIds)
					{
						// if it appears also in the tidset of itemset2
						if (itemset2.TransactionsIds.Contains(val1))
						{
							// add it to common tids
							list.Add(val1);
						}
					}

					// if the combination of itemset1 and itemset2 is frequent
					if (list.Count >= minSupRelative)
					{
						// Create a new candidate by combining itemset1 and itemset2
						int[] newItemset = new int[itemset1.Count + 1];

						Array.Copy(itemset1.itemset, 0, newItemset, 0, itemset1.Count);
						
						newItemset[itemset1.Count] = itemset2.GetItems()[itemset2.Count - 1];
						
						Itemset candidate = new Itemset(newItemset);
						
						candidate.SetTIDs(list);
						
						// add it to the list of candidates
						candidates.Add(candidate);

						// save the itemset
						sw.WriteLine(candidate.ToString() + " #SUP: " + candidate.TransactionsIds.Count);
					}
				}

				// continue loop 1
				if (flagLoop1)
				{
					continue;
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

			double minsup = 0.4;

			AprioriTID algo = new();

			// Run the algorithm
			algo.RunAlgorithm(input, output, minsup);
		}
	}
}