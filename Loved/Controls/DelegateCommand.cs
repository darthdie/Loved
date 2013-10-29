using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Loved {
    public class DelegateCommand : IDelegateCommand {
        private readonly Action<object> _command;
        private readonly Func<object, bool> _canExecute;
        private bool _autoCanExecuteRequery;
        private EventHandler _internalCanExecuteChanged;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="command">Function mapped to ICommand.Execute</param>
        public DelegateCommand(Action<object> command)
            : this(command, null, false) {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="command">Function mapped to ICommand.Execute</param>
        public DelegateCommand(Action command)
            : this(command, null, false) {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="command">Function mapped to ICommand.Execute</param>
        /// <param name="test">Function mapped to ICommand.CanExecute</param>
        public DelegateCommand(Action command, Func<bool> test)
            : this(command, test, true) {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="command">Function mapped to ICommand.Execute</param>
        /// <param name="test">Function mapped to ICommand.CanExecute</param>
        public DelegateCommand(Action<object> command, Func<object, bool> test)
            : this(command, test, true) {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="command">Function mapped to ICommand.Execute</param>
        /// <param name="test">Function mapped to ICommand.CanExecute</param>
        /// <param name="autoCanExecuteRequery">True to use WPF CommandManager for CanExecute re-query operations</param>
        public DelegateCommand(Action command, Func<bool> test, bool autoCanExecuteRequery) {
            Debug.Assert(command != null);
            _command = delegate { command(); };
            if (test != null) {
                _canExecute = delegate { return test(); };
                _autoCanExecuteRequery = autoCanExecuteRequery;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="command">Function mapped to ICommand.Execute</param>
        /// <param name="test">Function mapped to ICommand.CanExecute</param>
        /// <param name="autoCanExecuteRequery">True to use WPF command manager for CanExecute handler</param>
        public DelegateCommand(Action<object> command, Func<object, bool> test, bool autoCanExecuteRequery) {
            Debug.Assert(command != null);
            _command = command;
            _canExecute = test;
            _autoCanExecuteRequery = (test != null && autoCanExecuteRequery);
        }

        /// <summary>
        /// Enable or Disable the automatic CanExecute re-query support using the 
        /// WPF CommandManager.
        /// </summary>
        public bool AutoCanExecuteRequery {
            get { return _autoCanExecuteRequery; }
            set {
                if (_autoCanExecuteRequery != value) {
                    _autoCanExecuteRequery = value;
                    EventHandler eCanExecuteChanged = _internalCanExecuteChanged;
                    if (eCanExecuteChanged != null) {
                        if (_autoCanExecuteRequery) {
                            foreach (EventHandler handler in eCanExecuteChanged.GetInvocationList()) {
                                CommandManager.RequerySuggested += handler;
                            }
                        }
                        else {
                            foreach (EventHandler handler in eCanExecuteChanged.GetInvocationList()) {
                                CommandManager.RequerySuggested -= handler;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method can be used to raise the CanExecuteChanged handler.
        /// This will force WPF to re-query the status of this command directly.
        /// This is not necessary if you use the AutoCanExecuteRequery feature.
        /// </summary>
        public void RaiseCanExecuteChanged() {
            if (_canExecute != null)
                OnCanExecuteChanged();
        }

        /// <summary>
        /// This method is used to walk the delegate chain and well WPF that
        /// our command execution status has changed.
        /// </summary>
        protected virtual void OnCanExecuteChanged() {
            EventHandler eCanExecuteChanged = _internalCanExecuteChanged;
            if (eCanExecuteChanged != null)
                eCanExecuteChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event that is raised when the current state for our command has changed.
        /// Note that this is an instance event - unlike the CommandManager.RequerySuggested event
        /// and as such we don't need to manage weak references here.
        /// </summary>
        public event EventHandler CanExecuteChanged {
            add {
                _internalCanExecuteChanged += value;
                if (_autoCanExecuteRequery) {
                    CommandManager.RequerySuggested += value;
                }
            }
            remove {
                _internalCanExecuteChanged -= value;
                if (_autoCanExecuteRequery) {
                    CommandManager.RequerySuggested -= value;
                }
            }
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <returns>
        /// true if this command can be executed; otherwise, false.
        /// </returns>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        public bool CanExecute(object parameter) {
            return (_canExecute == null) || _canExecute(parameter);
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        public void Execute(object parameter) {
            _command(parameter);
        }
    }

    public interface IDelegateCommand : ICommand {
        /// <summary>
        /// This method can be used to raise the CanExecuteChanged handler.
        /// This will force WPF to re-query the status of this command directly.
        /// This is not necessary if you use the AutoCanExecuteRequery feature.
        /// </summary>
        void RaiseCanExecuteChanged();
    }
}
