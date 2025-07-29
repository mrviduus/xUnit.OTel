namespace xUnit.OTel.Core;

public class ContextValue<T>
{
    private readonly AsyncLocal<ValueHolder> _contextValue = new();

    public virtual T? Value
    {
        get
        {
            var value = _contextValue.Value;
            return value == null ? default : value.Value;
        }
        set
        {
            var holder = _contextValue.Value;
            if (holder != null)
            {
                // Clear current value trapped in the AsyncLocal, as its done.
                holder.Value = default;
            }

            if (value != null)
            {
                // Set the new value in the AsyncLocal.
                _contextValue.Value = new ValueHolder { Value = value };
            }
        }
    }
    private class ValueHolder
    {
        public T? Value { get; set; }
    }

}


