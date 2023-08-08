using Ar6yZuK.Habitica;

namespace HabiticaHourUpVSIX.AppSettings.Models;
public record struct HabiticaCredentials(string UserId, string ApiKey) : ICredentials;