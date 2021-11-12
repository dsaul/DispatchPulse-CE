using System.Text.RegularExpressions;

namespace SharedCode.RegexUtils
{
	public static class Konstants
	{
		public static System.Text.RegularExpressions.Regex NotLettersNumbersRegex { get; } = new System.Text.RegularExpressions.Regex("[^a-zA-Z0-9]");
	}
}
