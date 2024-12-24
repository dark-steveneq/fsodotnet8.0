using FSO.Common.Security;
using FSO.Server.Framework.Aries;

namespace FSO.Server.Framework.Voltron
{
    /// <summary>
    /// Interface for storing connection details
    /// </summary>
    public interface IVoltronSession : IAriesSession, ISecurityContext
    {
        /// <summary>
        /// Is connection after CAS?
        /// </summary>
        bool IsAnonymous { get; }

        /// <summary>
        /// UserID of sender
        /// </summary>
        uint UserId { get; }
        /// <summary>
        /// AvatarID of sender
        /// </summary>
        uint AvatarId { get; }
        /// <summary>
        /// Avatar Claim ID (token) of sender
        /// </summary>
        int AvatarClaimId { get; }

        /// <summary>
        /// IP address of sender
        /// </summary>
        string IpAddress { get; }
    }
}
