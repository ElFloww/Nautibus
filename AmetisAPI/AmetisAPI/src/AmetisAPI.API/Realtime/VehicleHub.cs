using Microsoft.AspNetCore.SignalR;

namespace AmetisAPI.API.Realtime
{
    /// <summary>Diffusion des positions de véhicules (événement « vehicles »). Lecture seule pour les clients.</summary>
    public class VehicleHub : Hub
    {
        /// <summary>Nom de l'événement envoyé aux clients.</summary>
        public const string VehiclesEvent = "vehicles";

        /// <summary>Nom de l'événement envoyé avec la liste des infos trafic.</summary>
        public const string EventsEvent = "events";

        /// <summary>Route du hub.</summary>
        public const string Route = "/hubs/vehicles";
    }
}
