// Generally:
//  - Add all proj libs 
//  - Add freq reused external libs

// Project libraries
global using CacheProvider.Caches;
global using CacheProvider.Caches.Interfaces;
global using CacheProvider.Providers;
global using Clutch.API.Controllers.Filters;
global using Clutch.API.Database.Context;
global using Clutch.API.Extensions;
global using Clutch.API.Models.Enums;
global using Clutch.API.Models.Image;
global using Clutch.API.Models.Registry;
global using Clutch.API.Properties;
global using Clutch.API.Providers.Event;
global using Clutch.API.Providers.Image;
global using Clutch.API.Providers.Interfaces;
global using Clutch.API.Providers.Registry;
global using Clutch.API.Repositories.Image;
global using Clutch.API.Repositories.Interfaces;
global using Clutch.API.Services.Image;
global using Clutch.API.Services.Interfaces;
global using Clutch.API.Utilities;

// External libraries
global using Microsoft.Extensions.Options;
global using System.Net;
global using System.Reflection;
global using System.Text;
global using System.Text.Json;