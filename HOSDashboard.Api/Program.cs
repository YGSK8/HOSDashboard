WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddSingleton<BigQueryService>(new BigQueryService(builder.Configuration["BigQuery:ProjectId"]!));
builder.Services.AddCors(options=>options.AddPolicy("AllowFrontend",policy=>policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
WebApplication app = builder.Build();
app.MapControllers();
app.UseCors("AllowFrontend");
app.Run();