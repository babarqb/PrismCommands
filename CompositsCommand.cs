namespace Commands
{
  /// <summary>
  /// The CompositeCommand composes one or more ICommands.
  /// 
  /// </summary>
  public class CompositeCommand : ICommand
  {
    private readonly List<ICommand> _registeredCommands = new List<ICommand>();
    private readonly bool _monitorCommandActivity;
    private readonly EventHandler _onRegisteredCommandCanExecuteChangedHandler;

    /// <summary>
    /// Gets the list of all the registered commands.
    /// 
    /// </summary>
    /// 
    /// <value>
    /// A list of registered commands.
    /// </value>
    /// 
    /// <remarks>
    /// This returns a copy of the commands subscribed to the CompositeCommand.
    /// </remarks>
    public IList<ICommand> RegisteredCommands
    {
      get
      {
        List<ICommand> list = this._registeredCommands;
        bool lockTaken = false;
        try
        {
          Monitor.Enter((object) list, ref lockTaken);
          return (IList<ICommand>) Enumerable.ToList<ICommand>((IEnumerable<ICommand>) this._registeredCommands);
        }
        finally
        {
          if (lockTaken)
            Monitor.Exit((object) list);
        }
      }
    }

    /// <summary>
    /// Occurs when any of the registered commands raise <see cref="E:System.Windows.Input.ICommand.CanExecuteChanged"/>.
    /// 
    /// </summary>
    public virtual event EventHandler CanExecuteChanged;

    /// <summary>
    /// Initializes a new instance of <see cref="T:Prism.Commands.CompositeCommand"/>.
    /// 
    /// </summary>
    public CompositeCommand()
    {
      this._onRegisteredCommandCanExecuteChangedHandler = new EventHandler(this.OnRegisteredCommandCanExecuteChanged);
    }

    /// <summary>
    /// Initializes a new instance of <see cref="T:Prism.Commands.CompositeCommand"/>.
    /// 
    /// </summary>
    /// <param name="monitorCommandActivity">Indicates when the command activity is going to be monitored.</param>
    public CompositeCommand(bool monitorCommandActivity)
      : this()
    {
      this._monitorCommandActivity = monitorCommandActivity;
    }

    /// <summary>
    /// Adds a command to the collection and signs up for the <see cref="E:System.Windows.Input.ICommand.CanExecuteChanged"/> event of it.
    /// 
    /// </summary>
    /// 
    /// <remarks>
    /// If this command is set to monitor command activity, and <paramref name="command"/>
    ///             implements the <see cref="!:IActiveAwareCommand"/> interface, this method will subscribe to its
    ///             <see cref="!:IActiveAwareCommand.IsActiveChanged"/> event.
    /// 
    /// </remarks>
    /// <param name="command">The command to register.</param>
    public virtual void RegisterCommand(ICommand command)
    {
      if (command == null)
        throw new ArgumentNullException("command");
      if (command == this)
        throw new ArgumentException(Resources.CannotRegisterCompositeCommandInItself);
      List<ICommand> list = this._registeredCommands;
      bool lockTaken = false;
      try
      {
        Monitor.Enter((object) list, ref lockTaken);
        if (this._registeredCommands.Contains(command))
          throw new InvalidOperationException(Resources.CannotRegisterSameCommandTwice);
        this._registeredCommands.Add(command);
      }
      finally
      {
        if (lockTaken)
          Monitor.Exit((object) list);
      }
      command.CanExecuteChanged += this._onRegisteredCommandCanExecuteChangedHandler;
      this.OnCanExecuteChanged();
      if (!this._monitorCommandActivity)
        return;
      IActiveAware activeAware = command as IActiveAware;
      if (activeAware == null)
        return;
      activeAware.IsActiveChanged += new EventHandler(this.Command_IsActiveChanged);
    }

    /// <summary>
    /// Removes a command from the collection and removes itself from the <see cref="E:System.Windows.Input.ICommand.CanExecuteChanged"/> event of it.
    /// 
    /// </summary>
    /// <param name="command">The command to unregister.</param>
    public virtual void UnregisterCommand(ICommand command)
    {
      if (command == null)
        throw new ArgumentNullException("command");
      List<ICommand> list = this._registeredCommands;
      bool lockTaken = false;
      bool flag;
      try
      {
        Monitor.Enter((object) list, ref lockTaken);
        flag = this._registeredCommands.Remove(command);
      }
      finally
      {
        if (lockTaken)
          Monitor.Exit((object) list);
      }
      if (!flag)
        return;
      command.CanExecuteChanged -= this._onRegisteredCommandCanExecuteChangedHandler;
      this.OnCanExecuteChanged();
      if (!this._monitorCommandActivity)
        return;
      IActiveAware activeAware = command as IActiveAware;
      if (activeAware == null)
        return;
      activeAware.IsActiveChanged -= new EventHandler(this.Command_IsActiveChanged);
    }

    private void OnRegisteredCommandCanExecuteChanged(object sender, EventArgs e)
    {
      this.OnCanExecuteChanged();
    }

    /// <summary>
    /// Forwards <see cref="M:System.Windows.Input.ICommand.CanExecute(System.Object)"/> to the registered commands and returns
    ///             <see langword="true"/> if all of the commands return <see langword="true"/>.
    /// 
    /// </summary>
    /// <param name="parameter">Data used by the command.
    ///             If the command does not require data to be passed, this object can be set to <see langword="null"/>.
    ///             </param>
    /// <returns>
    /// <see langword="true"/> if all of the commands return <see langword="true"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public virtual bool CanExecute(object parameter)
    {
      bool flag = false;
      List<ICommand> list = this._registeredCommands;
      bool lockTaken = false;
      ICommand[] commandArray;
      try
      {
        Monitor.Enter((object) list, ref lockTaken);
        commandArray = this._registeredCommands.ToArray();
      }
      finally
      {
        if (lockTaken)
          Monitor.Exit((object) list);
      }
      foreach (ICommand command in commandArray)
      {
        if (this.ShouldExecute(command))
        {
          if (!command.CanExecute(parameter))
            return false;
          flag = true;
        }
      }
      return flag;
    }

    /// <summary>
    /// Forwards <see cref="M:System.Windows.Input.ICommand.Execute(System.Object)"/> to the registered commands.
    /// 
    /// </summary>
    /// <param name="parameter">Data used by the command.
    ///             If the command does not require data to be passed, this object can be set to <see langword="null"/>.
    ///             </param>
    public virtual void Execute(object parameter)
    {
      List<ICommand> list = this._registeredCommands;
      bool lockTaken = false;
      Queue<ICommand> queue;
      try
      {
        Monitor.Enter((object) list, ref lockTaken);
        queue = new Queue<ICommand>((IEnumerable<ICommand>) Enumerable.ToList<ICommand>(Enumerable.Where<ICommand>((IEnumerable<ICommand>) this._registeredCommands, new Func<ICommand, bool>(this.ShouldExecute))));
      }
      finally
      {
        if (lockTaken)
          Monitor.Exit((object) list);
      }
      while (queue.Count > 0)
        queue.Dequeue().Execute(parameter);
    }

    /// <summary>
    /// Evaluates if a command should execute.
    /// 
    /// </summary>
    /// <param name="command">The command to evaluate.</param>
    /// <returns>
    /// A <see cref="T:System.Boolean"/> value indicating whether the command should be used
    ///             when evaluating <see cref="M:Prism.Commands.CompositeCommand.CanExecute(System.Object)"/> and <see cref="M:Prism.Commands.CompositeCommand.Execute(System.Object)"/>.
    /// </returns>
    /// 
    /// <remarks>
    /// If this command is set to monitor command activity, and <paramref name="command"/>
    ///             implements the <see cref="!:IActiveAwareCommand"/> interface,
    ///             this method will return <see langword="false"/> if the command's <see cref="!:IActiveAwareCommand.IsActive"/>
    ///             property is <see langword="false"/>; otherwise it always returns <see langword="true"/>.
    /// </remarks>
    protected virtual bool ShouldExecute(ICommand command)
    {
      IActiveAware activeAware = command as IActiveAware;
      if (this._monitorCommandActivity && activeAware != null)
        return activeAware.IsActive;
      return true;
    }

    /// <summary>
    /// Raises <see cref="E:System.Windows.Input.ICommand.CanExecuteChanged"/> on the UI thread so every
    ///             command invoker can requery <see cref="M:System.Windows.Input.ICommand.CanExecute(System.Object)"/> to check if the
    ///             <see cref="T:Prism.Commands.CompositeCommand"/> can execute.
    /// 
    /// </summary>
    protected virtual void OnCanExecuteChanged()
    {
      // ISSUE: reference to a compiler-generated field
      EventHandler eventHandler = this.CanExecuteChanged;
      if (eventHandler == null)
        return;
      eventHandler((object) this, EventArgs.Empty);
    }

    /// <summary>
    /// Handler for IsActiveChanged events of registered commands.
    /// 
    /// </summary>
    /// <param name="sender">The sender.</param><param name="e">EventArgs to pass to the event.</param>
    private void Command_IsActiveChanged(object sender, EventArgs e)
    {
      this.OnCanExecuteChanged();
    }
  }
}
