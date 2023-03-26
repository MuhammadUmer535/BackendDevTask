using Lib.RabbitMq;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IEventBus, RabbitMqBus>(s =>
            {
                var lifetime = s.GetRequiredService<IHostApplicationLifetime>();
                return new RabbitMqBus(lifetime, builder.Configuration);
            });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

StripeConfiguration.ApiKey = "sk_test_4eC39HqLyjWDarjtT1zdp7dc";

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
