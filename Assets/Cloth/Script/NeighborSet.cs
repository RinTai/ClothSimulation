using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Rendering.Universal;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI;

public class NeighborSet<TKey, TValue>
{
    //��֤һ��Keyֵ��˳�򣬲�����Ȼ��ʹ�÷���
    private SortedDictionary<TKey, HashSet<TValue>> keyToValues;
    public NeighborSet()
    {
        keyToValues = new SortedDictionary<TKey, HashSet<TValue>>();
    }


    public void UniqueAdd(TKey key, TValue value)
    {
        if (!keyToValues.ContainsKey(key))
        {
            keyToValues.Add(key, new HashSet<TValue>());
        }

        if (!Contains(key, value))
        {
            keyToValues[key].Add(value);
        }

    }

    public bool Contains(TKey key,TValue value)
    {
        if(keyToValues.ContainsKey(key))
            if (keyToValues[key].Contains(value))
            {
                return true;
            }
            return false;
    }
    public HashSet<TValue> GetValue(TKey key)
    {
        return keyToValues[key];
    }

    public void Remove(TKey key,TValue value)
    {
        if (keyToValues.ContainsKey(key))
        {
            keyToValues[key].Remove(value); // �� HashSet ���Ƴ��ڽӵ�
            if (keyToValues[key].Count == 0)
            {
                keyToValues.Remove(key); // ���û���κ��ڽӵ��ˣ��Ƴ���������
            }
        }
    }
    
    public void SplitSet(out List<int> valuesIndex,out List<TValue> values)
    {
        valuesIndex = new List<int>();
        values = new List<TValue>();

        int lastnum = 0;
        foreach(var kvp in keyToValues)
        {
            values.AddRange(kvp.Value.ToArray()); //�����ݴ洢��ȥ�����Ǳ�Ҫ��
            valuesIndex.Add(lastnum); //���������� �����һ���Ǵ�0 ��ʼ���ڶ����Ǳ� �ڶ����� �� 3  ��ʼ���������Ǳ� ���������� �Ǵ� 6 ��ʼ�����ĸ��Ǳߡ� 
            lastnum += kvp.Value.Count;
        }

        
    }

}
