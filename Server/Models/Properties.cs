using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Properties
{
    bool isPlayer = true;

    private float test;

    public float testMax;
    public float Test
    {
        get { return test; }
        set
        {
            test = isPlayer ? MakeValueSafe(value, testMax) : value;
        }
    }

    private Properties() { }
    public Properties(Properties temp)
    {
        isPlayer = temp.isPlayer;
        testMax = temp.testMax;
        Test = temp.Test;
    }
    public Properties(float _test, float _testMax)
    {
        isPlayer = true;
        BasePropertiesSet(_test);
        testMax = _testMax;
    }

    public Properties(float _test)
    {
        BasePropertiesSet(_test);
    }

    void BasePropertiesSet(float _test)
    {
        test = _test;
    }

    float MakeValueSafe(float value, float max)
    {
        if (value > max) value = max;
        if (value < 0) value = 0;
        return value;
    }

    public static Properties operator +(Properties a, Properties b)
    {
        Properties p = new Properties(a);
        p.Test = a.Test + b.Test;
        return p;
    }

}

