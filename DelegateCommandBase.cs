namespace Commands
{
  /// <summary>
  /// An <see cref="T:System.Windows.Input.ICommand"/> whose delegates can be attached for <see cref="M:Prism.Commands.DelegateCommandBase.Execute(System.Object)"/> and <see cref="M:Prism.Commands.DelegateCommandBase.CanExecute(System.Object)"/>.
  /// 
  /// </summary>
  public abstract class DelegateCommandBase : ICommand, IActiveAware
  {
    private readonly HashSet<string> _propertiesToObserve = new HashSet<string>();
    private bool _isActive;
    private INotifyPropertyChanged _inpc;
    protected readonly Func<object, Task> _executeMethod;
    protected Func<object, bool> _canExecuteMethod;

    /// <summary>
    /// Gets or sets a value indicating whether the object is active.
    /// 
    /// </summary>
    /// 
    /// <value>
    /// <see langword="true"/> if the object is active; otherwise <see langword="false"/>.
    /// </value>
    public bool IsActive
    {
      get
      {
        return this._isActive;
      }
      set
      {
        if (this._isActive == value)
          return;
        this._isActive = value;
        this.OnIsActiveChanged();
      }
    }

    /// <summary>
    /// Occurs when changes occur that affect whether or not the command should execute.
    /// 
    /// </summary>
    public virtual event EventHandler CanExecuteChanged;

    /// <summary>
    /// Fired if the <see cref="P:Prism.Commands.DelegateCommandBase.IsActive"/> property changes.
    /// 
    /// </summary>
    public virtual event EventHandler IsActiveChanged;

    /// <summary>
    /// Creates a new instance of a <see cref="T:Prism.Commands.DelegateCommandBase"/>, specifying both the execute action and the can execute function.
    /// 
    /// </summary>
    /// <param name="executeMethod">The <see cref="T:System.Action"/> to execute when <see cref="M:System.Windows.Input.ICommand.Execute(System.Object)"/> is invoked.</param><param name="canExecuteMethod">The <see cref="T:System.Func`2"/> to invoked when <see cref="M:System.Windows.Input.ICommand.CanExecute(System.Object)"/> is invoked.</param>
    protected DelegateCommandBase(Action<object> executeMethod, Func<object, bool> canExecuteMethod)
    {
      if (executeMethod == null || canExecuteMethod == null)
        throw new ArgumentNullException("executeMethod", Resources.DelegateCommandDelegatesCannotBeNull);
      this._executeMethod = (Func<object, Task>) (arg =>
      {
        executeMethod(arg);
        return Task.Delay(0);
      });
      this._canExecuteMethod = canExecuteMethod;
    }

    /// <summary>
    /// Creates a new instance of a <see cref="T:Prism.Commands.DelegateCommandBase"/>, specifying both the Execute action as an awaitable Task and the CanExecute function.
    /// 
    /// </summary>
    /// <param name="executeMethod">The <see cref="T:System.Func`2"/> to execute when <see cref="M:System.Windows.Input.ICommand.Execute(System.Object)"/> is invoked.</param><param name="canExecuteMethod">The <see cref="T:System.Func`2"/> to invoked when <see cref="M:System.Windows.Input.ICommand.CanExecute(System.Object)"/> is invoked.</param>
    protected DelegateCommandBase(Func<object, Task> executeMethod, Func<object, bool> canExecuteMethod)
    {
      if (executeMethod == null || canExecuteMethod == null)
        throw new ArgumentNullException("executeMethod", Resources.DelegateCommandDelegatesCannotBeNull);
      this._executeMethod = executeMethod;
      this._canExecuteMethod = canExecuteMethod;
    }

    /// <summary>
    /// Raises <see cref="E:System.Windows.Input.ICommand.CanExecuteChanged"/> on the UI thread so every
    ///             command invoker can requery <see cref="M:System.Windows.Input.ICommand.CanExecute(System.Object)"/>.
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
    /// Raises <see cref="E:Prism.Commands.DelegateCommandBase.CanExecuteChanged"/> on the UI thread so every command invoker
    ///             can requery to check if the command can execute.
    /// 
    /// <remarks>
    /// Note that this will trigger the execution of <see cref="M:Prism.Commands.DelegateCommandBase.CanExecute(System.Object)"/> once for each invoker.
    /// </remarks>
    /// 
    /// </summary>
    public void RaiseCanExecuteChanged()
    {
      this.OnCanExecuteChanged();
    }

    async void ICommand.Execute(object parameter)
    {
      await this.Execute(parameter);
    }

    bool ICommand.CanExecute(object parameter)
    {
      return this.CanExecute(parameter);
    }

    /// <summary>
    /// Executes the command with the provided parameter by invoking the <see cref="T:System.Action`1"/> supplied during construction.
    /// 
    /// </summary>
    /// <param name="parameter"/>
    protected async Task Execute(object parameter)
    {
      await this._executeMethod(parameter);
    }

    /// <summary>
    /// Determines if the command can execute with the provided parameter by invoking the <see cref="T:System.Func`2"/> supplied during construction.
    /// 
    /// </summary>
    /// <param name="parameter">The parameter to use when determining if this command can execute.</param>
    /// <returns>
    /// Returns <see langword="true"/> if the command can execute.  <see langword="False"/> otherwise.
    /// </returns>
    protected bool CanExecute(object parameter)
    {
      return this._canExecuteMethod(parameter);
    }

    /// <summary>
    /// Observes a property that implements INotifyPropertyChanged, and automatically calls DelegateCommandBase.RaiseCanExecuteChanged on property changed notifications.
    /// 
    /// </summary>
    /// <typeparam name="T">The object type containing the property specified in the expression.</typeparam><param name="propertyExpression">The property expression. Example: ObservesProperty(() =&gt; PropertyName).</param>
    /// <returns>
    /// The current instance of DelegateCommand
    /// </returns>
    protected internal void ObservesPropertyInternal<T>(Expression<Func<T>> propertyExpression)
    {
      this.AddPropertyToObserve(PropertySupport.ExtractPropertyName<T>(propertyExpression));
      this.HookInpc(propertyExpression.Body as MemberExpression);
    }

    /// <summary>
    /// Observes a property that is used to determine if this command can execute, and if it implements INotifyPropertyChanged it will automatically call DelegateCommandBase.RaiseCanExecuteChanged on property changed notifications.
    /// 
    /// </summary>
    /// <param name="canExecuteExpression">The property expression. Example: ObservesCanExecute((o) =&gt; PropertyName).</param>
    /// <returns>
    /// The current instance of DelegateCommand
    /// </returns>
    protected internal void ObservesCanExecuteInternal(Expression<Func<object, bool>> canExecuteExpression)
    {
      this._canExecuteMethod = canExecuteExpression.Compile();
      this.AddPropertyToObserve(PropertySupport.ExtractPropertyNameFromLambda((LambdaExpression) canExecuteExpression));
      this.HookInpc(canExecuteExpression.Body as MemberExpression);
    }

    protected void HookInpc(MemberExpression expression)
    {
      if (expression == null || this._inpc != null)
        return;
      ConstantExpression constantExpression = expression.Expression as ConstantExpression;
      if (constantExpression == null)
        return;
      this._inpc = constantExpression.Value as INotifyPropertyChanged;
      if (this._inpc == null)
        return;
      this._inpc.PropertyChanged += new PropertyChangedEventHandler(this.Inpc_PropertyChanged);
    }

    protected void AddPropertyToObserve(string property)
    {
      if (this._propertiesToObserve.Contains(property))
        throw new ArgumentException(string.Format("{0} is already being observed.", (object) property));
      this._propertiesToObserve.Add(property);
    }

    private void Inpc_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!this._propertiesToObserve.Contains(e.PropertyName))
        return;
      this.RaiseCanExecuteChanged();
    }

    /// <summary>
    /// This raises the <see cref="E:Prism.Commands.DelegateCommandBase.IsActiveChanged"/> event.
    /// 
    /// </summary>
    protected virtual void OnIsActiveChanged()
    {
      // ISSUE: reference to a compiler-generated field
      EventHandler eventHandler = this.IsActiveChanged;
      if (eventHandler == null)
        return;
      eventHandler((object) this, EventArgs.Empty);
    }
  }
}
