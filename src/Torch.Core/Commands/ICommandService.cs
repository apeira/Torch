using System;

namespace Torch.Core.Commands
{
    public interface ICommandService
    {
        /// <summary>
        /// Returns whether a string starts with the command prefix string.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>True if the string starts with the command prefix string, otherwise false.</returns>
        public bool IsCommand(string input);

        /// <summary>
        /// Executes the command pipeline with the provided sender and input.
        /// </summary>
        /// <param name="sender">The source of the command.</param>
        /// <param name="input">The command string to execute.</param>
        /// <param name="responseHandler">A delegate that handles responses from the command.</param>
        public void Execute(object sender, string input, Action<string> responseHandler);

        /// <summary>
        /// Register a command that is instantiated every time it is executed.
        /// </summary>
        /// <param name="commandName">The name to associate with the command.</param>
        /// <typeparam name="T">The command type.</typeparam>
        /// <returns>True if the command was registered, false if the name is already in use.</returns>
        public bool RegisterTransient<T>(string commandName) where T : class, ICommand;

        /// <summary>
        /// Register a command that reuses the same instance each execution.
        /// </summary>
        /// <param name="commandName">The name to associate with the command.</param>
        /// <param name="instance">The command instance.</param>
        /// <typeparam name="T">The command type.</typeparam>
        /// <returns>True if the command was registered, false if the name is already in use.</returns>
        public bool RegisterSingleton<T>(string commandName, T instance) where T : class, ICommand;

        /// <summary>
        /// Register a command that is handled by the provided delegates.
        /// </summary>
        /// <param name="commandName">The name to associate with the command.</param>
        /// <param name="execute">The delegate that handles executing the command.</param>
        /// <param name="canExecute">The delegate that checks if the command can be executed.</param>
        /// <returns>True if the command was registered, false if the name is already in use.</returns>
        public bool RegisterDelegate(string commandName, CommandExecuteDel execute, CommandCanExecuteDel? canExecute = null);

        /// <summary>
        /// Appends a preprocessor to the end of the command handler pipeline.
        /// </summary>
        /// <param name="preprocessor">The preprocessor instance to append.</param>
        public void AddProcessorStep(IProcessorStep preprocessor);

        /// <summary>
        /// Appends a preprocessor to the end of the command handler pipeline.
        /// </summary>
        /// <param name="processor">The preprocessor to append.</param>
        public void AddProcessorStep(CommandProcessorDel processor);
    }
}
