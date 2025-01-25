namespace ClassBuilder.Extensions
{
    public static class StringExtension
    {
        public static string GetWordWithFirstLetterDown(this string word)
        {
            return $"{word.ToLower()[0]}{word.Substring(1)}";
        }

        public static string GetWordWithFirstLetterUpper(this string word)
        {
            return $"{word.ToUpper()[0]}{word.Substring(1)}";
        }
    }
}