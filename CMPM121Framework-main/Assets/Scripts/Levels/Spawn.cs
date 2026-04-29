using System.Collections.Generic;

public class Spawn
{
    public string enemy;
    public string count;	//  Always an RPN Value
    public string hp;		//	Change these three values to public string <value_name> = "base";
                            //  I'm not doing it right now because I don't want to add too much or break anything
	public string damage;	//  RPN Value or BASE
	public string speed;	//  RPN Value or BASE
    public int delay = 2; // default value
    public List<int> sequence = null;
    public string location;
}
