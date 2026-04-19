# AgroServis
![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?logo=microsoftsqlserver&logoColor=white)
![Entity Framework](https://img.shields.io/badge/EF_Core-512BD4?logo=dotnet&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?logo=docker&logoColor=white)
![SendGrid](https://img.shields.io/badge/SendGrid-Email-00B2FF?logo=sendgrid&logoColor=white)
![License](https://img.shields.io/badge/license-MIT-green)
## About
App intended for farmers to track maintenance of their agricultural 
machines. Built with Clean Architecture, role-based access control, 
and a Dockerized SQL Server backend.
## Features
### Euqipment
Anyone can see the list of equipment, Worker can start a maintainance by clicking Maintain button, Admin can add, edit or delete equipment.
List is sortable by Id Manufacturer, Model, SerialNumber, Type, Category and the date of the Last Maintenance.
Equipment that passed the date due of the maintenance is highlighted in red.

<img width="1918" height="917" alt="equipmentScreen" src="https://github.com/user-attachments/assets/7afdac6c-f716-44f5-8f49-d9db5011270b" />



## Quick start
1. Clone the repository.
2. Configure settings in `appsettings.json` (see `EmailSettings` section).
3. Run locally
## Configuration
- Set `SendGrid:ApiKey` in `appsettings.json` or as an environment variable
- Use `ASPNETCORE_ENVIRONMENT` to switch environments
