#include "Common.h"
#include "List.h"


int main()
{
	List<int> li;
	List<int>::iterator eraselt;

	for (int i = 0; i < 10; i++)
	{
		if (i == 5)
		{
			eraselt = li.insert(li.end(), i);
		}
		else
		{
			li.push_back(i);
		}
	}

	li.pop_back();

	li.erase(eraselt);

	for (List<int>::iterator it = li.begin(); it != li.end(); it++)
	{
		cout << (*it) << endl;
	}

	return 0;
}

