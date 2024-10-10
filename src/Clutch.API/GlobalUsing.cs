// Generally:
//  - Add all proj libs 
//  - Add freq reused external libs

// Project libraries
global using Clutch.API.Database.Context;
global using Clutch.API.Extensions;
global using Clutch.API.Models.Enums;
global using Clutch.API.Models.Event;
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

// External libraries
global using DataFerry.Algorithms;
global using DataFerry.Caches;
global using DataFerry.Caches.Interfaces;
global using DataFerry.Collections;
global using DataFerry.Providers;
global using DataFerry.Providers.Interfaces;
global using DataFerry.Utilities;
global using Microsoft.Extensions.Options;
global using System.Net;
global using System.Reflection;
global using System.Text;
global using System.Text.Json;