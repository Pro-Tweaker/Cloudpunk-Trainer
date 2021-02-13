namespace Cloudpunk_Trainer.Models
{
    public abstract class Hack
    {
        abstract public bool Enabled { get; protected set; }
        abstract public long Value { get; }
        abstract public bool Enable();
        abstract public bool Disable();
    }
}
