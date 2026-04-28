using System.Collections.Generic;

public class Spawn
{
    public string enemy;
    public string count;	//  Always an RPN Value
    public string hp;		//	RPN Value or BASE
	public string damage;	//  RPN Value or BASE
	public string speed;	//  RPN Value or BASE
    public int delay;
    public List<int> sequence;
    public string location;
}
