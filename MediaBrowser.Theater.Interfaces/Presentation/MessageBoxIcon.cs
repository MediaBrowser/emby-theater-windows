
namespace MediaBrowser.Theater.Interfaces.Theming
{
    /// <summary>
    /// I had to make my own enum that essentially clones MessageBoxImage
    /// Some of the options share the same enum int value, and this was preventing databinding from working properly.
    /// </summary>
    public enum MessageBoxIcon
    {
        // Summary:
        //     No icon is displayed.
        None,
        //
        // Summary:
        //     The message box contains a symbol consisting of white X in a circle with
        //     a red background.
        Error,
        //
        // Summary:
        //     The message box contains a symbol consisting of a white X in a circle with
        //     a red background.
        Hand,
        //
        // Summary:
        //     The message box contains a symbol consisting of white X in a circle with
        //     a red background.
        Stop,
        //
        // Summary:
        //     The message box contains a symbol consisting of a question mark in a circle.
        Question,
        //
        // Summary:
        //     The message box contains a symbol consisting of an exclamation point in a
        //     triangle with a yellow background.
        Exclamation,
        //
        // Summary:
        //     The message box contains a symbol consisting of an exclamation point in a
        //     triangle with a yellow background.
        Warning,
        //
        // Summary:
        //     The message box contains a symbol consisting of a lowercase letter i in a
        //     circle.
        Information,
        //
        // Summary:
        //     The message box contains a symbol consisting of a lowercase letter i in a
        //     circle.
        Asterisk
    }
}
