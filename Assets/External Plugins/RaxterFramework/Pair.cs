
public struct Pair<T, U> 
{
    public T First;
    public U Second;
	
    public Pair(T first, U second) {
        this.First = first;
        this.Second = second;
    }

};

public class MutablePair<T, U> 
{
    public T First;
    public U Second;
	
    public MutablePair(T first, U second) {
        this.First = first;
        this.Second = second;
    }

};
