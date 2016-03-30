namespace Commands
{
  /// <summary>
  /// An <see cref="T:System.Windows.Input.ICommand"/> whose delegates do not take any parameters for <see cref="M:Prism.Commands.DelegateCommand.Execute"/> and <see cref="M:Prism.Commands.DelegateCommand.CanExecute"/>.
  /// 
  /// </summary>
  /// <see cref="T:Prism.Commands.DelegateCommandBase"/><see cref="T:Prism.Commands.DelegateCommand`1"/>
  public class DelegateCommand : DelegateCommandBase
  {
    /// <summary>
    /// Creates a new instance of <see cref="T:Prism.Commands.DelegateCommand"/> with the <see cref="T:System.Action"/> to invoke on execution.
    /// 
    /// </summary>
    /// <param name="executeMethod">The <see cref="T:System.Action"/> to invoke when <see cref="M:System.Windows.Input.ICommand.Execute(System.Object)"/> is called.</param>
    public DelegateCommand(Action executeMethod)
      : this(executeMethod, (Func<bool>) (() => true))
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="T:Prism.Commands.DelegateCommand"/> with the <see cref="T:System.Action"/> to invoke on execution
    ///             and a <see langword="Func"/> to query for determining if the command can execute.
    /// 
    /// </summary>
    /// <param name="executeMethod">The <see cref="T:System.Action"/> to invoke when <see cref="M:System.Windows.Input.ICommand.Execute(System.Object)"/> is called.</param><param name="canExecuteMethod">The <see cref="T:System.Func`1"/> to invoke when <see cref="M:System.Windows.Input.ICommand.CanExecute(System.Object)"/> is called</param>
    public DelegateCommand(Action executeMethod, Func<bool> canExecuteMethod)
      : base((Action<object>) (o => executeMethod()), (Func<object, bool>) (o => canExecuteMethod()))
    {
      if (executeMethod == null || canExecuteMethod == null)
        throw new ArgumentNullException("executeMethod", Resources.DelegateCommandDelegatesCannotBeNull);
    }

    private DelegateCommand(Func<Task> executeMethod)
      : this(executeMethod, (Func<bool>) (() => true))
    {
    }

    private DelegateCommand(Func<Task> executeMethod, Func<bool> canExecuteMethod)
      : base((Func<object, Task>) (o => executeMethod()), (Func<object, bool>) (o => canExecuteMethod()))
    {
      if (executeMethod == null || canExecuteMethod == null)
        throw new ArgumentNullException("executeMethod", Resources.DelegateCommandDelegatesCannotBeNull);
    }

    /// <summary>
    /// Observes a property that implements INotifyPropertyChanged, and automatically calls DelegateCommandBase.RaiseCanExecuteChanged on property changed notifications.
    /// 
    /// </summary>
    /// <typeparam name="T">The object type containing the property specified in the expression.</typeparam><param name="propertyExpression">The property expression. Example: ObservesProperty(() =&gt; PropertyName).</param>
    /// <returns>
    /// The current instance of DelegateCommand
    /// </returns>
    public DelegateCommand ObservesProperty<T>(Expression<Func<T>> propertyExpression)
    {
      this.ObservesPropertyInternal<T>(propertyExpression);
      return this;
    }

    /// <summary>
    /// Observes a property that is used to determine if this command can execute, and if it implements INotifyPropertyChanged it will automatically call DelegateCommandBase.RaiseCanExecuteChanged on property changed notifications.
    /// 
    /// </summary>
    /// <param name="canExecuteExpression">The property expression. Example: ObservesCanExecute((o) =&gt; PropertyName).</param>
    /// <returns>
    /// The current instance of DelegateCommand
    /// </returns>
    public DelegateCommand ObservesCanExecute(Expression<Func<object, bool>> canExecuteExpression)
    {
      this.ObservesCanExecuteInternal(canExecuteExpression);
      return this;
    }

    /// <summary>
    /// Factory method to create a new instance of <see cref="T:Prism.Commands.DelegateCommand"/> from an awaitable handler method.
    /// 
    /// </summary>
    /// <param name="executeMethod">Delegate to execute when Execute is called on the command.</param>
    /// <returns>
    /// Constructed instance of <see cref="T:Prism.Commands.DelegateCommand"/>
    /// </returns>
    public static DelegateCommand FromAsyncHandler(Func<Task> executeMethod)
    {
      return new DelegateCommand(executeMethod);
    }

    /// <summary>
    /// Factory method to create a new instance of <see cref="T:Prism.Commands.DelegateCommand"/> from an awaitable handler method.
    /// 
    /// </summary>
    /// <param name="executeMethod">Delegate to execute when Execute is called on the command. This can be null to just hook up a CanExecute delegate.</param><param name="canExecuteMethod">Delegate to execute when CanExecute is called on the command. This can be null.</param>
    /// <returns>
    /// Constructed instance of <see cref="T:Prism.Commands.DelegateCommand"/>
    /// </returns>
    public static DelegateCommand FromAsyncHandler(Func<Task> executeMethod, Func<bool> canExecuteMethod)
    {
      return new DelegateCommand(executeMethod, canExecuteMethod);
    }

    /// <summary>
    /// Executes the command.
    /// 
    /// </summary>
    public virtual async Task Execute()
    {
      await this.Execute((object) null);
    }

    /// <summary>
    /// Determines if the command can be executed.
    /// 
    /// </summary>
    /// 
    /// <returns>
    /// Returns <see langword="true"/> if the command can execute, otherwise returns <see langword="false"/>.
    /// </returns>
    public virtual bool CanExecute()
    {
      return this.CanExecute((object) null);
    }
  }
}
