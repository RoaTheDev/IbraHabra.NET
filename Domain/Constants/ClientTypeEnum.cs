namespace IbraHabra.NET.Domain.Constants;

public enum ClientTypeEnum
{
    Web,        // Server-side web app (confidential)
    Spa,        // Vue.js, React (public, PKCE)
    Mobile,     // iOS/Android (public, PKCE)
    Machine     // Backend service (confidential, client_credentials)
}