# A Survey of Itemset Mining Algorithms

## Introduction

This project surveys and implements popular itemset mining algorithms, including Apriori, Apriori-TID, Eclat, and FP-Growth. We cover theoretical foundations, practical implementations, and comparative analysis of these key algorithms in the field of data mining.

## Algorithms and Key Features

1. **Apriori Algorithm** (Breadth-First Search, Horizontal database)
2. **Apriori-TID** (Vertical database variant)
3. **Eclat** (Depth-First Search, Vertical database)
4. **FP-Growth** (Tree-based approach, eliminates candidate generation)

Features include in-depth explanations, step-by-step examples, comparative analysis, and discussions on advantages and limitations of each approach.

## Getting Started

1. Clone the repository: `git clone https://github.com/yourusername/itemset-mining-survey.git`
2. Ensure Python 3.x is installed
3. Install dependencies: `pip install -r requirements.txt`
4. Run examples:
python apriori/apriori_example.py
python eclat/eclat_example.py
python fp_growth/fp_growth_example.py
Copy
## Project Structure
itemset-mining-survey/
├── apriori/
├── apriori_tid/
├── eclat/
├── fp_growth/
├── data/
├── docs/
├── README.md
├── requirements.txt
└── LICENSE
Copy
## Contributors

- Huỳnh Trần Minh Tiến (520H0583): Apriori and Eclat algorithms
- Nguyễn Trung Tín (520H0589): Apriori-TID and FP-Growth algorithms

## Future Work

- Implement recent itemset mining algorithms
- Optimize for large-scale datasets
- Develop a unified comparison interface
- Extend to related topics like sequential pattern mining

## References

1. Agrawal, R., & Srikant, R. (1994). Fast algorithms for mining association rules.
2. Zaki, M. J. (2000). Scalable algorithms for association mining.
3. Han, J., Pei, J., & Yin, Y. (2000). Mining frequent patterns without candidate generation.
4. [Additional references from project document]

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
 
