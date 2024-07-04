using System.Diagnostics;
namespace eclat
{
    public class Eclat
    {
        public static int GetKeyIndex(Dictionary<List<int>, List<int>> data, List<int> key)
		{
			int index = -1;
			bool fl = false;

			foreach (List<int> item in data.Keys)
			{
				//ktra xem tất cả các item trong data.key có trong tập key không?
				//Có -> fl= true
				if (item.All(key.Contains))
				{
					index++;
					fl = true;
					break;
				}

				index++;
			}

			if (!fl)
			{
				index = -1;
			}

			return index;
		}

		public static Dictionary<List<int>, List<int>> removeitemset(Dictionary<List<int>, List<int>> data, int minsup)
		{
			Dictionary<List<int>, List<int>> transaction = new(data);

			foreach (KeyValuePair<List<int>, List<int>> item in data)
			{
				if (item.Value.Count < minsup)
				{
					transaction.Remove(item.Key);
				}
			}

			return transaction;
		}
		
		static void EclatAlgorithm(Dictionary<List<int>, List<int>> data, Dictionary<List<int>, List<int>> result, int minsup)
		{
			for (int i = 0; i < data.Count; i++)
			{
                // Dictionary<(Of <(TKey, TValue>)>) - Đại diện cho một tập hợp các khóa và giá trị
                // KeyValuePair<(Of <(TKey, TValue>)>) - Xác định cặp khóa/giá trị có thể được đặt hoặc truy xuất.   

                //Lấy tập key value trong data add vào result  
				KeyValuePair<List<int>, List<int>> itemset1 = data.ElementAt(i);
				result.Add(itemset1.Key, itemset1.Value);

                //Tạo 1 dict lưu các item giao nhau
				Dictionary<List<int>, List<int>> transaction = new Dictionary<List<int>, List<int>>();

				for (int j = i + 1; j < data.Count; j++)
				{
                    KeyValuePair<List<int>, List<int>> itemset2 = data.ElementAt(j);

					List<int> candidate = itemset1.Key.Union(itemset2.Key).ToList();
					List<int> tid = itemset1.Value.Intersect(itemset2.Value).ToList();

					if (tid.Count >= minsup)
					{
						transaction.Add(candidate, tid);
					}
				}

				EclatAlgorithm(transaction, result, minsup);
			}
		}

		public static Dictionary<string, int> convert(Dictionary<List<int>, List<int>> data)
		{
			Dictionary<string, int> table = new Dictionary<string, int>();

			foreach (KeyValuePair<List<int>, List<int>> kp in data.OrderBy(i => i.Key.Count))
			{
				string key = string.Join(" ", kp.Key.ToArray());
				int value = kp.Value.Count;
				table.Add(key, value);
			}

			return table;
		}
        public static void WriteToFile(Dictionary<string, int> dictItemsCount, string output)
		{
			StreamWriter sw = new StreamWriter(output);

			foreach (KeyValuePair<string, int> keyValuePair in dictItemsCount)
			{
				sw.WriteLine(keyValuePair.Key + " || " + keyValuePair.Value);
			}

			sw.Close();
		}

		static void Main(string[] args)
		{
			Dictionary<List<int>, List<int>> data = new();

			Dictionary<List<int>, List<int>> result = new();

			HashSet<int> transactionCount = new();

			string input = "Z:\\eclat\\input.txt";
			string output = "Z:\\eclat\\output.txt";
			
			StreamReader sr = new(input);

			string line = sr.ReadLine();
			// Console.WriteLine(line);

			while (line != null)
			{
				string[] splitedLine = line.Split(' ');
			
				List<int> key = new List<int> { int.Parse(splitedLine[0]) };
				
				int keyIndex = GetKeyIndex(data, key);

				for (int i = 1; i < splitedLine.Length; i++)
				{
					int value = int.Parse(splitedLine[i]);

					transactionCount.Add(value);

					if (keyIndex > - 1)
					{
						data.ElementAt(keyIndex).Value.Add(value);
					}
					//ktra item kế tiếp có phải là key kh?
					//không -> add vào value
					else
					{
						List<int> values = new List<int>() { value };

						data.Add(key, values);

						keyIndex = GetKeyIndex(data, key);
					}
				}

				line = sr.ReadLine();
			}

			sr.Close();


			int minsup = (int)(0.4 * transactionCount.Count);

            //Xóa các transaction có item < minsup
			Dictionary<List<int>, List<int>> transaction = removeitemset(data, minsup);

			EclatAlgorithm(transaction, result, minsup);
		
			Dictionary<string, int> table = convert(result);

			WriteToFile(table, output);
        
        }
    }
}