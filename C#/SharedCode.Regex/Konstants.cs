using System.Text.RegularExpressions;

namespace SharedCode.RegexUtils
{
	public static class Konstants
	{
		public static Regex NotLettersNumbersRegex { get; } = new Regex("[^a-zA-Z0-9]");
		public static Regex NotLettersNumbersUnderscoreRegex { get; } = new Regex("[^_a-zA-Z0-9]");
	}
}
