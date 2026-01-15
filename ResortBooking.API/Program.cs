using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using ResortBooking.API;
using ResortBooking.API.Filters;
using ResortBooking.Application.Interfaces.Repositories;
using ResortBooking.Application.Interfaces.Services;
using ResortBooking.Infrastructure.Persistence;
using ResortBooking.Infrastructure.Repositories;
using ResortBooking.Infrastructure.Services;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApiServices(builder.Configuration);
    //.AddApplicationServices()
    //.AddInfrastructureServices(builder.Configuration, builder.Environment)

var app = builder.Build();

app.UseApiServices(app.Environment);

app.Run();
