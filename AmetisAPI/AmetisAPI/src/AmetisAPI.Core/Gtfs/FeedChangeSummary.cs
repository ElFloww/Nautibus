namespace AmetisAPI.Core.Gtfs
{
    /// <summary>Résumé des ajouts et suppressions entre deux versions, pour le journal d'import.</summary>
    internal static class FeedChangeSummary
    {
        private const int MaxListedIds = 50;

        public static string Build(IReadOnlyCollection<string> p_OldRoutes, IReadOnlyCollection<string> p_NewRoutes, IReadOnlyCollection<string> p_OldStops, IReadOnlyCollection<string> p_NewStops)
        {
            return $"{Part("Lignes", p_OldRoutes, p_NewRoutes)} | {Part("Arrêts", p_OldStops, p_NewStops)}";
        }

        private static string Part(string p_Label, IReadOnlyCollection<string> p_Old, IReadOnlyCollection<string> p_New)
        {
            List<string> v_Added = p_New.Except(p_Old).Order().ToList();
            List<string> v_Removed = p_Old.Except(p_New).Order().ToList();
            string v_Text = $"{p_Label} : +{v_Added.Count} -{v_Removed.Count}";
            if (v_Removed.Count > 0)
                v_Text += $" (supprimés : {string.Join(", ", v_Removed.Take(MaxListedIds))}{(v_Removed.Count > MaxListedIds ? ", ..." : "")})";
            return v_Text;
        }
    }
}
