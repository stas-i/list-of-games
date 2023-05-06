namespace DataSinks.Api.Football;

public class MongoOptions
{
    public const string SectionName = "SinkStoreDatabase";
    public string ConnectionString { get; set; } = null!;

    public string DatabaseName { get; set; } = null!;
}
