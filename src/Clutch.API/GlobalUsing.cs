﻿// Generally:
//  - Add all proj libs 
//  - Add freq reused external libs

// Project libraries
global using Clutch.API.Extensions;
global using Clutch.API.Database.Context;
global using Clutch.API.Controllers.Filters;
global using Clutch.API.Models.Image;
global using Clutch.API.Properties;
global using Clutch.API.Providers.Image;
global using Clutch.API.Providers.Interfaces;
global using Clutch.API.Providers.Registry;
global using Clutch.API.Repositories.Image;
global using Clutch.API.Repositories.Interfaces;
global using Clutch.API.Services.Image;
global using Clutch.API.Models.Registry;
global using Clutch.API.Services.Interfaces;
global using Clutch.API.Models.Enums;
global using Clutch.API.Utilities;
global using CacheProvider.Providers;
global using CacheProvider.Caches;

// External libraries
global using Microsoft.Extensions.Options;
global using Microsoft.Extensions.Caching.Memory;
global using Microsoft.OpenApi.Models;
global using StackExchange.Redis;
global using Polly;
global using Polly.Wrap;
global using System.Text.Json;
global using System.Net;
global using System.Text;
global using System.Reflection;
global using CacheProvider.Caches.Interfaces;