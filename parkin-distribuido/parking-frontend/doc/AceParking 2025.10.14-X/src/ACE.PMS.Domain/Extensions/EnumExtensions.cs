namespace ACE.PMS.Domain.Extensions;

public class DisplayEn(string name) : Attribute
{
    public string Name { get; set; } = name;
}

public class DisplayTc(string name) : Attribute
{
    public string Name { get; set; } = name;
}

public static class EnumExtensions
{
    public static string GetDisplayEn(this Enum value)
    {
        var type = value.GetType();
        var memInfo = type.GetMember(value.ToString());
        if (memInfo.Length > 0)
        {
            var attrs = memInfo[0].GetCustomAttributes(typeof(DisplayEn), false);
            if (attrs.Length > 0)
            {
                return ((DisplayEn)attrs[0]).Name;
            }
        }
        return value.ToString();
    }
    public static string GetDisplayTc(this Enum value)
    {
        var type = value.GetType();
        var memInfo = type.GetMember(value.ToString());
        if (memInfo.Length > 0)
        {
            var attrs = memInfo[0].GetCustomAttributes(typeof(DisplayTc), false);
            if (attrs.Length > 0)
            {
                return ((DisplayTc)attrs[0]).Name;
            }
        }
        return value.ToString();
    }
}