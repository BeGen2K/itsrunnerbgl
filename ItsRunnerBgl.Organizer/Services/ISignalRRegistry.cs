namespace ItsRunnerBgl.Organizer.Services
{
    public interface ISignalRRegistry
    {
        void Register(string clientId, string username);
        string ClientIdFromUsername(string username);
    }
}
