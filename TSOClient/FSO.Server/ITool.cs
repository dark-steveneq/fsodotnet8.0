namespace FSO.Server
{
    /// <summary>
    /// Interface used by server tools
    /// </summary>
    interface ITool
    {
        /// <summary>
        /// Run tool
        /// </summary>
        /// <returns>Non-zero error code</returns>
        int Run();
    }
}
