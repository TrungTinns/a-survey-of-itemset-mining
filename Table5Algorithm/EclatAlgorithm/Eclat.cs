using AprioriAlgorithm;
using System.Diagnostics;

namespace EclatAlgorithm
{
	public class Eclat
	{
	
		static void EclatAlgorithm(Dictionary<List<int>, List<int>> database, Dictionary<List<int>, List<int>> eclatOutput, int minsup)
		{
			for (int i = 0; i < database.Count; i++)
			{
				KeyValuePair<List<int>, List<int>> itemset1 = database.ElementAt(i);

				eclatOutput.Add(itemset1.Key, itemset1.Value);

				Dictionary<List<int>, List<int>> filterDatabase = new Dictionary<List<int>, List<int>>();

				for (int j = i + 1; j < database.Count; j++)
				{
					KeyValuePair<List<int>, List<int>> itemset2 = database.ElementAt(j);

					List<int> candidate = itemset1.Key.Union(itemset2.Key).ToList();
					List<int> tid = itemset1.Value.Intersect(itemset2.Value).ToList();

					if (tid.Count >= minsup)
					{
						filterDatabase.Add(candidate, tid);
					}
				}

				EclatAlgorithm(filterDatabase, eclatOutput, minsup);
			}
		}

		public static Dictionary<List<int>, List<int>> RemoveKeyHasValueLessThanMinSup(Dictionary<List<int>, List<int>> database, int minsup)
		{
			Dictionary<List<int>, List<int>> filterDatabase = new(database);

			foreach (KeyValuePair<List<int>, List<int>> item in database)
			{
				if (item.Value.Count < minsup)
				{
					filterDatabase.Remove(item.Key);
				}
			}

			return filterDatabase;
		}

		public static int GetKeyIndex(Dictionary<List<int>, List<int>> database, List<int> key)
		{
			int index = -1;
			bool resetIndexFlag = false;

			foreach (List<int> item in database.Keys)
			{
				if (item.All(key.Contains))
				{
					index++;
					resetIndexFlag = true;
					break;
				}

				index++;
			}

			if (!resetIndexFlag)
			{
				index = -1;
			}

			return index;
		}

		public static Dictionary<string, int> ConvertRightForm(Dictionary<List<int>, List<int>> database)
		{
			Dictionary<string, int> table = new Dictionary<string, int>();

			foreach (KeyValuePair<List<int>, List<int>> kp in database.OrderBy(e => e.Key.Count))
			{
				string key = string.Join(" ", kp.Key.ToArray());
				int value = kp.Value.Count;
				table.Add(key, value);
			}

			return table;
		}

		static void Main(string[] args)
		{
			string input = "../../../input.txt";
			string output = "../../../output.txt";

			// store first all data in the form of vertical representation
			Dictionary<List<int>, List<int>> database = new();

			// store result
			Dictionary<List<int>, List<int>> eclatOutput = new();

			// get all distinct items
			HashSet<int> transactionCount = new();

			StreamReader sr = new(input);

			string line = sr.ReadLine();

			while (line != null)
			{
				// split "1 2 3" to [1, 2, 3]
				string[] splitedLine = line.Split(' ');

				// assign key at the first index into a list (Ex: {1})
				// Form database (dictionary)
				// Item : TIDs
				List<int> key = new List<int> { int.Parse(splitedLine[0]) };
				
				// function to get index of key because of its set form
				int keyIndex = GetKeyIndex(database, key);

				// loop from the second index of arr
				for (int i = 1; i < splitedLine.Length; i++)
				{
					// convert to int
					int value = int.Parse(splitedLine[i]);

					// add distinct item one by one to get all distinct items
					transactionCount.Add(value);

					// means have this key in dictionary
					// so add value into list values of database
					if (keyIndex > - 1)
					{
						database.ElementAt(keyIndex).Value.Add(value);
					}
					// = -1 means it does not have in dictionary
					// so add key into database first and add first value of it
					else
					{
						List<int> values = new List<int>() { value };

						database.Add(key, values);

						// assign index of key after initialize it in database 
						keyIndex = GetKeyIndex(database, key);
					}
				}

				line = sr.ReadLine();
			}

			sr.Close();

			// calculate min support
			int minsup = (int)(0.4 * transactionCount.Count);

			// remove 1-itemset less than minsup
			Dictionary<List<int>, List<int>> filterDatabase = RemoveKeyHasValueLessThanMinSup(database, minsup);

			// Measure all memory usage & runtime
			Process currProcess = Process.GetCurrentProcess();
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			EclatAlgorithm(filterDatabase, eclatOutput, minsup);

			// convert list to string to be can write file
			Dictionary<string, int> table = ConvertRightForm(eclatOutput);

			Apriori.WriteToFile(table, output);

			stopwatch.Stop();

			Console.WriteLine("Total time ~ " + stopwatch.Elapsed.TotalMilliseconds + " ms");
			Console.WriteLine("Total memory usage ~ " + (currProcess.PrivateMemorySize64 / (1024 * 1024)) + " mb");
		}
	}
}