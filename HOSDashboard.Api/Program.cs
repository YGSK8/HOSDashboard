WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddSingleton<BigQueryService>(new BigQueryService(builder.Configuration["BigQuery:ProjectId"]!));
WebApplication app = builder.Build();
app.MapControllers();
app.Run();