using System.ComponentModel;
using Google.Cloud.BigQuery.V2;
using Google.Apis.Bigquery.v2;
using Google.Apis.Bigquery.v2.Data;

public record Parameter(string Name,object Value);
public class RequestParameters
{
    public List<Parameter> Parameters = new List<Parameter>();
    public RequestParameters(Parameter[] parameters)
    {
        foreach(Parameter param in parameters)
        {
            Parameters.Add(param);
        }
    }
}
public class SchemaWithBigQueryType
{
    public Dictionary<string,BigQueryDbType>Schema {get;}=new Dictionary<string,BigQueryDbType>();
    public static Dictionary<string,string>TableFieldSchemaToBigQueryDbType=new Dictionary<string, string>
    {
        ["STRING"]="String",
        ["BYTES"]="Bytes",
        ["INTEGER"]="Int64",
        ["INT64"]="Int64",
        ["FLOAT"]="Float64",
        ["FLOAT64"]="Float64",
        ["BOOLEAN"]="Bool",
        ["TIMESTAMP"]="Timestamp",
        ["DATE"]="Date",
        ["TIME"]="Time",
        ["DATETIME"]="DateTime",
        ["GEOGRAPHY"]="Time",
        ["NUMERIC"]="Numeric",
        ["BIGNUMERIC"]="BigNumeric",
        ["JSON"]="Json",
    };
    public SchemaWithBigQueryType(BigQueryClient client, string projectId,string dataset,string tablename)
    {
        BigQueryTable table = client.GetTable(projectId,dataset,tablename);
        TableSchema schema = table.Schema;
        foreach(TableFieldSchema field in schema.Fields)
        {
            BigQueryDbType type;
            if(!Enum.TryParse(TableFieldSchemaToBigQueryDbType[field.Type],out type))
            {
                throw new InvalidEnumArgumentException();
            }
            Schema[field.Name]=type;
        }
    }

}