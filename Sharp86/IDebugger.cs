
namespace Sharp86
{
    public interface IDebugger
    {
        bool OnStep();
        bool OnSoftwareInterrupt(byte interruptNumber);
    } 
}
