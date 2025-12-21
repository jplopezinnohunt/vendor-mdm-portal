namespace VendorMdm.Api.Services.Helpers;

/// <summary>
/// Levenshtein distance calculator for fuzzy string matching
/// Used for duplicate vendor detection
/// Pattern from UNESCO MoUV system
/// </summary>
public class LevenshteinMatcher
{
    /// <summary>
    /// Calculate similarity between two strings (0.0 to 1.0)
    /// Higher score = more similar
    /// Threshold typically 0.75 for vendor matching
    /// </summary>
    public double CalculateSimilarity(string source, string target)
    {
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
            return 0.0;

        // Normalize: uppercase and trim
        source = source.ToUpper().Trim();
        target = target.ToUpper().Trim();

        if (source == target)
            return 1.0;

        int distance = ComputeLevenshteinDistance(source, target);
        int maxLength = Math.Max(source.Length, target.Length);
        
        return 1.0 - ((double)distance / maxLength);
    }

    /// <summary>
    /// Compute Levenshtein distance between two strings
    /// Classic dynamic programming algorithm
    /// </summary>
    private int ComputeLevenshteinDistance(string s, string t)
    {
        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];

        if (n == 0) return m;
        if (m == 0) return n;

        // Initialize first column and row
        for (int i = 0; i <= n; i++)
            d[i, 0] = i;
        for (int j = 0; j <= m; j++)
            d[0, j] = j;

        // Calculate distances
        for (int j = 1; j <= m; j++)
        {
            for (int i = 1; i <= n; i++)
            {
                int cost = (s[i - 1] == t[j - 1]) ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(
                        d[i - 1, j] + 1,      // deletion
                        d[i, j - 1] + 1),      // insertion
                    d[i - 1, j - 1] + cost);  // substitution
            }
        }

        return d[n, m];
    }

    /// <summary>
    /// Calculate similarity for person name (combines first + last)
    /// </summary>
    public double CalculatePersonNameSimilarity(
        string firstName1,
        string lastName1, 
        string firstName2,
        string lastName2)
    {
        string fullName1 = $"{lastName1} {firstName1}".Trim();
        string fullName2 = $"{lastName2} {firstName2}".Trim();
        
        return CalculateSimilarity(fullName1, fullName2);
    }
}
