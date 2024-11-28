using System.Xml.Linq;

var builder = WebApplication.CreateBuilder(args);

//Dobavil cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});


List<Order> repo = new List<Order>
{
    new Order(1,new DateTime(2024,2,23),"НоутБук","123","Работает","ККп","+79025682895","0", new List<string>())
};

bool isUpdatedStatus = false;
string message = "";

var app = builder.Build();
app.UseCors("AllowAllOrigins");

app.MapGet("/", () =>
{
    if (isUpdatedStatus)
    {
        string buffer = message;
        isUpdatedStatus = false;
        message = "";
        return Results.Json(new OrderUpdateStatusDTO(repo, buffer));
    }
    return Results.Json(repo);
});

app.MapPost("/", (Order o) =>
{
    repo.Add(o);
    return Results.Created($"/{o.Number}", o);
});

app.MapPut("/{num}", (int num, OrderUpdateDTO dto) =>
{
    Order buffer = repo.Find(o => o.Number == num);
    if (buffer == null)
        return Results.StatusCode(404);

    if (buffer.Description != dto.Description)
        buffer.Description = dto.Description;

    if (buffer.Master != dto.Master)
        buffer.Master = dto.Master;

    if (buffer.Status != dto.Status)
    {
        buffer.Status = dto.Status;
        isUpdatedStatus = true;
        message += "Заявка " + buffer.Number + " Изменена\n";
    }

    return Results.Json(buffer);
});

app.MapGet("/{num}", (int num) => repo.Find(o => o.Number == num));

app.MapGet("/filter/{param}", (string param) => repo.FindAll(o =>
    o.Model == param ||
    o.TypeTech == param ||
    o.Description == param ||
    o.Client == param ||
    o.Status == param ||
    o.Master == param));


app.MapGet("/statistics", () =>
{
    var completedOrders = repo.Count(o => o.Status == "Колличество");
    var averageCompletionTime = repo.Where(o => o.Status == "Время").Average(o => (o.DateAdded - DateTime.Now).TotalDays);
    var problemTypeStats = repo.GroupBy(o => o.TypeTech).Select(g => new { TypeTech = g.Key, Count = g.Count() });

    return Results.Json(new
    {
        CompletedOrders = completedOrders,
        AverageCompletionTime = averageCompletionTime,
        ProblemTypeStats = problemTypeStats
    });
});

app.Run();

class OrderUpdateDTO
{
    public string Status { get; set; }
    public string Description { get; set; }
    public string Master { get; set; }
    public string Comment { get; set; }
}

class OrderUpdateStatusDTO
{
    public List<Order> Orders { get; set; }
    public string Message { get; set; }

    public OrderUpdateStatusDTO(List<Order> orders, string message)
    {
        Orders = orders;
        Message = message;
    }
}



class Order
{
    public int Number { get; set; }
    public DateTime DateAdded { get; set; }
    public string TypeTech { get; set; }
    public string Model { get; set; }
    public string Description { get; set; }
    public string Client { get; set; }
    public string Phone { get; set; }
    public string Status { get; set; }
    public string Master { get; set; }
    public List<string> Comment { get; set; } = new List<string>();


    public Order(int number, DateTime dateAdded, string typeTech, string model, string description, string client, string phone, string status,List<string>Comment)
    {
        Number = number;   
        DateAdded = dateAdded;
        TypeTech = typeTech;
        Model = model;
        Description = description;
        Client = client;
        Phone = phone;
        Status = status;
        Master = "Я";
        this.Comment = Comment;
    }


}
