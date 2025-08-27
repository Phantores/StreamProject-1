[System.Serializable]
public class NullableInt
{
    public bool HasValue;
    public int Value;

    public int? ToNullable()
    {
        return HasValue ? Value : (int?)null;
    }
}
