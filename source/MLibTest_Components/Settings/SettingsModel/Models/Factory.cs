namespace SettingsModel.Models
{
    using SettingsModel.Interfaces;

    /// <summary>
    /// Factory class to create an <seealso cref="IEngine"/>
    /// object from a class that is otherwise unknown to the outside world.
    /// </summary>
    public static class Factory
    {
        /// <summary>
        /// Create a new engine object that provides all root functions required
        /// to model, track, persist, and load data at run-time.
        /// </summary>
        /// <returns></returns>
        public static IEngine CreateEngine()
        {
            return new OptionsEngine();
        }
    }
}
