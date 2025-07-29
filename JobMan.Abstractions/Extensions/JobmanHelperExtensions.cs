using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace JobMan;

public static class JobmanHelperExtensions
{
    public static readonly DateTime MinDateTime = new DateTime(1900, 1, 1);

    public static void Set<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value)
    {
        if (dict == null)
            throw new ArgumentNullException("dict");

        lock (dict)
        {
            if (dict.ContainsKey(key))
                dict[key] = value;
            else
                dict.Add(key, value);
        }
    }


    public static void Set<TKey, TValue>(this IDictionary<TKey, TValue> dict, IDictionary<TKey, TValue> with)
    {
        if (dict == null)
            throw new ArgumentNullException("dict");

        foreach (TKey withKey in with.Keys)
            dict.Set(withKey, with[withKey]);
    }

    /// <summary>
    /// Set dictionary value with function
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="dict"></param>
    /// <param name="key"></param>
    /// <param name="action">newValue = Func(currentValueOfKey) </param>
    public static void Set<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TValue, TValue> action)
    {
        if (dict == null)
            throw new ArgumentNullException("dict");

        lock (dict)
        {
            TValue currentValue = default(TValue);
            if (dict.ContainsKey(key))
                currentValue = dict[key];

            TValue newValue = action(currentValue);
            dict.Set(key, newValue);
        }
    }





    /// <summary>
    /// SortedList' te aynı anahtarda bir diğeri mevcut ise anahtarı bir ilerletir ve kaydetmeye zorlar
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="list"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public static void Set<TValue>(this SortedList<int, TValue> list, int key, TValue value)
    {
        if (list == null)
            throw new ArgumentNullException("dict");

        int _key = key;
        while (list.ContainsKey(_key))
        {
            _key++;
        }

        list.Add(_key, value);
    }

    public static bool TrySetForUniqueValue<TValue>(this SortedList<int, TValue> list, int key, TValue value)
    {
        if (list == null)
            throw new ArgumentNullException("dict");

        if (list.Values.Contains(value))
            return false;

        int _key = key;
        while (list.ContainsKey(_key))
        {
            _key++;
        }

        list.Add(_key, value);
        return true;
    }

    public static void AddKeys<TKey, TValue>(this IDictionary<TKey, TValue> dict, IEnumerable<TKey> keys)
    {
        if (dict == null)
            throw new ArgumentNullException("dict");

        lock (dict)
        {
            foreach (TKey key in keys)
                if (!dict.ContainsKey(key))
                    dict.Add(key, default(TValue));
        }
    }

    public static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, bool _throwIfNotExist = false)
    {
        if (dict == null)
            throw new ArgumentNullException("dict");

        lock (dict)
        {
            if (dict.ContainsKey(key))
                return dict[key];

            if (_throwIfNotExist)
                throw new InvalidOperationException($"value not found for: {key}");

            return default(TValue);
        }
    }

    public static TValue GetOr<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TValue> notAvailFunc)
    {
        if (dict == null)
            throw new ArgumentNullException("dict");

        if (dict.TryGetValue(key, out TValue value))
            return value;

        TValue val = notAvailFunc();
        dict.Set(key, val);
        return val;
    }

    public static bool IsActive(this WorkerStatus status)
    {
        switch (status)
        {
            case WorkerStatus.Running:
            case WorkerStatus.Idle:
            case WorkerStatus.WaitingStop:

                return true;
        }

        return false;
    }

    public static DateTime WithSecond(this DateTime time)
    {
        return new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second);
    }

    public static string Crop(this string str, int size, bool placeDots = false)
    {
        if (string.IsNullOrWhiteSpace(str))
            return str;

        if (str.Length < size)
            return str;

        if (placeDots)
            size = size - 3;

        string cropped = str.Substring(0, size);
        return placeDots ? cropped + "..." : cropped;
    }

    public static DateTime Bigger(this IEnumerable<DateTime> dateTimes)
    {
        if (dateTimes.Count() == 0)
            return DateTime.MinValue;

        if (dateTimes.Count() == 1)
            return dateTimes.First();

        return dateTimes.Max();
    }

    public static DateTime Bigger(params DateTime[] dateTimes)
    {
        return dateTimes.Bigger();
    }

    public static T To<T>(this DataRow row, string columnName, bool throwExceptionIfNotExist = false)
    {
        if (!row.Table.Columns.Contains(columnName))
        {
            if (throwExceptionIfNotExist)
                throw new ArgumentNullException(columnName);

            return default(T);
        }

        object objValue = row[columnName];
        if (Convert.IsDBNull(objValue))
        {
            if (throwExceptionIfNotExist)
                throw new ArgumentNullException(columnName);

            return default(T);
        }

        if (objValue is string strVal)
            objValue = strVal?.Trim();

        T value = (T)objValue;
        return value;
    }

    
}
