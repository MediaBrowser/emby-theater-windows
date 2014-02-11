namespace MediaBrowser.Theater.Api.Theming.Navigation
{
    #region Interfaces

    /// <summary>
    ///     The <see cref="INavigationPath" /> interface defines a location given to the theme when
    ///     instructing the theme to display content relavent to the given location.
    /// </summary>
    public interface INavigationPath { }

    /// <summary>
    ///     The <see cref="INavigationPath{TParent}" /> interface is a <see cref="INavigationPath" /> with
    ///     a parent location.
    /// </summary>
    /// <typeparam name="TParent">The type of parent path.</typeparam>
    public interface INavigationPath<out TParent>
        : INavigationPath where TParent : INavigationPath
    {
        /// <summary>
        ///     Gets the parent path.
        /// </summary>
        TParent Parent { get; }
    }

    /// <summary>
    ///     The <see cref="INavigationPathArg{TParameter}" /> interface is a <see cref="INavigationPath" />
    ///     wich can pass an argument to the theme during navigation.
    /// </summary>
    /// <typeparam name="TParameter">The type of argument to pass.</typeparam>
    public interface INavigationPathArg<out TParameter>
        : INavigationPath
    {
        /// <summary>
        ///     Gets the argument passed to the theme during navigation.
        /// </summary>
        TParameter Parameter { get; }
    }

    /// <summary>
    ///     The <see cref="INavigationPathArg{TParent, TParameter}" /> interface is a <see cref="INavigationPath" />
    ///     which has a parent location and can pass an argument to the theme during navigation.
    /// </summary>
    /// <typeparam name="TParent">The type of parent path.</typeparam>
    /// <typeparam name="TParameter">The type of argument to pass.</typeparam>
    public interface INavigationPathArg<out TParent, out TParameter>
        : INavigationPath<TParent> where TParent : INavigationPath
    {
        /// <summary>
        ///     Gets the argument passed to the theme during navigation.
        /// </summary>
        TParameter Parameter { get; }
    }

    #endregion

    #region Path Implementations

    /// <summary>
    ///     An abstract implementation of <see cref="INavigationPath" />.
    /// </summary>
    public abstract class NavigationPath
        : INavigationPath
    {
        public virtual string Name
        {
            get
            {
                string typeName = GetType().Name;
                if (typeName.EndsWith("Path"))
                {
                    typeName = typeName.Substring(0, typeName.Length - 4);
                }

                return typeName;
            }
        }

        public override string ToString()
        {
            return string.Format("/{0}", Name);
        }
    }

    /// <summary>
    ///     An abstract implementation of <see cref="INavigationPath{TParent}" />.
    /// </summary>
    /// <typeparam name="TParent">The type of parent path.</typeparam>
    public abstract class NavigationPath<TParent>
        : NavigationPath, INavigationPath<TParent> where TParent : INavigationPath
    {
        public TParent Parent { get; set; }

        public override string ToString()
        {
            return string.Format("{0}/{1}", Parent, Name);
        }
    }

    /// <summary>
    ///     An abstract implementation of <see cref="INavigationPathArg{TParent, TParameter}" />.
    /// </summary>
    /// <typeparam name="TParent">The type of parent path.</typeparam>
    /// <typeparam name="TParameter">The type of argument to pass.</typeparam>
    public abstract class NavigationPathArg<TParent, TParameter>
        : NavigationPath<TParent>, INavigationPathArg<TParent, TParameter> where TParent : INavigationPath
    {
        public TParameter Parameter { get; set; }

        public override string ToString()
        {
            return string.Format("{0}/{1}/{2}", Parent, Name, Parameter);
        }
    }

    /// <summary>
    ///     An abstract implementation of <see cref="INavigationPathArg{TParameter}" />.
    /// </summary>
    /// <typeparam name="TParameter">The type of argument to pass.</typeparam>
    public abstract class NavigationPathArg<TParameter>
        : NavigationPath, INavigationPathArg<TParameter>
    {
        public TParameter Parameter { get; set; }

        public override string ToString()
        {
            return string.Format("/{0}/{1}", Name, Parameter);
        }
    }

    #endregion
}