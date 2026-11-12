namespace AmetisAPI.Domain.Bo.Events
{
    public enum NetworkEventKind
    {
        /// <summary>Perturbation publiée par Ametis (Service Alerts).</summary>
        Official = 1,

        /// <summary>Déviation estimée à partir des positions des bus : non confirmée par Ametis.</summary>
        DetectedDetour = 2
    }
}
