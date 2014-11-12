namespace OutlookTfs
{
    public interface IView
    {
        /// <summary>
        /// Gets or sets the data context.
        /// </summary>
        /// <value>The data context.</value>
        object DataContext { get; set; }

        /// <summary>
        /// Finds the name.
        /// </summary>
        /// <param name="controlName">Name of the control.</param>
        /// <returns></returns>
        object FindName(string controlName);

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        void Initialize();
    }
}
