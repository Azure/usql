namespace CodeBehindException
{
    public static class Helpers
    {
        public static string NormalizeAuthor(string author)
        {
            if (author == "mrys")
            {
                throw new System.ArgumentException("Unsupported author");
            }

            author = author.Trim();
            author = author.ToUpperInvariant();
            return author;
        }
    }

}
