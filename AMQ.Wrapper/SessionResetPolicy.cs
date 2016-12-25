namespace AMQ.Wrapper
{
    public class SessionResetPolicy
    {
        /// <summary>
        /// if set to true, the session will be reset after every <see cref="ResetFrequencyHours"/>
        /// By default its set to false
        /// </summary>
        public bool ShouldResetSession { get; set; }

        /// <summary>
        /// the default value is 8 hours
        /// </summary>
        public int ResetFrequencyHours { get; set; }

        public SessionResetPolicy()
        {
            ResetFrequencyHours = 8;
            ShouldResetSession = false;
        }
    }
}