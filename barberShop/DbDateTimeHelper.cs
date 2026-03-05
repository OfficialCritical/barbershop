namespace barberShop;

/// <summary>PostgreSQL timestamp with time zone csak UTC kind-et fogad. Segéd a DateTime átalakításhoz.</summary>
public static class DbDateTimeHelper
{
    public static DateTime ToUtc(DateTime d) => DateTime.SpecifyKind(d, DateTimeKind.Utc);
}
