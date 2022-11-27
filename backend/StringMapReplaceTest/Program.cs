using SharedCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace StringMapReplaceTest
{
	class Program
	{
		

		static void Main(string[] args)
		{
			Console.WriteLine("String Map Replace");

			Dictionary<string, string> map = new Dictionary<string, string>();
			map.Add("cat", "dog");
			map.Add("dog", "goat");
			map.Add("goat", "cat");

			string mod = "I have a cat, a dog, and a goat.".MapReplace(map);
			Console.WriteLine(mod);
		}
	}
}
