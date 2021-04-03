using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;

namespace Torch.Core.Commands
{
    /// <inheritdoc />
    public class CommandService : ICommandService
    {
        private const char PREFIX = '.';
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, Type> _transientCommands;
        private readonly Dictionary<string, ICommand> _singletonCommands;
        private readonly List<IProcessorStep> _processors;
        private NextProcessorDel _compositeProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandService"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider to use to instantiate commands.</param>
        public CommandService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _transientCommands = new Dictionary<string, Type>();
            _singletonCommands = new Dictionary<string, ICommand>();
            _processors = new List<IProcessorStep>();
            _compositeProcessor = ProcessCommand;
        }

        /// <inheritdoc/>
        public bool IsCommand(string input)
        {
            return !string.IsNullOrEmpty(input) && input[0] == PREFIX;
        }

        /// <inheritdoc/>
        public void Execute(object sender, string input, Action<string> responseHandler)
        {
            input = input.TrimStart(PREFIX);
            var args = ParseArgs(input, out var name);
            _compositeProcessor(new Context(sender, name, args, responseHandler));
        }

        /// <inheritdoc/>
        public bool RegisterTransient<T>(string commandName) where T : class, ICommand
        {
            if (!ValidateName(commandName))
                return false;

            _transientCommands[commandName] = typeof(T);
            return true;
        }

        /// <inheritdoc/>
        public bool RegisterSingleton<T>(string commandName, T instance) where T : class, ICommand
        {
            if (!ValidateName(commandName))
                return false;

            _singletonCommands[commandName] = instance;
            return true;
        }

        /// <inheritdoc/>
        public bool RegisterDelegate(string commandName, CommandExecuteDel execute, CommandCanExecuteDel canExecute = null)
        {
            if (!ValidateName(commandName))
                return false;

            _singletonCommands[commandName] = new DelegateCommand(execute, canExecute);
            return true;
        }

        /// <inheritdoc/>
        public void AddProcessorStep(IProcessorStep preprocessor)
        {
            _processors.Add(preprocessor);
            ComposeProcessor();
        }

        /// <inheritdoc/>
        public void AddProcessorStep(CommandProcessorDel processor)
        {
            _processors.Add(new DelegateProcessorStep(processor));
            ComposeProcessor();
        }

        private void ComposeProcessor()
        {
            NextProcessorDel processor = ProcessCommand;

            // Include the additional processors by starting with the final processor and building
            // an invocation chain in reverse.
            for (var i = _processors.Count - 1; i >= 0; i--)
            {
                var nextProcessor = processor;
                var preprocessor = _processors[i];
                processor = context => preprocessor.Process(context, nextProcessor);
            }

            _compositeProcessor = processor;
        }

        private string[] ParseArgs(string input, out string name)
        {
            var matches = Regex.Matches(input, "(\"[^\"]+\"|\\S+)");
            name = matches[0].ToString().Trim('"');
            var arr = new string[matches.Count - 1];
            for (var i = 1; i < matches.Count; i++)
                arr[i - 1] = matches[i].ToString().Trim('"');

            return arr;
        }

        private void ProcessCommand(ICommandContext context)
        {
            ICommand command;
            if (_transientCommands.TryGetValue(context.CommandName, out var type))
            {
                command = _serviceProvider != null ?
                    (ICommand)ActivatorUtilities.CreateInstance(_serviceProvider, type) :
                    (ICommand)Activator.CreateInstance(type);
            }
            else if (!_singletonCommands.TryGetValue(context.CommandName, out command))
            {
                context.Respond($"The command '{context.CommandName}' does not exist.");
                return;
            }

            var reason = "No reason given.";
            if (command.CanExecute(context, ref reason))
                command.Execute(context);
            else
                context.Respond($"The command cannot be executed: {reason}");
        }

        private bool ValidateName(string commandName)
        {
            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            if (!commandName.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_'))
                throw new ArgumentException("Command names may only contain alphanumeric characters, '-', and '_'.");

            return !_transientCommands.ContainsKey(commandName) && !_singletonCommands.ContainsKey(commandName);
        }

        private class Context : ICommandContext
        {
            private readonly Action<string> _responseHandler;

            public Context(object sender, string commandName, string[] args, Action<string> responseHandler)
            {
                Sender = sender;
                CommandName = commandName;
                Args = args;
                _responseHandler = responseHandler;
            }

            public object Sender { get; set; }

            public string CommandName { get; set; }

            public string[] Args { get; set; }

            public IDictionary<string, object> OtherData { get; } = new Dictionary<string, object>();

            public void Respond(string message)
            {
                _responseHandler?.Invoke(message);
            }
        }
    }
}