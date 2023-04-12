namespace GOAP_Nez_Deprecated
{

    /// <summary>
    /// A subclass of action that takes in a context T. Useful when the action requires validation so that it has a way to get data that it needs
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GOAPAction<T> : GOAPAction
    {
        protected T _context;

        public GOAPAction(T context, string name) : base(name)
        {
            _context = context;
            Name = name;
        }

        public GOAPAction(T context, string name, int cost) : this(context, name)
        {
            Cost = cost;
        }

        public virtual void Execute() { }
    }
}