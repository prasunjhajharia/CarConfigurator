 # Car Configurator

A small web app where you pick a car, choose its options, and see the price change as you go. When you are happy, you can save your build and share it with a link.

I built it to practise Blazor, ASP.NET Core, C#, Entity Framework Core and PostgreSQL.

The cars, prices and photos are only examples. They are not real offers.




## What it does

- Pick a brand, a model and a trim
- Choose options like engine, color, wheels and packages
- The price updates after every click
- Some options need or block others. For example, the Sport package adds the wheels it needs and blocks the diesel engine
- Save your build 
- An admin area, with a password, to change options, trims and prices

## What I used

- Blazor and MudBlazor for the pages
- ASP.NET Core with C# (.NET 10)
- PostgreSQL with Entity Framework Core
- xUnit for tests
- GitHub Actions to build and test on every push

## How to run it

You need the .NET 10 SDK, PostgreSQL and the EF Core tool (`dotnet tool install --global dotnet-ef`).

1. Create an empty PostgreSQL database called `carconfigurator`.

2. Save your database password and an admin password 

3. Create the tables:

   ```bash
   dotnet ef database update --project src/CarConfigurator.Data --startup-project src/CarConfigurator.Web
   ```

4. Start the app. The example cars are added the first time:

   ```bash
   dotnet run --project src/CarConfigurator.Web
   ```

5. Open `http://localhost:5247`. The admin login is at `/login`.

To run the tests:

```bash
dotnet test
```

## How it is built

- `Domain` holds the cars, the price calculation and the rules
- `Data` holds the database and the services
- `Web` holds the pages and the API
- `Tests` holds the tests

The price is always worked out on the server, so it cannot be changed from the browser. The rules are checked again when saving, so they cannot be skipped.



## Photo credits

## Photo credits

All photos are from [Unsplash](https://unsplash.com). 
