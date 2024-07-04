using System;

namespace myproj
{
    class Program
    {
        static bool CheckIsNotIn(List<List<int>> firstItemsets, List<int> itemset)
		{
			bool isNotIn = false;

			foreach (List<int> visitedList in firstItemsets)
			{
				if (visitedList.All(itemset.Contains))
				{
					isNotIn = true;
					break;
				}
			}
			return isNotIn;
		}

        static void getSubsets(List<int> superSet, int k, int idx, List<int> current, List<List<int>> solution){
            if (current.Count == k){
                solution.Add(new List<int>(current));
                return;
            }
            if (idx == superSet.Count)
                return;
            int x = superSet.ElementAt(idx);
            current.Add(x);
            getSubsets(superSet,k,idx+1,current,solution);
            current.Remove(x);
            getSubsets(superSet,k,idx+1,current,solution);
        }

        static List<List<int>> getSubsets(List<int> superSet, int k){
            List<List<int>> res = new List<List<int>>();
            getSubsets(superSet,k,0,new List<int>(),res);
            return res;
        }

        static Dictionary<List<int>,int> removeInfrequent(Dictionary<List<int>,int> Candidate, int minsup)
        {
            foreach (var item in Candidate){
                if (item.Value < minsup){
                    Candidate.Remove(item.Key);
                }
            }
            return Candidate;
        }

        static List<List<int>> join_step (List<List<int>> Fk_1, List<int> FrequentItem){
            List<List<int>> Candidate = new List<List<int>>();
            foreach (var itemset in Fk_1){
                foreach (var item in FrequentItem){
                    itemset.Sort();
                    if (!itemset.Contains(item)){
                        List<int> tmp = new List<int> (itemset);
                        tmp.Add(item);
                        tmp.Sort();
                        if (!CheckIsNotIn(Candidate,tmp)){
                            Candidate.Add(tmp);
                        }
                    }
                    
                }
            }
            return Candidate;
        }

        static Dictionary<string,int> printResult(Dictionary<List<int>,int> frequentItemset, Dictionary<int,string> items){
            Dictionary<string,int> res = new Dictionary<string,int>();
            foreach (var itemset in frequentItemset){
                String key = "";
                for (int i = 0 ; i < itemset.Key.ToList().Count ; i++){
                    key += items[itemset.Key.ToList().ElementAt(i)];
                    if (i <= (itemset.Key.ToList().Count - 2)){
                        key += ", ";
                    }
                }
                res.Add(key,itemset.Value);
            }
            return res;
        }
    
        static int count_sup(int[] itemSet, int[,] db){
            int sup = 0;
            for (int i=0; i < db.GetLength(0); i++){
                int tmpCount = 0;
                for (int j = 0; j < itemSet.Length; j++){
                    if (db[i,itemSet[j]] == 1)   {
                        tmpCount++;
                    }
                }
                // if all item in itemSet occur in i-th transaction then sup increase
                if (tmpCount == itemSet.Length){
                    sup++;
                }
            }
            return sup;
        }

        static Dictionary<string, int> Apriori (int[,] db, Dictionary<int,string> items, int minsup) {
            // Find Frequent 1th-itemset
            Dictionary<List<int>,int> FrequentItemSet = new Dictionary<List<int>,int>();
            for(int i = 0; i < items.Count; i++) {
                FrequentItemSet.Add(new List<int>(){i},count_sup(new int[]{i},db));
                FrequentItemSet = removeInfrequent(FrequentItemSet,minsup);
            }
            int k = 2;
            Dictionary<List<int>,int> Fk = new Dictionary<List<int>, int>(FrequentItemSet);
            List<List<int>> Fk_Keys = new List<List<int>>(Fk.Keys.ToList());
            while (Fk.Count > 0){
                Fk.Clear();
                // Genrating Ck
                List<List<int>> Ck = new List<List<int>>();
                // Find the rest of item which is in k-1_th-FrequentItemset
                List<int> distinctItem = new List<int>();
                foreach (var itemset in Fk_Keys){
                    foreach (var item in itemset){
                        if (!distinctItem.Contains(item)){
                            distinctItem.Add(item);
                        }
                    }
                }

                Ck = join_step(new List<List<int>> (Fk_Keys),distinctItem);
                foreach (var itemset in Ck.ToList()){
                    // Find all subset of each itemset in Ck with k-1_th 
                    List<List<int>> subset = new List<List<int>>();
                    subset = getSubsets(itemset,k-1);
                    // Applying Apriori property "all their non-subset must be frequent"
                    foreach(var child in subset){
                        if (!CheckIsNotIn(new List<List<int>> (Fk_Keys), child)){
                            Ck.Remove(itemset);
                            break;
                        }
                    }
                    // Let Fk contains all frequent itemset
                    if (CheckIsNotIn(new List<List<int>> (Fk_Keys), itemset)){
                        Fk.Add(itemset,count_sup(itemset.ToArray(),db));
                        FrequentItemSet.Add(itemset,count_sup(itemset.ToArray(),db));
                    }
                }
                Fk = removeInfrequent(Fk,minsup);
                FrequentItemSet = removeInfrequent(FrequentItemSet,minsup);
                Fk_Keys = Fk.Keys.ToList();
                k++;
            }
            return printResult(FrequentItemSet,items);
        }

        static void Main(String[] args)
        {
            // Transaction Database
            Dictionary<int,string> items = new Dictionary<int, string>() {{0,"a"},{1,"b"},{2,"c"},{3,"d"},{4,"e"}};
            int[,] db = new int[,]{
                {1,0,1,1,0},
                {0,1,1,0,1},
                {1,1,1,0,1},
                {0,1,0,0,1},
                {1,1,1,0,1}
            };
            Dictionary<string,int> res = Apriori(db,items,3);
            foreach(KeyValuePair<string,int> kvp in res){
                Console.WriteLine(kvp.Key +" : "+kvp.Value);
            }
        }
    }
}