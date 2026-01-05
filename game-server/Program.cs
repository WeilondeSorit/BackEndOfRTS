var builder = WebApplication.CreateBuilder(args);

// Добавляем CORS (разрешаем запросы из Unity)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Добавляем контроллеры
builder.Services.AddControllers();

// Добавляем Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Используем CORS
app.UseCors("AllowAll");

// Включаем Swagger в development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Важно: указываем, что нужно использовать маршрутизацию
app.UseRouting();

app.UseAuthorization();

// Маппинг контроллеров
app.MapControllers();

app.Run();