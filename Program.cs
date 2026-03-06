namespace Project
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}


/*
 // Create order with transaction
using var transaction = await context.Database.BeginTransactionAsync();

try
{
    //var product = context.Products.AsNoTracking().ToList();
    //product.Add();

    //context.SaveChanges();
                

    var order = new Order
    {
        CreatedAt = DateTime.Now
    };

    context.Orders.Add(order);
    await context.SaveChangesAsync();

    var orderItem = new OrderItem
    {
        OrderId = order.Id,
        ProductId = product.Id,
        Quantity = 2,
        Price = product.Price
    };
    
    context.OrderItems.Add(orderItem);

    // error

    product.Stock -= 2;

    await context.SaveChangesAsync();

    await transaction.CommitAsync();

    Console.WriteLine("Order created successfully.");
}
catch (Exception ex)
{
    await transaction.RollbackAsync();
    Console.WriteLine("Error: " + ex.Message);
}
*/
