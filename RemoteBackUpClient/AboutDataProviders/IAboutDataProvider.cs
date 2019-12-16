namespace RemoteBackUpClient.AboutDataProviders
{
    public interface IAboutDataProvider
    {
        /// <summary>
        /// Gets the title property, which is display in the About dialogs window title.
        /// </summary>
        string Title
        {
            get;
        }

        /// <summary>
        /// Gets the application's version information to show.
        /// </summary>
        string Version
        {
            get;
        }

        /// <summary>
        /// Gets the description about the application.
        /// </summary>
        string Description
        {
            get;
        }

        /// <summary>
        ///  Gets the product's full name.
        /// </summary>
        string Product
        {
            get;
        }

        /// <summary>
        /// Gets the copyright information for the product.
        /// </summary>
        string Copyright
        {
            get;
        }

        /// <summary>
        /// Gets the product's company name.
        /// </summary>
        string Company
        {
            get;
        }

        /// <summary>
        /// Gets the link text to display in the About dialog.
        /// </summary>
        string LinkText
        {
            get;
        }

        /// <summary>
        /// Gets the link uri that is the navigation target of the link.
        /// </summary>
        string LinkUri
        {
            get;
        }
    }
}