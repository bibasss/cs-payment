Client:

step 1:
npm i

step 2:
npm run build


step 3:
npm run preview




Server:

step 1:
dotnet restore
dotnet build

step2:
dotnet run



envirments:

appsettings.Development.json:
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=calc3d;Username=postgres;Password="
  }
}