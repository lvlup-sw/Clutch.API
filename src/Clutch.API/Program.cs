using Microsoft.EntityFrameworkCore;

// Create the application
var builder = WebApplication.CreateBuilder(args);

// Aspire service defaults
builder.AddServiceDefaults();

// Extension methods to DI services + logging
builder.AddApplicationServices();
builder.AddApplicationLogging();

// Build the application
var app = builder.Build();

app.MapDefaultEndpoints();

// Env-specific configuration
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    // We need to seed the local db
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ContainerImageContext>();
        context.Database.Migrate();
    }
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    // We need to seed the local db
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ContainerImageContext>();
        context.Database.Migrate();
    }
}

// Add Swagger page
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Clutch.API V1");
});

// Configure HTTP Pipeline
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Setup ContainerImageController route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();