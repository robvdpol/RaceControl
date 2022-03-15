namespace RaceControl.Common.Generators;

public interface INumberGenerator
{
    long GetNextNumber();

    void RemoveNumber(long number);
}